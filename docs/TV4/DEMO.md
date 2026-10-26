# TV4 – Kịch bản demo 2 phút (bài thuyết trình tối đa 5 phút)

**Đúng tài liệu TV4 cập nhật 26/10/2026:** phần demo minh họa **3 quyết định thiết kế** thay vì chỉ giới thiệu tính năng chung. Hai trình duyệt: một thông thường, một ẩn danh; hai tài khoản Customer đã đăng nhập.

## Kịch bản 0:00–2:00

| Mốc | Demo | Điều giảng viên cần thấy |
| --- | --- | --- |
| 0:00–0:40 | **A – Tranh sản phẩm cuối:** 2 khách cùng có **Sổ tay bìa cứng A5 (bản giới hạn)** (Stock = 1) trong giỏ và ở Checkout. Bấm Đặt hàng gần đồng thời. | **Chỉ một đơn thành công**; đơn kia bị từ chối, tồn kho bằng 0 và không âm. Quyết định 1: `ExecuteUpdateAsync` có điều kiện `Stock >= Quantity`. |
| 0:40–1:20 | **B – Tranh lượt voucher cuối:** 2 khách khác nhau đã chuẩn bị giỏ, cùng áp `LUOTCUOI` (TotalLimit = 1, UsedCount = 0) và bấm Đặt hàng gần đồng thời. | **Chỉ một đơn dùng mã thành công**, đơn kia báo vừa hết lượt. Quyết định 2: cập nhật lượt nguyên tử trong transaction. |
| 1:20–2:00 | **C – Hoàn tồn kho, voucher:** hủy một đơn đang cho phép hủy, rồi mở Admin → Đơn hàng/Voucher/Sản phẩm. | `VoucherUsage` chuyển `Released`; `UsedCount` giảm theo quy tắc; `Product.Stock` tăng lại đúng số lượng. Quyết định 5. |

## Trước khi demo

1. Trên **cơ sở dữ liệu chỉ dành cho demo**: sản phẩm bản giới hạn Stock = 1, voucher `LUOTCUOI` còn đúng 1 lượt. Không reset bảng, xóa đơn hàng hoặc chạy SQL dọn dữ liệu trên CSDL thật hoặc máy đang chứa dữ liệu cần giữ.
2. Mở sẵn hai trình duyệt / phiên riêng cho khách; tab Admin đã đăng nhập; chuẩn bị đủ các giỏ hàng.
3. Tập ít nhất hai lần; quay video dự phòng và sao lưu `.bak` trước lúc reset thử nghiệm.
4. Một lần thử sử dụng hết hàng/mã sẽ thay đổi trạng thái. Giữa các lần tập, khôi phục **bản sao dữ liệu demo sạch** đã sao lưu hoặc yêu cầu TV3 cấp tiện ích reset phục vụ kiểm thử.

## Các kiểm thử bổ sung không nhất thiết trình diễn trực tiếp

- **TC16 – Sửa giá/tổng tiền bằng F12:** đặt hàng sau khi thay thông số trong form; đơn phải tính lại theo giá lưu trong CSDL. Quyết định thiết kế số 4.
- **TC17 – Hủy đơn hoàn tồn kho:** đặt số lượng 2, hủy và xác nhận stock tăng đúng 2 (không tăng hai lần).
- Toàn bộ 17 tình huống chính phải được ghi **Kết quả thực tế** trong `TEST_CASES.csv` khi có môi trường chạy.

## Nếu giảng viên hỏi

- **Tại sao hai khách không thể mua cùng một sản phẩm cuối?** Vì lệnh cập nhật tồn kho có điều kiện `Stock >= Quantity` chạy trong transaction; số dòng cập nhật = 0 thì từ chối đặt hàng.
- **Tại sao không bán quá lượt voucher?** Vì tăng `UsedCount` có điều kiện và giao dịch, không chỉ kiểm tra ở giao diện.
- **Vì sao hủy đơn phải hoàn cả tồn kho và voucher?** Bởi đơn đã giữ/trừ các tài nguyên khi tạo; hủy cần khôi phục theo `IOrderService`, tránh để tài nguyên bị chiếm vô thời hạn.
- **Ai làm các thành phần?** TV3 viết xử lý nguyên tử và rollback; TV4 làm Admin, dữ liệu demo, thanh toán giả lập, kiểm thử và thao tác demo; TV2 làm giao diện khách.

## Bằng chứng cần bàn giao TV1

Ảnh dashboard Admin, CRUD sản phẩm/voucher, lịch sử dùng voucher, màn hình thanh toán mô phỏng, hai kết quả thành công/thất bại khi tranh hàng và tranh voucher, kiểm tra tồn kho và lượt dùng sau hủy; bảng test có kết quả thực tế.
