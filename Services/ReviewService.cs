using LaptopStore.Data;
using LaptopStore.Models;
using LaptopStore.Models.Enums;
using LaptopStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Services;

public class ReviewService : IReviewService
{
    private readonly ApplicationDbContext _db;

    public ReviewService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<(bool Ok, string Message)> CreateAsync(string userId, int productId, int rating, string comment, string? imageUrl)
    {
        if (rating is < 1 or > 5)
            return (false, "Đánh giá phải từ 1 đến 5 sao.");
        if (string.IsNullOrWhiteSpace(comment))
            return (false, "Vui lòng nhập nội dung đánh giá.");

        var purchased = await _db.OrderDetails.AnyAsync(d =>
            d.ProductId == productId && d.Order.UserId == userId && d.Order.OrderStatus == OrderStatus.Completed);
        if (!purchased)
            return (false, "Bạn chỉ có thể đánh giá sản phẩm đã mua và hoàn thành.");

        if (await _db.Reviews.AnyAsync(r => r.UserId == userId && r.ProductId == productId))
            return (false, "Bạn đã đánh giá sản phẩm này.");

        _db.Reviews.Add(new Review
        {
            UserId = userId,
            ProductId = productId,
            Rating = rating,
            Comment = comment.Trim(),
            ImageUrl = imageUrl,
            IsApproved = true
        });
        await _db.SaveChangesAsync();
        return (true, "Cảm ơn bạn đã đánh giá sản phẩm.");
    }

    public async Task<(bool Ok, string Message)> UpdateAsync(string userId, int reviewId, int rating, string comment)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
        if (review == null)
            return (false, "Không tìm thấy đánh giá.");
        review.Rating = rating;
        review.Comment = comment.Trim();
        review.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return (true, "Đã cập nhật đánh giá.");
    }

    public async Task<(bool Ok, string Message)> DeleteAsync(string userId, int reviewId)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == userId);
        if (review == null)
            return (false, "Không tìm thấy đánh giá.");
        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();
        return (true, "Đã xóa đánh giá.");
    }

    public async Task<(bool Ok, string Message)> ModerateAsync(int reviewId, bool hide)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);
        if (review == null)
            return (false, "Không tìm thấy đánh giá.");
        review.IsHidden = hide;
        await _db.SaveChangesAsync();
        return (true, hide ? "Đã ẩn đánh giá vi phạm." : "Đã hiện lại đánh giá.");
    }
}
