-- Chay sau BO_SUNG_QUY_TRINH_KTV_20261107.sql tren CSDL V3.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51400, N'Hay chon CSDL ung dung V3.', 1;
IF OBJECT_ID(N'dbo.v3_schema_migration', N'U') IS NULL
    THROW 51401, N'CSDL chua co schema V3.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261107_KTV')
    THROW 51402, N'Can ap dung BO_SUNG_QUY_TRINH_KTV_20261107.sql truoc.', 1;
IF EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261108_RULES')
    THROW 51403, N'Migration quy tac va kiem ke da duoc ap dung.', 1;
IF COL_LENGTH(N'dbo.phieu_sua_chua', N'ly_do_khong_serial') IS NOT NULL
    OR COL_LENGTH(N'dbo.phieu_sua_chua', N'checklist_tiep_nhan_json') IS NOT NULL
    OR COL_LENGTH(N'dbo.anh_phieu_sua_chua', N'sha256') IS NOT NULL
    OR OBJECT_ID(N'dbo.quy_tac_van_hanh_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.xac_nhan_tiep_nhan_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.duyet_bao_hanh_lap_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.kiem_ke_kho_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.dong_kiem_ke_kho_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.bien_dong_serial_kiem_ke_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.cach_ly_vat_tu_v3', N'U') IS NOT NULL
    THROW 51404, N'Schema da thay doi mot phan; can kiem tra truoc khi chay migration.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    ALTER TABLE dbo.phieu_sua_chua ADD
        ly_do_khong_serial nvarchar(250) NULL,
        checklist_tiep_nhan_json nvarchar(max) NULL,
        ap_dung_kiem_soat_tiep_nhan bit NOT NULL
            CONSTRAINT df_psc_kiem_soat_tiep_nhan DEFAULT (0);
    EXEC sys.sp_executesql N'ALTER TABLE dbo.phieu_sua_chua ADD CONSTRAINT ck_psc_checklist_tiep_nhan
        CHECK (ap_dung_kiem_soat_tiep_nhan = 0 OR
            (checklist_tiep_nhan_json IS NOT NULL AND ISJSON(checklist_tiep_nhan_json) = 1))';
    ALTER TABLE dbo.anh_phieu_sua_chua ADD
        nguoi_tai_len_id bigint NULL,
        sha256 char(64) NULL;
    EXEC sys.sp_executesql N'ALTER TABLE dbo.anh_phieu_sua_chua ADD CONSTRAINT fk_anh_nguoi_tai_len
        FOREIGN KEY (nguoi_tai_len_id) REFERENCES dbo.nhan_vien(id)';

    CREATE TABLE dbo.quy_tac_van_hanh_v3 (
        id tinyint NOT NULL CONSTRAINT pk_quy_tac_van_hanh_v3 PRIMARY KEY,
        bao_hanh_lap_duyet_tu_lan tinyint NOT NULL
            CONSTRAINT ck_quy_tac_bh_lap CHECK (bao_hanh_lap_duyet_tu_lan BETWEEN 2 AND 10),
        anh_tiep_nhan_toi_thieu tinyint NOT NULL
            CONSTRAINT ck_quy_tac_anh CHECK (anh_tiep_nhan_toi_thieu BETWEEN 1 AND 5),
        quy_tac_tra_cong nvarchar(500) NOT NULL,
        chinh_sach_serial nvarchar(500) NOT NULL,
        loai_hinh_kinh_doanh nvarchar(100) NULL,
        phuong_phap_thue nvarchar(100) NULL,
        don_vi_hoa_don_dien_tu nvarchar(150) NULL,
        nguoi_cap_nhat_id bigint NULL REFERENCES dbo.nhan_vien(id),
        cap_nhat_luc datetime2(3) NOT NULL CONSTRAINT df_quy_tac_cap_nhat DEFAULT SYSUTCDATETIME(),
        CONSTRAINT ck_quy_tac_mot_dong CHECK (id = 1)
    );
    INSERT dbo.quy_tac_van_hanh_v3
        (id, bao_hanh_lap_duyet_tu_lan, anh_tiep_nhan_toi_thieu, quy_tac_tra_cong, chinh_sach_serial)
    VALUES (1, 2, 2,
        N'Ghi cong theo hang muc da hoan thanh; quan ly duyet truoc khi chi tra. Cong thuc chi tra can duoc ke toan chot.',
        N'Thiet bi co serial phai ghi serial; neu khong co, ghi ly do va dung ma thiet bi noi bo. Linh kien theo doi serial phai nhap/xuat dung serial.');

    CREATE TABLE dbo.xac_nhan_tiep_nhan_v3 (
        phieu_id bigint NOT NULL CONSTRAINT pk_xac_nhan_tiep_nhan_v3 PRIMARY KEY
            REFERENCES dbo.phieu_sua_chua(id),
        nguoi_xac_nhan nvarchar(150) NOT NULL,
        anh_bien_nhan_id bigint NOT NULL REFERENCES dbo.anh_phieu_sua_chua(id),
        nhan_vien_ghi_nhan_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        xac_nhan_luc datetime2(3) NOT NULL CONSTRAINT df_xntn_luc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT ck_xntn_nguoi CHECK (LEN(LTRIM(RTRIM(nguoi_xac_nhan))) > 0)
    );

    CREATE TABLE dbo.duyet_bao_hanh_lap_v3 (
        phieu_bao_hanh_id bigint NOT NULL CONSTRAINT pk_duyet_bao_hanh_lap_v3 PRIMARY KEY
            REFERENCES dbo.phieu_bao_hanh(id),
        lan_thu int NOT NULL CONSTRAINT ck_bhl_lan CHECK (lan_thu > 0),
        trang_thai varchar(20) NOT NULL
            CONSTRAINT ck_bhl_trang_thai CHECK (trang_thai IN ('KHONG_CAN_DUYET','CHO_DUYET','DA_DUYET','TU_CHOI')),
        nguoi_de_xuat_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        nguoi_duyet_id bigint NULL REFERENCES dbo.nhan_vien(id),
        nguyen_nhan_so_bo nvarchar(1000) NULL,
        ly_do_quyet_dinh nvarchar(1000) NULL,
        ngay_duyet datetime2(3) NULL,
        CONSTRAINT ck_bhl_duyet_du CHECK (
            (trang_thai IN ('KHONG_CAN_DUYET','CHO_DUYET') AND nguoi_duyet_id IS NULL AND ngay_duyet IS NULL)
            OR (trang_thai IN ('DA_DUYET','TU_CHOI') AND nguoi_duyet_id IS NOT NULL
                AND ngay_duyet IS NOT NULL AND ly_do_quyet_dinh IS NOT NULL
                AND nguoi_duyet_id <> nguoi_de_xuat_id))
    );

    CREATE TABLE dbo.kiem_ke_kho_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_kiem_ke_kho_v3 PRIMARY KEY,
        kho_id bigint NOT NULL REFERENCES dbo.kho(id),
        trang_thai varchar(15) NOT NULL
            CONSTRAINT ck_kkk_trang_thai CHECK (trang_thai IN ('CHO_DUYET','DA_DUYET','TU_CHOI')),
        nguoi_kiem_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        nguoi_duyet_id bigint NULL REFERENCES dbo.nhan_vien(id),
        tao_luc datetime2(3) NOT NULL CONSTRAINT df_kkk_tao DEFAULT SYSUTCDATETIME(),
        duyet_luc datetime2(3) NULL,
        ghi_chu nvarchar(500) NULL,
        ly_do_quyet_dinh nvarchar(500) NULL,
        CONSTRAINT ck_kkk_duyet_du CHECK (
            (trang_thai = 'CHO_DUYET' AND nguoi_duyet_id IS NULL AND duyet_luc IS NULL)
            OR (trang_thai IN ('DA_DUYET','TU_CHOI') AND nguoi_duyet_id IS NOT NULL
                AND nguoi_duyet_id <> nguoi_kiem_id AND duyet_luc IS NOT NULL
                AND ly_do_quyet_dinh IS NOT NULL))
    );
    CREATE INDEX ix_kkk_kho_trang_thai ON dbo.kiem_ke_kho_v3(kho_id, trang_thai);
    CREATE UNIQUE INDEX uq_kkk_mot_phieu_cho_duyet ON dbo.kiem_ke_kho_v3(kho_id)
        WHERE trang_thai = 'CHO_DUYET';

    CREATE TABLE dbo.dong_kiem_ke_kho_v3 (
        kiem_ke_id bigint NOT NULL REFERENCES dbo.kiem_ke_kho_v3(id),
        linh_kien_id bigint NOT NULL REFERENCES dbo.linh_kien(id),
        so_luong_so_sach int NOT NULL,
        so_luong_thuc_te int NOT NULL,
        serial_so_sach_json nvarchar(max) NULL,
        serial_thuc_te_json nvarchar(max) NULL,
        CONSTRAINT pk_dong_kiem_ke_kho_v3 PRIMARY KEY (kiem_ke_id, linh_kien_id),
        CONSTRAINT ck_dkk_so_luong CHECK (so_luong_so_sach >= 0 AND so_luong_thuc_te >= 0),
        CONSTRAINT ck_dkk_serial_json CHECK (
            (serial_so_sach_json IS NULL OR ISJSON(serial_so_sach_json) = 1)
            AND (serial_thuc_te_json IS NULL OR ISJSON(serial_thuc_te_json) = 1))
    );

    CREATE TABLE dbo.bien_dong_serial_kiem_ke_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_bd_serial_kk_v3 PRIMARY KEY,
        kiem_ke_id bigint NOT NULL REFERENCES dbo.kiem_ke_kho_v3(id),
        linh_kien_serial_id bigint NOT NULL REFERENCES dbo.linh_kien_serial(id),
        trang_thai_cu nvarchar(30) NOT NULL,
        trang_thai_moi nvarchar(30) NOT NULL,
        thoi_gian datetime2(3) NOT NULL CONSTRAINT df_bd_serial_kk_luc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT uq_bd_serial_kk UNIQUE (kiem_ke_id, linh_kien_serial_id)
    );

    CREATE TABLE dbo.cach_ly_vat_tu_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_cach_ly_vat_tu_v3 PRIMARY KEY,
        kho_id bigint NOT NULL REFERENCES dbo.kho(id),
        linh_kien_id bigint NOT NULL REFERENCES dbo.linh_kien(id),
        linh_kien_serial_id bigint NULL REFERENCES dbo.linh_kien_serial(id),
        so_luong int NOT NULL,
        trang_thai varchar(15) NOT NULL CONSTRAINT ck_clvt_trang_thai
            CHECK (trang_thai IN ('CACH_LY','NHAP_LAI','THANH_LY')),
        ly_do nvarchar(500) NOT NULL,
        nguoi_tao_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        tao_luc datetime2(3) NOT NULL CONSTRAINT df_clvt_tao DEFAULT SYSUTCDATETIME(),
        nguoi_xu_ly_id bigint NULL REFERENCES dbo.nhan_vien(id),
        xu_ly_luc datetime2(3) NULL,
        ly_do_xu_ly nvarchar(500) NULL,
        CONSTRAINT ck_clvt_so_luong CHECK
            (so_luong > 0 AND (linh_kien_serial_id IS NULL OR so_luong = 1)),
        CONSTRAINT ck_clvt_xu_ly_du CHECK (
            (trang_thai = 'CACH_LY' AND nguoi_xu_ly_id IS NULL AND xu_ly_luc IS NULL)
            OR (trang_thai IN ('NHAP_LAI','THANH_LY') AND nguoi_xu_ly_id IS NOT NULL
                AND nguoi_xu_ly_id <> nguoi_tao_id AND xu_ly_luc IS NOT NULL
                AND ly_do_xu_ly IS NOT NULL))
    );
    CREATE UNIQUE INDEX uq_clvt_serial_dang_cach_ly ON dbo.cach_ly_vat_tu_v3(linh_kien_serial_id)
        WHERE trang_thai = 'CACH_LY' AND linh_kien_serial_id IS NOT NULL;
    CREATE INDEX ix_clvt_kho_trang_thai ON dbo.cach_ly_vat_tu_v3(kho_id, trang_thai);

    ALTER TABLE dbo.linh_kien_serial DROP CONSTRAINT ck_trang_thai_lks;
    ALTER TABLE dbo.linh_kien_serial WITH CHECK ADD CONSTRAINT ck_trang_thai_lks
        CHECK (trang_thai IN ('TRONG_KHO','DA_XUAT','BAO_HANH','THANH_LY','THIEU_KIEM_KE','CACH_LY'));

    INSERT dbo.v3_schema_migration(version) VALUES ('V3_20261108_RULES');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
