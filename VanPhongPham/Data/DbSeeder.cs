using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VanPhongPham.Models;

namespace VanPhongPham.Data;

// Gọi SeedAsync lúc khởi động ứng dụng SAU KHI TV3 thiết lập Identity, DbContext và migrations.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var roles = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var users = sp.GetRequiredService<UserManager<IdentityUser>>();
        await db.Database.MigrateAsync();

        foreach (var role in new[] { "Admin", "Customer" }) {
            if (!await roles.RoleExistsAsync(role)) {
                var created = await roles.CreateAsync(new IdentityRole(role));
                if (!created.Succeeded) throw new InvalidOperationException(
                    "Không tạo được role " + role + ": " + string.Join("; ", created.Errors.Select(e => e.Description)));
            }
        }
        const string adminEmail = "admin@vpp.local";
        var admin = await users.FindByEmailAsync(adminEmail);
        if (admin == null) {
            // KHÔNG sử dụng mật khẩu demo trong môi trường thật.
            var password = Environment.GetEnvironmentVariable("TV4_DEMO_ADMIN_PASSWORD") ?? "Tv4DemoAdmin@2026!";
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var created = await users.CreateAsync(admin, password);
            if (!created.Succeeded) throw new InvalidOperationException(
                "Không tạo được Admin: " + string.Join("; ", created.Errors.Select(e => e.Description)));
        }
        if (!await users.IsInRoleAsync(admin, "Admin")) {
            var assigned = await users.AddToRoleAsync(admin, "Admin");
            if (!assigned.Succeeded) throw new InvalidOperationException("Không gán được role Admin");
        }

        // CustomerUserManager của TV3 tự gán Customer lúc tạo tài khoản.
        // Tài khoản demo dành riêng cho quản trị nên chỉ giữ vai trò Admin.
        if (await users.IsInRoleAsync(admin, "Customer")) {
            var removed = await users.RemoveFromRoleAsync(admin, "Customer");
            if (!removed.Succeeded) throw new InvalidOperationException("Không thể tách role Customer khỏi Admin");
        }

        var catNames = new[] { "Giấy", "Bút viết", "Sổ tay", "Dụng cụ văn phòng" };
        var slugs = new[] { "giay", "but-viet", "so-tay", "dung-cu-van-phong" };
        for (var i = 0; i < catNames.Length; i++)
            if (!await db.Categories.AnyAsync(c => c.Name == catNames[i]))
                db.Categories.Add(new Category { Name = catNames[i], Slug = slugs[i] });
        await db.SaveChangesAsync();
        var cats = await db.Categories.Where(c => catNames.Contains(c.Name))
            .ToDictionaryAsync(c => c.Name, c => c.Id);

        var products = new (string Name, string Category, decimal Price, int Stock)[] {
            ("Giấy A4 Double A 70gsm","Giấy",80000,200),
            ("Giấy A4 Paper One 80gsm","Giấy",96000,120),
            ("Giấy A5 70gsm (500 tờ)","Giấy",48000,100),
            ("Giấy ghi chú nhiều màu","Giấy",25000,110),
            ("Giấy note vàng 76x76","Giấy",18000,250),
            ("Giấy in ảnh A4 (20 tờ)","Giấy",65000,80),
            ("Bút bi Thiên Long TL-027 (hộp 20)","Bút viết",45000,150),
            ("Bút bi Thiên Long TL-08","Bút viết",5000,400),
            ("Bút gel xanh 0.5mm","Bút viết",12000,300),
            ("Bút chì 2B (hộp 12)","Bút viết",32000,160),
            ("Bút dạ quang 5 màu","Bút viết",39000,130),
            ("Bút xóa nước","Bút viết",17000,90),
            ("Sổ tay A5 bìa cứng","Sổ tay",45000,100),
            ("Sổ tay bìa cứng A5 (bản giới hạn)","Sổ tay",65000,1),
            ("Sổ lò xo A4 200 trang","Sổ tay",58000,110),
            ("Sổ ghi chép B5 120 trang","Sổ tay",32000,170),
            ("Sổ kế hoạch tuần","Sổ tay",49000,85),
            ("Sổ mini bỏ túi","Sổ tay",15000,200),
            ("Sổ vẽ phác thảo A4","Sổ tay",75000,75),
            ("Bấm kim số 10","Dụng cụ văn phòng",35000,110),
            ("Hộp kim bấm số 10","Dụng cụ văn phòng",9000,260),
            ("Kéo văn phòng 17cm","Dụng cụ văn phòng",26000,95),
            ("Thước kẻ nhựa 30cm","Dụng cụ văn phòng",7000,200),
            ("Băng keo trong 5cm","Dụng cụ văn phòng",19000,180),
            ("Kẹp giấy 33mm (hộp 12)","Dụng cụ văn phòng",15000,180)
        };
        var names = (await db.Products.Select(x => x.Name).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var p in products)
            if (!names.Contains(p.Name))
                db.Products.Add(new Product {
                    Name = p.Name, Description = "Sản phẩm mẫu phục vụ đồ án thương mại điện tử.",
                    CategoryId = cats[p.Category], Price = p.Price, Stock = p.Stock,
                    ImageUrl = "/images/products/placeholder.svg",
                    IsActive = true, CreatedAt = DateTime.Now
                });
        await db.SaveChangesAsync();

        var now = DateTime.Now;
        var start = now.AddDays(-7);
        var end = now.AddMonths(6);
        var vouchers = new[] {
            new Voucher { Code="VPP50", Type=VoucherType.Percent, Value=10, MaxDiscount=50000,
                MinOrder=150000, StartAt=start, EndAt=end, TotalLimit=100, IsActive=true },
            new Voucher { Code="FREESHIP", Type=VoucherType.Shipping, Value=0,
                MinOrder=300000, StartAt=start, EndAt=end, IsActive=true },
            new Voucher { Code="GIAM20K", Type=VoucherType.Fixed, Value=20000,
                MinOrder=100000, StartAt=start, EndAt=end, IsActive=true },
            new Voucher { Code="HETHAN", Type=VoucherType.Fixed, Value=10000,
                StartAt=now.AddDays(-30), EndAt=now.AddDays(-1), IsActive=true },
            new Voucher { Code="SAPTOI", Type=VoucherType.Fixed, Value=10000,
                StartAt=now.AddDays(7), EndAt=now.AddDays(30), IsActive=true },
            new Voucher { Code="HETLUOT", Type=VoucherType.Fixed, Value=10000,
                StartAt=start, EndAt=end, TotalLimit=1, UsedCount=1, IsActive=true },
            new Voucher { Code="LUOTCUOI", Type=VoucherType.Fixed, Value=10000,
                StartAt=start, EndAt=end, TotalLimit=1, UsedCount=0, IsActive=true },
            new Voucher { Code="CHAOMUNG", Type=VoucherType.Fixed, Value=20000,
                MinOrder=100000, StartAt=start, EndAt=end, PerUserLimit=1, NewUserOnly=true, IsActive=true },
            new Voucher { Code="GIAY10", Type=VoucherType.Percent, Value=10,
                StartAt=start, EndAt=end, IsActive=true }
        };
        foreach (var v in vouchers)
            if (!await db.Vouchers.AnyAsync(x => x.Code == v.Code))
                db.Vouchers.Add(v);
        await db.SaveChangesAsync();
        var giay = await db.Vouchers.SingleAsync(v => v.Code == "GIAY10");
        if (!await db.VoucherScopes.AnyAsync(s => s.VoucherId == giay.Id)) {
            db.VoucherScopes.Add(new VoucherScope { VoucherId = giay.Id, CategoryId = cats["Giấy"] });
            await db.SaveChangesAsync();
        }
    }
}
