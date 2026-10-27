using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanPhongPham.Areas.Admin.ViewModels;
using VanPhongPham.Data;
using VanPhongPham.Models;

namespace VanPhongPham.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public sealed class ProductsController : Controller
{
    private const int PageSize = 10;
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public ProductsController(ApplicationDbContext db, IWebHostEnvironment env) => (_db, _env) = (db, env);

    [HttpGet]
    public async Task<IActionResult> Index(string? q, int? categoryId, int page = 1)
    {
        var query = _db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Name.Contains(q.Trim()));
        if (categoryId > 0) query = query.Where(x => x.CategoryId == categoryId);
        var count = await query.CountAsync();
        ViewBag.Pages = Math.Max(1, (int)Math.Ceiling(count / (double)PageSize));
        page = Math.Clamp(page, 1, (int)ViewBag.Pages);
        ViewBag.Page = page; ViewBag.Q = q; ViewBag.CategoryId = categoryId;
        var cats = await _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        ViewBag.Categories = cats;
        ViewBag.CategoryNames = cats.ToDictionary(x => x.Id, x => x.Name);
        return View(await query.OrderByDescending(x => x.Id)
            .Skip((page - 1) * PageSize).Take(PageSize).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCategoriesAsync();
        return View("Form", new ProductFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormVm vm)
    {
        await ValidateFormAsync(vm);
        if (!ModelState.IsValid) { await LoadCategoriesAsync(); return View("Form", vm); }
        var product = new Product {
            Name = vm.Name.Trim(), Description = vm.Description?.Trim(), Price = vm.Price,
            Stock = vm.Stock, CategoryId = vm.CategoryId, IsActive = vm.IsActive,
            CreatedAt = DateTime.Now
        };
        if (vm.Image != null) product.ImageUrl = await SaveImageAsync(vm.Image);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm sản phẩm";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        await LoadCategoriesAsync();
        return View("Form", new ProductFormVm {
            Id = p.Id, Name = p.Name, Description = p.Description, Price = p.Price,
            Stock = p.Stock, OriginalStock = p.Stock, CategoryId = p.CategoryId, IsActive = p.IsActive,
            CurrentImageUrl = p.ImageUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormVm vm)
    {
        var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (p == null) return NotFound();
        if (vm.Id != id) return BadRequest();
        await ValidateFormAsync(vm);
        if (!vm.OriginalStock.HasValue || vm.OriginalStock.Value < 0)
            ModelState.AddModelError(nameof(vm.OriginalStock), "Thông tin tồn kho đã cũ. Vui lòng tải lại trang sửa sản phẩm.");
        if (!ModelState.IsValid) {
            vm.CurrentImageUrl = p.ImageUrl;
            await LoadCategoriesAsync();
            return View("Form", vm);
        }

        // Lệnh UPDATE kiểm tra tồn kho lúc mở form. Nếu Checkout đã trừ kho,
        // Admin không được lưu giá trị cũ đè lên tồn kho đang có.
        string? uploadedImage = null;
        var updateSucceeded = false;
        try {
            if (vm.Image != null) uploadedImage = await SaveImageAsync(vm.Image);
            var name = vm.Name.Trim();
            var description = vm.Description?.Trim();
            var selectedImageUrl = uploadedImage ?? p.ImageUrl;
            var rows = await _db.Products
                .Where(x => x.Id == id && x.Stock == vm.OriginalStock!.Value)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Name, name)
                    .SetProperty(x => x.Description, description)
                    .SetProperty(x => x.Price, vm.Price)
                    .SetProperty(x => x.Stock, vm.Stock)
                    .SetProperty(x => x.CategoryId, vm.CategoryId)
                    .SetProperty(x => x.IsActive, vm.IsActive)
                    .SetProperty(x => x.ImageUrl, selectedImageUrl));
            if (rows != 1) {
                ModelState.AddModelError("", "Tồn kho đã thay đổi bởi đơn hàng hoặc thao tác khác. Tải lại trang trước khi sửa.");
                vm.CurrentImageUrl = p.ImageUrl;
                await LoadCategoriesAsync();
                return View("Form", vm);
            }
            updateSucceeded = true;
        }
        finally {
            if (!updateSucceeded && uploadedImage != null) DeleteUploadedImage(uploadedImage);
        }
        TempData["Success"] = "Đã cập nhật sản phẩm";
        return RedirectToAction(nameof(Index));
    }

    // Ẩn/hiện thay vì xóa để bảo toàn OrderItem và lịch sử bán hàng.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();
        p.IsActive = !p.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = p.IsActive ? "Đã hiện sản phẩm" : "Đã ẩn sản phẩm";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategoriesAsync() =>
        ViewBag.Categories = await _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();

    private async Task ValidateFormAsync(ProductFormVm vm)
    {
        if (vm.Price != decimal.Truncate(vm.Price))
            ModelState.AddModelError(nameof(vm.Price), "Giá bán phải là số nguyên VNĐ");
        if (!await _db.Categories.AnyAsync(x => x.Id == vm.CategoryId))
            ModelState.AddModelError(nameof(vm.CategoryId), "Danh mục không tồn tại");
        if (vm.Image is not { } f) return;
        if (f.Length == 0 || f.Length > MaxImageBytes) {
            ModelState.AddModelError(nameof(vm.Image), "Ảnh phải có kích thước từ 1 byte đến 5 MB");
            return;
        }
        var ext = Path.GetExtension(f.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp")) {
            ModelState.AddModelError(nameof(vm.Image), "Chỉ nhận JPG, PNG hoặc WebP"); return;
        }
        await using var stream = f.OpenReadStream();
        var header = new byte[12];
        var n = await stream.ReadAsync(header.AsMemory());
        var jpeg = n >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255;
        var png = n >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[]{137,80,78,71,13,10,26,10});
        var webp = n >= 12 && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
            && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP";
        if (!(ext is ".jpg" or ".jpeg" ? jpeg : ext == ".png" ? png : webp))
            ModelState.AddModelError(nameof(vm.Image), "Nội dung tệp không khớp định dạng ảnh");
    }

    private void DeleteUploadedImage(string relativeUrl)
    {
        // Chỉ xóa ảnh có đường dẫn do chính controller tạo.
        var name = Path.GetFileName(relativeUrl);
        if (name.Length != 32 + Path.GetExtension(name).Length) return;
        if (!Guid.TryParseExact(Path.GetFileNameWithoutExtension(name), "N", out _)) return;
        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var path = Path.Combine(root, "images", "products", name);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private async Task<string> SaveImageAsync(IFormFile file)
    {
        var root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var directory = Path.Combine(root, "images", "products");
        Directory.CreateDirectory(directory);
        var name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        await using var output = new FileStream(Path.Combine(directory, name), FileMode.CreateNew);
        await file.CopyToAsync(output);
        return "/images/products/" + name;
    }
}
