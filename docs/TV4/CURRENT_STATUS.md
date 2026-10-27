# Trạng thái TV4 – bản yêu cầu mới 27/10/2026

## Đã cập nhật trên nhánh `tv4-quantri` (repo cá nhân)

- [x] Xóa chú thích nội bộ về `IOrderService của TV3 xử lý` khỏi `Areas/Admin/Views/Orders/Details.cshtml`.
- [x] Kiểm tra tất cả Razor View Admin: không còn TODO/TV2/TV3/TV4 hiển thị.
- [x] `TEST_CASES.csv` có **đúng 20 ca**, đủ 8 cột tài liệu yêu cầu, chưa tự đánh dấu PASS.
- [x] Demo A/B/C mới: A tranh sản phẩm cuối; B tranh LUOTCUOI; **C chống đặt trùng hai tab**. Thêm ô diễn tập hai lần.
- [x] Có danh mục ảnh (`EVIDENCE_INDEX.md`) và checklist backup/video (`BACKUP_AND_VIDEO.md`).
- [x] CI kiểm tra mã nguồn độc lập, Razor và PowerShell; có kiểm tra cấu trúc bảng test và chú thích nội bộ.

## Phần chưa thể hoàn thành từ GitHub

- [ ] Dọn đơn thử (kể cả #1 nếu đúng CSDL demo) và reset dữ liệu: **TV3 Hậu cung cấp script**, TV4 thực hiện trên máy có SQL Server sau khi backup.
- [ ] Điền kết quả thực tế tối thiểu **15/20** ca và gắn ảnh chứng minh.
- [ ] Chạy demo A/B/C **hai lần thực tế**, xác minh cả hai lượt không lỗi.
- [ ] Tạo .bak thực tế, kiểm tra được bản sao lưu; quay và mở thử video MP4.
- [ ] Bàn giao Word, ảnh, video/backup cho Đạt; **người dùng tự làm các bước Git nhóm/PR**.

**Không được đánh dấu các phần này DONE chỉ vì GitHub Actions PASS.** CI trên repo cá nhân chạy với `ContractStubs.cs`; không truy cập được database local, không chứng minh logic đồng thời.

## Các hạn từ tài liệu

- 27/10: dọn Admin, dọn dữ liệu thử trên máy demo.
- 28/10 15:00: bảng kiểm thử tối thiểu 15 ca có kết quả.
- 28/10 18:00: demo A/B/C hai lượt, gửi Đạt và chuẩn bị tích hợp.
- 28/10 tối: có backup .bak và video dự phòng, đã kiểm tra.
- 29/10 sáng: tập nói và demo 2 lần, chỉ sửa lỗi nghiêm trọng.

## Lưu ý về quyền sở hữu

- TV4: `Areas/Admin/`, bảng test, demo, ảnh, backup/video.
- TV3: `Services/`, Models, DbContext, Program.cs, script reset và chống đặt trùng.
- TV2: Views giao diện khách, CSS/JS, sản phẩm có ảnh thật.

Khi gặp lỗi thuộc phần TV2/TV3, báo đúng người phụ trách, không chỉnh chồng code.
