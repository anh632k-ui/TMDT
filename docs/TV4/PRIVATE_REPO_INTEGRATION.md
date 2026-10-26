# Ghép TV4 vào repository PRIVATE của nhóm

## Nguồn đã đối chiếu

- Nhóm: `datvau0401-tech/VanPhongPham` (nhánh `upload`, bản ZIP người dùng cung cấp ngày 26/10/2026). Đây là **private repository**.
- TV4: `anh632k-ui/TMDT`, nhánh `tv4-quantri` (repo công khai chứa phần TV4, kiểm thử riêng).
- **Tuyệt đối không push mã TV3/TV2 từ repo private vào repo TMDT public.**

Bản ZIP đã có `VanPhongPham.csproj` .NET 8, Model, DbContext, migrations, `IOrderService`, `VoucherService`, `Program.cs`, Views khách và controller thanh toán mock **tạm thời**. Không sửa lại backend, migrations hay database của TV3 chỉ để ghép TV4.

## Đã kiểm tra hợp đồng backend

- Các trường `Category`, `Product`, `Order`, `OrderItem`, `Voucher`, `VoucherScope`, `VoucherUsage` tương thích với TV4.
- Các hàm `ChangeStatusAsync(int,OrderStatus)`, `CancelAsync(int,string?,bool)`, `MarkPaidAsync(int)` trùng chữ ký TV4.
- Luồng `CheckoutController.PlaceOrder` đã redirect `/Payment/Mock/{id}` khi Online.
- `OrderService`: COD `Pending→Shipping→Completed`; Online `Pending→Paid→Shipping→Completed`. Chỉ hủy khi `Pending/Paid`; `CancelAsync` hoàn voucher và tăng lại tồn kho khi hợp lệ.
- `Program.cs` đã có ASP.NET Identity + roles, Area Admin route, IOrderService và các middleware cần thiết. **Thiếu lời gọi DbSeeder.SeedAsync**, phải nhờ TV3 thêm.

## Cách đưa code TV4 vào repo nhóm mà không ảnh hưởng TV3

1. Clone repo nhóm bằng tài khoản có quyền. Tại **gốc repo nhóm** (chứa thư mục `VanPhongPham/`), giữ working tree sạch.
2. Trên repo nhóm, tạo nhánh `tv4-quantri` từ `upload` bằng `git switch -c tv4-quantri upload` (nếu đã có nhánh TV4 trong repo nhóm, chuyển sang nhánh ấy).
3. Tải file `scripts/merge-tv4-into-group.ps1` từ repo TMDT, chạy trên PowerShell:

```powershell
# Đang ở gốc repository nhóm
powershell -ExecutionPolicy Bypass -File .\merge-tv4-into-group.ps1 -RepoRoot .
```

4. Script chỉ sao chép: `Areas/Admin`, `Controllers/PaymentController.cs`, `Views/Payment/Mock.cshtml`, `Data/DbSeeder.cs`, `wwwroot/images/products/placeholder.svg`. Nó **không copy** `Models`, `Services`, `Migrations`, `Program.cs`, các View khách. Hai file Payment hiện có của TV3 là bản mock tạm và sẽ được thay thế; script lưu backup các file ghi đè ở thư mục tạm và không tự commit.
5. Kiểm tra `git diff --check`, `git diff --stat`, `git status` rồi mới commit trên nhánh TV4 **của repo nhóm**. Không đẩy trực tiếp main/upload nếu nhóm chưa thống nhất.

## Việc TV3 phải thêm trong Program.cs

Dự án hiện có `var app=builder.Build();`. Trong `Program.cs`, **sau** dòng này và trước `app.Run();`, chèn (TV3 tự quản lý file):

```csharp
using (var scope = app.Services.CreateScope())
{
    await VanPhongPham.Data.DbSeeder.SeedAsync(scope.ServiceProvider);
}
```

Lưu ý: `DbSeeder` gọi `MigrateAsync`; nên TV3 xác nhận database demo và migrations đã sẵn sàng. Bản seed không sửa lại số lượng tồn kho/mã voucher đã tồn tại nhằm bảo vệ dữ liệu. Cần tạo **database demo sạch** hoặc reset có kiểm soát giữa các lần diễn tập tranh hàng/voucher. Tài khoản mẫu `admin@vpp.local / Admin@123` **chỉ dành cho môi trường demo**, không dùng trên hệ thống thật.

## Kiểm tra thực tế trên Windows (máy nhóm)

Vào thư mục chứa `VanPhongPham.csproj`, sau khi TV3 đã cấu hình connection string đúng máy:

```powershell
dotnet restore
dotnet tool restore
dotnet build
dotnet ef database update
dotnet run --launch-profile http
```

- Mở `http://localhost:5062/Admin`; đăng nhập Admin, thử CRUD sản phẩm/voucher và màn hình đơn hàng.
- Lập 2 phiên đăng nhập Customer để thử TC15 tranh sản phẩm cuối (Stock=1), TC14 tranh lượt voucher cuối (LUOTCUOI=1).
- Thử TC16 giá từ trình duyệt và TC17 hủy đơn hoàn Stock.
- Ghi kết quả thật vào `docs/TV4/TEST_CASES.csv`. Bảng kiểm thử hiện còn trống kết quả, **không được đánh dấu PASS nếu chưa chạy**.
- Xem `docs/TV4/DEMO.md` để chuẩn bị demo 2 phút, thuyết trình tối đa 5 phút.

## Các lưu ý so với source ZIP hiện tại

- Có `PaymentController.cs` và `Views/Payment/Mock.cshtml` tạm: thay bằng bản TV4 (không tạo thêm tên trùng).
- `OrderService.MarkPaidAsync` có thể ném `InvalidOperationException`: TV4 đã bắt và hiển thị lý do.
- COD ở trạng thái Pending không được yêu cầu chuyển sang Paid qua Admin; TV4 dùng Shipping.
- `CustomerUserManager` tự gán role Customer cho người mới: seed TV4 sau khi gán Admin sẽ loại role Customer trên tài khoản quản trị mẫu.
- ZIP có SQL thêm dữ liệu thử riêng; `DbSeeder` tự kiểm tra tên/mã tránh thêm trùng. Không chạy script DELETE/reset với dữ liệu cần giữ.
- **Chưa chạy build tích hợp hoặc test SQL Server trong phiên này** vì máy chạy công cụ không có .NET SDK/SQL Server; chỉ CI của nhánh TV4 compile độc lập bằng contract stubs. Điều này không chứng minh backend thật tương thích hoàn toàn.
