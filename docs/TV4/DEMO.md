# TV4 – Kịch bản demo 5 phút

## Trước khi trình bày

- Dùng một máy đã thử cả luồng ít nhất hai lần, đặt trình duyệt đúng tỷ lệ hiển thị.
- Mở sẵn tab khách và tab Admin (đăng nhập bằng hai phiên trình duyệt riêng nếu cần).
- Backup SQL Server sang `.bak` và quay trước một video thao tác dự phòng.
- Kịch bản này **chỉ có thể chạy đầy đủ khi TV2 và TV3 đã tích hợp xong**.

## Kịch bản

| Thời lượng | Thao tác | Kết quả cần trình bày |
| --- | --- | --- |
| 0:00–0:40 | Khách vào trang chủ, tìm "giấy A4", mở chi tiết, thêm 2 gói giấy và 1 hộp bút | Trang tìm kiếm, sản phẩm, giỏ hàng |
| 0:40–1:15 | Mở giỏ, chỉnh số lượng, tới Checkout, đăng nhập | Tổng hàng 205.000đ cho giỏ mẫu |
| 1:15–2:00 | Áp `VPP50`; thử `ABC123` để quan sát lý do không hợp lệ | VPP50 giảm 20.500đ; ship 22.000đ; tổng 206.500đ |
| 2:00–2:45 | Đặt đơn Online, sang `/Payment/Mock/{id}`, bấm "Thanh toán thành công" | Trạng thái từ Pending sang Paid |
| 2:45–4:00 | Mở Admin; xem đơn vừa đặt; cập nhật Shipping; kiểm tra voucher VPP50 | Admin quản lý đơn, lượt voucher |
| 4:00–5:00 | Hủy một đơn Pending khác có voucher rồi mở lịch sử voucher | VoucherUsage = Released, UsedCount hoàn theo service TV3 |

## Nếu giảng viên hỏi

- **TV4 phụ trách gì?** Khu vực quản trị sản phẩm, đơn hàng, voucher; thanh toán mô phỏng; seed dữ liệu; kiểm thử toàn bộ hệ thống.
- **Tại sao không dùng thanh toán thật?** Đồ án chỉ mô phỏng kết quả giao dịch; không giao dịch tài chính. Có thể tích hợp VNPay hoặc MoMo ở hướng phát triển.
- **Ai đảm bảo voucher không bị dùng vượt lượt?** TV3 triển khai giao dịch và cập nhật nguyên tử; TV4 kiểm thử ca 14 bằng hai tài khoản.
- **Hủy đơn có hoàn lượt voucher không?** Có theo hợp đồng IOrderService; TV4 kiểm thử bằng kiểm tra VoucherUsage và UsedCount trước/sau.
- **Admin khác Customer ở đâu?** Role Admin được kiểm tra bằng `[Authorize(Roles = "Admin")]`, Customer bị chặn.

## Bằng chứng khi nộp

Chụp trang tổng quan Admin, danh sách và form sản phẩm, danh sách và chi tiết đơn, danh sách và form voucher, lịch sử VoucherUsage, trang thanh toán mô phỏng. Bảng TEST_CASES.csv có **kết quả thực tế** và ảnh/video minh chứng sau khi chạy thử.
