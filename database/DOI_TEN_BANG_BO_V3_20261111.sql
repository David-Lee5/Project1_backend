-- Run once on a backup-tested copy of the existing QLSCTBDT_V3 database, then on the target database.
-- Renames table objects in place, preserving rows and foreign keys. SQL modules are updated below.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51700, N'Choose the application database, not a system database.', 1;
IF OBJECT_ID(N'dbo.v3_schema_migration', N'U') IS NULL
    THROW 51701, N'The V3 schema migration table is missing.', 1;
IF @@TRANCOUNT <> 0
    THROW 51702, N'Run this migration outside an existing transaction.', 1;
IF EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261111_TABLE_RENAME')
BEGIN
    IF EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID(N'dbo') AND name LIKE N'%[_]v3')
        THROW 51709, N'The rename marker exists but an old table name remains.', 1;
    PRINT N'Table rename migration has already been applied.';
    RETURN;
END;

DECLARE @TableRenames TABLE (
    id int IDENTITY(1,1) PRIMARY KEY,
    old_name sysname NOT NULL,
    new_name sysname NOT NULL,
    required bit NOT NULL
);
INSERT @TableRenames (old_name, new_name, required) VALUES
    (N'yeu_cau_linh_kien_v3', N'yeu_cau_linh_kien', 1),
    (N'chi_tiet_yeu_cau_linh_kien_v3', N'chi_tiet_yeu_cau_linh_kien', 1),
    (N'tra_linh_kien_kho_v3', N'tra_linh_kien_kho', 1),
    (N'chi_tiet_hoa_don_v3', N'chi_tiet_hoa_don', 1),
    (N'bi_mat_thiet_bi_v3', N'bi_mat_thiet_bi', 1),
    (N'ma_lien_ket_khach_hang_v3', N'ma_lien_ket_khach_hang', 1),
    (N'lich_hen_v3', N'lich_hen', 1),
    (N'xu_ly_bao_hanh_ncc_v3', N'xu_ly_bao_hanh_ncc', 1),
    (N'quy_tac_van_hanh_v3', N'quy_tac_van_hanh', 0),
    (N'xac_nhan_tiep_nhan_v3', N'xac_nhan_tiep_nhan', 0),
    (N'duyet_bao_hanh_lap_v3', N'duyet_bao_hanh_lap', 0),
    (N'kiem_ke_kho_v3', N'kiem_ke_kho', 0),
    (N'dong_kiem_ke_kho_v3', N'dong_kiem_ke_kho', 0),
    (N'bien_dong_serial_kiem_ke_v3', N'bien_dong_serial_kiem_ke', 0),
    (N'cach_ly_vat_tu_v3', N'cach_ly_vat_tu', 0),
    (N'cong_viec_sua_chua_v3', N'cong_viec_sua_chua', 0),
    (N'cong_viec_vat_tu_v3', N'cong_viec_vat_tu', 0),
    (N'bao_hanh_phan_tich_v3', N'bao_hanh_phan_tich', 0),
    (N'ho_so_thanh_ly_v3', N'ho_so_thanh_ly', 0),
    (N'tai_lieu_thanh_ly_v3', N'tai_lieu_thanh_ly', 0),
    (N'hoa_don_dau_vao_v3', N'hoa_don_dau_vao', 0),
    (N'hoa_don_dien_tu_v3', N'hoa_don_dien_tu', 0),
    (N'dieu_chinh_hoa_don_v3', N'dieu_chinh_hoa_don', 0);

IF EXISTS (
    SELECT 1 FROM @TableRenames
    WHERE OBJECT_ID(N'dbo.' + QUOTENAME(old_name), N'U') IS NOT NULL
      AND OBJECT_ID(N'dbo.' + QUOTENAME(new_name), N'U') IS NOT NULL
)
    THROW 51703, N'Both old and new table names exist. Resolve the collision first.', 1;
IF EXISTS (
    SELECT 1 FROM @TableRenames
    WHERE required = 1
      AND OBJECT_ID(N'dbo.' + QUOTENAME(old_name), N'U') IS NULL
      AND OBJECT_ID(N'dbo.' + QUOTENAME(new_name), N'U') IS NULL
)
    THROW 51704, N'A required V3 table is missing.', 1;
IF EXISTS (
    SELECT 1 FROM @TableRenames r
    JOIN sys.sql_expression_dependencies d
      ON d.referenced_id = OBJECT_ID(N'dbo.' + QUOTENAME(r.old_name), N'U')
    JOIN sys.sql_modules m ON m.object_id = d.referencing_id
    WHERE m.is_schema_bound = 1
)
    THROW 51705, N'A schema-bound module depends on a table to rename.', 1;
IF EXISTS (
    SELECT 1 FROM sys.sql_modules m
    JOIN sys.objects o ON o.object_id = m.object_id
    CROSS JOIN @TableRenames r
    WHERE CHARINDEX(r.old_name, m.definition) > 0
      AND (SCHEMA_NAME(o.schema_id) <> N'dbo' OR o.name NOT IN (N'trg_v3_tra_linh_kien_khong_sua', N'usp_v3_tra_linh_kien', N'v_phieu_chi_phi', N'trg_v3_chi_tiet_hoa_don_khong_sua', N'usp_v3_lap_hoa_don'))
)
    THROW 51706, N'An unexpected SQL module references a V3 table. Review it before renaming.', 1;

DECLARE @DryRun bit = 0;
BEGIN TRY
    BEGIN TRANSACTION;
    DECLARE @i int = 1, @count int = (SELECT COUNT(*) FROM @TableRenames);
    DECLARE @old_name sysname, @new_name sysname, @old_object nvarchar(517);
    WHILE @i <= @count
    BEGIN
        SELECT @old_name = old_name, @new_name = new_name
        FROM @TableRenames WHERE id = @i;
        SET @old_object = N'dbo.' + QUOTENAME(@old_name);
        IF OBJECT_ID(@old_object, N'U') IS NOT NULL
            EXEC sys.sp_rename @objname = @old_object, @newname = @new_name, @objtype = N'OBJECT';
        SET @i += 1;
    END;

    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_tra_linh_kien_khong_sua ON dbo.tra_linh_kien_kho
AFTER UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    THROW 51050, N''Không sửa hoặc xóa phiếu trả đã ghi; cần xử lý bằng chứng từ điều chỉnh.'', 1;
END;';

    EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.usp_v3_tra_linh_kien
    @chi_tiet_xuat_kho_id bigint, @so_luong int, @thu_kho_id bigint, @ly_do nvarchar(500), @tra_id bigint OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @own bit=CASE WHEN @@TRANCOUNT=0 THEN 1 ELSE 0 END;
    DECLARE @phieu bigint,@kho bigint,@lk bigint,@xuat int,@serial bigint,@tra int,@sau int,@status varchar(15);
    BEGIN TRY
        IF @so_luong IS NULL OR @so_luong<=0 OR NULLIF(LTRIM(RTRIM(@ly_do)),N'''') IS NULL
            THROW 51051, N''Cần số lượng trả hợp lệ và lý do.'', 1;
        IF @own=1 BEGIN TRANSACTION;
        -- Cùng thứ tự khóa phiếu -> dòng xuất như luồng xuất/lập hóa đơn.
        SELECT @phieu=phieu_id FROM dbo.chi_tiet_xuat_kho WHERE id=@chi_tiet_xuat_kho_id;
        IF @phieu IS NULL THROW 51052, N''Không tìm thấy dòng xuất linh kiện.'', 1;
        SELECT @status=trang_thai FROM dbo.phieu_sua_chua WITH (UPDLOCK,HOLDLOCK) WHERE id=@phieu;
        IF @status NOT IN (''DANG_SUA'',''CHO_LINH_KIEN'') OR EXISTS
            (SELECT 1 FROM dbo.hoa_don WHERE phieu_id=@phieu AND trang_thai<>''HUY'')
            THROW 51053, N''Chỉ trả linh kiện chưa sử dụng khi phiếu đang sửa và chưa lập hóa đơn.'', 1;
        SELECT @kho=kho_id,@lk=linh_kien_id,@xuat=so_luong_xuat,@serial=linh_kien_serial_id
          FROM dbo.chi_tiet_xuat_kho WITH (UPDLOCK,HOLDLOCK) WHERE id=@chi_tiet_xuat_kho_id;
        SELECT @tra=COALESCE(SUM(so_luong),0) FROM dbo.tra_linh_kien_kho WHERE chi_tiet_xuat_kho_id=@chi_tiet_xuat_kho_id;
        IF @so_luong>@xuat-@tra THROW 51054, N''Số lượng trả vượt số đã xuất còn lại.'', 1;
        IF NOT EXISTS(SELECT 1 FROM dbo.thu_kho WHERE nhan_vien_id=@thu_kho_id)
            THROW 51055, N''Người thực hiện không phải thủ kho.'', 1;
        IF @serial IS NOT NULL
        BEGIN
            IF @so_luong<>1 THROW 51056, N''Mỗi dòng serial chỉ có thể trả một chiếc.'', 1;
            UPDATE dbo.linh_kien_serial SET trang_thai=''TRONG_KHO'',kho_id=@kho,chi_tiet_xuat_kho_id=NULL,updated_at=SYSUTCDATETIME()
              WHERE id=@serial AND linh_kien_id=@lk AND trang_thai=''DA_XUAT'' AND chi_tiet_xuat_kho_id=@chi_tiet_xuat_kho_id;
            IF @@ROWCOUNT<>1 THROW 51057, N''Serial không ở trạng thái đã xuất cho dòng này.'', 1;
        END;
        INSERT dbo.tra_linh_kien_kho(chi_tiet_xuat_kho_id,so_luong,nguoi_thuc_hien_id,ly_do)
        VALUES(@chi_tiet_xuat_kho_id,@so_luong,@thu_kho_id,@ly_do);
        SET @tra_id=SCOPE_IDENTITY();
        UPDATE dbo.ton_kho SET @sau=so_luong_ton=so_luong_ton+@so_luong,ngay_cap_nhat=SYSUTCDATETIME()
          WHERE kho_id=@kho AND linh_kien_id=@lk;
        IF @@ROWCOUNT<>1 THROW 51058, N''Không tìm thấy dòng tồn kho gốc.'', 1;
        INSERT dbo.bien_dong_kho(kho_id,linh_kien_id,loai,so_luong_thay_doi,so_luong_sau,nguoi_thuc_hien_id,ly_do,tra_linh_kien_id)
        VALUES(@kho,@lk,''DIEU_CHINH'',@so_luong,@sau,@thu_kho_id,@ly_do,@tra_id);
        IF @own=1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own=1 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';

    EXEC sys.sp_executesql N'CREATE OR ALTER VIEW dbo.v_phieu_chi_phi AS
SELECT p.id AS phieu_id,
       COALESCE((SELECT SUM(d.thanh_tien) FROM dbo.chi_tiet_dich_vu d WHERE d.phieu_id=p.id),0) AS tien_dich_vu,
       COALESCE((SELECT SUM(CONVERT(decimal(19,2),x.so_luong_xuat-COALESCE(t.da_tra,0))*x.don_gia_xuat)
           FROM dbo.chi_tiet_xuat_kho x OUTER APPLY
             (SELECT SUM(r.so_luong) AS da_tra FROM dbo.tra_linh_kien_kho r WHERE r.chi_tiet_xuat_kho_id=x.id) t
           WHERE x.phieu_id=p.id),0) AS tien_linh_kien,
       COALESCE((SELECT SUM(d.thanh_tien) FROM dbo.chi_tiet_dich_vu d WHERE d.phieu_id=p.id),0)+
       COALESCE((SELECT SUM(CONVERT(decimal(19,2),x.so_luong_xuat-COALESCE(t.da_tra,0))*x.don_gia_xuat)
           FROM dbo.chi_tiet_xuat_kho x OUTER APPLY
             (SELECT SUM(r.so_luong) AS da_tra FROM dbo.tra_linh_kien_kho r WHERE r.chi_tiet_xuat_kho_id=x.id) t
           WHERE x.phieu_id=p.id),0) AS tong_tam_tinh
FROM dbo.phieu_sua_chua p;';

    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_chi_tiet_hoa_don_khong_sua ON dbo.chi_tiet_hoa_don
AFTER UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    THROW 51026, N''Không sửa hoặc xóa chi tiết đã chốt trên hóa đơn.'', 1;
END;';

    EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.usp_v3_lap_hoa_don
    @phieu_id bigint, @le_tan_id bigint, @giam_gia decimal(14,2)=0, @hoa_don_id bigint OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @own bit=CASE WHEN @@TRANCOUNT=0 THEN 1 ELSE 0 END;
    DECLARE @lock bigint, @net decimal(19,2), @tong decimal(14,2), @method varchar(15);
    BEGIN TRY
        IF @own=1 BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.phieu_sua_chua WITH (UPDLOCK,HOLDLOCK) WHERE id=@phieu_id)
            THROW 51033, N''Không tìm thấy phiếu sửa chữa.'', 1;
        -- Khóa các dòng và khoảng khóa theo phiếu để tổng và chi tiết được chốt cùng lúc.
        SELECT @lock=COUNT_BIG(*) FROM dbo.chi_tiet_dich_vu WITH (UPDLOCK,HOLDLOCK) WHERE phieu_id=@phieu_id;
        SELECT @lock=COUNT_BIG(*) FROM dbo.chi_tiet_xuat_kho WITH (UPDLOCK,HOLDLOCK) WHERE phieu_id=@phieu_id;
        EXEC dbo.usp_lap_hoa_don @phieu_id,@le_tan_id,@giam_gia,@hoa_don_id OUTPUT;
        INSERT dbo.chi_tiet_hoa_don(hoa_don_id,phieu_id,loai,dich_vu_id,ten_hang_muc,so_luong,don_gia)
        SELECT @hoa_don_id,c.phieu_id,''DICH_VU'',c.dich_vu_id,d.ten_dich_vu,c.so_luong,c.don_gia_thuc_te
          FROM dbo.chi_tiet_dich_vu c JOIN dbo.dich_vu d ON d.id=c.dich_vu_id WHERE c.phieu_id=@phieu_id;
        INSERT dbo.chi_tiet_hoa_don(hoa_don_id,phieu_id,loai,chi_tiet_xuat_kho_id,ten_hang_muc,so_luong,don_gia)
        SELECT @hoa_don_id,c.phieu_id,''LINH_KIEN'',c.id,l.ten_linh_kien,c.so_luong_xuat-COALESCE(t.da_tra,0),c.don_gia_xuat
          FROM dbo.chi_tiet_xuat_kho c JOIN dbo.linh_kien l ON l.id=c.linh_kien_id
          OUTER APPLY (SELECT SUM(r.so_luong) AS da_tra FROM dbo.tra_linh_kien_kho r WHERE r.chi_tiet_xuat_kho_id=c.id) t
          WHERE c.phieu_id=@phieu_id AND c.so_luong_xuat>COALESCE(t.da_tra,0);
        SELECT @tong=tong_thanh_toan FROM dbo.hoa_don WHERE id=@hoa_don_id;
        SELECT @net=COALESCE(SUM(CASE WHEN loai=''THU'' THEN so_tien ELSE -so_tien END),0)
          FROM dbo.giao_dich_thanh_toan WHERE phieu_id=@phieu_id;
        IF @net>@tong THROW 51034, N''Số tiền đã thu vượt hóa đơn; cần xử lý hoàn tiền trước khi lập.'', 1;
        IF @net=@tong
        BEGIN
            SELECT TOP(1) @method=phuong_thuc FROM dbo.giao_dich_thanh_toan
              WHERE phieu_id=@phieu_id AND loai=''THU'' ORDER BY thoi_gian DESC,id DESC;
            UPDATE dbo.hoa_don SET trang_thai=''DA_THANH_TOAN'',ngay_thanh_toan=SYSUTCDATETIME(),
                phuong_thuc_tt=@method,updated_at=SYSUTCDATETIME() WHERE id=@hoa_don_id;
        END;
        IF @own=1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own=1 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';

    IF EXISTS (
        SELECT 1 FROM @TableRenames
        WHERE OBJECT_ID(N'dbo.' + QUOTENAME(old_name), N'U') IS NOT NULL
    )
        THROW 51707, N'At least one old table name remains.', 1;
    IF EXISTS (
        SELECT 1 FROM sys.sql_modules m CROSS JOIN @TableRenames r
        WHERE CHARINDEX(r.old_name, m.definition) > 0
    )
        THROW 51708, N'At least one SQL module still references an old table name.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261111_TABLE_RENAME')
        INSERT dbo.v3_schema_migration(version) VALUES ('V3_20261111_TABLE_RENAME');
    IF @DryRun = 1
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT N'Dry run succeeded; all changes were rolled back.';
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;
        PRINT N'Renamed V3 tables and updated dependent modules.';
    END;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
