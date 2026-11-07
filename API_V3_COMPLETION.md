# API hoàn thiện V3

Các endpoint dưới đây dùng schema V3 hiện có; không cần chạy migration SQL mới.

## Danh mục linh kiện

| Vai trò | Endpoint | Nội dung |
|---|---|---|
| Admin | `PUT /api/v3/loai-linh-kien/{id}` | `{ "ma", "ten", "hoatDong" }` |
| Admin | `PUT /api/v3/loai-linh-kien/thuoc-tinh/{id}` | `{ "ma", "ten", "kieuDuLieu", "donVi", "batBuoc", "thuTu", "hoatDong" }` |
| Admin | `PUT /api/v3/loai-linh-kien/lua-chon/{id}` | `{ "giaTri", "thuTu", "hoatDong" }` |
| Admin/Thủ kho | `PUT /api/v3/mau-linh-kien/{id}` | Cùng payload với API tạo mẫu V3; danh sách `thongSo` là các giá trị của thuộc tính đang hoạt động. |

Không thể đổi kiểu thuộc tính đã có giá trị hoặc lựa chọn, bật thuộc tính bắt buộc khi mẫu cũ thiếu giá trị, đổi loại của mẫu đã có thông số, hay đổi chính sách serial của linh kiện đã có lịch sử kho. API mẫu giữ lại giá trị của thuộc tính đã ngừng dùng để bảo toàn dữ liệu cũ.

## Đặt lại mật khẩu

Admin gọi `POST /api/users/{id}/reset-password` với `{ "newPassword": "...", "confirmPassword": "..." }`. Mật khẩu mới dài 8–100 ký tự. API không cho tự đặt lại mật khẩu của chính Admin; dùng `POST /api/auth/change-password` cho trường hợp đó. Mật khẩu được băm BCrypt, refresh token của tài khoản bị thu hồi và thao tác được ghi audit không chứa mật khẩu. Access token đã cấp trước đó vẫn có hiệu lực đến khi hết hạn (cấu hình mặc định là 15 phút). Giao diện Admin tìm tài khoản theo tên đăng nhập, gồm cả khách hàng, trước khi đặt lại.

## Khung giờ đặt lịch

`GET /api/lich-hen/khung-gio?ngay=YYYY-MM-DD` công khai, trả các khung giờ theo UTC+7 và số chỗ còn lại. `POST /api/lich-hen` chỉ nhận đúng đầu giờ làm việc và kiểm tra lại sức chứa trong transaction. Lịch `CHO_XAC_NHAN` và `DA_XAC_NHAN` chiếm chỗ; hủy hoặc từ chối sẽ trả chỗ. Lịch cũ đặt lệch đầu giờ vẫn được tính vào khung giờ tương ứng.

Cấu hình trong `RepairSystem/appsettings.json`: `Appointments:StartHour` (mặc định 9), `EndHourExclusive` (18), `CapacityPerSlot` (3). Ví dụ 9–18 có các khung 09:00 đến 17:00. Thời gian của khách phải cách hiện tại hơn một giờ. Khách xem và hủy lịch của mình tại cổng khách; lễ tân xác nhận, từ chối và liên kết lịch với phiếu sửa chữa.
