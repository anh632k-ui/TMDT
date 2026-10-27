using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanPhongPham.Data;

namespace VanPhongPham.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public sealed class HomeController : Controller
{
    private readonly ApplicationDbContext _db;
    public HomeController(ApplicationDbContext db) => _db = db;
    public async Task<IActionResult> Index()
    {
        ViewBag.ProductCount = await _db.Products.CountAsync();
        ViewBag.OrderCount = await _db.Orders.CountAsync();
        ViewBag.VoucherCount = await _db.Vouchers.CountAsync();
        ViewBag.RecentOrders = await _db.Orders.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt).Take(8).ToListAsync();
        return View();
    }
}
