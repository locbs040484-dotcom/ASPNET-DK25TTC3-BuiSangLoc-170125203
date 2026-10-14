# LaptopZone – Website bán laptop ASP.NET Core MVC

Website thương mại điện tử bán laptop, xây dựng bằng **ASP.NET Core MVC .NET 8**, Entity Framework Core, Identity và SQL Server.

## Requirements

- .NET 8 SDK
- SQL Server hoặc SQL Server Express (LocalDB cũng dùng được nếu sửa connection string)
- Visual Studio Code + C# extension (hoặc Visual Studio)

## Installation

```bash
cd LaptopStore
dotnet restore
dotnet ef database update
dotnet run
```

Nếu chưa có migration:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Mở trình duyệt theo URL hiển thị trong terminal, thường là `https://localhost:7xxx`.

## Connection string

`appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=LaptopStoreDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

Đổi `Server=localhost` thành `.\SQLEXPRESS` nếu bạn dùng SQL Server Express.

## Accounts

| Role | Email | Password |
|---|---|---|
| Admin | admin@laptopstore.com | Admin@123 |
| Staff | staff@laptopstore.com | Staff@123 |
| Customer | customer@gmail.com | User@123 |

## Features

- Trang chủ, danh mục, thương hiệu, tìm kiếm gợi ý AJAX
- Lọc / sắp xếp / phân trang sản phẩm
- Chi tiết laptop, gallery, thông số, đánh giá, sản phẩm tương tự
- Giỏ hàng AJAX, voucher, checkout COD / chuyển khoản
- Đơn hàng, timeline, hủy đơn khi chờ xác nhận
- Wishlist, so sánh tối đa 4 máy, sản phẩm vừa xem
- Tài khoản, đổi mật khẩu, avatar, sổ địa chỉ, thông báo
- Blog, liên hệ, trang giới thiệu và chính sách
- Admin / Staff dashboard Chart.js, quản lý sản phẩm, tồn kho, đơn, khách, voucher, review, banner, blog, cấu hình
- Phân quyền Admin, Staff, Customer
- Seed 41 laptop, review, đơn hàng, voucher, blog

## Notes

- Ảnh sản phẩm là bộ SVG đồng bộ theo thương hiệu, tỷ lệ 1000x750, `object-fit: contain`.
- Tồn kho giảm khi đơn được **xác nhận**, hoàn lại khi hủy sau khi đã trừ.
- Chỉ khách đã có đơn **Hoàn thành** mới được đánh giá sản phẩm.
