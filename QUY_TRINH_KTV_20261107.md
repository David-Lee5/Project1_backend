# KTV nhận máy, kiểm tra và bắt đầu sửa

## Thứ tự thao tác trên giao diện

1. Lễ tân tạo phiếu tiếp nhận và chọn KTV, hoặc phân công trong mục **Điều phối**.
2. KTV đăng nhập, mở **Bàn làm việc kỹ thuật viên** → phiếu được phân công → **Xử lý** → **Xác nhận đã nhận máy**. Thời điểm và người nhận được lưu vào phiếu, có nhật ký thay đổi.
3. KTV kiểm tra máy, nhập **Kết quả kiểm tra**, bấm **Lưu kết quả kiểm tra**. Lễ tân mở phiếu và xem mục **Điều phối** để lấy thông tin này khi lập báo giá.
4. Lễ tân lập và gửi báo giá. Khách đồng ý tại cổng khách hàng, hoặc lễ tân ghi nhận phản hồi trực tiếp tại quầy. Nếu có báo giá phiên bản mới, phiên bản mới nhất phải được duyệt.
5. KTV bấm **Làm mới** trong phiếu. Khi báo giá mới nhất là `DA_DUYET`, chọn `DANG_SUA` và lưu trạng thái. Sau đó mới yêu cầu/xuất vật tư, hoàn thành, lập hóa đơn, thu tiền và bàn giao.

Phiếu đang ở `TIEP_NHAN` bị chặn khi thiếu một trong ba điều kiện: KTV được phân công đã xác nhận nhận máy, có kết quả kiểm tra, báo giá mới nhất đã được duyệt. Điều kiện được kiểm tra trong backend, kể cả khi gọi API trực tiếp. Phiếu cũ đã ở `DANG_SUA` hoặc `CHO_LINH_KIEN` vẫn tiếp tục các bước đang làm; quy tắc mới chỉ áp dụng lúc bắt đầu sửa từ `TIEP_NHAN`.

Khi lễ tân đổi KTV phụ trách, xác nhận nhận máy của người cũ bị xóa. Nếu phiếu còn `TIEP_NHAN`, kết quả kiểm tra cũ cũng được xóa để KTV mới kiểm tra lại. Nhật ký audit vẫn giữ thông tin trước và sau khi phân công.

## API

| Vai trò | Endpoint | Dữ liệu |
|---|---|---|
| KTV được phân công | `POST /api/phieu-sua-chua/{id}/nhan-may` | Không cần body; gọi lại không tạo nhận máy trùng. |
| KTV đã xác nhận | `PATCH /api/phieu-sua-chua/{id}/kiem-tra` | `{ "ketQuaKiemTra": "Kiểm tra nguồn, màn hình..." }`, tối đa 1000 ký tự. |
| Nhân viên có quyền xem phiếu | `GET /api/phieu-sua-chua/{id}` | Trả `ktvNhanId`, `ktvNhanLuc`, `ketQuaKiemTra`, `baoGiaMoiNhatTrangThai`. |
| KTV/Admin | `PUT /api/phieu-sua-chua/{id}/trang-thai` | `{ "trangThai": "DANG_SUA" }` chỉ thành công khi đủ điều kiện trên. |

## Cập nhật CSDL V3 đã tồn tại

Sao lưu CSDL và dừng backend, chọn đúng CSDL V3 rồi chạy lần lượt `database_v3/BO_SUNG_API_20261106.sql` nếu chưa có và `database_v3/BO_SUNG_QUY_TRINH_KTV_20261107.sql`. Không chạy lại file đã có dấu phiên bản. Script mới thêm ba cột nullable, khóa ngoại, ràng buộc đầy đủ cặp người nhận/thời điểm, rồi ghi `V3_20261107_KTV` trong `dbo.v3_schema_migration`. Khởi động lại backend sau migration. File `database_v3/TAO_MOI_QLSCTBDT_V3.sql` đã chứa cả hai phần bổ sung cho CSDL mới.
