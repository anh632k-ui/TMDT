using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanPhongPham.Data;
using VanPhongPham.Models;
using VanPhongPham.Services;

namespace VanPhongPham.Areas.Admin.Controllers;

[Area("Admin"), Authorize(Roles = "Admin")]
public sealed class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IOrderService _orders;
    public OrdersController(ApplicationDbContext db, IOrderService orders)
        => (_db, _orders) = (db, orders);

    [HttpGet]
    public async Task<IActionResult> Index(OrderStatus? status, DateTime? from, DateTime? to, int page = 1)
    {
        var q = _db.Orders.AsNoTracking().AsQueryable();
        if (status.HasValue) q = q.Where(x => x.Status == status.Value);
        if (from.HasValue) q = q.Where(x => x.CreatedAt >= from.Value.Date);
        if (to.HasValue) q = q.Where(x => x.CreatedAt < to.Value.Date.AddDays(1));
        var pages = Math.Max(1, (int)Math.Ceiling(await q.CountAsync() / 10.0));
        page = Math.Clamp(page, 1, pages);
        var orders = await q.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * 10)
            .Take(10).ToListAsync();
        var ids = orders.Select(x => x.UserId).Distinct().ToArray();
        ViewBag.Emails = await _db.Users.AsNoTracking().Where(u => ids.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email ?? u.UserName ?? u.Id);
        ViewBag.Pages = pages; ViewBag.Page = page;
        ViewBag.Status = status; ViewBag.From = from; ViewBag.To = to;
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (order == null) return NotFound();
        ViewBag.Items = await _db.OrderItems.AsNoTracking().Where(x => x.OrderId == id).ToListAsync();
        ViewBag.Email = await _db.Users.AsNoTracking().Where(x => x.Id == order.UserId)
            .Select(x => x.Email).FirstOrDefaultAsync();
        return View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, OrderStatus status)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), status)) return BadRequest();
        // TV3 xử lý chuyển trạng thái và các biến động VoucherUsage trong service duy nhất.
        var (ok, message) = await _orders.ChangeStatusAsync(id, status);
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var (ok, message) = await _orders.CancelAsync(id, null, isAdmin: true);
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction(nameof(Details), new { id });
    }
}
