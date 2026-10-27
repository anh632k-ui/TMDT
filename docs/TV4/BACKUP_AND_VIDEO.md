# TV4 – Checklist sao lưu .bak, video dự phòng và dọn demo

**Người thao tác:** Anh (TV4). **Thời hạn:** tối 28/10/2026. Các thao tác này phải chạy trên máy có SQL Server, không thể hoàn thành chỉ bằng mã nguồn GitHub.

## An toàn trước khi dọn dữ liệu

- Xác định chính xác instance và tên **database demo** trước khi thao tác. Máy có thể có nhiều instance SQL Server.
- Phối hợp Hậu lấy `scripts/reset-demo.sql`. Script mẫu trong tài liệu TV3 **xóa toàn bộ đơn thử**, vì thế không được chạy trên database thật/dùng chung có dữ liệu phải giữ.
- Sao lưu trước khi xóa dữ liệu. Trong SSMS, kiểm tra `DB_NAME()` bằng `SELECT DB_NAME();` trong cửa sổ query đúng CSDL.
- Đơn #1 và các đơn thử khác: ghi lại mã và ảnh, chỉ hủy/xóa theo thỏa thuận nhóm và script đã được rà soát. Không xóa trực tiếp tùy tiện để tránh lệch kho/voucher.
- Khi đã reset, xác minh:
  ```sql
  SELECT Name, Stock FROM dbo.Products WHERE Name = N'Sổ tay bìa cứng A5 (bản giới hạn)';
  SELECT Code, UsedCount, TotalLimit FROM dbo.Vouchers WHERE Code IN (N'LUOTCUOI', N'HETLUOT');
  SELECT COUNT(*) AS SoDonConLai FROM dbo.Orders;
  ```
  Kỳ vọng trước demo: sản phẩm demo Stock=1; LUOTCUOI UsedCount=0 / TotalLimit=1; HETLUOT đúng dữ liệu dùng cho ca hết lượt; đơn thử được dọn an toàn.

## Sao lưu database

1. Trong SSMS chọn **Databases > VanPhongPhamDb > Tasks > Back Up...**.
2. Chọn Backup type **Full**, chọn thư mục mà **tài khoản dịch vụ SQL Server có quyền ghi** (ưu tiên ổ D nếu có và được cấp quyền).
3. Chọn **Back Up**, chờ thông báo Success; kiểm tra file `.bak` được tạo, không có kích thước 0.
4. Để kiểm tra bằng T-SQL mà không restore đè database:
   ```sql
   RESTORE VERIFYONLY FROM DISK = N'D:\\k1\\TMDT\\BackupSQL\\VanPhongPhamDb.bak';
   ```
   Chỉ thay đường dẫn bằng **đường dẫn file thực tế**. `VERIFYONLY` không thay thế diễn tập khôi phục hoàn chỉnh.
5. **Không push .bak lên GitHub**. Gửi qua kênh lưu trữ được nhóm thống nhất.

## Video dự phòng

1. Dùng OBS Studio hoặc Xbox Game Bar (Win+G), quay A/B/C đúng thứ tự trong `DEMO.md`; có lời giải thích ngắn.
2. Kiểm tra phát thử `.mp4` có hình, có âm thanh; giữ bản trên máy và một thiết bị/kho khác.
3. Nếu demo trực tiếp gặp sự cố, nói rõ đây là **video đã quay trước**, không trình bày như demo trực tiếp.

## Checklist xác nhận

- [ ] Đã có backup .bak và chạy RESTORE VERIFYONLY không lỗi
- [ ] Đã có script reset Hậu bàn giao và xác minh tác động trước khi chạy
- [ ] Stock demo đúng 1; LUOTCUOI 0/1; đơn thử cần xóa đã được xử lý
- [ ] Đã tập A/B/C lần 1 và lần 2, ghi kết quả thật
- [ ] File MP4 mở được, có đủ A/B/C
- [ ] Đã bàn giao minh chứng và kết quả kiểm thử cho Đạt
