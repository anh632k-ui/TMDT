using System.ComponentModel.DataAnnotations;
using VanPhongPham.Models;

namespace VanPhongPham.Areas.Admin.ViewModels;

public sealed class VoucherFormVm
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Nhập mã voucher"), StringLength(30, MinimumLength = 2)]
    [RegularExpression("^[a-zA-Z0-9_-]+$", ErrorMessage = "Mã chỉ gồm chữ, số, _ và -")]
    public string Code { get; set; } = "";
    public VoucherType Type { get; set; }
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal Value { get; set; }
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal? MaxDiscount { get; set; }
    [Range(typeof(decimal), "0", "999999999999")]
    public decimal MinOrder { get; set; }
    public DateTime StartAt { get; set; } = DateTime.Today;
    public DateTime EndAt { get; set; } = DateTime.Today.AddMonths(1);
    [Range(1, int.MaxValue)] public int? TotalLimit { get; set; }
    [Range(1, int.MaxValue)] public int? PerUserLimit { get; set; }
    public bool NewUserOnly { get; set; }
    public bool IsActive { get; set; } = true;
    // Không cho phép sửa UsedCount: chỉ dịch vụ của TV3 cập nhật lượt sử dụng.
    public int[] IncludeCategoryIds { get; set; } = Array.Empty<int>();
    public int[] ExcludeCategoryIds { get; set; } = Array.Empty<int>();
    public int[] IncludeProductIds { get; set; } = Array.Empty<int>();
    public int[] ExcludeProductIds { get; set; } = Array.Empty<int>();
}
