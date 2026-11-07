# API bổ sung cho quy trình V3

## Cập nhật CSDL

- CSDL V3 **đã tồn tại**: chọn đúng `QLSCTBDT_V3` trong SSMS và chạy một lần
  `database_v3/BO_SUNG_API_20261106.sql`. Không chạy lại file tạo CSDL mới.
- CSDL tạo mới sau thay đổi này: `database_v3/TAO_MOI_QLSCTBDT_V3.sql` đã gồm phần bổ sung.
- Bản nâng cấp từ schema cũ: `database_v3/NANG_CAP_QLSCTBDT_V3.sql` đã gồm phần bổ sung.
- Dừng backend khi chạy migration, sao lưu CSDL trước, sau đó khởi động lại. API mới cần migration `V3_20261106_API`.
- Kiểm tra sau khi chạy: `SELECT version FROM dbo.v3_schema_migration WHERE version = 'V3_20261106_API';`
  và `SELECT OBJECT_ID(N'dbo.lich_hen_v3', N'U');` trên đúng CSDL mà backend kết nối.
  Kết quả phải có phiên bản và `OBJECT_ID` khác `NULL`. Backend cũng tự kiểm tra khi khởi động.

## Endpoint

| Vai trò | Endpoint | Công dụng |
|---|---|---|
| Admin/Lễ tân | `GET /api/khach-hang?soDienThoai=...` | Tìm đúng hồ sơ để cấp mã liên kết. |
| Admin/Lễ tân | `POST /api/khach-hang/{id}/ma-lien-ket` | Cấp mã một lần, hiệu lực 15 phút, sau khi xác minh khách tại quầy. Mã chỉ xuất hiện trong phản hồi này. |
| Công khai | `POST /api/auth/register` | Khi số điện thoại đã có hồ sơ, phải gửi thêm `maLienKet`. Hồ sơ mới không cần mã. |
| Khách | `GET /api/khach-hang/me/phieu-sua-chua`, `GET /api/khach-hang/me/phieu-sua-chua/{id}`, `GET /api/khach-hang/me/bao-hanh` | Chỉ trả dữ liệu thuộc tài khoản đang đăng nhập; không trả mật khẩu máy hay thông tin nội bộ. |
| Khách | `POST /api/khach-hang/me/bao-gia/{id}/phan-hoi` | Gửi `{ "quyetDinh": "DA_DUYET" hoặc "TU_CHOI", "noiDung": "..." }` cho phiên bản báo giá mới nhất đang chờ. |
| Khách | `POST /api/lich-hen`, `GET /api/lich-hen/me`, `POST /api/lich-hen/{id}/huy` | Đặt lịch mang máy đến, xem và hủy lịch của mình. |
| Admin/Lễ tân | `GET /api/lich-hen`, `PATCH /api/lich-hen/{id}/trang-thai` | Xác nhận/từ chối lịch, liên kết phiếu sửa khi tiếp nhận. |
| Admin/Lễ tân | `PATCH /api/phieu-sua-chua/{id}/phan-cong`, `PATCH /api/phieu-sua-chua/{id}/hen-tra` | Điều phối lại kỹ thuật viên và ngày hẹn trả. |
| Admin/Lễ tân/KTV phụ trách | `GET/POST /api/phieu-sua-chua/{id}/anh`, `GET /api/phieu-sua-chua/{id}/anh/{anhId}` | Xem/tải ảnh JPEG, PNG hoặc WebP tối đa 5 MB. Lễ tân/admin có thể xóa trước bàn giao. |
| Admin/Thủ kho | `POST /api/v3/bao-hanh/nha-cung-cap/{id}/gui`, `POST /api/v3/bao-hanh/nha-cung-cap/{id}/xu-ly-kho` | Gửi hàng từ `KHO` hoặc `KHACH`, sau khi nhận lại thì nhập lại, liên kết phiếu nhập hàng thay thế, hoặc không nhập. |

Mẫu gửi NCC: `{ "nguon": "KHO", "khoId": 1 }` hoặc
`{ "nguon": "KHACH" }`. Với linh kiện theo dõi serial phải chọn serial khi tạo hồ sơ. Gửi từ kho trừ tồn và đổi serial sang `BAO_HANH`; hàng khách đem đến không trừ tồn.

Mẫu xử lý kho: `{ "loai": "NHAP_LAI", "khoId": 1 }`,
`{ "loai": "NHAP_THAY_THE", "phieuNhapId": 12 }`, hoặc
`{ "loai": "KHONG_NHAP", "ghiChu": "Không đủ điều kiện" }`.
`NHAP_THAY_THE` yêu cầu phiếu nhập đã xác nhận từ cùng NCC và linh kiện; API không nhập lần hai. Hồ sơ NCC tạo trước migration có `nguon_hang=CU` và phải đối soát thủ công trước khi thay đổi tồn.

Ảnh lưu trong `RepairSystem/App_Data/repair-photos`; khi triển khai cần sao lưu thư mục này cùng CSDL. Backend không tự gửi SMS/email hoặc tự tạo phiếu sửa từ lịch hẹn. Lễ tân cấp mã liên kết tại quầy và liên kết lịch với phiếu sau khi tiếp nhận.
