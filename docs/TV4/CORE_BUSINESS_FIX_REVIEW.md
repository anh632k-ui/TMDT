# Rà soát và gói sửa nghiệp vụ TV3 – cần duyệt trước khi ghép

**Nguồn đối chiếu:** `VanPhongPham-main.zip` (snapshot từ nhánh nhóm ngày 26/10/2026). GitHub nhóm `datvau0401-tech/VanPhongPham` là private; tài khoản kết nối hiện tại không đọc được repo (404). Vì vậy **chưa có thay đổi nào được thực hiện trên nhánh TV3/nhánh nhóm**. 

## Đã sửa thật trên repository TV4 cá nhân

1. **Admin sửa tồn kho khi khách đặt đơn:** form sửa sản phẩm giữ `OriginalStock`; lệnh cập nhật dùng `ExecuteUpdateAsync` với điều kiện `Id` và `Stock` vẫn đúng giá trị ban đầu. Nếu kho thay đổi, từ chối lưu và đề nghị tải lại. Tệp ảnh mới upload được dọn khi cập nhật thất bại.
2. **Giá bằng 0 hoặc âm:** kiểm tra server `Range(1, ...)`; form `min=1`.
3. **Phân quyền và thanh toán:** giữ middleware `[Authorize]`, AntiForgery và điều kiện sở hữu đơn Online, không cho vượt luồng chuyển trạng thái.
4. **Seed Admin mới:** mật khẩu demo khởi tạo mới nâng lên `Tv4DemoAdmin@2026!` (chỉ dành cho demo), hoặc khai báo biến `TV4_DEMO_ADMIN_PASSWORD`. **Tài khoản Admin đã seed từ trước không tự đổi mật khẩu**, phải đổi bằng Identity khi cần.

> Kiểm tra tĩnh và biên dịch harness độc lập **không thay thế** chạy thực tế với SQL Server/TV3.

## Gói thay đổi đề xuất cho TV3 (CHƯA áp dụng)

File `scripts/tv3_core_patch.py` chứa thay đổi giới hạn theo snapshot đã đọc. Nó mặc định **dry-run và không viết file**, từ chối bản mã nguồn có đoạn khác hoặc working tree chưa sạch. TV3 phải đọc diff trước khi quyết định áp dụng.

| Mảng | Đề xuất sửa | Tác dụng |
| --- | --- | --- |
| `Program.cs` | Password 12+ gồm chữ hoa/thường/số/ký tự đặc biệt; email duy nhất, cấu hình Identity lockout | Không còn chính sách 3 ký tự; **cấu hình lockout chưa đảm bảo hiệu lực nếu UI đăng nhập mặc định không gọi `lockoutOnFailure: true`** |
| `Areas/Identity/Pages/Account/Register.cshtml(.cs)` | Kiểm tra tối thiểu 12 ký tự, email trùng, thông báo rõ | Nhất quán server/client |
| `Services/OrderService.cs` | Hủy đơn hoàn lượt voucher kể cả đã hết hạn; nếu bộ đếm sai, rollback | Tránh `Released` nhưng `UsedCount` không giảm |
| `Services/UnpaidOrderCleanup.cs` | Khoảng quét và hạn hủy đọc cấu hình, mặc định 5/30 phút | Dễ demo mà không sửa cứng vào service |
| `Data/ApplicationDbContext.cs` | Khai báo CHECK cho Stock/Price/Quantity/UsedCount/TotalLimit | Chặn dữ liệu bất thường ngay ở CSDL **sau khi tạo migration và áp dụng** |

### Cách kiểm tra gói trước khi áp dụng

Chỉ thực hiện khi TV3 đồng ý, trên **nhánh riêng sạch của repo nhóm private**. Giả sử Terminal PowerShell ở thư mục chứa `VanPhongPham.csproj`:

```powershell
python scripts/tv3_core_patch.py .               # dry-run; cần copy script vào đường dẫn có thể chạy
python scripts/tv3_core_patch.py . --apply       # chỉ khi đã rà chính xác snapshot
git --no-pager diff --check
git --no-pager diff --stat
dotnet build
```

**Lưu ý:** Không copy nguyên `VanPhongPham-main.zip` lên repo TMDT cá nhân. Script từ repo TV4 có thể dùng riêng trên máy có repo private. Nếu cảnh báo mã nguồn khác, **dừng và chuyển cho TV3 đối chiếu**, không ép chỉnh chuỗi text.

### Việc còn cần TV3 làm thủ công

1. Kiểm tra chính sách mật khẩu hiện đại; lựa chọn độ dài 12+ và yêu cầu ký tự ở bản vá là mục tiêu **đồ án**, không phải chứng nhận tuân thủ đầy đủ NIST. Nếu hướng tới môi trường thực tế, áp dụng chính sách độ dài cao hơn, từ chối mật khẩu phổ biến, khôi phục mật khẩu và xác minh email đúng quy trình.
2. **Login lockout:** không được tuyên bố đã bảo vệ hoàn chỉnh chỉ vì đã đặt `MaxFailedAccessAttempts`; UI đăng nhập cần dùng `PasswordSignInAsync(..., lockoutOnFailure: true)` hoặc giải pháp tương đương, kèm kiểm thử thực tế.
3. **Migration:** thay đổi `OnModelCreating` chưa tạo constraint trong database đang chạy. Sau backup, TV3 cần tạo và rà migration, kiểm tra dữ liệu cũ hợp lệ trước `dotnet ef database update`. Không chạy trên CSDL nhóm đang dùng nếu chưa thống nhất.
4. **Giỏ khách chưa đăng nhập**, đăng ký email và xác minh email: cần kiểm tra bản TV2/TV3 mới nhất. Không bịa là đã giải quyết chỉ từ bản ZIP cũ.
5. **Checkout hai tab và hai người tranh lượt cuối:** thực nghiệm ít nhất 3 lần, kiểm tra số đơn, tồn kho, UsedCount và trạng thái voucher sau rollback. Patch không thay thế test transaction.
6. **Thanh toán lặp/hủy lặp:** bấm 2 lần, xác nhận kho/voucher không biến động lần thứ hai.
7. Khi đổi chính sách đăng ký, tạo tài khoản mới test mật khẩu dài phù hợp. Admin có sẵn vẫn có mật khẩu cũ cho đến khi TV3 reset qua Identity.

## Ma trận kiểm thử bổ sung (không thay thế 20 ca của thầy)

| Case | Thao tác | Kết quả kỳ vọng |
| --- | --- | --- |
| M01 | Đăng ký mật khẩu 3 ký tự | Bị từ chối |
| M02 | Đăng ký email sai định dạng | Bị từ chối |
| M03 | Đăng ký trùng email | Không tạo tài khoản thứ hai |
| M04 | Khách thường vào `/Admin` | Bị từ chối |
| M05 | Admin gửi giá 0 / Stock âm | Bị từ chối server |
| M06 | Admin mở form Stock=10; khách đặt 2; Admin lưu form cũ | Báo kho đã thay đổi; kho vẫn 8 |
| M07 | Hai khách cùng mua Stock=1 | Một đơn thành công |
| M08 | Sửa giá ở browser Checkout | Backend dùng giá CSDL |
| M09 | Hủy đơn hai lần | Chỉ hoàn kho một lần |
| M10 | Hủy đơn sau khi voucher hết hạn | Released và UsedCount nhất quán |
| M11 | Thanh toán Online hai lần | Không nhân đôi giao dịch/trạng thái |
| M12 | Đặt cùng một checkout hai tab | Một đơn |
| M13 | Tải tệp giả JPG / quá 5MB | Từ chối upload |
| M14 | Ẩn sản phẩm sau khi khách thêm giỏ | Không tạo đơn cho món ẩn |
| M15 | Áp hai voucher giảm hàng | Mã sau thay mã trước |

**Kết quả thực tế:** các ca mới chưa được chạy; ghi PASS/FAIL sau khi kiểm thử máy nhóm.

## Không mở rộng quá phạm vi gần hạn

Đơn COD ở Shipping chỉ chuyển Completed là đúng workflow đã thống nhất. Giao hàng thất bại/đổi trả/hoàn tiền là tính năng mới, **không đưa vào sát hạn nộp** nếu thầy không yêu cầu và TV3 chưa thiết kế vòng đời đơn tương ứng.
