using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace VanPhongPham.Areas.Admin.ViewModels;

public sealed class ProductFormVm
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm"), StringLength(200)]
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    [Range(typeof(decimal), "0", "999999999999", ErrorMessage = "Giá không được âm")]
    public decimal Price { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "Tồn kho không được âm")]
    public int Stock { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục")]
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? CurrentImageUrl { get; set; }
    public IFormFile? Image { get; set; }
}
