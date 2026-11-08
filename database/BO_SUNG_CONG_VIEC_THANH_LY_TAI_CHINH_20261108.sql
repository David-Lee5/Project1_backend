-- Chay sau BO_SUNG_QUY_TAC_KIEM_KE_20261108.sql tren dung CSDL V3.
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
    THROW 51500, N'Hay chon CSDL ung dung V3.', 1;
IF OBJECT_ID(N'dbo.v3_schema_migration', N'U') IS NULL
    OR NOT EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261108_RULES')
    THROW 51501, N'Can ap dung migration V3_20261108_RULES truoc.', 1;
IF EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261108_WORKFLOW')
    THROW 51502, N'Migration V3_20261108_WORKFLOW da duoc ap dung.', 1;
IF OBJECT_ID(N'dbo.cong_viec_sua_chua_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.bao_hanh_phan_tich_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.ho_so_thanh_ly_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.tai_lieu_thanh_ly_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.hoa_don_dau_vao_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.hoa_don_dien_tu_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.dieu_chinh_hoa_don_v3', N'U') IS NOT NULL
    OR OBJECT_ID(N'dbo.cong_viec_vat_tu_v3', N'U') IS NOT NULL
    THROW 51503, N'Schema da thay doi mot phan; can kiem tra truoc khi chay.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.cong_viec_sua_chua_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_cvs_v3 PRIMARY KEY,
        phieu_id bigint NOT NULL REFERENCES dbo.phieu_sua_chua(id),
        phieu_bao_hanh_id bigint NULL REFERENCES dbo.phieu_bao_hanh(id),
        mo_ta nvarchar(500) NOT NULL,
        ky_thuat_vien_id bigint NOT NULL REFERENCES dbo.ky_thuat_vien(nhan_vien_id),
        trang_thai varchar(20) NOT NULL CONSTRAINT df_cvs_trang_thai DEFAULT 'CHO_LAM'
            CONSTRAINT ck_cvs_trang_thai CHECK (trang_thai IN ('CHO_LAM','DANG_LAM','HOAN_THANH','HUY')),
        tao_boi_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        tao_luc datetime2(3) NOT NULL CONSTRAINT df_cvs_tao DEFAULT SYSUTCDATETIME(),
        bat_dau_luc datetime2(3) NULL,
        hoan_thanh_luc datetime2(3) NULL,
        ket_qua nvarchar(1000) NULL,
        cong_de_xuat decimal(14,2) NULL,
        cong_de_xuat_boi_id bigint NULL REFERENCES dbo.nhan_vien(id),
        cong_duyet decimal(14,2) NULL,
        cong_duyet_boi_id bigint NULL REFERENCES dbo.nhan_vien(id),
        cong_duyet_luc datetime2(3) NULL,
        ly_do_duyet_cong nvarchar(500) NULL,
        chi_tra_luc datetime2(3) NULL,
        chi_tra_boi_id bigint NULL REFERENCES dbo.nhan_vien(id),
        ma_tham_chieu_chi_tra nvarchar(100) NULL,
        phuong_thuc_chi_tra varchar(20) NULL,
        CONSTRAINT ck_cvs_cong CHECK (
            (cong_de_xuat IS NULL OR cong_de_xuat >= 0)
            AND (cong_duyet IS NULL OR cong_duyet >= 0)
            AND (cong_duyet IS NULL OR (cong_de_xuat IS NOT NULL AND cong_duyet_boi_id IS NOT NULL
                AND cong_duyet_luc IS NOT NULL AND ly_do_duyet_cong IS NOT NULL
                AND cong_duyet_boi_id <> cong_de_xuat_boi_id))
            AND (chi_tra_luc IS NULL OR (cong_duyet IS NOT NULL AND chi_tra_boi_id IS NOT NULL
                AND phuong_thuc_chi_tra IS NOT NULL)))
    );
    CREATE INDEX ix_cvs_phieu ON dbo.cong_viec_sua_chua_v3(phieu_id, tao_luc);
    CREATE INDEX ix_cvs_ktv ON dbo.cong_viec_sua_chua_v3(ky_thuat_vien_id, trang_thai);

    CREATE TABLE dbo.cong_viec_vat_tu_v3 (
        cong_viec_id bigint NOT NULL REFERENCES dbo.cong_viec_sua_chua_v3(id),
        chi_tiet_xuat_kho_id bigint NOT NULL CONSTRAINT uq_cvvt_xuat UNIQUE
            REFERENCES dbo.chi_tiet_xuat_kho(id),
        CONSTRAINT pk_cvvt_v3 PRIMARY KEY (cong_viec_id, chi_tiet_xuat_kho_id)
    );

    CREATE TABLE dbo.bao_hanh_phan_tich_v3 (
        phieu_bao_hanh_id bigint NOT NULL CONSTRAINT pk_bhpt_v3 PRIMARY KEY
            REFERENCES dbo.phieu_bao_hanh(id),
        nguyen_nhan nvarchar(1000) NOT NULL,
        ben_chiu_chi_phi varchar(15) NOT NULL CONSTRAINT ck_bhpt_ben
            CHECK (ben_chiu_chi_phi IN ('CUA_HANG','KHACH','NCC','KTV')),
        chi_phi_du_kien decimal(14,2) NOT NULL CONSTRAINT ck_bhpt_du_kien CHECK (chi_phi_du_kien >= 0),
        chi_phi_thuc_te decimal(14,2) NULL CONSTRAINT ck_bhpt_thuc_te CHECK (chi_phi_thuc_te >= 0),
        nguoi_ghi_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        cap_nhat_luc datetime2(3) NOT NULL CONSTRAINT df_bhpt_luc DEFAULT SYSUTCDATETIME()
    );

    CREATE TABLE dbo.ho_so_thanh_ly_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_hstl_v3 PRIMARY KEY,
        cach_ly_id bigint NOT NULL REFERENCES dbo.cach_ly_vat_tu_v3(id),
        ket_qua_kiem_tra nvarchar(1000) NOT NULL,
        huong_xu_ly varchar(15) NOT NULL CONSTRAINT ck_hstl_huong
            CHECK (huong_xu_ly IN ('NHAP_LAI','THANH_LY','TIEU_HUY')),
        gia_tri_uoc_tinh decimal(14,2) NOT NULL CONSTRAINT ck_hstl_gia CHECK (gia_tri_uoc_tinh >= 0),
        trang_thai varchar(15) NOT NULL CONSTRAINT ck_hstl_trang_thai
            CHECK (trang_thai IN ('CHO_DUYET','DA_DUYET','TU_CHOI','DA_XU_LY')),
        nguoi_de_xuat_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        de_xuat_luc datetime2(3) NOT NULL CONSTRAINT df_hstl_de_xuat DEFAULT SYSUTCDATETIME(),
        nguoi_duyet_id bigint NULL REFERENCES dbo.nhan_vien(id),
        duyet_luc datetime2(3) NULL,
        ly_do_quyet_dinh nvarchar(500) NULL,
        gia_tri_duyet decimal(14,2) NULL CONSTRAINT ck_hstl_gia_duyet CHECK (gia_tri_duyet >= 0),
        gia_tri_thu_ve decimal(14,2) NULL CONSTRAINT ck_hstl_thu_ve CHECK (gia_tri_thu_ve >= 0),
        CONSTRAINT ck_hstl_duyet CHECK (
            (trang_thai = 'CHO_DUYET' AND nguoi_duyet_id IS NULL AND duyet_luc IS NULL)
            OR (trang_thai IN ('DA_DUYET','TU_CHOI','DA_XU_LY') AND nguoi_duyet_id IS NOT NULL
                AND nguoi_duyet_id <> nguoi_de_xuat_id AND duyet_luc IS NOT NULL
                AND ly_do_quyet_dinh IS NOT NULL))
    );
    CREATE INDEX ix_hstl_cach_ly ON dbo.ho_so_thanh_ly_v3(cach_ly_id, id DESC);

    CREATE TABLE dbo.tai_lieu_thanh_ly_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_tltl_v3 PRIMARY KEY,
        ho_so_id bigint NOT NULL REFERENCES dbo.ho_so_thanh_ly_v3(id),
        loai varchar(15) NOT NULL CONSTRAINT ck_tltl_loai CHECK (loai IN ('ANH','BIEN_BAN')),
        ten_goc nvarchar(200) NOT NULL,
        duong_dan nvarchar(400) NOT NULL,
        sha256 char(64) NOT NULL,
        nguoi_tai_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        tai_luc datetime2(3) NOT NULL CONSTRAINT df_tltl_luc DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX ix_tltl_ho_so ON dbo.tai_lieu_thanh_ly_v3(ho_so_id);

    CREATE TABLE dbo.hoa_don_dau_vao_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_hddv_v3 PRIMARY KEY,
        phieu_nhap_id bigint NOT NULL CONSTRAINT uq_hddv_phieu_nhap UNIQUE REFERENCES dbo.phieu_nhap(id),
        so_hoa_don nvarchar(100) NOT NULL,
        ngay_hoa_don date NOT NULL,
        ma_so_thue_ncc nvarchar(30) NOT NULL,
        tien_hang decimal(14,2) NOT NULL,
        tien_thue decimal(14,2) NOT NULL,
        tong_tien decimal(14,2) NOT NULL,
        trang_thai varchar(15) NOT NULL CONSTRAINT ck_hddv_trang_thai
            CHECK (trang_thai IN ('KHOP','CHENH_LECH')),
        chenh_lech decimal(14,2) NOT NULL,
        ghi_chu nvarchar(500) NULL,
        nguoi_ghi_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        ghi_luc datetime2(3) NOT NULL CONSTRAINT df_hddv_luc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT ck_hddv_tien CHECK (tien_hang >= 0 AND tien_thue >= 0
            AND tong_tien = tien_hang + tien_thue)
    );

    CREATE TABLE dbo.hoa_don_dien_tu_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_hddt_v3 PRIMARY KEY,
        hoa_don_id bigint NOT NULL CONSTRAINT uq_hddt_hoa_don UNIQUE REFERENCES dbo.hoa_don(id),
        nha_cung_cap nvarchar(150) NOT NULL,
        so_hoa_don nvarchar(100) NOT NULL,
        ngay_phat_hanh datetime2(3) NOT NULL,
        ma_tra_cuu nvarchar(150) NULL,
        tong_tien decimal(14,2) NOT NULL,
        trang_thai varchar(20) NOT NULL CONSTRAINT df_hddt_trang_thai DEFAULT 'DA_GHI_NHAN'
            CONSTRAINT ck_hddt_trang_thai CHECK (trang_thai IN ('DA_GHI_NHAN','DA_DIEU_CHINH')),
        nguoi_ghi_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        ghi_luc datetime2(3) NOT NULL CONSTRAINT df_hddt_luc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT ck_hddt_tong CHECK (tong_tien >= 0)
    );

    CREATE TABLE dbo.dieu_chinh_hoa_don_v3 (
        id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_dchd_v3 PRIMARY KEY,
        hoa_don_dien_tu_id bigint NOT NULL REFERENCES dbo.hoa_don_dien_tu_v3(id),
        so_chung_tu nvarchar(100) NOT NULL,
        loai varchar(15) NOT NULL CONSTRAINT ck_dchd_loai
            CHECK (loai IN ('DIEU_CHINH','THAY_THE','HUY')),
        ly_do nvarchar(1000) NOT NULL,
        chenh_lech_tien decimal(14,2) NOT NULL,
        ngay_chung_tu datetime2(3) NOT NULL,
        nguoi_ghi_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        ghi_luc datetime2(3) NOT NULL CONSTRAINT df_dchd_luc DEFAULT SYSUTCDATETIME()
    );

    INSERT dbo.v3_schema_migration(version) VALUES ('V3_20261108_WORKFLOW');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
