using LaptopStore.Models.Enums;

namespace LaptopStore.Helpers;

public static class OrderStatusHelper
{
    public static string ToVietnamese(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "Chờ xác nhận",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.Packing => "Đang đóng gói",
        OrderStatus.Shipping => "Đang giao hàng",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => status.ToString()
    };

    public static string BadgeClass(OrderStatus status) => status switch
    {
        OrderStatus.Pending => "badge-pending",
        OrderStatus.Confirmed => "badge-confirmed",
        OrderStatus.Packing => "badge-packing",
        OrderStatus.Shipping => "badge-shipping",
        OrderStatus.Completed => "badge-success",
        OrderStatus.Cancelled => "badge-danger",
        _ => "badge-muted"
    };

    public static string PaymentToVietnamese(PaymentStatus status) => status switch
    {
        PaymentStatus.Unpaid => "Chưa thanh toán",
        PaymentStatus.Pending => "Chờ thanh toán",
        PaymentStatus.Paid => "Đã thanh toán",
        PaymentStatus.Failed => "Thất bại",
        PaymentStatus.Refunded => "Đã hoàn tiền",
        _ => status.ToString()
    };

    public static string PaymentMethodToVietnamese(PaymentMethod method) => method switch
    {
        PaymentMethod.COD => "Thanh toán khi nhận hàng",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        PaymentMethod.VNPay => "VNPay",
        PaymentMethod.MoMo => "MoMo",
        _ => method.ToString()
    };
}
