using LaptopStore.Helpers;
using LaptopStore.Models;
using LaptopStore.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LaptopStore.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(roles);
        var accounts = await SeedUsersAsync(users);
        await SeedSettingsAsync(db);
        await SeedCatalogAsync(db);
        await SeedProductsAsync(db);
        await SeedVouchersAsync(db);
        await SeedBannersAsync(db);
        await SeedBlogAsync(db);
        await SeedReviewsAsync(db, accounts);
        await SeedOrdersAsync(db, accounts);
        await SeedNotificationsAsync(db, accounts);
        await SeedCustomerAddressAsync(db, accounts);
    }

    private static async Task SeedCustomerAddressAsync(ApplicationDbContext db, Dictionary<string, ApplicationUser> accounts)
    {
        if (await db.UserAddresses.AnyAsync()) return;
        if (!accounts.TryGetValue("Customer", out var customer)) return;
        db.UserAddresses.Add(new UserAddress
        {
            UserId = customer.Id,
            ReceiverName = customer.FullName,
            Phone = customer.PhoneNumber ?? "0908123456",
            Province = "TP. Hồ Chí Minh",
            District = "Quận 1",
            Ward = "Phường Bến Nghé",
            AddressLine = "12 Nguyễn Huệ",
            IsDefault = true
        });
        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roles)
    {
        foreach (var role in new[] { "Admin", "Staff", "Customer" })
        {
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task<Dictionary<string, ApplicationUser>> SeedUsersAsync(UserManager<ApplicationUser> users)
    {
        var map = new Dictionary<string, ApplicationUser>();

        async Task<ApplicationUser> Ensure(string email, string password, string name, string role, string phone)
        {
            var user = await users.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = name,
                    PhoneNumber = phone,
                    IsActive = true,
                    Avatar = "/images/avatars/default.svg",
                    CreatedAt = DateTime.UtcNow.AddMonths(-8)
                };
                var result = await users.CreateAsync(user, password);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
                await users.AddToRoleAsync(user, role);
            }
            map[role == "Customer" && email.StartsWith("customer") ? "Customer" : role] = user;
            return user;
        }

        await Ensure("admin@laptopstore.com", "Admin@123", "Quản trị viên", "Admin", "0901000001");
        await Ensure("staff@laptopstore.com", "Staff@123", "Nhân viên bán hàng", "Staff", "0901000002");
        var customer = await Ensure("customer@gmail.com", "User@123", "Nguyễn Minh Anh", "Customer", "0908123456");
        map["Customer"] = customer;

        var extras = new[]
        {
            ("tran.hoang@gmail.com", "Trần Hoàng Nam"),
            ("le.thu@gmail.com", "Lê Thị Thu"),
            ("pham.duc@gmail.com", "Phạm Đức Huy"),
            ("vo.lan@gmail.com", "Võ Ngọc Lan"),
            ("doan.khoa@gmail.com", "Đoàn Anh Khoa")
        };
        var i = 0;
        foreach (var (email, name) in extras)
        {
            var u = await users.FindByEmailAsync(email);
            if (u == null)
            {
                u = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = name,
                    PhoneNumber = $"0912{100000 + i}",
                    IsActive = true,
                    Avatar = "/images/avatars/default.svg",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6 + i)
                };
                await users.CreateAsync(u, "User@123");
                await users.AddToRoleAsync(u, "Customer");
            }
            map[$"C{i}"] = u;
            i++;
        }

        return map;
    }

    private static async Task SeedSettingsAsync(ApplicationDbContext db)
    {
        if (await db.WebsiteSettings.AnyAsync()) return;
        db.WebsiteSettings.Add(new WebsiteSetting());
        await db.SaveChangesAsync();
    }

    private static async Task SeedCatalogAsync(ApplicationDbContext db)
    {
        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Laptop Gaming", Slug = "laptop-gaming", Description = "Laptop chơi game, card rời RTX.", Icon = "bi-controller", DisplayOrder = 1, ImageUrl = "/images/categories/gaming.jpg" },
                new Category { Name = "Laptop Văn phòng", Slug = "laptop-van-phong", Description = "Máy văn phòng bền, cổng đầy đủ.", Icon = "bi-briefcase", DisplayOrder = 2, ImageUrl = "/images/categories/office.jpg" },
                new Category { Name = "Laptop Sinh viên", Slug = "laptop-sinh-vien", Description = "Giá tốt, nhẹ, đủ học tập.", Icon = "bi-mortarboard", DisplayOrder = 3, ImageUrl = "/images/categories/student.jpg" },
                new Category { Name = "Laptop Đồ họa", Slug = "laptop-do-hoa", Description = "Màn đẹp, card mạnh cho thiết kế.", Icon = "bi-palette", DisplayOrder = 4, ImageUrl = "/images/categories/design.jpg" },
                new Category { Name = "Laptop Mỏng nhẹ", Slug = "laptop-mong-nhe", Description = "Ultrabook di động hàng ngày.", Icon = "bi-laptop", DisplayOrder = 5, ImageUrl = "/images/categories/thin.jpg" },
                new Category { Name = "Laptop Doanh nhân", Slug = "laptop-doanh-nhan", Description = "Hoàn thiện cao, bảo mật tốt.", Icon = "bi-person-badge", DisplayOrder = 6, ImageUrl = "/images/categories/business.jpg" },
                new Category { Name = "MacBook", Slug = "macbook", Description = "MacBook Air và MacBook Pro.", Icon = "bi-apple", DisplayOrder = 7, ImageUrl = "/images/categories/mac.jpg" },
                new Category { Name = "Laptop Workstation", Slug = "laptop-workstation", Description = "Máy trạm di động cho kỹ thuật.", Icon = "bi-cpu", DisplayOrder = 8, ImageUrl = "/images/categories/workstation.jpg" }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Brands.AnyAsync())
        {
            db.Brands.AddRange(
                new Brand { Name = "ASUS", Slug = "asus", Logo = "/images/brands/asus.svg", Description = "Thương hiệu Đài Loan với ROG, TUF, Vivobook và Zenbook." },
                new Brand { Name = "Acer", Slug = "acer", Logo = "/images/brands/acer.svg", Description = "Aspire, Swift, Nitro và Predator cho nhiều phân khúc." },
                new Brand { Name = "Dell", Slug = "dell", Logo = "/images/brands/dell.svg", Description = "Inspiron, Vostro, Latitude, XPS và G-series." },
                new Brand { Name = "HP", Slug = "hp", Logo = "/images/brands/hp.svg", Description = "Pavilion, Victus, Envy và dòng doanh nghiệp." },
                new Brand { Name = "Lenovo", Slug = "lenovo", Logo = "/images/brands/lenovo.svg", Description = "IdeaPad, ThinkPad, LOQ và Legion." },
                new Brand { Name = "MSI", Slug = "msi", Logo = "/images/brands/msi.svg", Description = "Gaming Katana, Cyborg và dòng Modern văn phòng." },
                new Brand { Name = "Apple", Slug = "apple", Logo = "/images/brands/apple.svg", Description = "MacBook Air và MacBook Pro chip Apple Silicon." },
                new Brand { Name = "Gigabyte", Slug = "gigabyte", Logo = "/images/brands/gigabyte.svg", Description = "Aorus và Gaming cho người dùng hiệu năng cao." }
            );
            await db.SaveChangesAsync();
        }

        if (!await db.Cpus.AnyAsync())
        {
            db.Cpus.AddRange(
                new Cpu { Name = "Intel Core i3-1215U", Brand = "Intel", FilterKey = "i3" },
                new Cpu { Name = "Intel Core i5-1235U", Brand = "Intel", FilterKey = "i5" },
                new Cpu { Name = "Intel Core i5-1335U", Brand = "Intel", FilterKey = "i5" },
                new Cpu { Name = "Intel Core i7-1355U", Brand = "Intel", FilterKey = "i7" },
                new Cpu { Name = "Intel Core Ultra 5 125H", Brand = "Intel", FilterKey = "ultra" },
                new Cpu { Name = "Intel Core Ultra 7 155H", Brand = "Intel", FilterKey = "ultra" },
                new Cpu { Name = "AMD Ryzen 5 7530U", Brand = "AMD", FilterKey = "r5" },
                new Cpu { Name = "AMD Ryzen 7 7735HS", Brand = "AMD", FilterKey = "r7" },
                new Cpu { Name = "Apple M2", Brand = "Apple", FilterKey = "m2" },
                new Cpu { Name = "Apple M3", Brand = "Apple", FilterKey = "m3" }
            );
            db.Gpus.AddRange(
                new Gpu { Name = "Intel Iris Xe", Type = "Integrated", FilterKey = "integrated" },
                new Gpu { Name = "Intel Arc", Type = "Integrated", FilterKey = "integrated" },
                new Gpu { Name = "AMD Radeon", Type = "Integrated", FilterKey = "integrated" },
                new Gpu { Name = "Apple GPU", Type = "Integrated", FilterKey = "integrated" },
                new Gpu { Name = "NVIDIA RTX 2050 4GB", Type = "Dedicated", FilterKey = "rtx2050" },
                new Gpu { Name = "NVIDIA RTX 3050 6GB", Type = "Dedicated", FilterKey = "rtx3050" },
                new Gpu { Name = "NVIDIA RTX 4050 6GB", Type = "Dedicated", FilterKey = "rtx4050" },
                new Gpu { Name = "NVIDIA RTX 4060 8GB", Type = "Dedicated", FilterKey = "rtx4060" },
                new Gpu { Name = "NVIDIA RTX 4070 8GB", Type = "Dedicated", FilterKey = "rtx4070" }
            );
            db.RamOptions.AddRange(
                new RamOption { SizeGB = 8, Type = "DDR4" },
                new RamOption { SizeGB = 16, Type = "DDR5" },
                new RamOption { SizeGB = 32, Type = "DDR5" },
                new RamOption { SizeGB = 64, Type = "DDR5" }
            );
            db.StorageOptions.AddRange(
                new StorageOption { SizeGB = 256, Type = "SSD" },
                new StorageOption { SizeGB = 512, Type = "SSD" },
                new StorageOption { SizeGB = 1024, Type = "SSD" },
                new StorageOption { SizeGB = 2048, Type = "SSD" }
            );
            db.ScreenOptions.AddRange(
                new ScreenOption { SizeInch = 13.4m, Resolution = "1920x1200", RefreshRate = 60, PanelType = "IPS" },
                new ScreenOption { SizeInch = 13.6m, Resolution = "2560x1664", RefreshRate = 60, PanelType = "Liquid Retina" },
                new ScreenOption { SizeInch = 14m, Resolution = "1920x1200", RefreshRate = 60, PanelType = "IPS" },
                new ScreenOption { SizeInch = 15.6m, Resolution = "1920x1080", RefreshRate = 144, PanelType = "IPS" },
                new ScreenOption { SizeInch = 16m, Resolution = "2560x1600", RefreshRate = 165, PanelType = "IPS" },
                new ScreenOption { SizeInch = 17.3m, Resolution = "1920x1080", RefreshRate = 144, PanelType = "IPS" }
            );
            db.ColorOptions.AddRange(
                new ColorOption { Name = "Đen", Hex = "#111827" },
                new ColorOption { Name = "Bạc", Hex = "#9CA3AF" },
                new ColorOption { Name = "Xám", Hex = "#6B7280" },
                new ColorOption { Name = "Xanh", Hex = "#1D4ED8" },
                new ColorOption { Name = "Trắng", Hex = "#F3F4F6" }
            );
            await db.SaveChangesAsync();
        }
    }

    private static async Task SeedProductsAsync(ApplicationDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var brands = await db.Brands.ToDictionaryAsync(b => b.Name, b => b.Id);
        var cats = await db.Categories.ToDictionaryAsync(c => c.Slug, c => c.Id);
        var cpus = await db.Cpus.ToListAsync();
        var gpus = await db.Gpus.ToListAsync();
        var rams = await db.RamOptions.ToListAsync();
        var storages = await db.StorageOptions.ToListAsync();
        var screens = await db.ScreenOptions.ToListAsync();
        var colors = await db.ColorOptions.ToListAsync();

        var created = DateTime.UtcNow.AddDays(-90);
        foreach (var s in ProductSeedData.All())
        {
            if (!brands.ContainsKey(s.Brand) || !cats.ContainsKey(s.Category))
                continue;

            var discount = s.Original > s.Sale ? (int)Math.Round((1 - s.Sale / s.Original) * 100) : 0;
            var cpu = cpus.FirstOrDefault(c => s.Cpu.Contains(c.Name.Split(' ').Last()) || c.FilterKey == s.CpuKey)
                      ?? cpus.FirstOrDefault(c => c.FilterKey == s.CpuKey);
            var gpu = gpus.FirstOrDefault(g => g.FilterKey == s.GpuKey)
                      ?? gpus.FirstOrDefault(g => s.Gpu.Contains("RTX") && g.Name.Contains(s.Gpu.Split(' ').Skip(1).FirstOrDefault() ?? "x"));
            var ram = rams.OrderBy(r => Math.Abs(r.SizeGB - s.RamGb)).First();
            var storage = storages.OrderBy(st => Math.Abs(st.SizeGB - s.SsdGb)).First();
            var screen = screens.OrderBy(sc => Math.Abs((double)(sc.SizeInch - s.ScreenInch))).First();
            var color = colors[Math.Abs(s.Code.GetHashCode()) % colors.Count];

            var product = new Product
            {
                ProductCode = s.Code,
                Name = s.Name,
                Slug = SlugHelper.Generate(s.Name),
                BrandId = brands[s.Brand],
                CategoryId = cats[s.Category],
                ShortDescription = s.Short,
                Description = s.Description,
                OriginalPrice = s.Original,
                SalePrice = s.Sale,
                DiscountPercent = discount,
                StockQuantity = s.Stock,
                SoldQuantity = s.Sold,
                WarrantyMonths = s.Warranty,
                Status = "Active",
                IsFeatured = s.Featured,
                IsNew = s.New,
                IsBestSeller = s.Best,
                IsActive = true,
                MetaTitle = $"{s.Name} | LaptopZone",
                MetaDescription = s.Short,
                CreatedAt = created,
                UpdatedAt = created,
                CpuId = cpu?.Id,
                GpuId = gpu?.Id,
                RamOptionId = ram.Id,
                StorageOptionId = storage.Id,
                ScreenOptionId = screen.Id,
                ColorOptionId = color.Id,
                CpuName = s.Cpu,
                RamType = s.RamType,
                RamBus = s.RamBus,
                MaxRam = s.MaxRam,
                StorageLabel = s.SsdGb >= 1024 ? $"{s.SsdGb / 1024}TB SSD" : $"{s.SsdGb}GB SSD",
                StorageType = s.StorageType,
                ScreenLabel = $"{s.ScreenInch}\" {s.Panel} {s.Refresh}Hz",
                ScreenResolution = s.Resolution,
                RefreshRate = s.Refresh,
                PanelType = s.Panel,
                ColorGamut = s.Gamut,
                OperatingSystem = s.Os,
                Battery = s.Battery,
                Weight = s.Weight,
                Material = s.Material,
                Keyboard = s.Keyboard,
                Webcam = s.Webcam,
                WiFi = s.Wifi,
                Bluetooth = s.Bluetooth,
                Ports = s.Ports,
                Color = s.Color,
                Dimensions = s.Dimensions
            };

            for (var i = 1; i <= 5; i++)
            {
                product.Images.Add(new ProductImage
                {
                    ImageUrl = $"/images/products/{s.ImagePrefix}-{i}.jpg",
                    IsPrimary = i == 1,
                    DisplayOrder = i
                });
            }

            db.Products.Add(product);
            created = created.AddHours(6);
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedVouchersAsync(ApplicationDbContext db)
    {
        if (await db.Vouchers.AnyAsync()) return;
        var now = DateTime.UtcNow;
        db.Vouchers.AddRange(
            new Voucher { Code = "WELCOME500", Description = "Giảm 500.000 ₫ cho đơn từ 15 triệu", DiscountType = DiscountType.FixedAmount, DiscountValue = 500000, MinimumOrderAmount = 15000000, StartDate = now.AddMonths(-2), EndDate = now.AddMonths(6), Quantity = 200, UsedQuantity = 34, IsActive = true },
            new Voucher { Code = "LAPTOP10", Description = "Giảm 10%, tối đa 1.000.000 ₫", DiscountType = DiscountType.Percentage, DiscountValue = 10, MaximumDiscount = 1000000, MinimumOrderAmount = 10000000, StartDate = now.AddMonths(-1), EndDate = now.AddMonths(4), Quantity = 150, UsedQuantity = 21, IsActive = true },
            new Voucher { Code = "FREESHIP", Description = "Miễn phí giao hàng", DiscountType = DiscountType.FreeShip, DiscountValue = 0, MinimumOrderAmount = 5000000, StartDate = now.AddMonths(-3), EndDate = now.AddMonths(8), Quantity = 500, UsedQuantity = 88, IsActive = true },
            new Voucher { Code = "GAMING15", Description = "Giảm 15% laptop gaming, tối đa 2 triệu", DiscountType = DiscountType.Percentage, DiscountValue = 15, MaximumDiscount = 2000000, MinimumOrderAmount = 20000000, StartDate = now.AddDays(-20), EndDate = now.AddMonths(2), Quantity = 80, UsedQuantity = 12, IsActive = true },
            new Voucher { Code = "MAC1TR", Description = "Giảm 1.000.000 ₫ cho MacBook", DiscountType = DiscountType.FixedAmount, DiscountValue = 1000000, MinimumOrderAmount = 22000000, StartDate = now.AddDays(-10), EndDate = now.AddMonths(3), Quantity = 50, UsedQuantity = 9, IsActive = true },
            new Voucher { Code = "STUDENT300", Description = "Sinh viên giảm 300.000 ₫", DiscountType = DiscountType.FixedAmount, DiscountValue = 300000, MinimumOrderAmount = 10000000, StartDate = now.AddMonths(-1), EndDate = now.AddMonths(5), Quantity = 300, UsedQuantity = 47, IsActive = true },
            new Voucher { Code = "FLASH20", Description = "Flash sale giảm 20%, tối đa 1.5 triệu", DiscountType = DiscountType.Percentage, DiscountValue = 20, MaximumDiscount = 1500000, MinimumOrderAmount = 18000000, StartDate = now.AddDays(-5), EndDate = now.AddDays(25), Quantity = 40, UsedQuantity = 6, IsActive = true },
            new Voucher { Code = "VIP800", Description = "Khách hàng thân thiết giảm 800.000 ₫", DiscountType = DiscountType.FixedAmount, DiscountValue = 800000, MinimumOrderAmount = 25000000, StartDate = now.AddMonths(-2), EndDate = now.AddMonths(6), Quantity = 60, UsedQuantity = 11, IsActive = true }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedBannersAsync(ApplicationDbContext db)
    {
        if (await db.Banners.AnyAsync()) return;
        db.Banners.AddRange(
            new Banner { Title = "Laptop thế hệ mới", Subtitle = "Hiệu năng mạnh mẽ – Giá tốt mỗi ngày", ImageUrl = "/images/banners/banner-gaming.jpg", Link = "/products?category=laptop-gaming", ButtonText = "Khám phá ngay", DisplayOrder = 1 },
            new Banner { Title = "Laptop văn phòng", Subtitle = "Mỏng nhẹ, pin bền, làm việc cả ngày", ImageUrl = "/images/banners/banner-office.jpg", Link = "/products?category=laptop-van-phong", ButtonText = "Xem bộ sưu tập", DisplayOrder = 2 },
            new Banner { Title = "MacBook chính hãng", Subtitle = "Air & Pro chip M2 / M3 – bảo hành rõ ràng", ImageUrl = "/images/banners/banner-mac.jpg", Link = "/products?category=macbook", ButtonText = "Chọn MacBook", DisplayOrder = 3 }
        );
        await db.SaveChangesAsync();
    }

    private static async Task SeedBlogAsync(ApplicationDbContext db)
    {
        if (await db.BlogCategories.AnyAsync()) return;
        var cats = new[]
        {
            new BlogCategory { Name = "Tư vấn mua hàng", Slug = "tu-van-mua-hang" },
            new BlogCategory { Name = "Kinh nghiệm", Slug = "kinh-nghiem" },
            new BlogCategory { Name = "So sánh", Slug = "so-sanh" }
        };
        db.BlogCategories.AddRange(cats);
        await db.SaveChangesAsync();

        var posts = new (string Title, int Cat, string Thumb, string Short, string Body)[]
        {
            ("Top laptop sinh viên đáng mua", 1, "blog-sv.svg", "Gợi ý máy học tập trong các khoảng giá phổ biến tại Việt Nam.", "Sinh viên nên ưu tiên máy 1.4–1.8kg, RAM 16GB và SSD 512GB. Ở tầm 13–16 triệu, Aspire 5 hoặc IdeaPad Slim 3 đủ dùng. Nếu học lập trình, hãy cân nhắc LOQ 15 hoặc TUF để sau này làm đồ án nặng."),
            ("Hướng dẫn chọn laptop học lập trình", 1, "blog-code.svg", "Cấu hình tối thiểu và tối ưu cho sinh viên IT.", "VS Code, Android Studio và Docker cần RAM. 16GB là mức hợp lý năm nay. CPU i5/Ryzen 5 trở lên. Màn 14–16 inch, bàn phím êm như ThinkPad hoặc Legion sẽ dễ chịu khi gõ nhiều."),
            ("Laptop Gaming cần bao nhiêu RAM?", 2, "blog-ram.svg", "8GB, 16GB hay 32GB cho game hiện tại.", "Hầu hết game 2024–2026 chạy tốt với 16GB. 8GB sẽ thiếu khi mở Discord và Chrome cùng lúc. 32GB chỉ cần nếu stream, edit video song song. Laptop gaming tầm trung hiện nay gần như mặc định 16GB."),
            ("Core i5 và Ryzen 5 nên chọn loại nào?", 3, "blog-cpu.svg", "So sánh hai dòng CPU phổ biến trên laptop văn phòng và gaming.", "Ryzen 5 thường mạnh đa nhân, giá tốt. Intel i5 ổn hơn về tương thích phần mềm kế toán và pin trên ultrabook. Với máy gaming, cả hai đều tốt nếu đi kèm RTX 4050/4060. Hãy xem thêm tản nhiệt và bảo hành."),
            ("Có nên mua laptop màn hình OLED?", 2, "blog-oled.svg", "Ưu nhược điểm OLED trên laptop học tập và đồ họa.", "OLED đẹp hơn rõ khi xem phim và chỉnh ảnh, độ tương phản cao. Nhược điểm là giá cao hơn, cần chú ý burn-in nếu để logo tĩnh nhiều năm. Sinh viên chỉ soạn thảo có thể chọn IPS để tiết kiệm."),
            ("Hướng dẫn bảo quản pin laptop", 2, "blog-pin.svg", "Cách sạc và bảo dưỡng pin để dùng được 2–3 năm.", "Tránh để pin 0% thường xuyên. Cắm sạc khi còn khoảng 20–30%. Nhiều hãng có chế độ giới hạn 80% cho máy để bàn. Không để máy trên chăn khi chơi game vì nóng sẽ già pin nhanh."),
            ("Cách chọn laptop văn phòng dưới 20 triệu", 1, "blog-office.svg", "Checklist ngắn cho nhân viên hành chính và kế toán.", "Cần đủ cổng HDMI, USB-A và nếu được thì RJ45. Ưu tiên máy có bàn phím số. Inspiron 15, Pavilion 15, ThinkBook 14 đều là lựa chọn an toàn, dễ sửa chữa."),
            ("So sánh TUF Gaming F15 và LOQ 15", 3, "blog-tuf-loq.svg", "Hai mẫu gaming phổ thông đáng mua nhất tầm 23 triệu.", "TUF bền, chuẩn quân đội, hơi nặng. LOQ nhẹ hơn một chút, bàn phím Lenovo dễ gõ hơn khi code. Cùng RTX 4050 thì chọn máy nào tản nhiệt tốt hơn tại cửa hàng bạn sờ thử."),
            ("MacBook Air M3 có đáng nâng từ M2?", 3, "blog-m2m3.svg", "Ai nên đổi máy, ai nên giữ Air M2.", "Nếu bạn đang dùng M2 16GB thì chưa cần đổi. Người mới mua nên lấy M3 vì hỗ trợ màn hình ngoài tốt hơn và giữ giá lâu hơn. Sinh viên ngân sách hẹp vẫn nên cân Air M2 giảm giá."),
            ("Checklist kiểm tra laptop khi nhận hàng", 2, "blog-check.svg", "Những việc nên làm trong 15 phút đầu sau khi unbox.", "Kiểm tra vỏ, bản lề, pixel chết, cổng sạc, micro và loa. Chạy thử Wi-Fi, cập nhật Windows/macOS. Giữ hộp và hóa đơn để bảo hành. Nếu mua online, quay video lúc mở hộp.")
        };

        var day = DateTime.UtcNow.AddDays(-60);
        foreach (var p in posts)
        {
            db.BlogPosts.Add(new BlogPost
            {
                BlogCategoryId = cats[p.Cat - 1].Id,
                Title = p.Title,
                Slug = SlugHelper.Generate(p.Title),
                Thumbnail = $"/images/blog/{Path.GetFileNameWithoutExtension(p.Thumb)}.jpg",
                ShortDescription = p.Short,
                Content = $"<p>{p.Body}</p><p>LaptopZone luôn sẵn sàng tư vấn cấu hình theo nhu cầu học tập, văn phòng hoặc gaming. Bạn có thể mang máy cũ đến cửa hàng để được định giá thu cũ đổi mới.</p>",
                Author = "Biên tập LaptopZone",
                MetaTitle = p.Title,
                MetaDescription = p.Short,
                PublishedAt = day,
                CreatedAt = day,
                IsPublished = true
            });
            day = day.AddDays(5);
        }
        await db.SaveChangesAsync();
    }

    private static readonly string[] ReviewTexts =
    [
        "Máy chạy ổn định, màn hình đẹp và khá nhẹ.",
        "Hiệu năng tốt trong tầm giá, dùng code và học tập rất ổn.",
        "Bàn phím êm, pin dùng được cả buổi sáng trên lớp.",
        "Tản nhiệt tốt hơn mình nghĩ, chơi game không quá ồn.",
        "Màn sắc nét, màu ổn để xem phim và làm slide.",
        "Shop tư vấn rõ, máy nguyên seal, kích hoạt bảo hành nhanh.",
        "Máy hơi nặng nếu mang cả ngày nhưng hiệu năng đáng tiền.",
        "SSD nhanh, mở máy chỉ vài giây. Rất hài lòng.",
        "Loa khá to, họp Zoom rõ. Webcam tạm ổn.",
        "Giá tốt so với cấu hình. Sẽ giới thiệu bạn bè.",
        "Trackpad chính xác, gõ Word lâu không mỏi tay.",
        "Chơi LOL và Valorant mượt, setting cao vẫn ổn định.",
        "Vỏ chắc, bản lề không bị kênh. Đáng mua.",
        "Pin thực tế khoảng 6 tiếng văn phòng, chấp nhận được.",
        "Màu máy đẹp, nhìn sang hơn ảnh trên web.",
        "Nhân viên giao hàng đúng hẹn, đóng gói cẩn thận.",
        "Dùng Photoshop cơ bản mượt. Render video thì hơi lâu.",
        "Không có điểm trừ lớn nào trong tầm giá này.",
        "Máy mát khi soạn thảo, chỉ ấm nhẹ khi compile.",
        "Rất phù hợp sinh viên năm nhất, không cần cấu hình quá cao."
    ];

    private static async Task SeedReviewsAsync(ApplicationDbContext db, Dictionary<string, ApplicationUser> accounts)
    {
        if (await db.Reviews.AnyAsync()) return;
        var products = await db.Products.Select(p => p.Id).ToListAsync();
        // Keep the main demo customer free of seed reviews so the purchase → review flow still works.
        var reviewers = accounts.Values
            .DistinctBy(u => u.Id)
            .Where(u => !string.Equals(u.Email, "customer@gmail.com", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var rng = new Random(42);
        var created = DateTime.UtcNow.AddDays(-80);
        var count = 0;
        foreach (var pid in products)
        {
            var n = rng.Next(1, 3);
            for (var i = 0; i < n && count < 70; i++)
            {
                var user = reviewers[rng.Next(reviewers.Count)];
                if (await db.Reviews.AnyAsync(r => r.ProductId == pid && r.UserId == user.Id))
                    continue;
                db.Reviews.Add(new Review
                {
                    ProductId = pid,
                    UserId = user.Id,
                    Rating = rng.Next(0, 10) < 8 ? rng.Next(4, 6) : rng.Next(3, 5),
                    Comment = ReviewTexts[rng.Next(ReviewTexts.Length)],
                    IsApproved = true,
                    CreatedAt = created.AddHours(count * 3)
                });
                count++;
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedOrdersAsync(ApplicationDbContext db, Dictionary<string, ApplicationUser> accounts)
    {
        if (await db.Orders.AnyAsync()) return;
        var products = await db.Products.Include(p => p.Images).ToListAsync();
        var customers = accounts.Values.Where(u => u.Email != "admin@laptopstore.com" && u.Email != "staff@laptopstore.com").DistinctBy(u => u.Id).ToList();
        var statuses = new[]
        {
            OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Completed,
            OrderStatus.Shipping, OrderStatus.Shipping, OrderStatus.Packing,
            OrderStatus.Confirmed, OrderStatus.Pending, OrderStatus.Pending,
            OrderStatus.Cancelled, OrderStatus.Completed, OrderStatus.Completed,
            OrderStatus.Shipping, OrderStatus.Completed, OrderStatus.Confirmed,
            OrderStatus.Completed, OrderStatus.Packing, OrderStatus.Cancelled,
            OrderStatus.Completed, OrderStatus.Completed, OrderStatus.Shipping,
            OrderStatus.Pending, OrderStatus.Completed, OrderStatus.Completed
        };

        var rng = new Random(7);
        for (var i = 0; i < statuses.Length; i++)
        {
            var user = customers[i % customers.Count];
            var pick = products.OrderBy(_ => rng.Next()).Take(rng.Next(1, 3)).ToList();
            var created = DateTime.UtcNow.AddDays(-(statuses.Length - i) * 8 - rng.Next(0, 5));
            var details = pick.Select(p =>
            {
                var qty = 1;
                var price = p.DisplayPrice;
                return new OrderDetail
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    ProductImage = p.PrimaryImage,
                    Price = price,
                    Quantity = qty,
                    SubTotal = price * qty
                };
            }).ToList();

            var sub = details.Sum(d => d.SubTotal);
            var ship = sub >= 20000000 ? 0 : 50000;
            var discount = i % 4 == 0 ? 500000 : 0;
            var status = statuses[i];
            var order = new Order
            {
                OrderCode = $"LZ{created:yyMMdd}{100 + i}",
                UserId = user.Id,
                ReceiverName = user.FullName,
                Phone = user.PhoneNumber ?? "0908000000",
                Email = user.Email ?? "",
                Address = "12 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                SubTotal = sub,
                DiscountAmount = discount,
                ShippingFee = ship,
                TotalAmount = Math.Max(0, sub - discount + ship),
                PaymentMethod = i % 3 == 0 ? PaymentMethod.BankTransfer : PaymentMethod.COD,
                PaymentStatus = status == OrderStatus.Completed ? PaymentStatus.Paid : (status == OrderStatus.Cancelled ? PaymentStatus.Unpaid : PaymentStatus.Unpaid),
                OrderStatus = status,
                VoucherCode = discount > 0 ? "WELCOME500" : null,
                StockDeducted = status is OrderStatus.Confirmed or OrderStatus.Packing or OrderStatus.Shipping or OrderStatus.Completed,
                CreatedAt = created,
                UpdatedAt = created.AddDays(1),
                Details = details
            };
            order.StatusHistories.Add(new OrderStatusHistory { Status = OrderStatus.Pending, Note = "Đặt hàng", CreatedAt = created });
            if (status != OrderStatus.Pending)
                order.StatusHistories.Add(new OrderStatusHistory { Status = status, Note = "Cập nhật hệ thống", CreatedAt = created.AddDays(1) });
            db.Orders.Add(order);
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedNotificationsAsync(ApplicationDbContext db, Dictionary<string, ApplicationUser> accounts)
    {
        if (await db.Notifications.AnyAsync()) return;
        if (!accounts.TryGetValue("Customer", out var customer)) return;
        db.Notifications.AddRange(
            new Notification { UserId = customer.Id, Title = "Chào mừng đến LaptopZone", Message = "Cảm ơn bạn đã tạo tài khoản. Khám phá laptop phù hợp ngay hôm nay.", Link = "/products", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new Notification { UserId = customer.Id, Title = "Mã WELCOME500", Message = "Bạn có mã giảm 500.000 ₫ cho đơn từ 15 triệu.", Link = "/cart", IsRead = false, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        );
        await db.SaveChangesAsync();
    }
}
