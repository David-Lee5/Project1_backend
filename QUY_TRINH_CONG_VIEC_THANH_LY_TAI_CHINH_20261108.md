# Bước 3–5: công việc sửa chữa, thanh lý và đối soát tài chính

## Cập nhật CSDL

Sao lưu CSDL, dừng backend rồi chạy `database/BO_SUNG_CONG_VIEC_THANH_LY_TAI_CHINH_20261108.sql`
trên đúng CSDL V3 **sau** `BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql`. Script dùng transaction,
ghi `V3_20261108_WORKFLOW` vào `dbo.v3_schema_migration` và từ chối chạy lại.
Backend kiểm tra migration này khi khởi động.

Trên máy phát triển hiện tại, migration đã được áp dụng cho `QLSCTBDT_V3` ngày
08/11/2026. Bản sao lưu trước migration nằm trong thư mục backup mặc định của SQL Server,
tên `QLSCTBDT_V3_before_workflow_20261108.bak`. Đồng nghiệp dùng CSDL riêng vẫn cần chạy
script trên máy của họ. Không gửi file `.bak` lên Git.

## Bước 3: theo dõi sửa chữa và bảo hành

- Lễ tân/Admin gọi `POST /api/v3/cong-viec-sua-chua` để giao từng việc theo phiếu,
  thợ phụ trách và mã hồ sơ bảo hành nếu có.
- KTV được giao gọi `POST /api/v3/cong-viec-sua-chua/{id}/bat-dau` khi phiếu ở `DANG_SUA`;
  thời điểm bắt đầu được lưu. Khi xong, gọi `/{id}/hoan-thanh` với `ketQua` và
  `chiTietXuatKhoIds` thuộc cùng phiếu. Mỗi dòng xuất kho chỉ được gắn vào một việc.
  Phiếu còn việc `CHO_LAM`/`DANG_LAM` không thể chuyển `HOAN_THANH`.
  Giao nhầm có thể gọi `/{id}/huy` với lý do; hệ thống giữ audit và việc đã hủy
  không chặn hoàn thành phiếu. Thợ được giao việc riêng có quyền xem phiếu, ảnh và
  vật tư dù không phải KTV chính; quyền đổi trạng thái toàn phiếu vẫn thuộc KTV chính.
- `GET /api/v3/cong-viec-sua-chua?phieuId=...` trả nhật trình từng việc.
  Lễ tân xem phần nghiệp vụ; tiền công chỉ hiện cho Admin/KTV.
- `GET /api/v3/bao-hanh/khach` trả số lần quay lại, duyệt lặp lại và phân tích chi phí.
  Admin/Lễ tân gọi `PUT /api/v3/bao-hanh/khach/{id}/phan-tich-chi-phi` với nguyên nhân,
  bên chịu chi phí (`CUA_HANG`, `KHACH`, `NCC`, `KTV`), chi phí dự kiến và thực tế.
  Cần ghi chi phí thực tế trước khi hoàn thành bảo hành.
- Khi tiếp nhận bảo hành có thể liên kết `phieuSuaBaoHanhId` cùng thiết bị;
  nếu tạo phiếu sửa sau, dùng `PATCH /api/v3/bao-hanh/khach/{id}/lien-ket-phieu-sua`.
  Việc sửa trên phiếu đó có thể gắn `phieuBaoHanhId`.

Ví dụ giao việc:

```json
{"phieuId":12,"kyThuatVienId":5,"phieuBaoHanhId":null,"moTa":"Thay cổng sạc"}
```

## Bước 4: giám sát đồ hỏng

1. Thủ kho/Admin cách ly bằng `POST /api/v3/cach-ly-vat-tu`; tồn khả dụng và serial
   được khóa ngay như bước 2.
2. Kiểm tra, ghi hướng `NHAP_LAI`, `THANH_LY` hoặc `TIEU_HUY` và giá trị ước tính qua
   `POST /api/v3/cach-ly-vat-tu/{id}/de-xuat`.
3. Tải ít nhất một `ANH` và một `BIEN_BAN` lên
   `POST /api/v3/cach-ly-vat-tu/{hoSoId}/tai-lieu` (multipart: `loai`, `file`).
   Ảnh JPEG/PNG hoặc biên bản PDF tối đa 10 MB; file được lưu ngoài `wwwroot`,
   API đọc kiểm tra SHA-256 trước khi trả. Khi sao lưu hoặc chuyển máy, cần sao lưu
   cả CSDL và thư mục `RepairSystem/App_Data/thanh-ly`; bản `.bak` không chứa file này.
4. Admin khác người cách ly và người đề xuất duyệt bằng
   `POST /api/v3/cach-ly-vat-tu/{hoSoId}/duyet`; ghi lý do và giá trị được duyệt.
   Hồ sơ bị từ chối có thể lập đề xuất mới và giữ lịch sử đề xuất cũ.
5. Sau duyệt, Admin gọi `POST /api/v3/cach-ly-vat-tu/{cachLyId}/xu-ly` đúng hướng
   được duyệt, ghi lý do và giá trị thu về. `TIEU_HUY` dùng trạng thái serial
   `THANH_LY` hiện có nhưng hồ sơ giữ rõ loại xử lý. Nhập lại cộng tồn và ghi biến động;
   thanh lý/tiêu hủy giữ tồn đã trừ.

## Bước 5: chứng từ, công nợ và tiền công

- `POST /api/v3/chung-tu/dau-vao`: ghi hóa đơn NCC cho phiếu nhập đã xác nhận.
  Backend so tiền hàng với tổng giá trị các dòng nhập và so mã số thuế với NCC.
  Chênh lệch được giữ ở trạng thái `CHENH_LECH`, bắt buộc ghi chú.
  `GET /api/v3/chung-tu/dau-vao` xem kết quả.
- Hóa đơn `hoa_don` hiện có là chứng từ nội bộ. `POST /api/v3/chung-tu/dau-ra`
  ghi số chứng từ **đã phát hành ở hệ thống ngoài**, so tổng tiền với hóa đơn nội bộ;
  `POST /api/v3/chung-tu/dau-ra/{id}/dieu-chinh` ghi chứng từ điều chỉnh/thay thế/hủy
  đã thực hiện ở bên ngoài. API này không phát hành hoặc hủy hóa đơn thật.
- Thu/hoàn tiền và công nợ tiếp tục qua API `.../giao-dich`, `.../tai-chinh` hiện có.
  `GET /api/v3/chung-tu/doi-soat` liệt kê phiếu nhập thiếu hóa đơn, hóa đơn đầu ra
  thiếu chứng từ, chênh lệch chứng từ sau điều chỉnh, số tiền đã thu/còn nợ
  và tiền công đã duyệt chưa chi.
- KTV hoàn thành việc rồi gọi `/{id}/de-xuat-cong` với số tiền. Admin khác KTV duyệt
  bằng `/{id}/duyet-cong`, ghi số tiền và lý do; sau đó ghi chi trả một lần bằng
  `/{id}/chi-tra` với phương thức và mã chứng từ. Đây là quản lý khoản công,
  chưa phải bảng lương/khấu trừ thuế tự động.

Chưa chọn nhà cung cấp hóa đơn điện tử hoặc công thức công thợ, nên phần phát hành
trực tiếp và tính công tự động chỉ có thể triển khai khi có hợp đồng/API của nhà cung cấp
và quy tắc đã được người phụ trách tài chính chốt. Không coi dữ liệu ghi nhận trong
`hoa_don_dien_tu_v3` là bằng chứng pháp lý tự xác thực; cần đối chiếu chứng từ gốc.

## Giao diện

- **Lễ tân & Thu ngân → phiếu → Công việc sửa chữa**: giao việc theo thợ;
  **Bảo hành khách**: phân tích nguyên nhân/chi phí; **Hóa đơn · thu tiền**:
  ghi số hóa đơn điện tử đã phát hành ngoài hệ thống.
- **Bàn làm việc KTV → phiếu → Xử lý**: nhận việc, bắt đầu, ghi kết quả và đề xuất công.
- **Quản lý kho → Kiểm kê & cách ly**: đề xuất, tải ảnh/biên bản, duyệt và thực hiện;
  **Hóa đơn đầu vào**: ghi/đối chiếu hóa đơn NCC.
- **Đối soát tài chính** (Admin): xem thiếu/chênh lệch chứng từ, công nợ và
  duyệt/ghi chi trả công thợ.
