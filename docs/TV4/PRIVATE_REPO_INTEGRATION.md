# Ghép TV4 vào repository PRIVATE của nhóm

## Nguồn đã đối chiếu

- Nhóm: `datvau0401-tech/VanPhongPham` (**private repository**). ZIP đã đối chiếu là snapshot nhánh `upload` ngày 26/10/2026. Nhóm trưởng sau đó thông báo đã đưa khung dự án vào **`main`**. Nội dung `main` mới **chưa kiểm chứng trực tiếp** vì tài khoản GitHub kết nối chưa có quyền đọc repository private.
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
2. Trên repo nhóm, cập nhật `main` sạch rồi tạo nhánh `tv4-quantri` **từ `main`** (không dùng nhánh `upload` cũ). Với repo vừa clone và chưa có nhánh TV4:

```powershell
git switch main
git pull --ff-only origin main
git switch -c tv4-quantri
```

Nếu nhánh TV4 của repo nhóm đã tồn tại, không tạo đè. Hãy kiểm tra `git branch -a`, chuyển đến nhánh hiện hữu và cập nhật `main` qua merge/PR theo thống nhất nhóm trước khi tiếp tục.
3. Tải script vào **thư mục tạm ngoài repository** (script kiểm tra `git status` sạch) rồi chạy PowerShell tại gốc repo nhóm:

```powershell
# Đang ở gốc repository nhóm (có thư mục VanPhongPham/)
$script = Join-Path $env:TEMP "merge-tv4-into-group.ps1"
Invoke-WebRequest "https://raw.githubusercontent.com/anh632k-ui/TMDT/tv4-quantri/scripts/merge-tv4-into-group.ps1" -OutFile $script
powershell -ExecutionPolicy Bypass -File $script -RepoRoot .
```

4. Script chỉ sao chép: `Areas/Admin`, `Controllers/PaymentController.cs`, `Views/Payment/Mock.cshtml`, `Data/DbSeeder.cs`, `wwwroot/images/products/placeholder.svg`. Nó **không copy** `Models`, `Services`, `Migrations`, `Program.cs`, các View khách. Hai file Payment hiện có của TV3 là bản mock tạm và sẽ được thay thế; script lưu backup các file ghi đè ở thư mục tạm và không tự commit.
5. Kiểm tra `git diff --check`, `git diff --stat`, `git status` rồi mới commit trên nhánh TV4 **của repo nhóm**. Không đẩy trực tiếp `main` hoặc nhánh `upload` cũ nếu nhóm chưa thống nhất.

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

## Cập nhật theo tin nhắn Zalo nhóm trưởng (26/10/2026)

TV3 thông báo đã gộp khung dự án vào nhánh `main` của repository nhóm và yêu cầu TV4 clone/pull main, chạy README và tạo nhánh `tv4-quantri`. Trước khi ghép mã nguồn, cần **kiểm chứng nội dung main mới** bằng `git log -1 --oneline`, `git ls-files VanPhongPham` và `dotnet build VanPhongPham/VanPhongPham.csproj` trên máy có quyền truy cập. Những bước và kết quả này **chưa thể xác nhận qua kết nối GitHub hiện tại**; không được đánh dấu DONE tích hợp từ riêng ảnh chụp Zalo.

## Kiểm chứng thêm từ ZIP main (26/10/2026)

Bản `VanPhongPham-main.zip` đã nhận có project `.NET 8`, 7 DbSet, migrations, `AddRoles<IdentityRole>()`, route Area và `IOrderService` phù hợp, nên có thể dùng làm nền ghép TV4. So với ZIP `upload` trước đó, **danh sách 113 file giống nhau**, có thay đổi nội dung 6 file, nổi bật là `OrderService.cs` và `DuLieuThuVoucher.sql` để hỗ trợ tình huống tranh sản phẩm cuối.

**Hai việc nhờ TV3 rà lại:**
1. `Program.cs` trong ZIP main vẫn chưa gọi `DbSeeder.SeedAsync` — khi ghép TV4, tài khoản Admin và dữ liệu demo không thể tự xuất hiện nếu không thêm lời gọi.
2. README trong ZIP main còn hướng dẫn tải nhánh `upload` và chạy `dotnet tool restore`, nhưng ZIP không có `.config/dotnet-tools.json` (local tool manifest). Có thể cài `dotnet-ef` toàn cục khi cần, hoặc TV3 thêm manifest và cập nhật README cho đúng nhánh `main`.

**Chưa chạy được `dotnet build` với source chính / SQL Server trong phiên kiểm tra ZIP:** môi trường đọc ZIP không có .NET SDK. Kết quả CI PASS của repo TMDT vẫn chỉ là *biên dịch TV4 độc lập với contract stubs*; lần chạy tích hợp phải thực hiện trên máy thành viên có SQL Server và ứng dụng hoàn chỉnh.
