namespace LaptopStore.ViewModels.Admin;

public class DashboardVm
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ShippingOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public List<string> MonthLabels { get; set; } = [];
    public List<decimal> MonthlyRevenue { get; set; } = [];
    public List<int> MonthlyOrders { get; set; } = [];
    public List<int> StatusCounts { get; set; } = [];
    public List<string> StatusLabels { get; set; } = [];
    public List<TopProductVm> TopProducts { get; set; } = [];
    public List<TopBrandVm> TopBrands { get; set; } = [];
    public List<LowStockVm> LowStock { get; set; } = [];
}

public class TopProductVm
{
    public string Name { get; set; } = string.Empty;
    public int Sold { get; set; }
    public string? Image { get; set; }
}

public class TopBrandVm
{
    public string Name { get; set; } = string.Empty;
    public int Sold { get; set; }
}

public class LowStockVm
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Stock { get; set; }
    public string? Image { get; set; }
}
