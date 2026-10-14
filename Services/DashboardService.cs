using LaptopStore.Data;
using LaptopStore.Helpers;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using LaptopStore.ViewModels.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LaptopStore.Models;

namespace LaptopStore.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public DashboardService(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task<DashboardVm> GetAsync()
    {
        var now = DateTime.UtcNow;
        var startToday = now.Date;
        var startMonth = new DateTime(now.Year, now.Month, 1);

        var paidStatuses = new[] { OrderStatus.Confirmed, OrderStatus.Packing, OrderStatus.Shipping, OrderStatus.Completed };
        var orders = _db.Orders.AsNoTracking();

        var vm = new DashboardVm
        {
            TotalRevenue = await orders.Where(o => paidStatuses.Contains(o.OrderStatus)).SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
            TodayRevenue = await orders.Where(o => paidStatuses.Contains(o.OrderStatus) && o.CreatedAt >= startToday).SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
            MonthRevenue = await orders.Where(o => paidStatuses.Contains(o.OrderStatus) && o.CreatedAt >= startMonth).SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
            TotalOrders = await orders.CountAsync(),
            PendingOrders = await orders.CountAsync(o => o.OrderStatus == OrderStatus.Pending),
            ShippingOrders = await orders.CountAsync(o => o.OrderStatus == OrderStatus.Shipping),
            CompletedOrders = await orders.CountAsync(o => o.OrderStatus == OrderStatus.Completed),
            CancelledOrders = await orders.CountAsync(o => o.OrderStatus == OrderStatus.Cancelled),
            TotalCustomers = (await _users.GetUsersInRoleAsync("Customer")).Count,
            TotalProducts = await _db.Products.CountAsync(p => p.IsActive),
            LowStockCount = await _db.Products.CountAsync(p => p.IsActive && p.StockQuantity <= 5)
        };

        for (var i = 11; i >= 0; i--)
        {
            var month = startMonth.AddMonths(-i);
            var next = month.AddMonths(1);
            vm.MonthLabels.Add($"T{month.Month}/{month.Year}");
            vm.MonthlyRevenue.Add(await orders
                .Where(o => paidStatuses.Contains(o.OrderStatus) && o.CreatedAt >= month && o.CreatedAt < next)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0);
            vm.MonthlyOrders.Add(await orders.CountAsync(o => o.CreatedAt >= month && o.CreatedAt < next));
        }

        var statuses = Enum.GetValues<OrderStatus>();
        foreach (var status in statuses)
        {
            vm.StatusLabels.Add(OrderStatusHelper.ToVietnamese(status));
            vm.StatusCounts.Add(await orders.CountAsync(o => o.OrderStatus == status));
        }

        vm.TopProducts = await _db.Products.AsNoTracking()
            .Include(p => p.Images)
            .OrderByDescending(p => p.SoldQuantity)
            .Take(5)
            .Select(p => new TopProductVm
            {
                Name = p.Name,
                Sold = p.SoldQuantity,
                Image = p.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
            }).ToListAsync();

        vm.TopBrands = await _db.Products.AsNoTracking()
            .Include(p => p.Brand)
            .GroupBy(p => p.Brand.Name)
            .Select(g => new TopBrandVm { Name = g.Key, Sold = g.Sum(x => x.SoldQuantity) })
            .OrderByDescending(x => x.Sold)
            .Take(5)
            .ToListAsync();

        vm.LowStock = await _db.Products.AsNoTracking()
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.StockQuantity <= 5)
            .OrderBy(p => p.StockQuantity)
            .Take(8)
            .Select(p => new LowStockVm
            {
                Id = p.Id,
                Name = p.Name,
                ProductCode = p.ProductCode,
                Stock = p.StockQuantity,
                Image = p.Images.OrderByDescending(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
            }).ToListAsync();

        return vm;
    }
}
