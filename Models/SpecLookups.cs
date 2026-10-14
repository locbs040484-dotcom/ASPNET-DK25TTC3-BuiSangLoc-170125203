using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models;

public class Cpu
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Brand { get; set; }

    [StringLength(60)]
    public string? FilterKey { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class Gpu
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [StringLength(40)]
    public string Type { get; set; } = "Dedicated";

    [StringLength(60)]
    public string? FilterKey { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class RamOption
{
    public int Id { get; set; }

    public int SizeGB { get; set; }

    [StringLength(40)]
    public string Type { get; set; } = "DDR5";

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class StorageOption
{
    public int Id { get; set; }

    public int SizeGB { get; set; }

    [StringLength(40)]
    public string Type { get; set; } = "SSD";

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class ScreenOption
{
    public int Id { get; set; }

    public decimal SizeInch { get; set; }

    [StringLength(40)]
    public string? Resolution { get; set; }

    public int RefreshRate { get; set; } = 60;

    [StringLength(40)]
    public string? PanelType { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

public class ColorOption
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Name { get; set; } = string.Empty;

    [StringLength(10)]
    public string? Hex { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
