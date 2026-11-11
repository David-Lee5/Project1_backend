# Đổi tên bảng cho đồng nghiệp đã có CSDL V3

Hướng dẫn này dành cho máy **đã có CSDL `QLSCTBDT_V3`**, nhưng các bảng nghiệp vụ còn hậu tố `_v3` (ví dụ `lich_hen_v3`). Mục tiêu là giữ nguyên dữ liệu, đổi tên các bảng đó giống CSDL hiện tại của dự án, rồi chạy backend mới. **Không đổi tên CSDL `QLSCTBDT_V3`.**

File cần dùng: [database/DOI_TEN_BANG_BO_V3_20261111.sql](database/DOI_TEN_BANG_BO_V3_20261111.sql). File này đổi tên tối đa 23 bảng tại chỗ, cập nhật các procedure, view và trigger phụ thuộc. Nó không tạo những bảng còn thiếu.

Hãy lấy **cùng một bản cập nhật repo** chứa cả hướng dẫn này, file SQL trên và mã backend mới trước khi thực hiện.

## 1. Kiểm tra CSDL trước khi làm

Trong SSMS, kết nối đúng SQL Server của bạn, chọn CSDL `QLSCTBDT_V3`, rồi chạy:

```sql
SELECT DB_NAME() AS ten_csdl;
SELECT name AS bang_con_hau_to_v3
FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo') AND name LIKE N'%[_]v3'
ORDER BY name;
SELECT version, applied_at
FROM dbo.v3_schema_migration
ORDER BY applied_at;
```

`ten_csdl` phải là `QLSCTBDT_V3`. Trên bản V3 cùng cấu trúc với CSDL đã được đổi tên của dự án, truy vấn thứ hai liệt kê **23 bảng** và lịch sử có `V3_20261106_API`, `V3_20261107_KTV`, `V3_20261108_RULES`, `V3_20261108_WORKFLOW`. Nếu thiếu migration hoặc thiếu bảng, cần bổ sung đúng migration còn thiếu trước; file đổi tên không bổ sung schema. Đừng chạy file `TAO_MOI_QLSCTBDT_V3.sql` hoặc `CAI_DAT_QLSCTBDT_V3_MOT_FILE.sql` lên CSDL đã có dữ liệu.

Ghi lại số dòng của vài bảng để đối chiếu sau khi đổi tên:

```sql
SELECT COUNT_BIG(*) AS lich_hen_truoc FROM dbo.lich_hen_v3;
SELECT COUNT_BIG(*) AS chi_tiet_hoa_don_truoc FROM dbo.chi_tiet_hoa_don_v3;
SELECT COUNT_BIG(*) AS yeu_cau_linh_kien_truoc FROM dbo.yeu_cau_linh_kien_v3;
```

Nếu một trong ba bảng đã có tên không kèm `_v3`, kiểm tra tên hiện tại và bỏ qua truy vấn đếm tương ứng; migration có thể xử lý CSDL đã đổi tên một phần khi không có xung đột.

## 2. Sao lưu và chạy migration

Tắt backend đang kết nối tới CSDL. Trong SSMS, nhấp phải `QLSCTBDT_V3` → **Tasks → Back Up...**, tạo bản sao lưu và xác minh đường dẫn file `.bak` trước khi tiếp tục.

Chọn lại đúng CSDL `QLSCTBDT_V3` trong cửa sổ query, mở [file đổi tên bảng](database/DOI_TEN_BANG_BO_V3_20261111.sql) và chạy **toàn bộ** file. Hoặc chạy từ thư mục gốc repo `Project1_backend` bằng PowerShell (đổi `.` thành tên instance của bạn nếu cần):

```powershell
sqlcmd -S "." -E -C -b -f 65001 -d "QLSCTBDT_V3" -i "database\DOI_TEN_BANG_BO_V3_20261111.sql"
```

Thông báo thành công là `Renamed V3 tables and updated dependent modules.`. Script chạy trong một transaction và rollback khi có lỗi. Nếu thấy lỗi `51703` (trùng tên cũ/mới), `51704` (thiếu bảng bắt buộc), `51706` (module SQL khác đang dùng tên cũ) hoặc `51709` (dấu migration không khớp bảng), dừng lại và đối chiếu schema; không tự chạy lại script tạo CSDL.

## 3. Kiểm tra kết quả

Trong **cùng CSDL**, chạy:

```sql
SELECT COUNT(*) AS so_bang_con_hau_to_v3
FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo') AND name LIKE N'%[_]v3';

SELECT COUNT(*) AS so_bang_ten_moi
FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo')
  AND name IN (
      N'yeu_cau_linh_kien', N'chi_tiet_yeu_cau_linh_kien', N'tra_linh_kien_kho',
      N'chi_tiet_hoa_don', N'bi_mat_thiet_bi', N'ma_lien_ket_khach_hang',
      N'lich_hen', N'xu_ly_bao_hanh_ncc', N'quy_tac_van_hanh',
      N'xac_nhan_tiep_nhan', N'duyet_bao_hanh_lap', N'kiem_ke_kho',
      N'dong_kiem_ke_kho', N'bien_dong_serial_kiem_ke', N'cach_ly_vat_tu',
      N'cong_viec_sua_chua', N'cong_viec_vat_tu', N'bao_hanh_phan_tich',
      N'ho_so_thanh_ly', N'tai_lieu_thanh_ly', N'hoa_don_dau_vao',
      N'hoa_don_dien_tu', N'dieu_chinh_hoa_don');

SELECT version, applied_at
FROM dbo.v3_schema_migration
WHERE version = 'V3_20261111_TABLE_RENAME';

SELECT COUNT_BIG(*) AS lich_hen_sau FROM dbo.lich_hen;
SELECT COUNT_BIG(*) AS chi_tiet_hoa_don_sau FROM dbo.chi_tiet_hoa_don;
SELECT COUNT_BIG(*) AS yeu_cau_linh_kien_sau FROM dbo.yeu_cau_linh_kien;
```

Kết quả mong đợi với CSDL có đủ schema: `so_bang_con_hau_to_v3 = 0`, `so_bang_ten_moi = 23`, có một dòng `V3_20261111_TABLE_RENAME`, và số dòng ba bảng bằng trước khi đổi tên. Nếu ban đầu thiếu bảng tùy chọn, `so_bang_ten_moi` sẽ nhỏ hơn 23; cần bổ sung migration tạo bảng đó để có cùng schema.

Sau đó dùng mã backend mới nhất và chạy `dotnet build RepairSystem.sln --no-restore -m:1`, rồi khởi động lại API. Chạy lại file đổi tên sau khi đã hoàn tất sẽ chỉ báo `Table rename migration has already been applied.`
