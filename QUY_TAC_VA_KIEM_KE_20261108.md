# Quy tắc vận hành và kiểm soát tiếp nhận/kho V3

## Cập nhật CSDL

Áp dụng `database/BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql` **một lần** trên CSDL V3 sau
`BO_SUNG_QUY_TRINH_KTV_20261107.sql`, rồi khởi động backend. Script chạy trong transaction,
kiểm tra migration trước và ghi phiên bản `V3_20261108_RULES`. Sao lưu CSDL trước khi áp dụng.
Backend kiểm tra phiên bản này khi khởi động. Các phiếu cũ có
`ap_dung_kiem_soat_tiep_nhan = 0`; phiếu mới có giá trị `1`.

## Bước 1: quy tắc vận hành

- `GET /api/v3/quy-tac-van-hanh`: nhân viên xem quy tắc hiện hành.
- `PUT /api/v3/quy-tac-van-hanh`: Admin cập nhật ngưỡng bảo hành lặp (2–10), số ảnh
  tiếp nhận tối thiểu (1–5), nguyên tắc trả công, chính sách serial và thông tin về
  loại hình kinh doanh, phương pháp thuế, đơn vị hóa đơn điện tử. Có audit log.
- Người tạo yêu cầu vật tư không được tự duyệt, kể cả Admin.
- Mỗi hồ sơ bảo hành khách ghi số lần quay lại theo phiếu gốc. Từ ngưỡng cấu hình,
  hồ sơ chờ Admin khác người tiếp nhận duyệt trước khi chuyển trạng thái. Admin phải
  ghi nguyên nhân sơ bộ và lý do quyết định:
  `POST /api/v3/bao-hanh/khach/{id}/duyet-lap-lai` với
  `{ "quyetDinh":"DA_DUYET", "nguyenNhanSoBo":"...", "lyDoQuyetDinh":"..." }`.
  `GET /api/v3/bao-hanh/khach` trả thêm `pheDuyetLap`.
- Nguyên tắc trả công và thông tin thuế trong quy tắc là dữ liệu nghiệp vụ để chốt
  với quản lý/kế toán; bản này **không tính lương, thuế hay phát hành hóa đơn điện tử**.

## Bước 2: tiếp nhận thiết bị

`POST /api/phieu-sua-chua` yêu cầu `tinhTrangLoi`, `tinhTrangNgoaiQuan`,
`phuKienKemTheo` (ghi `KHONG_CO` nếu không có) và `checklistTiepNhan` gồm
`manHinh`, `voMay`, `nguon`, `temOc`, `dauHieuNuoc`. Giá trị hợp lệ:
`manHinh`/`voMay`: `BINH_THUONG`, `TRAY_XUOC`, `VO_HONG`, `KHONG_KIEM_TRA`;
`nguon`: `BAT_DUOC`, `KHONG_BAT_DUOC`, `KHONG_KIEM_TRA`;
`temOc`: `NGUYEN_VEN`, `HONG_THIEU`, `KHONG_KIEM_TRA`;
`dauHieuNuoc`: `CO`, `KHONG`, `KHONG_KIEM_TRA`. Thiết bị mới cần `soSerial` hoặc
`lyDoKhongSerial` (tối đa 250 ký tự); thiết bị cũ không có serial cũng cần lý do.
Mã `maThietBi` sẵn có là mã nội bộ để dán lên thiết bị không có serial.

Sau khi tạo phiếu:

1. Tải ít nhất số ảnh `LUC_TIEP_NHAN` trong quy tắc qua API ảnh hiện có.
2. Tải ảnh biên nhận khách đã ký với `loai=KHAC`.
3. Gọi `POST /api/phieu-sua-chua/{id}/xac-nhan-tiep-nhan` với
   `{ "nguoiXacNhan":"Tên người ký", "anhBienNhanId":123 }`.
4. Sau đó kỹ thuật viên mới gọi `/{id}/nhan-may`. Ảnh đầu vào của phiếu đã xác nhận
   không thể thêm/xóa; mọi ảnh trên phiếu đã xác nhận không thể xóa.

Đây là lưu chứng cứ biên nhận giấy đã ký; API không tự xác thực chữ ký hoặc OTP.
Ảnh mới lưu người tải lên và SHA-256; API xem ảnh kiểm tra mã này trước khi trả file.
Giao diện tiếp nhận gửi các trường bắt buộc, cho tải ảnh và ghi nhận biên nhận đã ký
trước khi kỹ thuật viên nhận máy; các phiếu cũ không áp dụng bước xác nhận mới.

## Bước 2: kiểm kê và serial

- `POST /api/v3/kiem-ke-kho`: thủ kho/Admin gửi một phiếu gồm **toàn bộ** dòng tồn
  của kho, kể cả dòng tồn 0. Mỗi dòng có `linhKienId`, `soLuongThucTe`; linh kiện
  theo serial có thêm `serialThucTe` là danh sách serial đã quét. Chỉ một phiếu chờ
  duyệt trên mỗi kho.
- `GET /api/v3/kiem-ke-kho?khoId=...` và `GET /api/v3/kiem-ke-kho/{id}` để đối chiếu.
- `POST /api/v3/kiem-ke-kho/{id}/duyet` với
  `{ "quyetDinh":"DA_DUYET", "lyDo":"..." }` hoặc `TU_CHOI`: chỉ Admin khác
  người kiểm được duyệt. Nếu tồn hoặc serial thay đổi sau khi đếm, phải kiểm lại.
- Khi duyệt, chênh lệch số lượng được ghi vào `bien_dong_kho`; serial thiếu chuyển
  thành `THIEU_KIEM_KE` và không thể xuất. Serial tìm lại ở cùng kho có thể được
  đưa về `TRONG_KHO` qua một phiếu kiểm kê mới. Mọi đổi trạng thái serial do kiểm
  kê lưu ở `bien_dong_serial_kiem_ke_v3`.
- Serial không có trong hệ thống hoặc thuộc kho khác bị từ chối để đối soát nguồn
  gốc bằng phiếu nhập/chuyển kho; kiểm kê không tự tạo serial mới.

### Cách ly hàng lỗi

- `POST /api/v3/cach-ly-vat-tu`: thủ kho/Admin ghi kho, linh kiện, số lượng,
  serial (bắt buộc nếu mẫu theo serial) và lý do. Tồn có thể xuất giảm ngay;
  serial chuyển sang `CACH_LY`, không thể xuất cho phiếu sửa.
- `GET /api/v3/cach-ly-vat-tu?khoId=...&trangThai=CACH_LY` để xem hồ sơ.
- `POST /api/v3/cach-ly-vat-tu/{id}/xu-ly`: Admin khác người tạo chọn
  `NHAP_LAI` hoặc `THANH_LY` và ghi lý do. Nhập lại cộng tồn và ghi biến động;
  thanh lý giữ tồn đã trừ và chuyển serial sang `THANH_LY`.

Ví dụ tạo phiếu:

```json
{
  "khoId": 1,
  "ghiChu": "Kiểm kê cuối ngày",
  "chiTiet": [
    { "linhKienId": 1, "soLuongThucTe": 2, "serialThucTe": ["RAM-A", "RAM-B"] },
    { "linhKienId": 2, "soLuongThucTe": 5, "serialThucTe": [] }
  ]
}
```
