// CHỈ DÀNH CHO KIỂM TRA TV4 ĐỘC LẬP. KHÔNG THÊM VÀO PROJECT CHÍNH.
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace VanPhongPham.Models
{
    public enum VoucherType { Percent, Fixed, Shipping }
    public enum OrderStatus { Pending, Paid, Shipping, Completed, Cancelled }
    public enum UsageStatus { Reserved, Used, Released }
    public class Category {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Slug { get; set; }
    }
    public class Product {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class Order {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public string ShippingName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string PaymentMethod { get; set; } = "COD";
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal ShippingDiscount { get; set; }
        public decimal Total { get; set; }
        public string? VoucherCodes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class OrderItem {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
    public class Voucher {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public VoucherType Type { get; set; }
        public decimal Value { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal MinOrder { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public int? TotalLimit { get; set; }
        public int UsedCount { get; set; }
        public int? PerUserLimit { get; set; }
        public bool NewUserOnly { get; set; }
        public bool IsActive { get; set; }
        public List<VoucherScope> Scopes { get; set; } = new();
    }
    public class VoucherScope {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public int? CategoryId { get; set; }
        public int? ProductId { get; set; }
        public bool IsExclude { get; set; }
    }
    public class VoucherUsage {
        public int Id { get; set; }
        public int VoucherId { get; set; }
        public string UserId { get; set; } = "";
        public int OrderId { get; set; }
        public UsageStatus Status { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
namespace VanPhongPham.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}
        public DbSet<VanPhongPham.Models.Category> Categories => Set<VanPhongPham.Models.Category>();
        public DbSet<VanPhongPham.Models.Product> Products => Set<VanPhongPham.Models.Product>();
        public DbSet<VanPhongPham.Models.Order> Orders => Set<VanPhongPham.Models.Order>();
        public DbSet<VanPhongPham.Models.OrderItem> OrderItems => Set<VanPhongPham.Models.OrderItem>();
        public DbSet<VanPhongPham.Models.Voucher> Vouchers => Set<VanPhongPham.Models.Voucher>();
        public DbSet<VanPhongPham.Models.VoucherScope> VoucherScopes => Set<VanPhongPham.Models.VoucherScope>();
        public DbSet<VanPhongPham.Models.VoucherUsage> VoucherUsages => Set<VanPhongPham.Models.VoucherUsage>();
    }
}
namespace VanPhongPham.Services
{
    public interface IOrderService {
        Task<(bool ok, string message)> ChangeStatusAsync(int orderId, VanPhongPham.Models.OrderStatus status);
        Task<(bool ok, string message)> CancelAsync(int orderId, string? userId, bool isAdmin);
        Task MarkPaidAsync(int orderId);
    }
}
