using LaptopStore.Models;

namespace LaptopStore.ViewModels.Product;

public class ProductDetailVm
{
    public Models.Product Product { get; set; } = null!;
    public List<ProductImage> Images { get; set; } = [];
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public Dictionary<int, int> RatingBreakdown { get; set; } = new();
    public List<Review> Reviews { get; set; } = [];
    public List<ProductCardVm> Related { get; set; } = [];
    public bool CanReview { get; set; }
    public bool HasReviewed { get; set; }
    public bool InWishlist { get; set; }
}
