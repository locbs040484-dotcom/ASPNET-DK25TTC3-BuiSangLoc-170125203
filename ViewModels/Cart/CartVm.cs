namespace LaptopStore.ViewModels.Cart;

public class CartItemVm
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int StockQuantity { get; set; }
    public decimal SubTotal => Price * Quantity;
}

public class CartVm
{
    public List<CartItemVm> Items { get; set; } = [];
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public string? VoucherCode { get; set; }
    public string? VoucherMessage { get; set; }
    public bool VoucherValid { get; set; }
}
