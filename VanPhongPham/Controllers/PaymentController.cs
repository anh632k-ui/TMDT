using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VanPhongPham.Data;
using VanPhongPham.Models;
using VanPhongPham.Services;

namespace VanPhongPham.Controllers;

// Chỉ mô phỏng trạng thái giao dịch: không thu tiền thật và không kết nối cổng thanh toán.
[Authorize]
public sealed class PaymentController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IOrderService _orders;
    private readonly UserManager<IdentityUser> _users;
    public PaymentController(ApplicationDbContext db, IOrderService orders, UserManager<IdentityUser> users)
        => (_db, _orders, _users) = (db, orders, users);

    [HttpGet]
    public async Task<IActionResult> Mock(int id)
    {
        var order = await OwnedOnlineOrderAsync(id);
        return order == null ? NotFound() : View(order);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Success(int id)
    {
        var order = await OwnedOnlineOrderAsync(id);
        if (order == null) return NotFound();
        if (order.Status != OrderStatus.Pending) {
            TempData["Error"] = "Đơn không ở trạng thái chờ thanh toán";
            return RedirectToAction("Details", "Orders", new { id });
        }
        // TV3 phải bảo đảm MarkPaidAsync chỉ xử lý một lần, cập nhật voucher trong transaction.
        await _orders.MarkPaidAsync(id);
        TempData["Success"] = "Thanh toán mô phỏng thành công";
        return RedirectToAction("Success", "Orders", new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var order = await OwnedOnlineOrderAsync(id);
        if (order == null) return NotFound();
        if (order.Status != OrderStatus.Pending) {
            TempData["Error"] = "Không thể hủy giao dịch khi đơn đã xử lý";
            return RedirectToAction("Details", "Orders", new { id });
        }
        var (ok, message) = await _orders.CancelAsync(id, _users.GetUserId(User), isAdmin: false);
        TempData[ok ? "Success" : "Error"] = message;
        return RedirectToAction("Index", "Orders");
    }

    private async Task<Order?> OwnedOnlineOrderAsync(int id)
    {
        var userId = _users.GetUserId(User);
        if (userId == null) return null;
        return await _db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId && x.PaymentMethod == "Online");
    }
}
