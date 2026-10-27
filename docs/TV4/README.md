# TV4 – Quản trị, thanh toán, dữ liệu mẫu và kiểm thử

## Phạm vi

Thực hiện đúng phần việc TV4 của đồ án **Website bán văn phòng phẩm trực tuyến** (.NET 8 ASP.NET Core MVC, EF Core, SQL Server, Identity).

- `VanPhongPham/Areas/Admin/Controllers`: Home, Products, Orders, Vouchers.
- `VanPhongPham/Areas/Admin/ViewModels`: form nhập sản phẩm, voucher.
- `VanPhongPham/Areas/Admin/Views`: layout, dashboard, CRUD sản phẩm, đơn hàng, voucher và thống kê lượt dùng.
- `VanPhongPham/Controllers/PaymentController.cs` và `VanPhongPham/Views/Payment/Mock.cshtml`: thanh toán trực tuyến **GIẢ LẬP**.
- `VanPhongPham/Data/DbSeeder.cs`: Admin demo, 4 danh mục, 25 sản phẩm (gồm sổ A5 giới hạn Stock = 1), 9 mã voucher.
- `docs/TV4/TEST_CASES.csv`: 20 tình huống chính thức theo hướng dẫn mới ngày 27/10/2026 (kết quả thực tế để trống đến khi chạy).
- `docs/TV4/DEMO.md`: demo 2 phút trong phần thuyết trình tối đa 5 phút, kiểm tra tích hợp.

## Trạng thái hiện tại

**Hoàn tất mã nguồn phần TV4 theo hợp đồng tài liệu. Chưa tích hợp/chưa chạy thực tế.**
Repo TMDT của cá nhân chỉ chứa phần TV4. Đã nhận bản ZIP source TV3/TV2 ở repo private `datvau0401-tech/VanPhongPham` (nhánh `upload`, 26/10/2026): có .csproj, Models, DbContext, migrations, Services và giao diện khách cơ bản. Đã ghép vào bản dự án nhóm trên máy và người dùng đã xác nhận build, truy cập Admin thành công; kết quả 20 ca kiểm thử, bản sao lưu và video vẫn chưa được xác nhận.
Không được ghi ĐẠT cho bất kỳ ca kiểm thử nào khi chưa chạy.

## Những gì cần TV3 cung cấp

1. Dự án `VanPhongPham/VanPhongPham.csproj` dạng ASP.NET Core MVC (.NET 8), Identity + EF Core SQL Server.
2. `VanPhongPham.Data.ApplicationDbContext`: `Categories`, `Products`, `Orders`, `OrderItems`, `Vouchers`, `VoucherScopes`, `VoucherUsages` và `Users`.
3. `VanPhongPham.Models`: `Category`, `Product`, `Order`, `OrderItem`, `Voucher`, `VoucherScope`, `VoucherUsage`. Các trường, enums `VoucherType`, `OrderStatus` và `UsageStatus` đúng theo tài liệu TV3.
4. `VanPhongPham.Services.IOrderService` với **chữ ký thống nhất**:
   ```csharp
   Task<(bool ok, string message)> ChangeStatusAsync(int orderId, OrderStatus status);
   Task<(bool ok, string message)> CancelAsync(int orderId, string? userId, bool isAdmin);
   Task MarkPaidAsync(int orderId);
   ```
   *Nếu dịch vụ TV3 dùng tên/chữ ký khác, hai thành viên phải thống nhất và điều chỉnh controller TV4 trước khi tích hợp.*
5. `ChangeStatusAsync` phải từ chối chuyển trạng thái sai thứ tự; `MarkPaidAsync` phải idempotent (không ghi nhận hai lần); `CancelAsync` phải hoàn lượt voucher một lần duy nhất. Mọi cập nhật voucher do TV3 thực hiện trong transaction.
6. TV3 cấu hình **role-based Identity** (`.AddRoles<IdentityRole>()`), đăng nhập; riêng Admin phải có role `Admin`.

## Hai thay đổi Program.cs nhờ TV3 thực hiện

Trong `Program.cs`, khi đã đăng ký `ApplicationDbContext`, Identity, IOrderService, route và middleware, thêm:

```csharp
// Route Area Admin phải khai báo trước route mặc định:
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Sau khi var app = builder.Build() và trước app.Run():
using (var scope = app.Services.CreateScope())
{
    await VanPhongPham.Data.DbSeeder.SeedAsync(scope.ServiceProvider);
}
```

Với `DbSeeder`, TV3 cần bảo đảm EF Migrations đã được tạo. Môi trường demo **khởi tạo mới** tài khoản `admin@vpp.local` với mật khẩu mặc định `Tv4DemoAdmin@2026!` (chỉ dùng ở môi trường học tập); nên đặt biến `TV4_DEMO_ADMIN_PASSWORD` riêng cho từng máy. **Nếu tài khoản đã tồn tại, DbSeeder không thay mật khẩu cũ**: tài khoản từ lần seed trước có thể vẫn dùng `Admin@123` cho đến khi đổi qua Identity. Không dùng mật khẩu demo trên hệ thống thật.

TV3: Sau `Checkout/PlaceOrder`, nếu chọn `Online`, điều hướng `/Payment/Mock/{orderId}`. Nếu `COD`, đơn ở `Pending` và chuyển về `Orders/Success`. URL Mock không thanh toán thật.

TV2: Giữ `Views/Checkout` và JS voucher thuộc TV2, không di chuyển vào `Views/Payment`. TV4 chỉ phụ trách mock sau đặt hàng.

## Chạy sau khi tích hợp

```powershell
git clone https://github.com/anh632k-ui/TMDT.git
cd TMDT
git switch tv4-quantri
# Khi TV3 đã có .csproj + migration trong nhánh tích hợp:
cd VanPhongPham
dotnet restore
dotnet ef database update
dotnet run
```

Kiểm tra: `/Admin`, `/Admin/Products`, `/Admin/Orders`, `/Admin/Vouchers`, `/Payment/Mock/{orderId}`.

**Phân quyền:** khách thường bị chặn khỏi `/Admin`; trang mock chỉ hiển thị đơn `Online` của chính người đăng nhập. Mọi form thay đổi dữ liệu phải là POST và có anti-forgery token.

## Lưu ý

- Chỉ sửa phần TV4; không tự ý tạo/thay thế `Models/`, `Services/`, `Migrations/`, `Program.cs` của TV3.
- Sản phẩm đã bán chỉ ẩn bằng `IsActive`; không xóa để giữ lịch sử đơn hàng.
- Voucher có bản ghi `VoucherUsage` chỉ được tắt; không xóa.
- Bộ dữ liệu seed tự tránh thêm trùng dựa trên tên/mã, phục vụ demo, không thay thế hệ thống import dữ liệu sản phẩm.
- Các giá trị mã `HETHAN`, `SAPTOI`, `LUOTCUOI` phụ thuộc thời gian và trạng thái kiểm thử. Khi chạy lại demo, reset môi trường test có kiểm soát nếu các ca đã làm thay đổi số lượt.
- Sau khi TV3 làm xong, chạy `dotnet build`, kiểm tra toàn bộ use case, ghi kết quả thực tế trong bảng test, chụp bằng chứng, quay demo và backup database.

## Làm việc với repository private của nhóm (đã đối chiếu ZIP 26/10/2026)

- **Nguồn nhóm:** `datvau0401-tech/VanPhongPham` (private, nhánh `upload` theo ảnh). Nhóm giữ quyền sở hữu mọi Models/Services/migrations.
- **Nguồn TV4:** `anh632k-ui/TMDT`, nhánh `tv4-quantri` (public, chỉ chứa TV4).
- **Không đẩy bản ZIP riêng tư TV3 lên public TMDT**. Để đưa code TV4 vào repo nhóm, dùng `scripts/merge-tv4-into-group.ps1` theo hướng dẫn ở `docs/TV4/PRIVATE_REPO_INTEGRATION.md` trên bản sao sạch của repo nhóm.
- Bản zip TV3 có database và `IOrderService` phù hợp chữ ký trong tài liệu; luồng thanh toán Online chuyển đến Mock. Bản TV4 đã chỉnh COD/Online UI và xử lý lỗi `MarkPaidAsync` theo code thật.
- **Chưa thể gọi là DONE khi chưa chạy thử thực tế** 20 tình huống chính thức, phân quyền, seed và checkout trên SQL Server.

## Cập nhật công việc TV4 ngày 27/10/2026

Theo tài liệu mới `HuongDan_Anh_Admin_KiemThu_Demo`, phạm vi bàn giao gồm:

1. Xóa chú thích nội bộ khỏi chi tiết đơn Admin; kiểm tra các View khác không có TODO/TV2/TV3/TV4 hiển thị.
2. Nhận script reset an toàn từ Hậu, dọn đơn test trên database DEMO và khôi phục Stock/voucher đúng trạng thái. **Chưa thực hiện từ GitHub**.
3. `docs/TV4/TEST_CASES.csv` gồm **20 tình huống đúng nguyên bảng mới**, tám cột chuẩn. Kết quả thực tế chưa được giả lập.
4. `docs/TV4/DEMO.md`: A tranh sản phẩm cuối, B tranh LUOTCUOI, **C chống đặt đơn trùng hai tab**; mỗi kịch bản tập hai lần.
5. `docs/TV4/BACKUP_AND_VIDEO.md`: checklist sao lưu .bak, xác minh bản sao lưu, quay video .mp4 trên máy demo; **chưa có file .bak/.mp4 thực tế**.
6. `docs/TV4/EVIDENCE_INDEX.md`: danh mục ảnh để dán vào Word kiểm thử và gửi Đạt.

Hạn bảng kiểm thử: **28/10 15:00**; demo/PR: **28/10 18:00**; backup và video: tối 28/10. Phần Git nhóm do người dùng tự đảm nhiệm, không push hay merge hộ.
