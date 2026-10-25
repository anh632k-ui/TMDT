using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanPhongPham.Areas.Admin.ViewModels;
using VanPhongPham.Data;
using VanPhongPham.Models;

namespace VanPhongPham.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public sealed class VouchersController : Controller
{
    private readonly ApplicationDbContext _db;
    public VouchersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var query = _db.Vouchers.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Code.Contains(q.Trim()));
        ViewBag.Q = q;
        return View(await query.OrderByDescending(x => x.Id).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var v = await _db.Vouchers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound();
        ViewBag.Scopes = await _db.VoucherScopes.AsNoTracking().Where(x => x.VoucherId == id).ToListAsync();
        ViewBag.Usages = await _db.VoucherUsages.AsNoTracking().Where(x => x.VoucherId == id)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
        return View(v);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadOptionsAsync();
        return View("Form", new VoucherFormVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VoucherFormVm vm)
    {
        vm.Code = Normalize(vm.Code);
        await ValidateAsync(vm);
        if (!ModelState.IsValid) { await LoadOptionsAsync(); return View("Form", vm); }
        var voucher = new Voucher();
        Apply(vm, voucher);
        await using var tx = await _db.Database.BeginTransactionAsync();
        _db.Vouchers.Add(voucher);
        await _db.SaveChangesAsync();
        _db.VoucherScopes.AddRange(Scopes(vm, voucher.Id));
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["Success"] = "Đã tạo voucher";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var v = await _db.Vouchers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound();
        var scopes = await _db.VoucherScopes.AsNoTracking().Where(x => x.VoucherId == id).ToListAsync();
        await LoadOptionsAsync();
        return View("Form", new VoucherFormVm {
            Id=v.Id, Code=v.Code, Type=v.Type, Value=v.Value, MaxDiscount=v.MaxDiscount,
            MinOrder=v.MinOrder, StartAt=v.StartAt, EndAt=v.EndAt,
            TotalLimit=v.TotalLimit, PerUserLimit=v.PerUserLimit,
            NewUserOnly=v.NewUserOnly, IsActive=v.IsActive,
            IncludeCategoryIds=scopes.Where(s => !s.IsExclude && s.CategoryId.HasValue).Select(s => s.CategoryId!.Value).ToArray(),
            ExcludeCategoryIds=scopes.Where(s => s.IsExclude && s.CategoryId.HasValue).Select(s => s.CategoryId!.Value).ToArray(),
            IncludeProductIds=scopes.Where(s => !s.IsExclude && s.ProductId.HasValue).Select(s => s.ProductId!.Value).ToArray(),
            ExcludeProductIds=scopes.Where(s => s.IsExclude && s.ProductId.HasValue).Select(s => s.ProductId!.Value).ToArray()
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VoucherFormVm vm)
    {
        var v = await _db.Vouchers.FirstOrDefaultAsync(x => x.Id == id);
        if (v == null) return NotFound();
        if (vm.Id != id) return BadRequest();
        vm.Code = Normalize(vm.Code);
        await ValidateAsync(vm, id);
        if (await _db.VoucherUsages.AnyAsync(x => x.VoucherId == id) &&
            !string.Equals(v.Code, vm.Code, StringComparison.OrdinalIgnoreCase))
            ModelState.AddModelError(nameof(vm.Code), "Voucher có lịch sử sử dụng: không được đổi mã");
        if (!ModelState.IsValid) { await LoadOptionsAsync(); return View("Form", vm); }
        Apply(vm, v); // Không gán UsedCount từ form.
        var old = await _db.VoucherScopes.Where(x => x.VoucherId == id).ToListAsync();
        _db.VoucherScopes.RemoveRange(old);
        _db.VoucherScopes.AddRange(Scopes(vm, id));
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật voucher";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var v = await _db.Vouchers.FindAsync(id);
        if (v == null) return NotFound();
        v.IsActive = !v.IsActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = v.IsActive ? "Đã bật voucher" : "Đã tắt voucher";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var v = await _db.Vouchers.FindAsync(id);
        if (v == null) return NotFound();
        if (await _db.VoucherUsages.AnyAsync(x => x.VoucherId == id)) {
            TempData["Error"] = "Voucher đã có lịch sử: chỉ được tắt, không được xóa";
            return RedirectToAction(nameof(Index));
        }
        _db.VoucherScopes.RemoveRange(await _db.VoucherScopes.Where(x => x.VoucherId == id).ToListAsync());
        _db.Vouchers.Remove(v);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa voucher chưa sử dụng";
        return RedirectToAction(nameof(Index));
    }

    private static string Normalize(string? code) => (code ?? "").Trim().ToUpperInvariant();
    private async Task LoadOptionsAsync()
    {
        ViewBag.Categories = await _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        ViewBag.Products = await _db.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
    }
    private async Task ValidateAsync(VoucherFormVm vm, int? ignoredId = null)
    {
        if (!Enum.IsDefined(typeof(VoucherType), vm.Type))
            ModelState.AddModelError(nameof(vm.Type), "Loại voucher không hợp lệ");
        if (vm.EndAt <= vm.StartAt)
            ModelState.AddModelError(nameof(vm.EndAt), "Ngày kết thúc phải sau ngày bắt đầu");
        if (vm.Type == VoucherType.Percent && (vm.Value < 1 || vm.Value > 100))
            ModelState.AddModelError(nameof(vm.Value), "Phần trăm phải từ 1 đến 100");
        if (vm.Type != VoucherType.Shipping && vm.Value <= 0)
            ModelState.AddModelError(nameof(vm.Value), "Giá trị giảm phải lớn hơn 0");
        if (vm.MaxDiscount is < 0)
            ModelState.AddModelError(nameof(vm.MaxDiscount), "Mức giảm tối đa không được âm");
        if (await _db.Vouchers.AnyAsync(x => x.Code == vm.Code && (!ignoredId.HasValue || x.Id != ignoredId.Value)))
            ModelState.AddModelError(nameof(vm.Code), "Mã voucher đã tồn tại");
        var ci=vm.IncludeCategoryIds ?? Array.Empty<int>(); var ce=vm.ExcludeCategoryIds ?? Array.Empty<int>();
        var pi=vm.IncludeProductIds ?? Array.Empty<int>(); var pe=vm.ExcludeProductIds ?? Array.Empty<int>();
        if (ci.Intersect(ce).Any() || pi.Intersect(pe).Any())
            ModelState.AddModelError("", "Không thể vừa bao gồm và loại trừ cùng một mục");
        var categoryIds=ci.Concat(ce).Distinct().ToArray(); var productIds=pi.Concat(pe).Distinct().ToArray();
        if (categoryIds.Any(x=>x<=0) || await _db.Categories.CountAsync(x=>categoryIds.Contains(x.Id)) != categoryIds.Length)
            ModelState.AddModelError("", "Danh mục trong phạm vi không hợp lệ");
        if (productIds.Any(x=>x<=0) || await _db.Products.CountAsync(x=>productIds.Contains(x.Id)) != productIds.Length)
            ModelState.AddModelError("", "Sản phẩm trong phạm vi không hợp lệ");
    }
    private static void Apply(VoucherFormVm vm, Voucher v)
    {
        v.Code=vm.Code; v.Type=vm.Type; v.Value=vm.Value;
        v.MaxDiscount=vm.Type == VoucherType.Percent ? vm.MaxDiscount : null;
        v.MinOrder=vm.MinOrder; v.StartAt=vm.StartAt; v.EndAt=vm.EndAt;
        v.TotalLimit=vm.TotalLimit; v.PerUserLimit=vm.PerUserLimit;
        v.NewUserOnly=vm.NewUserOnly; v.IsActive=vm.IsActive;
    }
    private static List<VoucherScope> Scopes(VoucherFormVm vm, int id)
    {
        var list=new List<VoucherScope>();
        list.AddRange((vm.IncludeCategoryIds ?? Array.Empty<int>()).Distinct().Select(x=>new VoucherScope {VoucherId=id, CategoryId=x}));
        list.AddRange((vm.ExcludeCategoryIds ?? Array.Empty<int>()).Distinct().Select(x=>new VoucherScope {VoucherId=id, CategoryId=x, IsExclude=true}));
        list.AddRange((vm.IncludeProductIds ?? Array.Empty<int>()).Distinct().Select(x=>new VoucherScope {VoucherId=id, ProductId=x}));
        list.AddRange((vm.ExcludeProductIds ?? Array.Empty<int>()).Distinct().Select(x=>new VoucherScope {VoucherId=id, ProductId=x, IsExclude=true}));
        return list;
    }
}
