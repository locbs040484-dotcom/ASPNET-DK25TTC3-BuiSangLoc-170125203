using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class BlogCategory
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Slug { get; set; } = string.Empty;

    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}

public class BlogPost
{
    public int Id { get; set; }

    public int BlogCategoryId { get; set; }
    public BlogCategory BlogCategory { get; set; } = null!;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Thumbnail { get; set; }

    [StringLength(400)]
    public string ShortDescription { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    [StringLength(120)]
    public string Author { get; set; } = "LaptopZone";

    [StringLength(200)]
    public string? MetaTitle { get; set; }

    [StringLength(400)]
    public string? MetaDescription { get; set; }

    public bool IsPublished { get; set; } = true;
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
