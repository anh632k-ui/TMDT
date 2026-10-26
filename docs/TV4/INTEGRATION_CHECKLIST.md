# Checklist tích hợp TV4

- [x] Đã nhận ZIP repo nhóm private nhánh upload có .csproj và dependencies; chưa ghép vào nhánh TV4 của repo nhóm.
- [x] Đã đối chiếu ZIP TV3 có đủ 7 DbSet: Categories, Products, Orders, OrderItems, Vouchers, VoucherScopes, VoucherUsages.
- [x] Đã đối chiếu ZIP TV3 có enums OrderStatus: Pending/Paid/Shipping/Completed/Cancelled; VoucherType: Percent/Fixed/Shipping.
- [x] Đã đối chiếu chữ ký IOrderService.ChangeStatusAsync / CancelAsync / MarkPaidAsync đúng hợp đồng trong ZIP.
- [x] ZIP TV3 có AddRoles và MapControllerRoute cho areas.
- [ ] **TV3 thêm lời gọi DbSeeder.SeedAsync vào Program.cs** (ZIP chưa có).
- [x] ZIP TV3 đã chuyển Checkout Online → /Payment/Mock/{orderId}.
- [ ] TV2 hoàn thiện giao diện khách; không chỉnh phần Views/Payment của TV4.
- [ ] Kiểm tra tài khoản Admin seed trên môi trường demo; Customer không thể vào /Admin.
- [ ] dotnet restore; dotnet build; dotnet ef database update; dotnet run – tất cả thành công.
- [ ] Kiểm thử 17 tình huống chính (14 voucher + 3 tồn kho/bảo mật) và 6 ca hệ thống; điền kết quả thực tế PASS/FAIL.
- [ ] Quay video dự phòng, backup .bak, chụp ảnh bằng chứng.
- [ ] Mở PR tv4-quantri → main sau khi TV3 có nền tương thích; ưu tiên tích hợp trên nhánh staging nếu nhóm thống nhất.

## Bổ sung do tài liệu cập nhật (26/10/2026)

- [x] Seed sản phẩm "Sổ tay bìa cứng A5 (bản giới hạn)" Stock=1; tổng 25 sản phẩm.
- [x] UI Admin đổi trạng thái COD phù hợp OrderService thật; Payment bắt InvalidOperationException.
- [x] Kịch bản demo chuyển sang 2 phút gồm A (tranh Stock), B (tranh lượt LUOTCUOI), C (hủy hoàn tồn kho và voucher).
- [ ] Trên repo PRIVATE: chạy script ghép phần TV4 từ `scripts/merge-tv4-into-group.ps1` và xem diff.
- [ ] TV3 duyệt Program.cs/Seeder, chạy DB demo sạch, xác nhận role Admin.
- [ ] Build/run toàn bộ app và TC15, TC16, TC17 thật; lưu hình/video/backup.
- [ ] Chỉ merge PR khi TV3 và TV2 chốt tích hợp và test đạt.
