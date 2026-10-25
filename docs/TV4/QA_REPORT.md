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
- Chưa chạy ứng dụng thật, chưa kết nối SQL Server, chưa kiểm thử thao tác thực tế, chưa có kết quả thực tế cho 14 tình huống voucher.
- Chưa kiểm chứng phân quyền qua HTTP, tính đồng thời tranh lượt cuối, cập nhật trạng thái đơn và hoàn lượt voucher qua database.
- Chưa quay video hoặc lưu bản backup `.bak`.
- Chưa merge PR #1 vào main.

## Các bước còn lại để DONE 100%

1. TV3 đưa dự án thật, Models, DbContext, Migration, IOrderService và route Admin/Seeder vào nhánh tích hợp. TV2 đưa giao diện khách.
2. Chạy GitHub Actions cho ứng dụng thật; sửa lỗi phát sinh nếu có.
3. Chạy `dotnet ef database update`, `dotnet run` với SQL Server trên máy thực tế; xác nhận Admin login và 3 khu vực quản lý.
4. Chạy 14 tình huống voucher và 6 ca bổ sung trong `TEST_CASES.csv`. Điền *Kết quả thực tế* và trạng thái PASS/FAIL; gắn bằng chứng kiểm thử.
5. Kiểm tra thanh toán giả lập, hoàn lượt, tranh lượt voucher cuối, upload ảnh, phân quyền Customer/Admin và responsive.
6. Quay demo 5 phút, backup SQL Server, bổ sung ảnh chụp vào báo cáo; cập nhật PR khỏi Draft sau khi tất cả điều kiện thành công.

## Phạm vi file

Dùng `git diff main...tv4-quantri --name-only` để kiểm tra. Tất cả đường dẫn thay đổi phải nằm trong phần TV4 (`Areas/Admin`, `PaymentController`, `Views/Payment`, `DbSeeder`, ảnh placeholder) hoặc các thư mục kiểm thử/tài liệu/CI thuộc TV4. **Không thay thế Models, Services, Program.cs của TV3 hay Views của TV2.**
