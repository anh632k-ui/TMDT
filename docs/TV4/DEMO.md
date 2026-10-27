# TV4 – Kịch bản demo A/B/C (khoảng 2 phút)

**Theo Hướng dẫn công việc: Anh (Admin, kiểm thử, demo), cập nhật 27/10/2026.** Demo minh họa ba quyết định xử lý hệ thống được nêu trong slide 8. Kết quả ở dưới là **kỳ vọng**, chưa phải kết quả kiểm chứng.

## Trước khi diễn tập

- Phối hợp Hậu (TV3) lấy **script reset dành riêng database demo**, kiểm tra dữ liệu đích, sao lưu .bak trước khi dùng. Không chạy script xóa đơn trên dữ liệu cần giữ.
- Chuẩn bị hai phiên trình duyệt khác nhau, đăng nhập **hai Customer khác nhau**. Mở sẵn trang Thanh toán, nhập sẵn địa chỉ/tên/số điện thoại.
- Mở thêm Admin: Sản phẩm, Voucher và Đơn hàng. Giữ website chạy trên cùng máy.
- Với Demo A: sản phẩm `Sổ tay bìa cứng A5 (bản giới hạn)` có `Stock=1`. Với Demo B: voucher `LUOTCUOI` có `UsedCount=0`, `TotalLimit=1` và sản phẩm dùng trong giỏ **còn đủ hàng**.
- Demo C yêu cầu cơ chế chống đặt trùng từ TV3 (request key được xử lý một lần). Nếu chưa kiểm chứng, dừng và báo Hậu thay vì trình bày là đã đạt.

## Kịch bản tối đa 2 phút

| Thời điểm | Demo | Kết quả cần chứng minh |
| --- | --- | --- |
| 00:00–00:40 | **A – Không bán vượt kho.** Hai Customer đã mở Checkout với sản phẩm chỉ còn 1. Đếm 3–2–1 rồi gần đồng thời bấm Đặt hàng; chuyển Admin > Sản phẩm. | Một đơn thành công; khách còn lại nhận lỗi hết hàng; tồn kho **0**, không âm. |
| 00:40–01:20 | **B – Không vượt lượt voucher.** Dùng hai giỏ hàng còn đủ tồn kho, mỗi khách áp `LUOTCUOI` còn 1 lượt và đặt gần đồng thời; chuyển Admin > Voucher. | Một đơn sử dụng mã thành công, người còn lại nhận lỗi vừa hết lượt; `LUOTCUOI: 1/1`. |
| 01:20–02:00 | **C – Không đặt đơn trùng.** Một tài khoản mở cùng Checkout trên **hai tab**, bấm Đặt hàng tab 1 rồi ngay sau đó tab 2; chuyển Admin > Đơn hàng. | Chỉ **một đơn mới**, tồn kho chỉ trừ một lần. |

## Nếu còn thời gian: minh họa hoàn trả (quyết định 4)

Admin > Đơn hàng > Chi tiết một đơn còn được phép hủy và có voucher `LUOTCUOI`, chọn Hủy đơn. Kiểm tra `UsedCount` giảm về 0/1, Stock tăng lại đúng số đã trừ. Không hủy đơn đã hoàn tất; không hủy các đơn có dữ liệu cần giữ.

## Phiếu diễn tập 2 lần

| Demo | Lần 1 – kết quả thật | Lần 2 – kết quả thật | Ảnh/video |
| --- | --- | --- | --- |
| A: Tranh Stock cuối | Chưa chạy | Chưa chạy | Chưa có |
| B: Tranh LUOTCUOI | Chưa chạy | Chưa chạy | Chưa có |
| C: Chống đặt trùng | Chưa chạy | Chưa chạy | Chưa có |
| D: Hoàn trả (tùy chọn) | Chưa chạy | Chưa chạy | Chưa có |

**Nếu FAIL:** chụp lỗi và ghi các bước tái hiện; lỗi giao diện khách chuyển Giang, lỗi `OrderService`/voucher/transaction chuyển Hậu, lỗi trang Admin do Anh sửa. Chỉ đánh dấu ĐẠT sau khi chạy lại thành công.

## Chụp và gửi Đạt

Ưu tiên 6–10 ảnh: hai kết quả tranh stock, Admin kho = 0, hai kết quả tranh voucher, Admin LUOTCUOI = 1/1, Admin chỉ 1 đơn khi đặt trùng, Admin sau hủy (tùy chọn). Giữ ảnh và bảng test trong `docs/TV4` ở repo nhóm **sau khi mày tự ghép**; không để ảnh chưa thử làm bằng chứng đã đạt.

## Mốc và bàn giao

- **28/10 15:00:** bảng kiểm thử tối thiểu 15 ca đã có kết quả thực tế.
- **28/10 18:00:** tập trọn A/B/C hai lượt và gửi Đạt minh chứng.
- **28/10 tối:** lưu và mở thử bản backup `.bak` và video `.mp4` (không đưa binary này lên GitHub).
- **29/10 sáng:** tập thuyết trình hai lần có bấm giờ; chỉ sửa lỗi nghiêm trọng, không thêm chức năng mới.
