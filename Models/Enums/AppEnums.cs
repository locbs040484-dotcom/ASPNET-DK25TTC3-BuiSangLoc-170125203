namespace LaptopStore.Models.Enums;

public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Packing = 2,
    Shipping = 3,
    Completed = 4,
    Cancelled = 5
}

public enum PaymentMethod
{
    COD = 0,
    BankTransfer = 1,
    VNPay = 2,
    MoMo = 3
}

public enum PaymentStatus
{
    Unpaid = 0,
    Pending = 1,
    Paid = 2,
    Failed = 3,
    Refunded = 4
}

public enum DiscountType
{
    Percentage = 0,
    FixedAmount = 1,
    FreeShip = 2
}

public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2
}

public enum ContactStatus
{
    New = 0,
    Processing = 1,
    Resolved = 2
}
