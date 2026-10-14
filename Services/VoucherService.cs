using LaptopStore.Data;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class VoucherService : IVoucherService
{
    private readonly ApplicationDbContext _db;

    public VoucherService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Ok, string Message, decimal Discount, decimal ShippingFee)> ApplyAsync(
        string code, decimal subTotal, decimal defaultShipping)
    {
        var now = DateTime.UtcNow;
        var voucher = await _db.Vouchers.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Code == code.Trim().ToUpper() && v.IsActive);

        if (voucher == null)
            return (false, "Mã giảm giá không tồn tại.", 0, defaultShipping);

        if (voucher.StartDate > now || voucher.EndDate < now)
            return (false, "Mã giảm giá đã hết hạn hoặc chưa đến thời gian áp dụng.", 0, defaultShipping);

        if (voucher.UsedQuantity >= voucher.Quantity)
            return (false, "Mã giảm giá đã hết lượt sử dụng.", 0, defaultShipping);

        if (subTotal < voucher.MinimumOrderAmount)
            return (false, $"Đơn hàng tối thiểu {Helpers.CurrencyHelper.ToVnd(voucher.MinimumOrderAmount)} để dùng mã này.", 0, defaultShipping);

        decimal discount = 0;
        var shipping = defaultShipping;

        switch (voucher.DiscountType)
        {
            case DiscountType.Percentage:
                discount = Math.Round(subTotal * voucher.DiscountValue / 100);
                if (voucher.MaximumDiscount.HasValue)
                    discount = Math.Min(discount, voucher.MaximumDiscount.Value);
                break;
            case DiscountType.FixedAmount:
                discount = Math.Min(voucher.DiscountValue, subTotal);
                break;
            case DiscountType.FreeShip:
                shipping = 0;
                break;
        }

        return (true, "Áp dụng mã giảm giá thành công.", discount, shipping);
    }
}
