using LaptopStore.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Cpu> Cpus => Set<Cpu>();
    public DbSet<Gpu> Gpus => Set<Gpu>();
    public DbSet<RamOption> RamOptions => Set<RamOption>();
    public DbSet<StorageOption> StorageOptions => Set<StorageOption>();
    public DbSet<ScreenOption> ScreenOptions => Set<ScreenOption>();
    public DbSet<ColorOption> ColorOptions => Set<ColorOption>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<VoucherUsage> VoucherUsages => Set<VoucherUsage>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RecentlyViewed> RecentlyViewed => Set<RecentlyViewed>();
    public DbSet<WebsiteSetting> WebsiteSettings => Set<WebsiteSetting>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Brand>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<Product>().HasIndex(x => x.ProductCode).IsUnique();
        builder.Entity<Voucher>().HasIndex(x => x.Code).IsUnique();
        builder.Entity<Order>().HasIndex(x => x.OrderCode).IsUnique();
        builder.Entity<BlogPost>().HasIndex(x => x.Slug).IsUnique();
        builder.Entity<NewsletterSubscriber>().HasIndex(x => x.Email).IsUnique();

        builder.Entity<Product>()
            .HasOne(p => p.Brand).WithMany(b => b.Products).HasForeignKey(p => p.BrandId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Product>()
            .HasOne(p => p.Category).WithMany(c => c.Products).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Product>()
            .HasOne(p => p.Cpu).WithMany(c => c.Products).HasForeignKey(p => p.CpuId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Product>()
            .HasOne(p => p.Gpu).WithMany(g => g.Products).HasForeignKey(p => p.GpuId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Product>()
            .HasOne(p => p.RamOption).WithMany(r => r.Products).HasForeignKey(p => p.RamOptionId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Product>()
            .HasOne(p => p.StorageOption).WithMany(s => s.Products).HasForeignKey(p => p.StorageOptionId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Product>()
            .HasOne(p => p.ScreenOption).WithMany(s => s.Products).HasForeignKey(p => p.ScreenOptionId).OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Product>()
            .HasOne(p => p.ColorOption).WithMany(c => c.Products).HasForeignKey(p => p.ColorOptionId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<CartItem>()
            .HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
        builder.Entity<WishlistItem>()
            .HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();

        builder.Entity<OrderDetail>()
            .HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Review>()
            .HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ScreenOption>().Property(x => x.SizeInch).HasPrecision(4, 1);
    }
}
