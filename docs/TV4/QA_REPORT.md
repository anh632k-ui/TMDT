# TV4 – Báo cáo rà soát mã nguồn và CI

**Ngày kiểm tra:** 26/10/2026  
**Repository:** `anh632k-ui/TMDT`  
**Nhánh:** `tv4-quantri`  
**PR:** [#1 (Draft)](https://github.com/anh632k-ui/TMDT/pull/1)

## Kết quả theo ba lượt kiểm tra

| Lượt | Hình thức | Kết quả |
| --- | --- | --- |
| 1 | Đối chiếu Controller, ViewModel, DbSeeder, Razor Views với tài liệu TV4, TV2, TV3 | Phát hiện và sửa kiểm tra xóa voucher có UsedCount, giới hạn lượt thấp hơn lượt đã dùng, định dạng mã và giá VNĐ |
| 2 | GitHub Actions: kiểm tra tĩnh bằng Python, kiểm tra phân quyền và CSRF, ma trận test, dữ liệu seed | PASS (11 bài kiểm tra) |
| 3 | GitHub Actions: `dotnet build` dự án kiểm tra độc lập liên kết mã C# TV4 + biên dịch Razor với mô hình và IOrderService mẫu | PASS: 0 warnings, 0 errors tại commit `16c2bf49` |

### Lỗi Razor đã được phát hiện và xử lý

Trước khi sửa, `Orders/Index.cshtml` và `Products/Index.cshtml` có chữ `Trang @page / @pages`, khiến Razor hiểu `@page` thành directive (RZ3906, RZ2005, RZ1016). Đã đổi thành `Trang @(page) / @(pages)`, kiểm tra lại biên dịch thành công.

**Build độc lập là kiểm tra hợp đồng biên dịch, KHÔNG phải kiểm thử hệ thống thực tế.** File `tests/TV4ContractHarness/ContractStubs.cs` chỉ phục vụ CI, không được đưa vào project chính. Các stub phản ánh mô hình theo tài liệu; nếu TV3 triển khai khác, vẫn phải sửa tích hợp.

## Kết quả có thể kiểm chứng

- [GitHub Actions chạy trên commit `16c2bf49` – PASS](https://github.com/anh632k-ui/TMDT/actions/runs/40218913326)
- [GitHub Actions chạy trong Pull Request – PASS](https://github.com/anh632k-ui/TMDT/actions/runs/40218916693)
- Workflow `.github/workflows/tv4-ci.yml`: kiểm tra mã nguồn và biên dịch độc lập mỗi lần push/PR.
- Nếu không thấy `VanPhongPham/VanPhongPham.csproj`, job **Actual application build** phải hiển thị **SKIPPED**, không phải PASS.

## Chưa thể khẳng định

- Chưa có project `VanPhongPham/VanPhongPham.csproj`, Models, DbContext, migrations và Services do TV3 phụ trách; chưa có màn hình mua hàng TV2.
- Chưa chạy ứng dụng thật, chưa kết nối SQL Server, chưa kiểm thử thao tác thực tế, chưa có kết quả thực tế cho 17 tình huống chính.
- Chưa kiểm chứng phân quyền qua HTTP, tính đồng thời tranh lượt cuối, cập nhật trạng thái đơn và hoàn lượt voucher qua database.
- Chưa quay video hoặc lưu bản backup `.bak`.
- Chưa merge PR #1 vào main.

## Các bước còn lại để DONE 100%

1. TV3 đưa dự án thật, Models, DbContext, Migration, IOrderService và route Admin/Seeder vào nhánh tích hợp. TV2 đưa giao diện khách.
2. Chạy GitHub Actions cho ứng dụng thật; sửa lỗi phát sinh nếu có.
3. Chạy `dotnet ef database update`, `dotnet run` với SQL Server trên máy thực tế; xác nhận Admin login và 3 khu vực quản lý.
4. Chạy 17 tình huống chính và 6 ca bổ sung trong `TEST_CASES.csv`. Điền *Kết quả thực tế* và trạng thái PASS/FAIL; gắn bằng chứng kiểm thử.
5. Kiểm tra thanh toán giả lập, hoàn lượt, tranh lượt voucher cuối, upload ảnh, phân quyền Customer/Admin và responsive.
6. Quay demo 2 phút (trong bài thuyết trình tối đa 5 phút), backup SQL Server, bổ sung ảnh chụp vào báo cáo; cập nhật PR khỏi Draft sau khi tất cả điều kiện thành công.

## Phạm vi file

Dùng `git diff main...tv4-quantri --name-only` để kiểm tra. Tất cả đường dẫn thay đổi phải nằm trong phần TV4 (`Areas/Admin`, `PaymentController`, `Views/Payment`, `DbSeeder`, ảnh placeholder) hoặc các thư mục kiểm thử/tài liệu/CI thuộc TV4. **Không thay thế Models, Services, Program.cs của TV3 hay Views của TV2.**

## Bổ sung rà soát source ZIP TV3/TV2 và tài liệu mới – 26/10/2026

- Đã đọc bản ZIP nhánh `upload` của repo nhóm private `datvau0401-tech/VanPhongPham`. Nguồn này **không được push lên repo TMDT public**.
- Đối chiếu cấu trúc: .NET 8, Model, DbContext 7 DbSet, Identity Admin/Customer, ASP.NET MVC Area route, `IOrderService`, Cart/Checkout, Mock Payment controller có mặt. Các chữ ký hàm TV4 gọi khớp.
- Phát hiện `Program.cs` thiếu `DbSeeder.SeedAsync` (TV3 phụ trách cấu hình); source nhóm có `PaymentController` và `Mock.cshtml` tạm trùng tên phần TV4, cần thay thế đúng 2 file này.
- TV4 đã sửa giao diện Admin Pending COD → Shipping, Online Pending → Paid; bắt ngoại lệ `MarkPaidAsync`; bổ sung seed sản phẩm bản giới hạn còn **1** trong kho.
- Bảng test nay có **17 tình huống chính** (14 voucher, 3 tồn kho/bảo mật) + **6 kiểm thử hệ thống bổ sung**, tổng **23** dòng. Demo đúng **2 phút**, gồm tranh sản phẩm cuối, tranh voucher cuối và hoàn tồn kho/voucher khi hủy.
- Sau cập nhật, `scripts/check_tv4.py` có **12 bài kiểm tra tĩnh PASS**. `dotnet build` harness C#/Razor TV4 **PASS, 0 errors, 0 warnings**, chứng cứ [Actions run 40254340983](https://github.com/anh632k-ui/TMDT/actions/runs/40254340983).
- `Actual application build` của TMDT public vẫn `skipped` vì không có .csproj/TV3 trong repo này. **Chưa chạy build, EF migration, CRUD, kịch bản đồng thời, thanh toán hay DB trên source TV3 thật**.
- Đã tạo `scripts/merge-tv4-into-group.ps1` và `docs/TV4/PRIVATE_REPO_INTEGRATION.md` để người dùng ghép theo đúng quyền sở hữu file, giữ nguyên backend private.
