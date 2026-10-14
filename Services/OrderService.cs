using LaptopStore.Data;
using LaptopStore.Helpers;
using LaptopStore.Models;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly ICartService _carts;
    private readonly IVoucherService _vouchers;
    private readonly INotificationService _notifications;
    private readonly ICatalogService _catalog;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        ApplicationDbContext db,
        ICartService carts,
        IVoucherService vouchers,
        INotificationService notifications,
        ICatalogService catalog,
        ILogger<OrderService> logger)
    {
        _db = db;
        _carts = carts;
        _vouchers = vouchers;
        _notifications = notifications;
        _catalog = catalog;
        _logger = logger;
    }

    public async Task<(bool Ok, string Message, Order? Order)> CheckoutAsync(string userId, CheckoutInput input)
    {
        var cart = await _carts.GetCartAsync(userId, input.VoucherCode);
        if (cart.Items.Count == 0)
            return (false, "Giỏ hàng đang trống.", null);

        foreach (var item in cart.Items)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (product == null || !product.IsActive)
                return (false, $"Sản phẩm {item.Name} không còn khả dụng.", null);
            if (item.Quantity > product.StockQuantity)
                return (false, $"{item.Name} chỉ còn {product.StockQuantity} sản phẩm.", null);
        }

        var setting = await _catalog.GetSettingsAsync();
        var shipping = cart.ShippingFee;
        var discount = cart.Discount;
        if (!string.IsNullOrWhiteSpace(input.VoucherCode) && !cart.VoucherValid)
            return (false, cart.VoucherMessage ?? "Mã giảm giá không hợp lệ.", null);

        var order = new Order
        {
            OrderCode = GenerateCode(),
            UserId = userId,
            ReceiverName = input.ReceiverName.Trim(),
            Phone = input.Phone.Trim(),
            Email = input.Email.Trim(),
            Address = input.Address.Trim(),
            Note = input.Note,
            SubTotal = cart.SubTotal,
            DiscountAmount = discount,
            ShippingFee = shipping,
            TotalAmount = cart.Total,
            PaymentMethod = input.PaymentMethod,
            PaymentStatus = input.PaymentMethod == PaymentMethod.COD ? PaymentStatus.Unpaid : PaymentStatus.Pending,
            OrderStatus = OrderStatus.Pending,
            VoucherCode = cart.VoucherValid ? cart.VoucherCode : null
        };

        foreach (var item in cart.Items)
        {
            order.Details.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                ProductName = item.Name,
                ProductImage = item.ImageUrl,
                Price = item.Price,
                Quantity = item.Quantity,
                SubTotal = item.SubTotal
            });
        }

        order.StatusHistories.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Pending,
            Note = "Khách hàng đặt hàng",
            UpdatedBy = userId
        });

        if (!string.IsNullOrWhiteSpace(order.VoucherCode))
        {
            var voucher = await _db.Vouchers.FirstOrDefaultAsync(v => v.Code == order.VoucherCode);
            if (voucher != null)
            {
                voucher.UsedQuantity++;
                _db.VoucherUsages.Add(new VoucherUsage
                {
                    VoucherId = voucher.Id,
                    UserId = userId
                });
            }
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        await _carts.ClearAsync(userId);
        await _notifications.NotifyAsync(userId, "Đặt hàng thành công",
            $"Đơn hàng {order.OrderCode} đã được tạo. Chúng tôi sẽ xác nhận trong thời gian sớm nhất.",
            $"/account/orders/{order.OrderCode}");

        _logger.LogInformation("Created order {OrderCode} for user {UserId}", order.OrderCode, userId);
        return (true, "Đặt hàng thành công.", order);
    }

    public Task<List<Order>> GetUserOrdersAsync(string userId, OrderStatus? status)
    {
        var query = _db.Orders.AsNoTracking()
            .Include(o => o.Details)
            .Where(o => o.UserId == userId);
        if (status.HasValue)
            query = query.Where(o => o.OrderStatus == status);
        return query.OrderByDescending(o => o.CreatedAt).ToListAsync();
    }

    public Task<Order?> GetUserOrderAsync(string userId, string orderCode)
        => _db.Orders
            .Include(o => o.Details)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.OrderCode == orderCode);

    public async Task<(bool Ok, string Message)> CancelAsync(string userId, string orderCode, bool isStaff)
    {
        var order = await _db.Orders.Include(o => o.Details)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode && (isStaff || o.UserId == userId));
        if (order == null)
            return (false, "Không tìm thấy đơn hàng.");

        if (order.OrderStatus is OrderStatus.Shipping or OrderStatus.Completed)
            return (false, "Không thể hủy đơn đã giao hoặc hoàn thành.");

        if (!isStaff && order.OrderStatus != OrderStatus.Pending)
            return (false, "Bạn chỉ có thể hủy đơn khi đơn chưa được xử lý.");

        var previous = order.OrderStatus;
        order.OrderStatus = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        order.StatusHistories.Add(new OrderStatusHistory
        {
            Status = OrderStatus.Cancelled,
            Note = isStaff ? "Nhân viên xử lý hủy đơn" : "Khách hàng hủy đơn",
            UpdatedBy = userId
        });

        if (order.StockDeducted)
            await RestoreStockAsync(order);

        await _db.SaveChangesAsync();
        await _notifications.NotifyAsync(order.UserId, "Đơn hàng đã hủy",
            $"Đơn hàng {order.OrderCode} đã được hủy.", $"/account/orders/{order.OrderCode}");
        _logger.LogInformation("Cancelled order {OrderCode} from {Previous}", order.OrderCode, previous);
        return (true, "Đã hủy đơn hàng.");
    }

    public async Task<(bool Ok, string Message)> UpdateStatusAsync(int orderId, OrderStatus status, string? note, string updatedBy, bool markPaid = false)
    {
        var order = await _db.Orders.Include(o => o.Details).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return (false, "Không tìm thấy đơn hàng.");
        if (order.OrderStatus == OrderStatus.Cancelled && status != OrderStatus.Cancelled)
            return (false, "Không thể cập nhật đơn đã hủy.");

        var previous = order.OrderStatus;
        order.OrderStatus = status;
        order.UpdatedAt = DateTime.UtcNow;
        if (markPaid)
            order.PaymentStatus = PaymentStatus.Paid;
        if (status == OrderStatus.Completed && order.PaymentMethod == PaymentMethod.COD)
            order.PaymentStatus = PaymentStatus.Paid;

        // Deduct stock when confirming; restore if cancelled after deduction
        if (status == OrderStatus.Confirmed && !order.StockDeducted)
            await DeductStockAsync(order);
        if (status == OrderStatus.Cancelled && order.StockDeducted)
            await RestoreStockAsync(order);

        order.StatusHistories.Add(new OrderStatusHistory
        {
            Status = status,
            Note = note,
            UpdatedBy = updatedBy
        });

        await _db.SaveChangesAsync();
        await _notifications.NotifyAsync(order.UserId, $"Cập nhật đơn {order.OrderCode}",
            $"Đơn hàng của bạn hiện ở trạng thái: {OrderStatusHelper.ToVietnamese(status)}.",
            $"/account/orders/{order.OrderCode}");

        if (status == OrderStatus.Confirmed)
            _logger.LogInformation("Confirmed order {OrderCode}", order.OrderCode);
        if (status == OrderStatus.Cancelled)
            _logger.LogInformation("Cancelled order {OrderCode}", order.OrderCode);

        return (true, "Đã cập nhật trạng thái đơn hàng.");
    }

    public Task<Order?> GetByIdAsync(int id)
        => _db.Orders
            .Include(o => o.User)
            .Include(o => o.Details)
            .Include(o => o.StatusHistories)
            .FirstOrDefaultAsync(o => o.Id == id);

    private async Task DeductStockAsync(Order order)
    {
        foreach (var detail in order.Details)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == detail.ProductId);
            if (product == null) continue;
            product.StockQuantity = Math.Max(0, product.StockQuantity - detail.Quantity);
            product.SoldQuantity += detail.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
        }
        order.StockDeducted = true;
    }

    private async Task RestoreStockAsync(Order order)
    {
        foreach (var detail in order.Details)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == detail.ProductId);
            if (product == null) continue;
            product.StockQuantity += detail.Quantity;
            product.SoldQuantity = Math.Max(0, product.SoldQuantity - detail.Quantity);
            product.UpdatedAt = DateTime.UtcNow;
        }
        order.StockDeducted = false;
    }

    private static string GenerateCode()
        => $"LZ{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(10, 99)}";
}
