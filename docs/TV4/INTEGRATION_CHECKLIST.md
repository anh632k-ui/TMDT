# Checklist tích hợp TV4

- [ ] TV3 đưa .csproj và các dependencies vào nhánh tích hợp.
- [ ] TV3 cung cấp đủ 7 DbSet: Categories, Products, Orders, OrderItems, Vouchers, VoucherScopes, VoucherUsages.
- [ ] TV3 cung cấp enums OrderStatus: Pending/Paid/Shipping/Completed/Cancelled; VoucherType: Percent/Fixed/Shipping.
- [ ] TV3 chốt IOrderService.ChangeStatusAsync / CancelAsync / MarkPaidAsync đúng hợp đồng.
- [ ] TV3 thêm .AddRoles<IdentityRole>(), MapControllerRoute cho areas, lời gọi DbSeeder.SeedAsync.
- [ ] TV3 chốt chuyển Checkout Online → /Payment/Mock/{orderId}.
- [ ] TV2 hoàn thiện giao diện khách; không chỉnh phần Views/Payment của TV4.
- [ ] Kiểm tra tài khoản Admin seed trên môi trường demo; Customer không thể vào /Admin.
- [ ] dotnet restore; dotnet build; dotnet ef database update; dotnet run – tất cả thành công.
- [ ] Kiểm thử 14 voucher và 6 ca hệ thống; điền kết quả thực tế, PASS/FAIL.
- [ ] Quay video dự phòng, backup .bak, chụp ảnh bằng chứng.
- [ ] Mở PR tv4-quantri → main sau khi TV3 có nền tương thích; ưu tiên tích hợp trên nhánh staging nếu nhóm thống nhất.
