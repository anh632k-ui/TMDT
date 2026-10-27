# TV4 – Danh mục ảnh minh chứng và cách ghi kết quả

Dùng cùng mã TC01–TC20 với `TEST_CASES.csv`. Ảnh chỉ được xem là minh chứng **khi thao tác thực tế đã được chạy**. Có thể dán các ảnh vào Word kiểm thử trước khi gửi nhóm trưởng.

| Mã | Ảnh nên chụp | Trạng thái |
| --- | --- | --- |
| TC01–02 | Đăng nhập thành công, giỏ được giữ sau đăng nhập | Chưa có bằng chứng |
| TC03–04 | Thông báo vượt tồn kho/giỏ rỗng | Chưa có bằng chứng |
| TC05–06 | VPP50 hợp lệ, VPP50 dưới đơn tối thiểu | Chưa có bằng chứng |
| TC07–09 | HETHAN, SAPTOI, HETLUOT trả thông báo đúng | Chưa có bằng chứng |
| TC10–11 | Hai mã đồng thời và thay mã | Chưa có bằng chứng |
| TC12 | 2 cửa sổ tranh sản phẩm; Admin Stock = 0 | Chưa có bằng chứng |
| TC13 | 2 cửa sổ tranh LUOTCUOI; Admin UsedCount = 1/1 | Chưa có bằng chứng |
| TC14 | 2 tab cùng tài khoản; Admin chỉ một đơn | Chưa có bằng chứng |
| TC15/17 | Ảnh kho và voucher trước/sau hủy | Chưa có bằng chứng |
| TC16 | Ảnh thay đổi trạng thái COD Pending→Shipping→Completed | Chưa có bằng chứng |
| TC18 | Mock Online và trạng thái Paid | Chưa có bằng chứng |
| TC19 | Khách thường vào /Admin bị từ chối | Chưa có bằng chứng |
| TC20 | F12 mobile: sản phẩm, giỏ, Checkout, Admin | Chưa có bằng chứng |

## Cách ghi

1. Mở `TEST_CASES.csv` bằng Excel hoặc VS Code, xem các bước và kết quả mong đợi.
2. Chạy thử trên **database demo**. Nếu có lỗi, chụp màn hình và ghi kết quả thực tế, chuyển `Đạt/Không đạt` = `Không đạt`, cột `Người sửa` điền người chịu trách nhiệm sau khi xác định lỗi.
3. Chỉ chuyển `Đạt/Không đạt` = `Đạt` khi đã quan sát kết quả đúng, **không dựa riêng vào build PASS**.
4. Ảnh tên `TC12_Stock_0.png`, `TC13_LUOTCUOI_1of1.png`, `TC14_OneOrder.png`; chèn vào bản Word minh chứng. Không đăng ảnh chứa mật khẩu, token hoặc dữ liệu cá nhân lên repo công khai.
5. Cập nhật bảng theo từng lượt; không ghi kết quả tưởng tượng.

**Tình trạng:** Bảng đã chuẩn bị sẵn 20 tình huống, chưa có log/ảnh chạy test được xác minh trong repository TV4 cá nhân.
