namespace LaptopStore.ViewModels.Product;

public class ProductFilterVm
{
    public string? Q { get; set; }
    public string? Category { get; set; }
    public List<string> Brands { get; set; } = [];
    public List<string> PriceRanges { get; set; } = [];
    public List<string> Cpus { get; set; } = [];
    public List<string> Rams { get; set; } = [];
    public List<string> Storages { get; set; } = [];
    public List<string> Gpus { get; set; } = [];
    public List<string> Screens { get; set; } = [];
    public List<string> RefreshRates { get; set; } = [];
    public string Sort { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public class ProductListVm
{
    public ProductFilterVm Filter { get; set; } = new();
    public List<ProductCardVm> Products { get; set; } = [];
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public string? Title { get; set; }
    public string? CategoryName { get; set; }
}
