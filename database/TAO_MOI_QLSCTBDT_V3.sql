-- TẠO CSDL MỚI RỖNG, KHÔNG ĐỤNG ĐẾN QLSCTBDT CŨ.
-- Yêu cầu quyền CREATE DATABASE. Tên mặc định: QLSCTBDT_V3.
USE [master];
GO
IF DB_ID(N'QLSCTBDT_V3') IS NOT NULL
BEGIN
    RAISERROR(N'CSDL QLSCTBDT_V3 đã tồn tại. Script dừng; không ghi đè.',16,1);
    SET NOEXEC ON;
END
ELSE
BEGIN TRY
    EXEC(N'CREATE DATABASE [QLSCTBDT_V3]');
END TRY
BEGIN CATCH
    DECLARE @create_error nvarchar(2048)=ERROR_MESSAGE();
    RAISERROR(N'Không tạo được CSDL mới: %s',16,1,@create_error);
    SET NOEXEC ON;
END CATCH;
GO
USE [QLSCTBDT_V3];
GO
/****** Object:  Table [dbo].[phieu_sua_chua]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[phieu_sua_chua](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_phieu]  AS ('SC'+case when [id]<(10000000) then right('0000000'+CONVERT([varchar](20),[id]),(7)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[thiet_bi_id] [bigint] NOT NULL,
	[le_tan_id] [bigint] NOT NULL,
	[ky_thuat_vien_id] [bigint] NULL,
	[ngay_nhan] [datetime2](3) NOT NULL,
	[ngay_hen_tra] [datetime2](3) NULL,
	[ngay_hoan_thanh] [datetime2](3) NULL,
	[ngay_tra] [datetime2](3) NULL,
	[tinh_trang_loi] [nvarchar](max) NOT NULL,
	[ket_qua_sua_chua] [nvarchar](max) NULL,
	[trang_thai] [varchar](15) NOT NULL,
	[ly_do_huy] [nvarchar](max) NULL,
	[ghi_chu] [nvarchar](max) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
	[tinh_trang_ngoai_quan] [nvarchar](500) NULL,
	[phu_kien_kem_theo] [nvarchar](500) NULL,
	[mat_khau_may] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_phieu_sua_chua_ma] UNIQUE NONCLUSTERED 
(
	[ma_phieu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chi_tiet_dich_vu]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chi_tiet_dich_vu](
	[phieu_id] [bigint] NOT NULL,
	[dich_vu_id] [bigint] NOT NULL,
	[so_luong] [int] NOT NULL,
	[don_gia_thuc_te] [decimal](14, 2) NOT NULL,
	[thanh_tien]  AS (CONVERT([decimal](14,2),[so_luong]*[don_gia_thuc_te])) PERSISTED,
	[created_at] [datetime2](3) NOT NULL,
 CONSTRAINT [pk_chi_tiet_dich_vu] PRIMARY KEY CLUSTERED 
(
	[phieu_id] ASC,
	[dich_vu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chi_tiet_xuat_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chi_tiet_xuat_kho](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[phieu_id] [bigint] NOT NULL,
	[kho_id] [bigint] NOT NULL,
	[linh_kien_id] [bigint] NOT NULL,
	[thu_kho_id] [bigint] NOT NULL,
	[so_luong_xuat] [int] NOT NULL,
	[don_gia_xuat] [decimal](14, 2) NOT NULL,
	[thanh_tien]  AS (CONVERT([decimal](14,2),[so_luong_xuat]*[don_gia_xuat])) PERSISTED,
	[ngay_xuat] [datetime2](3) NOT NULL,
	[bao_hanh_den] [date] NULL,
	[linh_kien_serial_id] [bigint] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_phieu_chi_phi]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =====================================================================
-- H. VIEW PHỤC VỤ API / FE (không lưu dữ liệu dẫn xuất để khỏi lệch số)
--    Mỗi CREATE VIEW phải nằm trong batch riêng nên có GO giữa các view.
-- =====================================================================

-- Chi phí tạm tính của phiếu đang sửa
CREATE VIEW [dbo].[v_phieu_chi_phi] AS
SELECT p.id AS phieu_id,
       COALESCE((SELECT SUM(d.thanh_tien) FROM chi_tiet_dich_vu d WHERE d.phieu_id = p.id), 0) AS tien_dich_vu,
       COALESCE((SELECT SUM(x.thanh_tien) FROM chi_tiet_xuat_kho x WHERE x.phieu_id = p.id), 0) AS tien_linh_kien,
       COALESCE((SELECT SUM(d.thanh_tien) FROM chi_tiet_dich_vu d WHERE d.phieu_id = p.id), 0)
     + COALESCE((SELECT SUM(x.thanh_tien) FROM chi_tiet_xuat_kho x WHERE x.phieu_id = p.id), 0) AS tong_tam_tinh
FROM phieu_sua_chua p;

GO
/****** Object:  Table [dbo].[phieu_nhap]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[phieu_nhap](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_pn]  AS ('PN'+case when [id]<(10000000) then right('0000000'+CONVERT([varchar](20),[id]),(7)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[nha_cung_cap_id] [bigint] NOT NULL,
	[kho_id] [bigint] NOT NULL,
	[thu_kho_id] [bigint] NOT NULL,
	[so_chung_tu_ncc] [varchar](50) NULL,
	[ngay_nhap] [datetime2](3) NOT NULL,
	[trang_thai] [varchar](15) NOT NULL,
	[ghi_chu] [nvarchar](max) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_phieu_nhap_ma] UNIQUE NONCLUSTERED 
(
	[ma_pn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[chi_tiet_nhap_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[chi_tiet_nhap_kho](
	[phieu_nhap_id] [bigint] NOT NULL,
	[linh_kien_id] [bigint] NOT NULL,
	[so_luong_nhap] [int] NOT NULL,
	[don_gia_nhap] [decimal](14, 2) NOT NULL,
	[thanh_tien]  AS (CONVERT([decimal](14,2),[so_luong_nhap]*[don_gia_nhap])) PERSISTED,
	[danh_sach_serial] [nvarchar](max) NULL,
 CONSTRAINT [pk_chi_tiet_nhap_kho] PRIMARY KEY CLUSTERED 
(
	[phieu_nhap_id] ASC,
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_phieu_nhap_tong]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Tổng tiền phiếu nhập (thay cho cột tongTienNhap)
CREATE VIEW [dbo].[v_phieu_nhap_tong] AS
SELECT pn.id AS phieu_nhap_id,
       COALESCE(SUM(c.thanh_tien), 0) AS tong_tien_nhap
FROM phieu_nhap pn
LEFT JOIN chi_tiet_nhap_kho c ON c.phieu_nhap_id = pn.id
GROUP BY pn.id;

GO
/****** Object:  Table [dbo].[linh_kien]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[linh_kien](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_lk]  AS ('LK'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[ten_linh_kien] [nvarchar](150) NOT NULL,
	[hang_san_xuat] [nvarchar](100) NULL,
	[mo_ta] [nvarchar](max) NULL,
	[don_vi_tinh] [nvarchar](20) NOT NULL,
	[don_gia_ban] [decimal](14, 2) NOT NULL,
	[thoi_han_bao_hanh_thang] [smallint] NOT NULL,
	[deleted_at] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_linh_kien_ma] UNIQUE NONCLUSTERED 
(
	[ma_lk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[anh_linh_kien]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[anh_linh_kien](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[linh_kien_id] [bigint] NOT NULL,
	[duong_dan] [varchar](500) NOT NULL,
	[la_anh_chinh] [bit] NOT NULL,
	[thu_tu] [smallint] NOT NULL,
	[created_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ton_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ton_kho](
	[kho_id] [bigint] NOT NULL,
	[linh_kien_id] [bigint] NOT NULL,
	[so_luong_ton] [int] NOT NULL,
	[muc_ton_toi_thieu] [int] NOT NULL,
	[ngay_cap_nhat] [datetime2](3) NOT NULL,
 CONSTRAINT [pk_ton_kho] PRIMARY KEY CLUSTERED 
(
	[kho_id] ASC,
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_linh_kien_hien_thi]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Danh sách linh kiện cho FE: kèm ảnh chính + tổng tồn mọi kho
CREATE VIEW [dbo].[v_linh_kien_hien_thi] AS
SELECT lk.id, lk.ma_lk, lk.ten_linh_kien, lk.hang_san_xuat, lk.mo_ta, lk.don_vi_tinh,
       lk.don_gia_ban, lk.thoi_han_bao_hanh_thang,
       (SELECT TOP (1) a.duong_dan FROM anh_linh_kien a
         WHERE a.linh_kien_id = lk.id
         ORDER BY a.la_anh_chinh DESC, a.thu_tu, a.id) AS anh_chinh,
       COALESCE((SELECT SUM(t.so_luong_ton) FROM ton_kho t WHERE t.linh_kien_id = lk.id), 0) AS tong_ton
FROM linh_kien lk
WHERE lk.deleted_at IS NULL;

GO
/****** Object:  Table [dbo].[kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[kho](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_kho]  AS ('KHO'+case when [id]<(1000) then right('000'+CONVERT([varchar](20),[id]),(3)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[ten_kho] [nvarchar](100) NOT NULL,
	[dia_diem] [nvarchar](255) NULL,
	[thu_kho_id] [bigint] NULL,
	[deleted_at] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_kho_ma] UNIQUE NONCLUSTERED 
(
	[ma_kho] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[v_ton_kho_sap_het]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Cảnh báo sắp hết hàng
CREATE VIEW [dbo].[v_ton_kho_sap_het] AS
SELECT t.kho_id, k.ten_kho, t.linh_kien_id, lk.ten_linh_kien, t.so_luong_ton, t.muc_ton_toi_thieu
FROM ton_kho t
JOIN kho k        ON k.id  = t.kho_id
JOIN linh_kien lk ON lk.id = t.linh_kien_id
WHERE t.so_luong_ton <= t.muc_ton_toi_thieu;

GO
/****** Object:  Table [dbo].[anh_phieu_sua_chua]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[anh_phieu_sua_chua](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[phieu_id] [bigint] NOT NULL,
	[loai] [varchar](15) NOT NULL,
	[duong_dan] [varchar](500) NOT NULL,
	[mo_ta] [nvarchar](255) NULL,
	[created_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[audit_log]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[audit_log](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[bang] [varchar](128) NOT NULL,
	[ban_ghi_id] [bigint] NULL,
	[hanh_dong] [varchar](10) NOT NULL,
	[du_lieu_cu] [nvarchar](max) NULL,
	[du_lieu_moi] [nvarchar](max) NULL,
	[nguoi_thuc_hien_id] [bigint] NULL,
	[thoi_gian] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[bien_dong_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[bien_dong_kho](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[kho_id] [bigint] NOT NULL,
	[linh_kien_id] [bigint] NOT NULL,
	[loai] [varchar](12) NOT NULL,
	[so_luong_thay_doi] [int] NOT NULL,
	[so_luong_sau] [int] NOT NULL,
	[phieu_nhap_id] [bigint] NULL,
	[chi_tiet_xuat_kho_id] [bigint] NULL,
	[nguoi_thuc_hien_id] [bigint] NOT NULL,
	[thoi_gian] [datetime2](3) NOT NULL,
	[ly_do] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[dich_vu]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[dich_vu](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_dv]  AS ('DV'+case when [id]<(100000) then right('00000'+CONVERT([varchar](20),[id]),(5)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[ten_dich_vu] [nvarchar](150) NOT NULL,
	[mo_ta] [nvarchar](max) NULL,
	[hinh_anh] [varchar](500) NULL,
	[gia_niem_yet] [decimal](14, 2) NOT NULL,
	[deleted_at] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_dich_vu_ma] UNIQUE NONCLUSTERED 
(
	[ma_dv] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[hoa_don]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[hoa_don](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_hd]  AS ('HD'+case when [id]<(10000000) then right('0000000'+CONVERT([varchar](20),[id]),(7)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[phieu_id] [bigint] NOT NULL,
	[le_tan_id] [bigint] NOT NULL,
	[ngay_lap] [datetime2](3) NOT NULL,
	[tien_dich_vu] [decimal](14, 2) NOT NULL,
	[tien_linh_kien] [decimal](14, 2) NOT NULL,
	[giam_gia] [decimal](14, 2) NOT NULL,
	[tong_thanh_toan] [decimal](14, 2) NOT NULL,
	[phuong_thuc_tt] [varchar](15) NULL,
	[trang_thai] [varchar](20) NOT NULL,
	[ngay_thanh_toan] [datetime2](3) NULL,
	[ghi_chu] [nvarchar](max) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_hoa_don_ma] UNIQUE NONCLUSTERED 
(
	[ma_hd] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ky_thuat_vien]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ky_thuat_vien](
	[nhan_vien_id] [bigint] NOT NULL,
	[chuc_vu] [varchar](20) NOT NULL,
	[chuyen_mon] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[nhan_vien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[khach_hang]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[khach_hang](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_kh]  AS ('KH'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[ho_ten] [nvarchar](100) NOT NULL,
	[so_dien_thoai] [varchar](20) NOT NULL,
	[email] [varchar](255) NULL,
	[dia_chi] [nvarchar](255) NULL,
	[tai_khoan_id] [bigint] NULL,
	[vai_tro_tk] [varchar](20) NOT NULL,
	[deleted_at] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_khach_hang_ma] UNIQUE NONCLUSTERED 
(
	[ma_kh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[le_tan]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[le_tan](
	[nhan_vien_id] [bigint] NOT NULL,
	[chuc_vu] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[nhan_vien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[lich_su_trang_thai_phieu]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[lich_su_trang_thai_phieu](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[phieu_id] [bigint] NOT NULL,
	[trang_thai_cu] [varchar](15) NULL,
	[trang_thai_moi] [varchar](15) NOT NULL,
	[nguoi_thuc_hien_id] [bigint] NULL,
	[thoi_gian] [datetime2](3) NOT NULL,
	[ghi_chu] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[linh_kien_serial]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[linh_kien_serial](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[linh_kien_id] [bigint] NOT NULL,
	[so_serial] [varchar](150) NOT NULL,
	[kho_id] [bigint] NULL,
	[phieu_nhap_id] [bigint] NOT NULL,
	[chi_tiet_xuat_kho_id] [bigint] NULL,
	[trang_thai] [nvarchar](30) NOT NULL,
	[ghi_chu] [nvarchar](500) NULL,
	[created_at] [datetime2](7) NOT NULL,
	[updated_at] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_linh_kien_serial_so_serial] UNIQUE NONCLUSTERED 
(
	[so_serial] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[nha_cung_cap]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[nha_cung_cap](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_ncc]  AS ('NCC'+case when [id]<(10000) then right('0000'+CONVERT([varchar](20),[id]),(4)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[ten_ncc] [nvarchar](150) NOT NULL,
	[ma_so_thue] [varchar](20) NULL,
	[so_dien_thoai] [varchar](20) NULL,
	[email] [varchar](255) NULL,
	[dia_chi] [nvarchar](255) NULL,
	[deleted_at] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_nha_cung_cap_ma] UNIQUE NONCLUSTERED 
(
	[ma_ncc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[nhan_vien]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[nhan_vien](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_nv]  AS ('NV'+case when [id]<(100000) then right('00000'+CONVERT([varchar](20),[id]),(5)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[ho_ten] [nvarchar](100) NOT NULL,
	[so_dien_thoai] [varchar](20) NOT NULL,
	[email] [varchar](255) NULL,
	[chuc_vu] [varchar](20) NOT NULL,
	[tai_khoan_id] [bigint] NOT NULL,
	[deleted_at] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_nhan_vien_id_chuc_vu] UNIQUE NONCLUSTERED 
(
	[id] ASC,
	[chuc_vu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_nhan_vien_ma] UNIQUE NONCLUSTERED 
(
	[ma_nv] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_nhan_vien_tai_khoan] UNIQUE NONCLUSTERED 
(
	[tai_khoan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[refresh_token]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[refresh_token](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[tai_khoan_id] [bigint] NOT NULL,
	[token_hash] [varchar](128) NOT NULL,
	[het_han] [datetime2](3) NOT NULL,
	[thu_hoi_luc] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_refresh_token_hash] UNIQUE NONCLUSTERED 
(
	[token_hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tai_khoan]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tai_khoan](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ten_dang_nhap] [varchar](50) NOT NULL,
	[mat_khau_hash] [varchar](255) NOT NULL,
	[vai_tro] [varchar](20) NOT NULL,
	[hoat_dong] [bit] NOT NULL,
	[so_lan_dang_nhap_sai] [smallint] NOT NULL,
	[khoa_den] [datetime2](3) NULL,
	[dang_nhap_cuoi] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_tai_khoan_id_vai_tro] UNIQUE NONCLUSTERED 
(
	[id] ASC,
	[vai_tro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_tai_khoan_ten_dang_nhap] UNIQUE NONCLUSTERED 
(
	[ten_dang_nhap] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[thiet_bi]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[thiet_bi](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ma_thiet_bi]  AS ('TB'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end) PERSISTED NOT NULL,
	[khach_hang_id] [bigint] NOT NULL,
	[ten_thiet_bi] [nvarchar](150) NOT NULL,
	[loai_may] [nvarchar](50) NULL,
	[so_serial] [varchar](100) NULL,
	[ngay_mua] [date] NULL,
	[ghi_chu] [nvarchar](max) NULL,
	[deleted_at] [datetime2](3) NULL,
	[created_at] [datetime2](3) NOT NULL,
	[updated_at] [datetime2](3) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [uq_thiet_bi_ma] UNIQUE NONCLUSTERED 
(
	[ma_thiet_bi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[thu_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[thu_kho](
	[nhan_vien_id] [bigint] NOT NULL,
	[chuc_vu] [varchar](20) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[nhan_vien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [ix_anh_linh_kien_lk]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_anh_linh_kien_lk] ON [dbo].[anh_linh_kien]
(
	[linh_kien_id] ASC,
	[thu_tu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [uq_anh_linh_kien_chinh]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_anh_linh_kien_chinh] ON [dbo].[anh_linh_kien]
(
	[linh_kien_id] ASC
)
WHERE ([la_anh_chinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_anh_phieu]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_anh_phieu] ON [dbo].[anh_phieu_sua_chua]
(
	[phieu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_audit_log_bang]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_audit_log_bang] ON [dbo].[audit_log]
(
	[bang] ASC,
	[ban_ghi_id] ASC,
	[thoi_gian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_bien_dong_kho_lk]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_bien_dong_kho_lk] ON [dbo].[bien_dong_kho]
(
	[kho_id] ASC,
	[linh_kien_id] ASC,
	[thoi_gian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_bien_dong_pn]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_bien_dong_pn] ON [dbo].[bien_dong_kho]
(
	[phieu_nhap_id] ASC
)
WHERE ([phieu_nhap_id] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_bien_dong_xuat]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_bien_dong_xuat] ON [dbo].[bien_dong_kho]
(
	[chi_tiet_xuat_kho_id] ASC
)
WHERE ([chi_tiet_xuat_kho_id] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_ctdv_dich_vu]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_ctdv_dich_vu] ON [dbo].[chi_tiet_dich_vu]
(
	[dich_vu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_ctnk_linh_kien]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_ctnk_linh_kien] ON [dbo].[chi_tiet_nhap_kho]
(
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_ctxk_kho_lk]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_ctxk_kho_lk] ON [dbo].[chi_tiet_xuat_kho]
(
	[kho_id] ASC,
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_ctxk_phieu]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_ctxk_phieu] ON [dbo].[chi_tiet_xuat_kho]
(
	[phieu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_ctxk_thu_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_ctxk_thu_kho] ON [dbo].[chi_tiet_xuat_kho]
(
	[thu_kho_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_dich_vu_ten]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_dich_vu_ten] ON [dbo].[dich_vu]
(
	[ten_dich_vu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_hoa_don_le_tan]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_hoa_don_le_tan] ON [dbo].[hoa_don]
(
	[le_tan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_hoa_don_trang_thai]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_hoa_don_trang_thai] ON [dbo].[hoa_don]
(
	[trang_thai] ASC,
	[ngay_lap] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [uq_hoa_don_phieu_hieu_luc]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_hoa_don_phieu_hieu_luc] ON [dbo].[hoa_don]
(
	[phieu_id] ASC
)
WHERE ([trang_thai]<>'HUY')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_khach_hang_ho_ten]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_khach_hang_ho_ten] ON [dbo].[khach_hang]
(
	[ho_ten] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_khach_hang_sdt]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_khach_hang_sdt] ON [dbo].[khach_hang]
(
	[so_dien_thoai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [uq_khach_hang_email]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_khach_hang_email] ON [dbo].[khach_hang]
(
	[email] ASC
)
WHERE ([email] IS NOT NULL AND [deleted_at] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [uq_khach_hang_tai_khoan]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_khach_hang_tai_khoan] ON [dbo].[khach_hang]
(
	[tai_khoan_id] ASC
)
WHERE ([tai_khoan_id] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_kho_thu_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_kho_thu_kho] ON [dbo].[kho]
(
	[thu_kho_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_lich_su_phieu]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_lich_su_phieu] ON [dbo].[lich_su_trang_thai_phieu]
(
	[phieu_id] ASC,
	[thoi_gian] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_linh_kien_ten]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_linh_kien_ten] ON [dbo].[linh_kien]
(
	[ten_linh_kien] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_lks_kho_id]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_lks_kho_id] ON [dbo].[linh_kien_serial]
(
	[kho_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_lks_linh_kien_id]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_lks_linh_kien_id] ON [dbo].[linh_kien_serial]
(
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_lks_phieu_nhap_id]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_lks_phieu_nhap_id] ON [dbo].[linh_kien_serial]
(
	[phieu_nhap_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_lks_trang_thai]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_lks_trang_thai] ON [dbo].[linh_kien_serial]
(
	[trang_thai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_phieu_nhap_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_nhap_kho] ON [dbo].[phieu_nhap]
(
	[kho_id] ASC,
	[ngay_nhap] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_phieu_nhap_ncc]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_nhap_ncc] ON [dbo].[phieu_nhap]
(
	[nha_cung_cap_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_phieu_nhap_thu_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_nhap_thu_kho] ON [dbo].[phieu_nhap]
(
	[thu_kho_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_phieu_ktv]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_ktv] ON [dbo].[phieu_sua_chua]
(
	[ky_thuat_vien_id] ASC,
	[trang_thai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_phieu_le_tan]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_le_tan] ON [dbo].[phieu_sua_chua]
(
	[le_tan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_phieu_thiet_bi]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_thiet_bi] ON [dbo].[phieu_sua_chua]
(
	[thiet_bi_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [ix_phieu_trang_thai]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_trang_thai] ON [dbo].[phieu_sua_chua]
(
	[trang_thai] ASC,
	[ngay_nhan] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_refresh_token_tai_khoan]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_refresh_token_tai_khoan] ON [dbo].[refresh_token]
(
	[tai_khoan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_ton_kho_linh_kien]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_ton_kho_linh_kien] ON [dbo].[ton_kho]
(
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [ix_thiet_bi_khach_hang]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE NONCLUSTERED INDEX [ix_thiet_bi_khach_hang] ON [dbo].[thiet_bi]
(
	[khach_hang_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [uq_thiet_bi_so_serial]    Script Date: 11/4/2026 10:41:46 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_thiet_bi_so_serial] ON [dbo].[thiet_bi]
(
	[so_serial] ASC
)
WHERE ([so_serial] IS NOT NULL AND [deleted_at] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[anh_linh_kien] ADD  DEFAULT ((0)) FOR [la_anh_chinh]
GO
ALTER TABLE [dbo].[anh_linh_kien] ADD  DEFAULT ((0)) FOR [thu_tu]
GO
ALTER TABLE [dbo].[anh_linh_kien] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[anh_phieu_sua_chua] ADD  DEFAULT ('KHAC') FOR [loai]
GO
ALTER TABLE [dbo].[anh_phieu_sua_chua] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[audit_log] ADD  DEFAULT (sysutcdatetime()) FOR [thoi_gian]
GO
ALTER TABLE [dbo].[bien_dong_kho] ADD  DEFAULT (sysutcdatetime()) FOR [thoi_gian]
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu] ADD  DEFAULT ((1)) FOR [so_luong]
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho] ADD  DEFAULT (sysutcdatetime()) FOR [ngay_xuat]
GO
ALTER TABLE [dbo].[dich_vu] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[dich_vu] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[hoa_don] ADD  DEFAULT (sysutcdatetime()) FOR [ngay_lap]
GO
ALTER TABLE [dbo].[hoa_don] ADD  DEFAULT ((0)) FOR [tien_dich_vu]
GO
ALTER TABLE [dbo].[hoa_don] ADD  DEFAULT ((0)) FOR [tien_linh_kien]
GO
ALTER TABLE [dbo].[hoa_don] ADD  DEFAULT ((0)) FOR [giam_gia]
GO
ALTER TABLE [dbo].[hoa_don] ADD  DEFAULT ('CHUA_THANH_TOAN') FOR [trang_thai]
GO
ALTER TABLE [dbo].[hoa_don] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[hoa_don] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[ky_thuat_vien] ADD  DEFAULT ('KY_THUAT_VIEN') FOR [chuc_vu]
GO
ALTER TABLE [dbo].[khach_hang] ADD  DEFAULT ('KHACH_HANG') FOR [vai_tro_tk]
GO
ALTER TABLE [dbo].[khach_hang] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[khach_hang] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[kho] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[kho] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[le_tan] ADD  DEFAULT ('LE_TAN') FOR [chuc_vu]
GO
ALTER TABLE [dbo].[lich_su_trang_thai_phieu] ADD  DEFAULT (sysutcdatetime()) FOR [thoi_gian]
GO
ALTER TABLE [dbo].[linh_kien] ADD  DEFAULT (N'cái') FOR [don_vi_tinh]
GO
ALTER TABLE [dbo].[linh_kien] ADD  DEFAULT ((0)) FOR [thoi_han_bao_hanh_thang]
GO
ALTER TABLE [dbo].[linh_kien] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[linh_kien] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[linh_kien_serial] ADD  DEFAULT ('TRONG_KHO') FOR [trang_thai]
GO
ALTER TABLE [dbo].[linh_kien_serial] ADD  DEFAULT (getutcdate()) FOR [created_at]
GO
ALTER TABLE [dbo].[linh_kien_serial] ADD  DEFAULT (getutcdate()) FOR [updated_at]
GO
ALTER TABLE [dbo].[nha_cung_cap] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[nha_cung_cap] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[nhan_vien] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[nhan_vien] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[phieu_nhap] ADD  DEFAULT (sysutcdatetime()) FOR [ngay_nhap]
GO
ALTER TABLE [dbo].[phieu_nhap] ADD  DEFAULT ('NHAP') FOR [trang_thai]
GO
ALTER TABLE [dbo].[phieu_nhap] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[phieu_nhap] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[phieu_sua_chua] ADD  DEFAULT (sysutcdatetime()) FOR [ngay_nhan]
GO
ALTER TABLE [dbo].[phieu_sua_chua] ADD  DEFAULT ('TIEP_NHAN') FOR [trang_thai]
GO
ALTER TABLE [dbo].[phieu_sua_chua] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[phieu_sua_chua] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[refresh_token] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[tai_khoan] ADD  DEFAULT ((1)) FOR [hoat_dong]
GO
ALTER TABLE [dbo].[tai_khoan] ADD  DEFAULT ((0)) FOR [so_lan_dang_nhap_sai]
GO
ALTER TABLE [dbo].[tai_khoan] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[tai_khoan] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[ton_kho] ADD  DEFAULT ((0)) FOR [so_luong_ton]
GO
ALTER TABLE [dbo].[ton_kho] ADD  DEFAULT ((0)) FOR [muc_ton_toi_thieu]
GO
ALTER TABLE [dbo].[ton_kho] ADD  DEFAULT (sysutcdatetime()) FOR [ngay_cap_nhat]
GO
ALTER TABLE [dbo].[thiet_bi] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]
GO
ALTER TABLE [dbo].[thiet_bi] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]
GO
ALTER TABLE [dbo].[thu_kho] ADD  DEFAULT ('THU_KHO') FOR [chuc_vu]
GO
ALTER TABLE [dbo].[anh_linh_kien]  WITH CHECK ADD  CONSTRAINT [fk_anh_linh_kien_lk] FOREIGN KEY([linh_kien_id])
REFERENCES [dbo].[linh_kien] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[anh_linh_kien] CHECK CONSTRAINT [fk_anh_linh_kien_lk]
GO
ALTER TABLE [dbo].[anh_phieu_sua_chua]  WITH CHECK ADD  CONSTRAINT [fk_anh_phieu] FOREIGN KEY([phieu_id])
REFERENCES [dbo].[phieu_sua_chua] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[anh_phieu_sua_chua] CHECK CONSTRAINT [fk_anh_phieu]
GO
ALTER TABLE [dbo].[audit_log]  WITH CHECK ADD  CONSTRAINT [fk_audit_log_tai_khoan] FOREIGN KEY([nguoi_thuc_hien_id])
REFERENCES [dbo].[tai_khoan] ([id])
GO
ALTER TABLE [dbo].[audit_log] CHECK CONSTRAINT [fk_audit_log_tai_khoan]
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD  CONSTRAINT [fk_bdk_nguoi] FOREIGN KEY([nguoi_thuc_hien_id])
REFERENCES [dbo].[nhan_vien] ([id])
GO
ALTER TABLE [dbo].[bien_dong_kho] CHECK CONSTRAINT [fk_bdk_nguoi]
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD  CONSTRAINT [fk_bdk_phieu_nhap] FOREIGN KEY([phieu_nhap_id])
REFERENCES [dbo].[phieu_nhap] ([id])
GO
ALTER TABLE [dbo].[bien_dong_kho] CHECK CONSTRAINT [fk_bdk_phieu_nhap]
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD  CONSTRAINT [fk_bdk_ton_kho] FOREIGN KEY([kho_id], [linh_kien_id])
REFERENCES [dbo].[ton_kho] ([kho_id], [linh_kien_id])
GO
ALTER TABLE [dbo].[bien_dong_kho] CHECK CONSTRAINT [fk_bdk_ton_kho]
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD  CONSTRAINT [fk_bdk_xuat_kho] FOREIGN KEY([chi_tiet_xuat_kho_id])
REFERENCES [dbo].[chi_tiet_xuat_kho] ([id])
GO
ALTER TABLE [dbo].[bien_dong_kho] CHECK CONSTRAINT [fk_bdk_xuat_kho]
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu]  WITH CHECK ADD  CONSTRAINT [fk_ctdv_dich_vu] FOREIGN KEY([dich_vu_id])
REFERENCES [dbo].[dich_vu] ([id])
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu] CHECK CONSTRAINT [fk_ctdv_dich_vu]
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu]  WITH CHECK ADD  CONSTRAINT [fk_ctdv_phieu] FOREIGN KEY([phieu_id])
REFERENCES [dbo].[phieu_sua_chua] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu] CHECK CONSTRAINT [fk_ctdv_phieu]
GO
ALTER TABLE [dbo].[chi_tiet_nhap_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctnk_linh_kien] FOREIGN KEY([linh_kien_id])
REFERENCES [dbo].[linh_kien] ([id])
GO
ALTER TABLE [dbo].[chi_tiet_nhap_kho] CHECK CONSTRAINT [fk_ctnk_linh_kien]
GO
ALTER TABLE [dbo].[chi_tiet_nhap_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctnk_phieu_nhap] FOREIGN KEY([phieu_nhap_id])
REFERENCES [dbo].[phieu_nhap] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[chi_tiet_nhap_kho] CHECK CONSTRAINT [fk_ctnk_phieu_nhap]
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctxk_phieu] FOREIGN KEY([phieu_id])
REFERENCES [dbo].[phieu_sua_chua] ([id])
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho] CHECK CONSTRAINT [fk_ctxk_phieu]
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctxk_serial] FOREIGN KEY([linh_kien_serial_id])
REFERENCES [dbo].[linh_kien_serial] ([id])
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho] CHECK CONSTRAINT [fk_ctxk_serial]
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctxk_ton_kho] FOREIGN KEY([kho_id], [linh_kien_id])
REFERENCES [dbo].[ton_kho] ([kho_id], [linh_kien_id])
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho] CHECK CONSTRAINT [fk_ctxk_ton_kho]
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctxk_thu_kho] FOREIGN KEY([thu_kho_id])
REFERENCES [dbo].[thu_kho] ([nhan_vien_id])
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho] CHECK CONSTRAINT [fk_ctxk_thu_kho]
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD  CONSTRAINT [fk_hoa_don_le_tan] FOREIGN KEY([le_tan_id])
REFERENCES [dbo].[le_tan] ([nhan_vien_id])
GO
ALTER TABLE [dbo].[hoa_don] CHECK CONSTRAINT [fk_hoa_don_le_tan]
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD  CONSTRAINT [fk_hoa_don_phieu] FOREIGN KEY([phieu_id])
REFERENCES [dbo].[phieu_sua_chua] ([id])
GO
ALTER TABLE [dbo].[hoa_don] CHECK CONSTRAINT [fk_hoa_don_phieu]
GO
ALTER TABLE [dbo].[ky_thuat_vien]  WITH CHECK ADD  CONSTRAINT [fk_ky_thuat_vien_nhan_vien] FOREIGN KEY([nhan_vien_id], [chuc_vu])
REFERENCES [dbo].[nhan_vien] ([id], [chuc_vu])
GO
ALTER TABLE [dbo].[ky_thuat_vien] CHECK CONSTRAINT [fk_ky_thuat_vien_nhan_vien]
GO
ALTER TABLE [dbo].[khach_hang]  WITH CHECK ADD  CONSTRAINT [fk_khach_hang_tai_khoan] FOREIGN KEY([tai_khoan_id], [vai_tro_tk])
REFERENCES [dbo].[tai_khoan] ([id], [vai_tro])
GO
ALTER TABLE [dbo].[khach_hang] CHECK CONSTRAINT [fk_khach_hang_tai_khoan]
GO
ALTER TABLE [dbo].[kho]  WITH CHECK ADD  CONSTRAINT [fk_kho_thu_kho] FOREIGN KEY([thu_kho_id])
REFERENCES [dbo].[thu_kho] ([nhan_vien_id])
GO
ALTER TABLE [dbo].[kho] CHECK CONSTRAINT [fk_kho_thu_kho]
GO
ALTER TABLE [dbo].[le_tan]  WITH CHECK ADD  CONSTRAINT [fk_le_tan_nhan_vien] FOREIGN KEY([nhan_vien_id], [chuc_vu])
REFERENCES [dbo].[nhan_vien] ([id], [chuc_vu])
GO
ALTER TABLE [dbo].[le_tan] CHECK CONSTRAINT [fk_le_tan_nhan_vien]
GO
ALTER TABLE [dbo].[lich_su_trang_thai_phieu]  WITH CHECK ADD  CONSTRAINT [fk_lich_su_nguoi] FOREIGN KEY([nguoi_thuc_hien_id])
REFERENCES [dbo].[nhan_vien] ([id])
GO
ALTER TABLE [dbo].[lich_su_trang_thai_phieu] CHECK CONSTRAINT [fk_lich_su_nguoi]
GO
ALTER TABLE [dbo].[lich_su_trang_thai_phieu]  WITH CHECK ADD  CONSTRAINT [fk_lich_su_phieu] FOREIGN KEY([phieu_id])
REFERENCES [dbo].[phieu_sua_chua] ([id])
GO
ALTER TABLE [dbo].[lich_su_trang_thai_phieu] CHECK CONSTRAINT [fk_lich_su_phieu]
GO
ALTER TABLE [dbo].[linh_kien_serial]  WITH CHECK ADD  CONSTRAINT [fk_lks_chi_tiet_xuat_kho] FOREIGN KEY([chi_tiet_xuat_kho_id])
REFERENCES [dbo].[chi_tiet_xuat_kho] ([id])
GO
ALTER TABLE [dbo].[linh_kien_serial] CHECK CONSTRAINT [fk_lks_chi_tiet_xuat_kho]
GO
ALTER TABLE [dbo].[linh_kien_serial]  WITH CHECK ADD  CONSTRAINT [fk_lks_kho] FOREIGN KEY([kho_id])
REFERENCES [dbo].[kho] ([id])
GO
ALTER TABLE [dbo].[linh_kien_serial] CHECK CONSTRAINT [fk_lks_kho]
GO
ALTER TABLE [dbo].[linh_kien_serial]  WITH CHECK ADD  CONSTRAINT [fk_lks_linh_kien] FOREIGN KEY([linh_kien_id])
REFERENCES [dbo].[linh_kien] ([id])
GO
ALTER TABLE [dbo].[linh_kien_serial] CHECK CONSTRAINT [fk_lks_linh_kien]
GO
ALTER TABLE [dbo].[linh_kien_serial]  WITH CHECK ADD  CONSTRAINT [fk_lks_phieu_nhap] FOREIGN KEY([phieu_nhap_id])
REFERENCES [dbo].[phieu_nhap] ([id])
GO
ALTER TABLE [dbo].[linh_kien_serial] CHECK CONSTRAINT [fk_lks_phieu_nhap]
GO
ALTER TABLE [dbo].[nhan_vien]  WITH CHECK ADD  CONSTRAINT [fk_nhan_vien_tai_khoan] FOREIGN KEY([tai_khoan_id], [chuc_vu])
REFERENCES [dbo].[tai_khoan] ([id], [vai_tro])
GO
ALTER TABLE [dbo].[nhan_vien] CHECK CONSTRAINT [fk_nhan_vien_tai_khoan]
GO
ALTER TABLE [dbo].[phieu_nhap]  WITH CHECK ADD  CONSTRAINT [fk_phieu_nhap_kho] FOREIGN KEY([kho_id])
REFERENCES [dbo].[kho] ([id])
GO
ALTER TABLE [dbo].[phieu_nhap] CHECK CONSTRAINT [fk_phieu_nhap_kho]
GO
ALTER TABLE [dbo].[phieu_nhap]  WITH CHECK ADD  CONSTRAINT [fk_phieu_nhap_ncc] FOREIGN KEY([nha_cung_cap_id])
REFERENCES [dbo].[nha_cung_cap] ([id])
GO
ALTER TABLE [dbo].[phieu_nhap] CHECK CONSTRAINT [fk_phieu_nhap_ncc]
GO
ALTER TABLE [dbo].[phieu_nhap]  WITH CHECK ADD  CONSTRAINT [fk_phieu_nhap_thu_kho] FOREIGN KEY([thu_kho_id])
REFERENCES [dbo].[thu_kho] ([nhan_vien_id])
GO
ALTER TABLE [dbo].[phieu_nhap] CHECK CONSTRAINT [fk_phieu_nhap_thu_kho]
GO
ALTER TABLE [dbo].[phieu_sua_chua]  WITH CHECK ADD  CONSTRAINT [fk_phieu_ktv] FOREIGN KEY([ky_thuat_vien_id])
REFERENCES [dbo].[ky_thuat_vien] ([nhan_vien_id])
GO
ALTER TABLE [dbo].[phieu_sua_chua] CHECK CONSTRAINT [fk_phieu_ktv]
GO
ALTER TABLE [dbo].[phieu_sua_chua]  WITH CHECK ADD  CONSTRAINT [fk_phieu_le_tan] FOREIGN KEY([le_tan_id])
REFERENCES [dbo].[le_tan] ([nhan_vien_id])
GO
ALTER TABLE [dbo].[phieu_sua_chua] CHECK CONSTRAINT [fk_phieu_le_tan]
GO
ALTER TABLE [dbo].[phieu_sua_chua]  WITH CHECK ADD  CONSTRAINT [fk_phieu_thiet_bi] FOREIGN KEY([thiet_bi_id])
REFERENCES [dbo].[thiet_bi] ([id])
GO
ALTER TABLE [dbo].[phieu_sua_chua] CHECK CONSTRAINT [fk_phieu_thiet_bi]
GO
ALTER TABLE [dbo].[refresh_token]  WITH CHECK ADD  CONSTRAINT [fk_refresh_token_tai_khoan] FOREIGN KEY([tai_khoan_id])
REFERENCES [dbo].[tai_khoan] ([id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[refresh_token] CHECK CONSTRAINT [fk_refresh_token_tai_khoan]
GO
ALTER TABLE [dbo].[ton_kho]  WITH CHECK ADD  CONSTRAINT [fk_ton_kho_kho] FOREIGN KEY([kho_id])
REFERENCES [dbo].[kho] ([id])
GO
ALTER TABLE [dbo].[ton_kho] CHECK CONSTRAINT [fk_ton_kho_kho]
GO
ALTER TABLE [dbo].[ton_kho]  WITH CHECK ADD  CONSTRAINT [fk_ton_kho_linh_kien] FOREIGN KEY([linh_kien_id])
REFERENCES [dbo].[linh_kien] ([id])
GO
ALTER TABLE [dbo].[ton_kho] CHECK CONSTRAINT [fk_ton_kho_linh_kien]
GO
ALTER TABLE [dbo].[thiet_bi]  WITH CHECK ADD  CONSTRAINT [fk_thiet_bi_khach_hang] FOREIGN KEY([khach_hang_id])
REFERENCES [dbo].[khach_hang] ([id])
GO
ALTER TABLE [dbo].[thiet_bi] CHECK CONSTRAINT [fk_thiet_bi_khach_hang]
GO
ALTER TABLE [dbo].[thu_kho]  WITH CHECK ADD  CONSTRAINT [fk_thu_kho_nhan_vien] FOREIGN KEY([nhan_vien_id], [chuc_vu])
REFERENCES [dbo].[nhan_vien] ([id], [chuc_vu])
GO
ALTER TABLE [dbo].[thu_kho] CHECK CONSTRAINT [fk_thu_kho_nhan_vien]
GO
ALTER TABLE [dbo].[anh_phieu_sua_chua]  WITH CHECK ADD CHECK  (([loai]='KHAC' OR [loai]='SAU_SUA' OR [loai]='LUC_TIEP_NHAN'))
GO
ALTER TABLE [dbo].[audit_log]  WITH CHECK ADD CHECK  (([du_lieu_cu] IS NULL OR isjson([du_lieu_cu])=(1)))
GO
ALTER TABLE [dbo].[audit_log]  WITH CHECK ADD CHECK  (([du_lieu_moi] IS NULL OR isjson([du_lieu_moi])=(1)))
GO
ALTER TABLE [dbo].[audit_log]  WITH CHECK ADD CHECK  (([hanh_dong]='DELETE' OR [hanh_dong]='UPDATE' OR [hanh_dong]='INSERT'))
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD CHECK  (([loai]='DIEU_CHINH' OR [loai]='XUAT' OR [loai]='NHAP'))
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD CHECK  (([so_luong_thay_doi]<>(0)))
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD CHECK  (([so_luong_sau]>=(0)))
GO
ALTER TABLE [dbo].[bien_dong_kho]  WITH CHECK ADD  CONSTRAINT [ck_bien_dong_chung_tu] CHECK  (([loai]='NHAP' AND [so_luong_thay_doi]>(0) AND [phieu_nhap_id] IS NOT NULL AND [chi_tiet_xuat_kho_id] IS NULL OR [loai]='XUAT' AND [so_luong_thay_doi]<(0) AND [chi_tiet_xuat_kho_id] IS NOT NULL AND [phieu_nhap_id] IS NULL OR [loai]='DIEU_CHINH' AND [phieu_nhap_id] IS NULL AND [chi_tiet_xuat_kho_id] IS NULL AND [ly_do] IS NOT NULL))
GO
ALTER TABLE [dbo].[bien_dong_kho] CHECK CONSTRAINT [ck_bien_dong_chung_tu]
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu]  WITH CHECK ADD CHECK  (([don_gia_thuc_te]>=(0)))
GO
ALTER TABLE [dbo].[chi_tiet_dich_vu]  WITH CHECK ADD CHECK  (([so_luong]>(0)))
GO
ALTER TABLE [dbo].[chi_tiet_nhap_kho]  WITH CHECK ADD CHECK  (([don_gia_nhap]>=(0)))
GO
ALTER TABLE [dbo].[chi_tiet_nhap_kho]  WITH CHECK ADD CHECK  (([so_luong_nhap]>(0)))
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD CHECK  (([don_gia_xuat]>=(0)))
GO
ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD CHECK  (([so_luong_xuat]>(0)))
GO
ALTER TABLE [dbo].[dich_vu]  WITH CHECK ADD CHECK  (([gia_niem_yet]>=(0)))
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD CHECK  (([giam_gia]>=(0)))
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD CHECK  (([phuong_thuc_tt]='VI_DIEN_TU' OR [phuong_thuc_tt]='THE' OR [phuong_thuc_tt]='CHUYEN_KHOAN' OR [phuong_thuc_tt]='TIEN_MAT'))
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD CHECK  (([tien_dich_vu]>=(0)))
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD CHECK  (([tien_linh_kien]>=(0)))
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD CHECK  (([tong_thanh_toan]>=(0)))
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD CHECK  (([trang_thai]='HUY' OR [trang_thai]='DA_THANH_TOAN' OR [trang_thai]='CHUA_THANH_TOAN'))
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD  CONSTRAINT [ck_hoa_don_da_tt] CHECK  (([trang_thai]<>'DA_THANH_TOAN' OR [ngay_thanh_toan] IS NOT NULL AND [phuong_thuc_tt] IS NOT NULL))
GO
ALTER TABLE [dbo].[hoa_don] CHECK CONSTRAINT [ck_hoa_don_da_tt]
GO
ALTER TABLE [dbo].[hoa_don]  WITH CHECK ADD  CONSTRAINT [ck_hoa_don_tong] CHECK  (([tong_thanh_toan]=(([tien_dich_vu]+[tien_linh_kien])-[giam_gia])))
GO
ALTER TABLE [dbo].[hoa_don] CHECK CONSTRAINT [ck_hoa_don_tong]
GO
ALTER TABLE [dbo].[ky_thuat_vien]  WITH CHECK ADD CHECK  (([chuc_vu]='KY_THUAT_VIEN'))
GO
ALTER TABLE [dbo].[khach_hang]  WITH CHECK ADD CHECK  (([email] like '%_@_%._%' AND NOT [email] like '% %'))
GO
ALTER TABLE [dbo].[khach_hang]  WITH CHECK ADD CHECK  ((NOT [so_dien_thoai] like '%[^0-9+ ]%' AND (len([so_dien_thoai])>=(8) AND len([so_dien_thoai])<=(15))))
GO
ALTER TABLE [dbo].[khach_hang]  WITH CHECK ADD CHECK  (([vai_tro_tk]='KHACH_HANG'))
GO
ALTER TABLE [dbo].[le_tan]  WITH CHECK ADD CHECK  (([chuc_vu]='LE_TAN'))
GO
ALTER TABLE [dbo].[linh_kien]  WITH CHECK ADD CHECK  (([don_gia_ban]>=(0)))
GO
ALTER TABLE [dbo].[linh_kien]  WITH CHECK ADD CHECK  (([thoi_han_bao_hanh_thang]>=(0)))
GO
ALTER TABLE [dbo].[linh_kien_serial]  WITH CHECK ADD  CONSTRAINT [ck_trang_thai_lks] CHECK  (([trang_thai]='THANH_LY' OR [trang_thai]='BAO_HANH' OR [trang_thai]='DA_XUAT' OR [trang_thai]='TRONG_KHO'))
GO
ALTER TABLE [dbo].[linh_kien_serial] CHECK CONSTRAINT [ck_trang_thai_lks]
GO
ALTER TABLE [dbo].[nha_cung_cap]  WITH CHECK ADD CHECK  (([email] like '%_@_%._%' AND NOT [email] like '% %'))
GO
ALTER TABLE [dbo].[nhan_vien]  WITH CHECK ADD CHECK  (([chuc_vu]='THU_KHO' OR [chuc_vu]='KY_THUAT_VIEN' OR [chuc_vu]='LE_TAN' OR [chuc_vu]='ADMIN'))
GO
ALTER TABLE [dbo].[nhan_vien]  WITH CHECK ADD CHECK  (([email] like '%_@_%._%' AND NOT [email] like '% %'))
GO
ALTER TABLE [dbo].[nhan_vien]  WITH CHECK ADD CHECK  ((NOT [so_dien_thoai] like '%[^0-9+ ]%' AND (len([so_dien_thoai])>=(8) AND len([so_dien_thoai])<=(15))))
GO
ALTER TABLE [dbo].[phieu_nhap]  WITH CHECK ADD CHECK  (([trang_thai]='HUY' OR [trang_thai]='DA_NHAP_KHO' OR [trang_thai]='NHAP'))
GO
ALTER TABLE [dbo].[phieu_sua_chua]  WITH CHECK ADD CHECK  (([trang_thai]='HUY' OR [trang_thai]='DA_TRA' OR [trang_thai]='HOAN_THANH' OR [trang_thai]='CHO_LINH_KIEN' OR [trang_thai]='DANG_SUA' OR [trang_thai]='TIEP_NHAN'))
GO
ALTER TABLE [dbo].[phieu_sua_chua]  WITH CHECK ADD  CONSTRAINT [ck_phieu_co_ktv] CHECK  (([trang_thai]='HUY' OR [trang_thai]='TIEP_NHAN' OR [ky_thuat_vien_id] IS NOT NULL))
GO
ALTER TABLE [dbo].[phieu_sua_chua] CHECK CONSTRAINT [ck_phieu_co_ktv]
GO
ALTER TABLE [dbo].[phieu_sua_chua]  WITH CHECK ADD  CONSTRAINT [ck_phieu_ly_do_huy] CHECK  (([trang_thai]<>'HUY' OR [ly_do_huy] IS NOT NULL))
GO
ALTER TABLE [dbo].[phieu_sua_chua] CHECK CONSTRAINT [ck_phieu_ly_do_huy]
GO
ALTER TABLE [dbo].[phieu_sua_chua]  WITH CHECK ADD  CONSTRAINT [ck_phieu_ngay_hen] CHECK  (([ngay_hen_tra] IS NULL OR [ngay_hen_tra]>=[ngay_nhan]))
GO
ALTER TABLE [dbo].[phieu_sua_chua] CHECK CONSTRAINT [ck_phieu_ngay_hen]
GO
ALTER TABLE [dbo].[tai_khoan]  WITH CHECK ADD CHECK  (([so_lan_dang_nhap_sai]>=(0)))
GO
ALTER TABLE [dbo].[tai_khoan]  WITH CHECK ADD CHECK  (([vai_tro]='KHACH_HANG' OR [vai_tro]='THU_KHO' OR [vai_tro]='KY_THUAT_VIEN' OR [vai_tro]='LE_TAN' OR [vai_tro]='ADMIN'))
GO
ALTER TABLE [dbo].[ton_kho]  WITH CHECK ADD CHECK  (([muc_ton_toi_thieu]>=(0)))
GO
ALTER TABLE [dbo].[ton_kho]  WITH CHECK ADD CHECK  (([so_luong_ton]>=(0)))
GO
ALTER TABLE [dbo].[thu_kho]  WITH CHECK ADD CHECK  (([chuc_vu]='THU_KHO'))
GO
/****** Object:  StoredProcedure [dbo].[usp_lap_hoa_don]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ---------------------------------------------------------------------
-- 4. LẬP HÓA ĐƠN TỪ PHIẾU (chốt số tiền từ chi tiết hiện có)
--    Chỉ lập được khi phiếu HOAN_THANH. Hóa đơn sau đó bất biến về số tiền.
--    Lập trùng sẽ bị uq_hoa_don_phieu_hieu_luc chặn.
-- ---------------------------------------------------------------------
CREATE PROCEDURE [dbo].[usp_lap_hoa_don]
    @phieu_id     BIGINT,
    @le_tan_id    BIGINT,
    @giam_gia     DECIMAL(14,2) = 0,
    @hoa_don_id   BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @own BIT = CASE WHEN @@TRANCOUNT = 0 THEN 1 ELSE 0 END;
    DECLARE @trang_thai VARCHAR(15), @dv DECIMAL(14,2), @lk DECIMAL(14,2), @msg NVARCHAR(400);

    BEGIN TRY
        IF @own = 1 BEGIN TRANSACTION;

        SELECT @trang_thai = trang_thai
          FROM phieu_sua_chua WITH (UPDLOCK, ROWLOCK)
         WHERE id = @phieu_id;
        IF @@ROWCOUNT = 0
        BEGIN
            SET @msg = CONCAT(N'Không tìm thấy phiếu sửa chữa id=', @phieu_id);
            THROW 50030, @msg, 1;
        END;
        IF @trang_thai <> 'HOAN_THANH'
        BEGIN
            SET @msg = CONCAT(N'Chỉ lập hóa đơn cho phiếu đã hoàn thành (hiện: ', @trang_thai, N')');
            THROW 50031, @msg, 1;
        END;

        SELECT @dv = tien_dich_vu, @lk = tien_linh_kien
          FROM v_phieu_chi_phi WHERE phieu_id = @phieu_id;

        IF @giam_gia < 0 OR @giam_gia > @dv + @lk
        BEGIN
            SET @msg = CONCAT(N'Giảm giá không hợp lệ (0 .. ', @dv + @lk, N')');
            THROW 50032, @msg, 1;
        END;

        INSERT INTO hoa_don (phieu_id, le_tan_id, tien_dich_vu, tien_linh_kien, giam_gia, tong_thanh_toan)
        VALUES (@phieu_id, @le_tan_id, @dv, @lk, @giam_gia, @dv + @lk - @giam_gia);
        SET @hoa_don_id = SCOPE_IDENTITY();

        IF @own = 1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own = 1 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_xac_nhan_nhap_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ---------------------------------------------------------------------
-- 2. XÁC NHẬN NHẬP KHO
--    Cộng tồn + ghi sổ cái cho TỪNG dòng, rồi đóng phiếu. Một transaction.
--    Dùng được cả khi backend đã mở sẵn transaction (thủ tục không tự COMMIT
--    transaction của người gọi).
-- ---------------------------------------------------------------------
CREATE PROCEDURE [dbo].[usp_xac_nhan_nhap_kho]
    @phieu_nhap_id BIGINT,
    @nhan_vien_id  BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @own BIT = CASE WHEN @@TRANCOUNT = 0 THEN 1 ELSE 0 END;
    DECLARE @kho_id BIGINT, @trang_thai VARCHAR(15), @ma VARCHAR(30), @msg NVARCHAR(400);
    DECLARE @lk BIGINT, @sl INT, @sau INT;

    BEGIN TRY
        IF @own = 1 BEGIN TRANSACTION;

        -- Khóa phiếu để chống bấm xác nhận 2 lần cùng lúc
        SELECT @kho_id = kho_id, @trang_thai = trang_thai, @ma = ma_pn
          FROM phieu_nhap WITH (UPDLOCK, ROWLOCK)
         WHERE id = @phieu_nhap_id;
        IF @@ROWCOUNT = 0
        BEGIN
            SET @msg = CONCAT(N'Không tìm thấy phiếu nhập id=', @phieu_nhap_id);
            THROW 50001, @msg, 1;
        END;
        IF @trang_thai <> 'NHAP'
        BEGIN
            SET @msg = CONCAT(N'Phiếu nhập ', @ma, N' đang ở trạng thái ', @trang_thai, N', không thể xác nhận');
            THROW 50002, @msg, 1;
        END;
        IF NOT EXISTS (SELECT 1 FROM chi_tiet_nhap_kho WHERE phieu_nhap_id = @phieu_nhap_id)
        BEGIN
            SET @msg = CONCAT(N'Phiếu nhập ', @ma, N' chưa có dòng chi tiết nào');
            THROW 50003, @msg, 1;
        END;

        -- ORDER BY cố định thứ tự khóa dòng để tránh deadlock giữa các phiếu chạy song song
        DECLARE cur CURSOR LOCAL FAST_FORWARD FOR
            SELECT linh_kien_id, so_luong_nhap
              FROM chi_tiet_nhap_kho
             WHERE phieu_nhap_id = @phieu_nhap_id
             ORDER BY linh_kien_id;
        OPEN cur;
        FETCH NEXT FROM cur INTO @lk, @sl;
        WHILE @@FETCH_STATUS = 0
        BEGIN
            -- Upsert tồn kho: HOLDLOCK khóa cả khoảng khóa để hai phiếu cùng tạo dòng mới không đụng nhau
            UPDATE ton_kho WITH (UPDLOCK, HOLDLOCK)
               SET @sau = so_luong_ton = so_luong_ton + @sl,
                   ngay_cap_nhat = SYSUTCDATETIME()
             WHERE kho_id = @kho_id AND linh_kien_id = @lk;
            IF @@ROWCOUNT = 0
            BEGIN
                INSERT INTO ton_kho (kho_id, linh_kien_id, so_luong_ton) VALUES (@kho_id, @lk, @sl);
                SET @sau = @sl;
            END;

            INSERT INTO bien_dong_kho (kho_id, linh_kien_id, loai, so_luong_thay_doi, so_luong_sau,
                                       phieu_nhap_id, nguoi_thuc_hien_id)
            VALUES (@kho_id, @lk, 'NHAP', @sl, @sau, @phieu_nhap_id, @nhan_vien_id);

            FETCH NEXT FROM cur INTO @lk, @sl;
        END;
        CLOSE cur;
        DEALLOCATE cur;

        UPDATE phieu_nhap SET trang_thai = 'DA_NHAP_KHO' WHERE id = @phieu_nhap_id;

        IF @own = 1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own = 1 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_xuat_kho]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ---------------------------------------------------------------------
-- 3. XUẤT LINH KIỆN CHO PHIẾU SỬA CHỮA
--    Trừ tồn có điều kiện (so_luong_ton >= số xuất) trong MỘT câu UPDATE
--    => hai thợ cùng xuất một món không thể làm tồn âm.
--    Trả id dòng chi_tiet_xuat_kho vừa tạo qua @chi_tiet_id (OUTPUT).
-- ---------------------------------------------------------------------
CREATE PROCEDURE [dbo].[usp_xuat_kho]
    @phieu_id      BIGINT,
    @kho_id        BIGINT,
    @linh_kien_id  BIGINT,
    @so_luong      INT,
    @thu_kho_id    BIGINT,
    @chi_tiet_id   BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @own BIT = CASE WHEN @@TRANCOUNT = 0 THEN 1 ELSE 0 END;
    DECLARE @trang_thai VARCHAR(15), @sau INT, @gia DECIMAL(14,2), @bh SMALLINT, @msg NVARCHAR(400);

    BEGIN TRY
        IF @so_luong IS NULL OR @so_luong <= 0
            THROW 50020, N'Số lượng xuất phải lớn hơn 0', 1;

        IF @own = 1 BEGIN TRANSACTION;

        -- Khóa phiếu để trạng thái không bị đổi giữa chừng
        SELECT @trang_thai = trang_thai
          FROM phieu_sua_chua WITH (UPDLOCK, ROWLOCK)
         WHERE id = @phieu_id;
        IF @@ROWCOUNT = 0
        BEGIN
            SET @msg = CONCAT(N'Không tìm thấy phiếu sửa chữa id=', @phieu_id);
            THROW 50021, @msg, 1;
        END;
        IF @trang_thai NOT IN ('DANG_SUA', 'CHO_LINH_KIEN')
        BEGIN
            SET @msg = CONCAT(N'Phiếu đang ở trạng thái ', @trang_thai, N' nên không được xuất linh kiện');
            THROW 50022, @msg, 1;
        END;

        UPDATE ton_kho
           SET @sau = so_luong_ton = so_luong_ton - @so_luong,
               ngay_cap_nhat = SYSUTCDATETIME()
         WHERE kho_id = @kho_id
           AND linh_kien_id = @linh_kien_id
           AND so_luong_ton >= @so_luong;
        IF @@ROWCOUNT = 0
        BEGIN
            SET @msg = CONCAT(N'Kho ', @kho_id, N' không đủ tồn linh kiện ', @linh_kien_id,
                              N' để xuất ', @so_luong, N' cái');
            THROW 50023, @msg, 1;
        END;

        SELECT @gia = don_gia_ban, @bh = thoi_han_bao_hanh_thang
          FROM linh_kien WHERE id = @linh_kien_id;

        INSERT INTO chi_tiet_xuat_kho (phieu_id, kho_id, linh_kien_id, thu_kho_id,
                                       so_luong_xuat, don_gia_xuat, bao_hanh_den)
        VALUES (@phieu_id, @kho_id, @linh_kien_id, @thu_kho_id,
                @so_luong, @gia,
                CASE WHEN @bh > 0
                     THEN DATEADD(MONTH, @bh,
                                  CAST(SYSDATETIMEOFFSET() AT TIME ZONE N'SE Asia Standard Time' AS DATE))
                END);
        SET @chi_tiet_id = SCOPE_IDENTITY();

        INSERT INTO bien_dong_kho (kho_id, linh_kien_id, loai, so_luong_thay_doi, so_luong_sau,
                                   chi_tiet_xuat_kho_id, nguoi_thuc_hien_id)
        VALUES (@kho_id, @linh_kien_id, 'XUAT', -@so_luong, @sau, @chi_tiet_id, @thu_kho_id);

        IF @own = 1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own = 1 AND @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[usp_xuat_kho_serial]    Script Date: 11/4/2026 10:41:46 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 5. Tạo/Cập nhật Stored Procedure xuất kho serial (đã kèm Transaction chuẩn an toàn)
CREATE   PROCEDURE [dbo].[usp_xuat_kho_serial]
    @chi_tiet_xuat_kho_id BIGINT,
    @linh_kien_serial_id  BIGINT,
    @nhan_vien_id         BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (
            SELECT 1 FROM [dbo].[linh_kien_serial]
            WHERE [id] = @linh_kien_serial_id AND [trang_thai] = 'TRONG_KHO'
        )
            THROW 50010, N'Linh kiện serial này không còn trong kho hoặc đã được xuất.', 1;

        UPDATE [dbo].[linh_kien_serial]
        SET [trang_thai]           = 'DA_XUAT',
            [chi_tiet_xuat_kho_id] = @chi_tiet_xuat_kho_id,
            [kho_id]               = NULL,
            [updated_at]           = SYSUTCDATETIME()
        WHERE [id] = @linh_kien_serial_id;

        UPDATE [dbo].[chi_tiet_xuat_kho]
        SET [linh_kien_serial_id] = @linh_kien_serial_id
        WHERE [id] = @chi_tiet_xuat_kho_id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;
GO
GO
-- NÂNG CẤP V3: chọn đúng CSDL bản sao cần nâng cấp trước khi chạy.
-- File này chạy trong một giao dịch; không tự chọn hay tạo CSDL.
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
IF DB_NAME() IN (N'master',N'model',N'msdb',N'tempdb')
    THROW 51100, N'Hãy chọn CSDL ứng dụng; không chạy trong CSDL hệ thống.', 1;
IF @@TRANCOUNT <> 0
    THROW 51101, N'Hãy chạy migration khi không có giao dịch đang mở.', 1;
IF OBJECT_ID(N'dbo.v3_schema_migration',N'U') IS NOT NULL
    THROW 51102, N'CSDL đã có bảng phiên bản V3. Hãy kiểm tra trạng thái trước khi chạy lại.', 1;
IF OBJECT_ID(N'dbo.linh_kien',N'U') IS NULL OR OBJECT_ID(N'dbo.phieu_sua_chua',N'U') IS NULL
 OR OBJECT_ID(N'dbo.hoa_don',N'U') IS NULL OR OBJECT_ID(N'dbo.linh_kien_serial',N'U') IS NULL
 OR OBJECT_ID(N'dbo.chi_tiet_xuat_kho',N'U') IS NULL OR COL_LENGTH(N'dbo.chi_tiet_xuat_kho',N'linh_kien_serial_id') IS NULL
 OR COL_LENGTH(N'dbo.chi_tiet_nhap_kho',N'danh_sach_serial') IS NULL
    THROW 51103, N'CSDL không khớp bản QLSCTBDT.sql ngày 04/11/2026; không tự nâng cấp.', 1;
BEGIN TRY
    BEGIN TRANSACTION;
    -- Khối 1/30
    EXEC sys.sp_executesql N'-- NGUỒN SINH MIGRATION. Không chạy trực tiếp: chạy NANG_CAP_QLSCTBDT_V3.sql.
-- Các khối được bọc trong cùng một giao dịch bởi script build.

CREATE TABLE dbo.v3_schema_migration (
    version varchar(30) NOT NULL CONSTRAINT pk_v3_schema_migration PRIMARY KEY,
    applied_at datetime2(3) NOT NULL CONSTRAINT df_v3_migration_time DEFAULT SYSUTCDATETIME()
);';

    -- Khối 2/30
    EXEC sys.sp_executesql N'CREATE TABLE dbo.loai_linh_kien (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_loai_linh_kien PRIMARY KEY,
    ma varchar(40) NOT NULL CONSTRAINT uq_loai_linh_kien_ma UNIQUE,
    ten nvarchar(100) NOT NULL,
    hoat_dong bit NOT NULL CONSTRAINT df_loai_linh_kien_hd DEFAULT 1,
    created_at datetime2(3) NOT NULL CONSTRAINT df_loai_linh_kien_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT ck_loai_linh_kien_ten CHECK (LEN(LTRIM(RTRIM(ten))) > 0)
);

CREATE TABLE dbo.thuoc_tinh_loai_linh_kien (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_thuoc_tinh_loai_lk PRIMARY KEY,
    loai_linh_kien_id bigint NOT NULL,
    ma varchar(40) NOT NULL,
    ten nvarchar(100) NOT NULL,
    kieu_du_lieu varchar(15) NOT NULL,
    don_vi nvarchar(30) NULL,
    bat_buoc bit NOT NULL CONSTRAINT df_tt_loai_bat_buoc DEFAULT 0,
    thu_tu int NOT NULL CONSTRAINT df_tt_loai_thu_tu DEFAULT 0,
    hoat_dong bit NOT NULL CONSTRAINT df_tt_loai_hd DEFAULT 1,
    CONSTRAINT fk_tt_loai_lk FOREIGN KEY (loai_linh_kien_id) REFERENCES dbo.loai_linh_kien(id),
    CONSTRAINT uq_tt_loai_ma UNIQUE (loai_linh_kien_id, ma),
    CONSTRAINT ck_tt_loai_kieu CHECK (kieu_du_lieu IN (''VAN_BAN'',''SO'',''LUA_CHON'',''DUNG_SAI'')),
    CONSTRAINT ck_tt_loai_ten CHECK (LEN(LTRIM(RTRIM(ten))) > 0)
);

CREATE TABLE dbo.lua_chon_thuoc_tinh_linh_kien (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_lua_chon_tt_lk PRIMARY KEY,
    thuoc_tinh_id bigint NOT NULL,
    gia_tri nvarchar(150) NOT NULL,
    thu_tu int NOT NULL CONSTRAINT df_lua_chon_tt_thu_tu DEFAULT 0,
    hoat_dong bit NOT NULL CONSTRAINT df_lua_chon_tt_hd DEFAULT 1,
    CONSTRAINT fk_lua_chon_tt FOREIGN KEY (thuoc_tinh_id) REFERENCES dbo.thuoc_tinh_loai_linh_kien(id),
    CONSTRAINT uq_lua_chon_tt_gia_tri UNIQUE (thuoc_tinh_id, gia_tri),
    CONSTRAINT uq_lua_chon_tt_id_tt UNIQUE (id, thuoc_tinh_id),
    CONSTRAINT ck_lua_chon_tt_gia_tri CHECK (LEN(LTRIM(RTRIM(gia_tri))) > 0)
);

ALTER TABLE dbo.linh_kien ADD
    loai_linh_kien_id bigint NULL,
    ma_model nvarchar(100) NULL,
    -- NULL: hàng cũ/chưa quyết định; 0: theo số lượng; 1: bắt buộc serial khi app V3 áp dụng.
    theo_doi_serial bit NULL;';

    -- Khối 3/30
    EXEC sys.sp_executesql N'ALTER TABLE dbo.linh_kien ADD
    CONSTRAINT fk_linh_kien_loai_v3 FOREIGN KEY (loai_linh_kien_id) REFERENCES dbo.loai_linh_kien(id);

CREATE TABLE dbo.thong_so_linh_kien (
    linh_kien_id bigint NOT NULL,
    thuoc_tinh_id bigint NOT NULL,
    gia_tri_van_ban nvarchar(500) NULL,
    gia_tri_so decimal(18,4) NULL,
    gia_tri_dung_sai bit NULL,
    lua_chon_id bigint NULL,
    CONSTRAINT pk_thong_so_linh_kien PRIMARY KEY (linh_kien_id, thuoc_tinh_id),
    CONSTRAINT fk_ts_lk FOREIGN KEY (linh_kien_id) REFERENCES dbo.linh_kien(id),
    CONSTRAINT fk_ts_tt FOREIGN KEY (thuoc_tinh_id) REFERENCES dbo.thuoc_tinh_loai_linh_kien(id),
    CONSTRAINT fk_ts_lua_chon FOREIGN KEY (lua_chon_id, thuoc_tinh_id)
        REFERENCES dbo.lua_chon_thuoc_tinh_linh_kien(id, thuoc_tinh_id),
    CONSTRAINT ck_ts_mot_gia_tri CHECK (
        (CASE WHEN gia_tri_van_ban IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN gia_tri_so IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN gia_tri_dung_sai IS NOT NULL THEN 1 ELSE 0 END +
         CASE WHEN lua_chon_id IS NOT NULL THEN 1 ELSE 0 END) = 1),
    CONSTRAINT ck_ts_van_ban CHECK (gia_tri_van_ban IS NULL OR LEN(LTRIM(RTRIM(gia_tri_van_ban))) > 0)
);';

    -- Khối 4/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_thong_so_kieu ON dbo.thong_so_linh_kien
AFTER INSERT, UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN dbo.linh_kien lk ON lk.id=i.linh_kien_id
        JOIN dbo.thuoc_tinh_loai_linh_kien t ON t.id=i.thuoc_tinh_id
        WHERE lk.loai_linh_kien_id IS NULL OR lk.loai_linh_kien_id<>t.loai_linh_kien_id)
        THROW 51007, N''Thông số không thuộc loại của linh kiện.'', 1;
    IF EXISTS (
        SELECT 1 FROM inserted i JOIN dbo.thuoc_tinh_loai_linh_kien t ON t.id=i.thuoc_tinh_id
        WHERE NOT ((t.kieu_du_lieu=''VAN_BAN'' AND i.gia_tri_van_ban IS NOT NULL)
                OR (t.kieu_du_lieu=''SO'' AND i.gia_tri_so IS NOT NULL)
                OR (t.kieu_du_lieu=''DUNG_SAI'' AND i.gia_tri_dung_sai IS NOT NULL)
                OR (t.kieu_du_lieu=''LUA_CHON'' AND i.lua_chon_id IS NOT NULL)))
        THROW 51001, N''Giá trị thông số không khớp kiểu dữ liệu của loại linh kiện.'', 1;
END;';

    -- Khối 5/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_linh_kien_khoa_loai ON dbo.linh_kien
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(loai_linh_kien_id) AND EXISTS (
        SELECT 1 FROM inserted i JOIN deleted d ON d.id=i.id
        WHERE (i.loai_linh_kien_id<>d.loai_linh_kien_id
               OR (i.loai_linh_kien_id IS NULL AND d.loai_linh_kien_id IS NOT NULL)
               OR (i.loai_linh_kien_id IS NOT NULL AND d.loai_linh_kien_id IS NULL))
          AND EXISTS (SELECT 1 FROM dbo.thong_so_linh_kien s WHERE s.linh_kien_id=i.id))
        THROW 51008, N''Linh kiện đã có thông số: không thể đổi loại.'', 1;
END;';

    -- Khối 6/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_thuoc_tinh_khoa_kieu ON dbo.thuoc_tinh_loai_linh_kien
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON d.id=i.id
        WHERE (i.kieu_du_lieu<>d.kieu_du_lieu OR i.loai_linh_kien_id<>d.loai_linh_kien_id)
          AND (EXISTS (SELECT 1 FROM dbo.thong_so_linh_kien s WHERE s.thuoc_tinh_id=i.id)
            OR EXISTS (SELECT 1 FROM dbo.lua_chon_thuoc_tinh_linh_kien c WHERE c.thuoc_tinh_id=i.id)))
        THROW 51005, N''Thuộc tính đang được dùng: tạo thuộc tính mới nếu cần đổi loại hoặc kiểu.'', 1;
END;';

    -- Khối 7/30
    EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.usp_v3_kiem_tra_linh_kien @linh_kien_id bigint
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.linh_kien WHERE id=@linh_kien_id AND deleted_at IS NULL)
        THROW 51002, N''Không tìm thấy linh kiện.'', 1;
    IF EXISTS (SELECT 1 FROM dbo.linh_kien WHERE id=@linh_kien_id AND (loai_linh_kien_id IS NULL OR theo_doi_serial IS NULL))
        THROW 51003, N''Cần chọn loại linh kiện và chính sách theo dõi serial.'', 1;
    IF EXISTS (
        SELECT 1 FROM dbo.linh_kien lk
        JOIN dbo.thuoc_tinh_loai_linh_kien t ON t.loai_linh_kien_id=lk.loai_linh_kien_id
        WHERE lk.id=@linh_kien_id AND t.bat_buoc=1 AND t.hoat_dong=1
          AND NOT EXISTS (SELECT 1 FROM dbo.thong_so_linh_kien s WHERE s.linh_kien_id=lk.id AND s.thuoc_tinh_id=t.id))
        THROW 51004, N''Chưa nhập đủ thông số bắt buộc của loại linh kiện.'', 1;
END;';

    -- Khối 8/30
    EXEC sys.sp_executesql N'INSERT INTO dbo.loai_linh_kien(ma,ten) VALUES (''RAM'',N''RAM''),(''QUAT_LAPTOP'',N''Quạt laptop''),(''MAN_HINH'',N''Màn hình'');
INSERT INTO dbo.thuoc_tinh_loai_linh_kien(loai_linh_kien_id,ma,ten,kieu_du_lieu,don_vi,thu_tu)
SELECT l.id, x.ma, x.ten, x.kieu, x.don_vi, x.thu_tu
FROM dbo.loai_linh_kien l JOIN (VALUES
 (''RAM'',''DUNG_LUONG'',N''Dung lượng'',''LUA_CHON'',N''GB'',10),
 (''RAM'',''CHUAN_DDR'',N''Chuẩn DDR'',''LUA_CHON'',NULL,20),
 (''RAM'',''TOC_DO'',N''Tốc độ'',''SO'',N''MT/s'',30),
 (''RAM'',''KIEU_KHE'',N''Kiểu khe'',''LUA_CHON'',NULL,40),
 (''QUAT_LAPTOP'',''MA_PART'',N''Mã part'',''VAN_BAN'',NULL,10),
 (''QUAT_LAPTOP'',''DONG_MAY'',N''Dòng máy tương thích'',''VAN_BAN'',NULL,20),
 (''QUAT_LAPTOP'',''DAU_CAM'',N''Đầu cắm'',''VAN_BAN'',NULL,30),
 (''MAN_HINH'',''KICH_THUOC'',N''Kích thước'',''SO'',N''inch'',10),
 (''MAN_HINH'',''DO_PHAN_GIAI'',N''Độ phân giải'',''VAN_BAN'',NULL,20),
 (''MAN_HINH'',''SO_CHAN_CAP'',N''Số chân cáp'',''SO'',N''chân'',30)
) x(loai,ma,ten,kieu,don_vi,thu_tu) ON x.loai=l.ma;
INSERT INTO dbo.lua_chon_thuoc_tinh_linh_kien(thuoc_tinh_id,gia_tri,thu_tu)
SELECT t.id, x.gia_tri, x.thu_tu FROM dbo.thuoc_tinh_loai_linh_kien t
JOIN dbo.loai_linh_kien l ON l.id=t.loai_linh_kien_id
JOIN (VALUES
 (''DUNG_LUONG'',N''4'',10),(''DUNG_LUONG'',N''8'',20),(''DUNG_LUONG'',N''16'',30),(''DUNG_LUONG'',N''32'',40),(''DUNG_LUONG'',N''64'',50),
 (''CHUAN_DDR'',N''DDR3'',10),(''CHUAN_DDR'',N''DDR4'',20),(''CHUAN_DDR'',N''DDR5'',30),
 (''KIEU_KHE'',N''SO-DIMM'',10),(''KIEU_KHE'',N''DIMM'',20)
) x(ma,gia_tri,thu_tu) ON x.ma=t.ma WHERE l.ma=''RAM'';';

    -- Khối 9/30
    EXEC sys.sp_executesql N'-- Trigger này bảo đảm mẫu đã bật theo dõi serial không được xác nhận nếu thiếu danh sách.
CREATE OR ALTER TRIGGER dbo.trg_v3_phieu_nhap_serial_bat_buoc ON dbo.phieu_nhap
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i JOIN deleted d ON d.id=i.id
        JOIN dbo.chi_tiet_nhap_kho c ON c.phieu_nhap_id=i.id
        JOIN dbo.linh_kien lk ON lk.id=c.linh_kien_id
        OUTER APPLY (SELECT COUNT(*) AS so_serial,
            COUNT(DISTINCT UPPER(LTRIM(RTRIM(CONVERT(nvarchar(150), j.value))))) AS so_khac_nhau,
            SUM(CASE WHEN j.type<>1 OR LEN(LTRIM(RTRIM(CONVERT(nvarchar(max),j.value)))) NOT BETWEEN 1 AND 150 THEN 1 ELSE 0 END) AS so_khong_hop_le
          FROM OPENJSON(CASE WHEN ISJSON(c.danh_sach_serial)=1 AND LEFT(LTRIM(c.danh_sach_serial),1)=''[''
                             THEN c.danh_sach_serial ELSE N''[]'' END) j) ds
        WHERE i.trang_thai=''DA_NHAP_KHO'' AND d.trang_thai<>''DA_NHAP_KHO'' AND lk.theo_doi_serial=1
          AND (ISJSON(c.danh_sach_serial)<>1 OR c.danh_sach_serial IS NULL OR LEFT(LTRIM(c.danh_sach_serial),1)<>''[''
               OR ds.so_serial<>c.so_luong_nhap OR ds.so_khac_nhau<>ds.so_serial OR COALESCE(ds.so_khong_hop_le,0)>0))
        THROW 51006, N''Linh kiện bắt buộc theo dõi serial: phải nhập đúng số serial duy nhất, hợp lệ.'', 1;
END;';

    -- Khối 10/30
    EXEC sys.sp_executesql N'CREATE TABLE dbo.yeu_cau_linh_kien_v3 (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_yeu_cau_lk_v3 PRIMARY KEY,
    phieu_id bigint NOT NULL,
    nguoi_yeu_cau_id bigint NOT NULL,
    ngay_yeu_cau datetime2(3) NOT NULL CONSTRAINT df_yclk_ngay DEFAULT SYSUTCDATETIME(),
    trang_thai varchar(15) NOT NULL CONSTRAINT df_yclk_status DEFAULT ''CHO_DUYET'',
    nguoi_duyet_id bigint NULL,
    ngay_duyet datetime2(3) NULL,
    ghi_chu nvarchar(500) NULL,
    CONSTRAINT fk_yclk_phieu FOREIGN KEY(phieu_id) REFERENCES dbo.phieu_sua_chua(id),
    CONSTRAINT fk_yclk_nguoi FOREIGN KEY(nguoi_yeu_cau_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT fk_yclk_duyet FOREIGN KEY(nguoi_duyet_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT uq_yclk_id_phieu UNIQUE(id,phieu_id),
    CONSTRAINT ck_yclk_status CHECK (trang_thai IN (''CHO_DUYET'',''DA_DUYET'',''TU_CHOI'',''HUY'')),
    CONSTRAINT ck_yclk_duyet CHECK (trang_thai NOT IN (''DA_DUYET'',''TU_CHOI'') OR (nguoi_duyet_id IS NOT NULL AND ngay_duyet IS NOT NULL))
);
CREATE TABLE dbo.chi_tiet_yeu_cau_linh_kien_v3 (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_ct_yclk_v3 PRIMARY KEY,
    yeu_cau_id bigint NOT NULL,
    phieu_id bigint NOT NULL,
    linh_kien_id bigint NOT NULL,
    so_luong_yeu_cau int NOT NULL,
    so_luong_duyet int NULL,
    ghi_chu nvarchar(500) NULL,
    CONSTRAINT fk_ct_yclk_phieu FOREIGN KEY(yeu_cau_id,phieu_id) REFERENCES dbo.yeu_cau_linh_kien_v3(id,phieu_id),
    CONSTRAINT fk_ct_yclk_lk FOREIGN KEY(linh_kien_id) REFERENCES dbo.linh_kien(id),
    CONSTRAINT uq_ct_yclk_id_phieu_lk UNIQUE(id,phieu_id,linh_kien_id),
    CONSTRAINT uq_ct_yclk_lk UNIQUE(yeu_cau_id,linh_kien_id),
    CONSTRAINT ck_ct_yclk_sl CHECK (so_luong_yeu_cau>0 AND (so_luong_duyet IS NULL OR so_luong_duyet BETWEEN 0 AND so_luong_yeu_cau))
);
ALTER TABLE dbo.chi_tiet_xuat_kho ADD yeu_cau_chi_tiet_id bigint NULL;';

    -- Khối 11/30
    EXEC sys.sp_executesql N'ALTER TABLE dbo.chi_tiet_xuat_kho ADD CONSTRAINT fk_ctxk_yeu_cau_v3
    FOREIGN KEY(yeu_cau_chi_tiet_id,phieu_id,linh_kien_id)
    REFERENCES dbo.chi_tiet_yeu_cau_linh_kien_v3(id,phieu_id,linh_kien_id);

CREATE TABLE dbo.tra_linh_kien_kho_v3 (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_tra_lk_v3 PRIMARY KEY,
    chi_tiet_xuat_kho_id bigint NOT NULL,
    so_luong int NOT NULL,
    nguoi_thuc_hien_id bigint NOT NULL,
    thoi_gian datetime2(3) NOT NULL CONSTRAINT df_tra_lk_ngay DEFAULT SYSUTCDATETIME(),
    ly_do nvarchar(500) NOT NULL,
    CONSTRAINT fk_tra_lk_ctxk FOREIGN KEY(chi_tiet_xuat_kho_id) REFERENCES dbo.chi_tiet_xuat_kho(id),
    CONSTRAINT fk_tra_lk_nguoi FOREIGN KEY(nguoi_thuc_hien_id) REFERENCES dbo.thu_kho(nhan_vien_id),
    CONSTRAINT ck_tra_lk_sl CHECK (so_luong>0),
    CONSTRAINT ck_tra_lk_ly_do CHECK (LEN(LTRIM(RTRIM(ly_do)))>0)
);
CREATE INDEX ix_tra_lk_ctxk ON dbo.tra_linh_kien_kho_v3(chi_tiet_xuat_kho_id);
ALTER TABLE dbo.bien_dong_kho ADD tra_linh_kien_id bigint NULL;';

    -- Khối 12/30
    EXEC sys.sp_executesql N'ALTER TABLE dbo.bien_dong_kho ADD CONSTRAINT fk_bdk_tra_lk_v3
    FOREIGN KEY(tra_linh_kien_id) REFERENCES dbo.tra_linh_kien_kho_v3(id);
CREATE UNIQUE INDEX uq_bdk_tra_lk_v3 ON dbo.bien_dong_kho(tra_linh_kien_id) WHERE tra_linh_kien_id IS NOT NULL;';

    -- Khối 13/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_tra_linh_kien_khong_sua ON dbo.tra_linh_kien_kho_v3
AFTER UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    THROW 51050, N''Không sửa hoặc xóa phiếu trả đã ghi; cần xử lý bằng chứng từ điều chỉnh.'', 1;
END;';

    -- Khối 14/30
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
        SELECT @tra=COALESCE(SUM(so_luong),0) FROM dbo.tra_linh_kien_kho_v3 WHERE chi_tiet_xuat_kho_id=@chi_tiet_xuat_kho_id;
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
        INSERT dbo.tra_linh_kien_kho_v3(chi_tiet_xuat_kho_id,so_luong,nguoi_thuc_hien_id,ly_do)
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

    -- Khối 15/30
    EXEC sys.sp_executesql N'CREATE OR ALTER VIEW dbo.v_phieu_chi_phi AS
SELECT p.id AS phieu_id,
       COALESCE((SELECT SUM(d.thanh_tien) FROM dbo.chi_tiet_dich_vu d WHERE d.phieu_id=p.id),0) AS tien_dich_vu,
       COALESCE((SELECT SUM(CONVERT(decimal(19,2),x.so_luong_xuat-COALESCE(t.da_tra,0))*x.don_gia_xuat)
           FROM dbo.chi_tiet_xuat_kho x OUTER APPLY
             (SELECT SUM(r.so_luong) AS da_tra FROM dbo.tra_linh_kien_kho_v3 r WHERE r.chi_tiet_xuat_kho_id=x.id) t
           WHERE x.phieu_id=p.id),0) AS tien_linh_kien,
       COALESCE((SELECT SUM(d.thanh_tien) FROM dbo.chi_tiet_dich_vu d WHERE d.phieu_id=p.id),0)+
       COALESCE((SELECT SUM(CONVERT(decimal(19,2),x.so_luong_xuat-COALESCE(t.da_tra,0))*x.don_gia_xuat)
           FROM dbo.chi_tiet_xuat_kho x OUTER APPLY
             (SELECT SUM(r.so_luong) AS da_tra FROM dbo.tra_linh_kien_kho_v3 r WHERE r.chi_tiet_xuat_kho_id=x.id) t
           WHERE x.phieu_id=p.id),0) AS tong_tam_tinh
FROM dbo.phieu_sua_chua p;';

    -- Khối 16/30
    EXEC sys.sp_executesql N'CREATE TABLE dbo.bao_gia (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_bao_gia PRIMARY KEY,
    phieu_id bigint NOT NULL,
    phien_ban int NOT NULL,
    trang_thai varchar(15) NOT NULL CONSTRAINT df_bao_gia_trang_thai DEFAULT ''NHAP'',
    giam_gia decimal(14,2) NOT NULL CONSTRAINT df_bao_gia_giam_gia DEFAULT 0,
    nguoi_tao_id bigint NOT NULL,
    ngay_tao datetime2(3) NOT NULL CONSTRAINT df_bao_gia_ngay_tao DEFAULT SYSUTCDATETIME(),
    ngay_gui datetime2(3) NULL,
    ngay_phan_hoi datetime2(3) NULL,
    kenh_phan_hoi nvarchar(100) NULL,
    nguoi_ghi_nhan_id bigint NULL,
    noi_dung_phan_hoi nvarchar(1000) NULL,
    CONSTRAINT fk_bg_phieu FOREIGN KEY (phieu_id) REFERENCES dbo.phieu_sua_chua(id),
    CONSTRAINT fk_bg_nguoi_tao FOREIGN KEY (nguoi_tao_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT fk_bg_nguoi_ghi_nhan FOREIGN KEY (nguoi_ghi_nhan_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT uq_bao_gia_phien_ban UNIQUE (phieu_id,phien_ban),
    CONSTRAINT ck_bg_phien_ban CHECK (phien_ban>0),
    CONSTRAINT ck_bg_giam_gia CHECK (giam_gia>=0),
    CONSTRAINT ck_bg_trang_thai CHECK (trang_thai IN (''NHAP'',''CHO_DUYET'',''DA_DUYET'',''TU_CHOI'',''HUY'')),
    CONSTRAINT ck_bg_gui CHECK (trang_thai NOT IN (''CHO_DUYET'',''DA_DUYET'',''TU_CHOI'') OR ngay_gui IS NOT NULL),
    CONSTRAINT ck_bg_phan_hoi CHECK (trang_thai NOT IN (''DA_DUYET'',''TU_CHOI'') OR
       (ngay_phan_hoi IS NOT NULL AND kenh_phan_hoi IS NOT NULL AND LEN(LTRIM(RTRIM(kenh_phan_hoi)))>0 AND nguoi_ghi_nhan_id IS NOT NULL)),
    CONSTRAINT ck_bg_thoi_gian CHECK ((ngay_gui IS NULL OR ngay_gui>=ngay_tao) AND
       (ngay_phan_hoi IS NULL OR (ngay_gui IS NOT NULL AND ngay_phan_hoi>=ngay_gui)))
);
CREATE TABLE dbo.chi_tiet_bao_gia (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_chi_tiet_bao_gia PRIMARY KEY,
    bao_gia_id bigint NOT NULL,
    loai varchar(15) NOT NULL,
    dich_vu_id bigint NULL,
    linh_kien_id bigint NULL,
    ten_hang_muc nvarchar(250) NOT NULL,
    so_luong int NOT NULL,
    don_gia decimal(14,2) NOT NULL,
    thanh_tien AS CONVERT(decimal(19,2),CONVERT(decimal(19,2),so_luong)*don_gia) PERSISTED,
    CONSTRAINT fk_ctbg_bg FOREIGN KEY (bao_gia_id) REFERENCES dbo.bao_gia(id),
    CONSTRAINT fk_ctbg_dv FOREIGN KEY (dich_vu_id) REFERENCES dbo.dich_vu(id),
    CONSTRAINT fk_ctbg_lk FOREIGN KEY (linh_kien_id) REFERENCES dbo.linh_kien(id),
    CONSTRAINT ck_ctbg_loai CHECK ((loai=''DICH_VU'' AND dich_vu_id IS NOT NULL AND linh_kien_id IS NULL)
       OR (loai=''LINH_KIEN'' AND linh_kien_id IS NOT NULL AND dich_vu_id IS NULL)
       OR (loai=''KHAC'' AND linh_kien_id IS NULL AND dich_vu_id IS NULL)),
    CONSTRAINT ck_ctbg_ten CHECK (LEN(LTRIM(RTRIM(ten_hang_muc)))>0),
    CONSTRAINT ck_ctbg_gia CHECK (so_luong>0 AND don_gia>=0)
);
CREATE INDEX ix_ctbg_bao_gia ON dbo.chi_tiet_bao_gia(bao_gia_id);';

    -- Khối 17/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_chi_tiet_bao_gia_khoa ON dbo.chi_tiet_bao_gia
AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM (SELECT bao_gia_id FROM inserted UNION SELECT bao_gia_id FROM deleted) x
      JOIN dbo.bao_gia b ON b.id=x.bao_gia_id WHERE b.trang_thai<>''NHAP'')
        THROW 51010, N''Báo giá đã gửi: tạo phiên bản mới để thay đổi chi tiết.'', 1;
END;';

    -- Khối 18/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_bao_gia_chuyen_trang_thai ON dbo.bao_gia
AFTER INSERT, UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM inserted i LEFT JOIN deleted d ON d.id=i.id WHERE d.id IS NULL AND i.trang_thai<>''NHAP'')
        THROW 51013, N''Báo giá mới phải được tạo ở trạng thái nháp.'', 1;
    IF EXISTS (SELECT 1 FROM deleted d LEFT JOIN inserted i ON i.id=d.id
      WHERE (i.id IS NULL AND d.trang_thai<>''NHAP'')
         OR (i.id IS NOT NULL AND d.trang_thai IN (''DA_DUYET'',''TU_CHOI'',''HUY''))
         OR (i.id IS NOT NULL AND i.trang_thai<>d.trang_thai AND NOT
           ((d.trang_thai=''NHAP'' AND i.trang_thai IN (''CHO_DUYET'',''HUY''))
           OR (d.trang_thai=''CHO_DUYET'' AND i.trang_thai IN (''DA_DUYET'',''TU_CHOI'',''HUY'')))))
        THROW 51011, N''Chuyển trạng thái báo giá không hợp lệ hoặc báo giá đã kết thúc.'', 1;
    IF EXISTS (SELECT 1 FROM deleted d JOIN inserted i ON i.id=d.id WHERE d.trang_thai=''CHO_DUYET''
        AND (i.phieu_id<>d.phieu_id OR i.phien_ban<>d.phien_ban OR i.giam_gia<>d.giam_gia OR
             i.nguoi_tao_id<>d.nguoi_tao_id OR i.ngay_tao<>d.ngay_tao OR i.ngay_gui<>d.ngay_gui))
        THROW 51014, N''Không được sửa nội dung báo giá đã gửi; hãy tạo phiên bản mới.'', 1;
    IF EXISTS (SELECT 1 FROM inserted i WHERE i.trang_thai IN (''CHO_DUYET'',''DA_DUYET'') AND
       (NOT EXISTS (SELECT 1 FROM dbo.chi_tiet_bao_gia c WHERE c.bao_gia_id=i.id)
       OR i.giam_gia>(SELECT COALESCE(SUM(c.thanh_tien),0) FROM dbo.chi_tiet_bao_gia c WHERE c.bao_gia_id=i.id)))
        THROW 51012, N''Báo giá phải có chi tiết và mức giảm không vượt tổng tiền.'', 1;
END;';

    -- Khối 19/30
    EXEC sys.sp_executesql N'CREATE OR ALTER VIEW dbo.v_v3_bao_gia AS
SELECT b.id,b.phieu_id,b.phien_ban,b.trang_thai,b.giam_gia,
       COALESCE(SUM(c.thanh_tien),0) AS tong_truoc_giam,
       COALESCE(SUM(c.thanh_tien),0)-b.giam_gia AS tong_sau_giam,
       b.ngay_tao,b.ngay_gui,b.ngay_phan_hoi,b.kenh_phan_hoi,b.nguoi_ghi_nhan_id
FROM dbo.bao_gia b LEFT JOIN dbo.chi_tiet_bao_gia c ON c.bao_gia_id=b.id
GROUP BY b.id,b.phieu_id,b.phien_ban,b.trang_thai,b.giam_gia,b.ngay_tao,b.ngay_gui,b.ngay_phan_hoi,b.kenh_phan_hoi,b.nguoi_ghi_nhan_id;';

    -- Khối 20/30
    EXEC sys.sp_executesql N'ALTER TABLE dbo.hoa_don ADD CONSTRAINT uq_hoa_don_id_phieu_v3 UNIQUE(id,phieu_id);
ALTER TABLE dbo.phieu_sua_chua ADD CONSTRAINT uq_phieu_id_thiet_bi_v3 UNIQUE(id,thiet_bi_id);
ALTER TABLE dbo.chi_tiet_xuat_kho ADD CONSTRAINT uq_ctxk_id_phieu_v3 UNIQUE(id,phieu_id);
ALTER TABLE dbo.linh_kien_serial ADD CONSTRAINT uq_lks_id_lk_v3 UNIQUE(id,linh_kien_id);
ALTER TABLE dbo.thiet_bi ADD hang_san_xuat nvarchar(100) NULL, model nvarchar(100) NULL;
-- Cho phép hóa đơn 0 đồng được hoàn tất mà không bịa phương thức thanh toán.
ALTER TABLE dbo.hoa_don DROP CONSTRAINT ck_hoa_don_da_tt;
ALTER TABLE dbo.hoa_don ADD CONSTRAINT ck_hoa_don_da_tt CHECK
    (trang_thai<>''DA_THANH_TOAN'' OR (ngay_thanh_toan IS NOT NULL AND (phuong_thuc_tt IS NOT NULL OR tong_thanh_toan=0)));

CREATE TABLE dbo.chi_tiet_hoa_don_v3 (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_chi_tiet_hoa_don_v3 PRIMARY KEY,
    hoa_don_id bigint NOT NULL,
    phieu_id bigint NOT NULL,
    loai varchar(15) NOT NULL,
    dich_vu_id bigint NULL,
    chi_tiet_xuat_kho_id bigint NULL,
    ten_hang_muc nvarchar(250) NOT NULL,
    so_luong int NOT NULL,
    don_gia decimal(14,2) NOT NULL,
    thanh_tien AS CONVERT(decimal(19,2),CONVERT(decimal(19,2),so_luong)*don_gia) PERSISTED,
    CONSTRAINT fk_cthd_hd_phieu FOREIGN KEY(hoa_don_id,phieu_id) REFERENCES dbo.hoa_don(id,phieu_id),
    CONSTRAINT fk_cthd_dv_phieu FOREIGN KEY(phieu_id,dich_vu_id) REFERENCES dbo.chi_tiet_dich_vu(phieu_id,dich_vu_id),
    CONSTRAINT fk_cthd_xuat_phieu FOREIGN KEY(chi_tiet_xuat_kho_id,phieu_id) REFERENCES dbo.chi_tiet_xuat_kho(id,phieu_id),
    CONSTRAINT ck_cthd_loai CHECK ((loai=''DICH_VU'' AND dich_vu_id IS NOT NULL AND chi_tiet_xuat_kho_id IS NULL)
        OR (loai=''LINH_KIEN'' AND dich_vu_id IS NULL AND chi_tiet_xuat_kho_id IS NOT NULL)),
    CONSTRAINT ck_cthd_gia CHECK (so_luong>0 AND don_gia>=0)
);
CREATE UNIQUE INDEX uq_cthd_dv ON dbo.chi_tiet_hoa_don_v3(hoa_don_id,dich_vu_id) WHERE dich_vu_id IS NOT NULL;
CREATE UNIQUE INDEX uq_cthd_xuat ON dbo.chi_tiet_hoa_don_v3(hoa_don_id,chi_tiet_xuat_kho_id) WHERE chi_tiet_xuat_kho_id IS NOT NULL;

-- Cột mat_khau_may cũ vẫn tồn tại để không làm hỏng backend cũ.
-- Bảng này chỉ chứa dữ liệu ĐÃ mã hóa ở tầng ứng dụng; không lưu khóa tại đây.
CREATE TABLE dbo.bi_mat_thiet_bi_v3 (
    phieu_id bigint NOT NULL CONSTRAINT pk_bi_mat_thiet_bi_v3 PRIMARY KEY,
    du_lieu_ma_hoa varbinary(max) NOT NULL,
    ma_khoa nvarchar(200) NOT NULL,
    ngay_tao datetime2(3) NOT NULL CONSTRAINT df_bi_mat_tao DEFAULT SYSUTCDATETIME(),
    het_han datetime2(3) NOT NULL,
    CONSTRAINT fk_bi_mat_phieu FOREIGN KEY(phieu_id) REFERENCES dbo.phieu_sua_chua(id),
    CONSTRAINT ck_bi_mat_han CHECK (het_han>ngay_tao AND DATALENGTH(du_lieu_ma_hoa)>0)
);

CREATE TABLE dbo.giao_dich_thanh_toan (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_giao_dich_thanh_toan PRIMARY KEY,
    phieu_id bigint NOT NULL,
    hoa_don_id bigint NULL,
    loai varchar(10) NOT NULL,
    so_tien decimal(14,2) NOT NULL,
    phuong_thuc varchar(15) NOT NULL,
    thoi_gian datetime2(3) NOT NULL CONSTRAINT df_gdtt_thoi_gian DEFAULT SYSUTCDATETIME(),
    nguoi_thuc_hien_id bigint NULL,
    nguon varchar(15) NOT NULL CONSTRAINT df_gdtt_nguon DEFAULT ''MOI'',
    ma_tham_chieu nvarchar(100) NULL,
    ghi_chu nvarchar(500) NULL,
    CONSTRAINT fk_gdtt_phieu FOREIGN KEY (phieu_id) REFERENCES dbo.phieu_sua_chua(id),
    CONSTRAINT fk_gdtt_hd_phieu FOREIGN KEY (hoa_don_id,phieu_id) REFERENCES dbo.hoa_don(id,phieu_id),
    CONSTRAINT fk_gdtt_nguoi FOREIGN KEY (nguoi_thuc_hien_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT ck_gdtt_loai CHECK (loai IN (''THU'',''HOAN'')),
    CONSTRAINT ck_gdtt_so_tien CHECK (so_tien>0),
    CONSTRAINT ck_gdtt_phuong_thuc CHECK (phuong_thuc IN (''TIEN_MAT'',''CHUYEN_KHOAN'',''THE'',''VI_DIEN_TU'')),
    CONSTRAINT ck_gdtt_nguon CHECK ((nguon=''MOI'' AND nguoi_thuc_hien_id IS NOT NULL) OR
                                    (nguon=''HOA_DON_CU'' AND hoa_don_id IS NOT NULL AND loai=''THU''))
);
CREATE INDEX ix_gdtt_phieu ON dbo.giao_dich_thanh_toan(phieu_id,thoi_gian);
CREATE UNIQUE INDEX uq_gdtt_hoa_don_cu ON dbo.giao_dich_thanh_toan(hoa_don_id) WHERE nguon=''HOA_DON_CU'';

-- Chỉ chuyển bản ghi tổng hợp khi hóa đơn cũ đã đánh dấu thanh toán đầy đủ.
-- Đây là số tổng từ hóa đơn, KHÔNG phải chứng từ từng lần thu.
INSERT INTO dbo.giao_dich_thanh_toan(phieu_id,hoa_don_id,loai,so_tien,phuong_thuc,thoi_gian,nguon,ghi_chu)
SELECT phieu_id,id,''THU'',tong_thanh_toan,phuong_thuc_tt,ngay_thanh_toan,''HOA_DON_CU'',
       N''Số tổng chuyển từ hóa đơn cũ đã thanh toán; cần đối chiếu chứng từ gốc.''
FROM dbo.hoa_don WHERE trang_thai=''DA_THANH_TOAN'' AND tong_thanh_toan>0;';

    -- Khối 21/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_giao_dich_khong_sua ON dbo.giao_dich_thanh_toan
AFTER UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    THROW 51025, N''Không sửa hoặc xóa giao dịch đã ghi; hãy ghi giao dịch hoàn hoặc điều chỉnh theo nghiệp vụ.'', 1;
END;';

    -- Khối 22/30
    EXEC sys.sp_executesql N'CREATE OR ALTER TRIGGER dbo.trg_v3_chi_tiet_hoa_don_khong_sua ON dbo.chi_tiet_hoa_don_v3
AFTER UPDATE, DELETE AS
BEGIN
    SET NOCOUNT ON;
    THROW 51026, N''Không sửa hoặc xóa chi tiết đã chốt trên hóa đơn.'', 1;
END;';

    -- Khối 23/30
    EXEC sys.sp_executesql N'CREATE TABLE dbo.ban_giao_thiet_bi (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_ban_giao_thiet_bi PRIMARY KEY,
    phieu_id bigint NOT NULL CONSTRAINT uq_ban_giao_phieu UNIQUE,
    nguoi_giao_id bigint NOT NULL,
    ten_nguoi_nhan nvarchar(150) NOT NULL,
    thoi_gian datetime2(3) NOT NULL CONSTRAINT df_ban_giao_thoi_gian DEFAULT SYSUTCDATETIME(),
    tinh_trang_may nvarchar(1000) NULL,
    phu_kien_da_tra nvarchar(1000) NULL,
    xac_nhan nvarchar(1000) NULL,
    con_no_luc_giao decimal(14,2) NOT NULL CONSTRAINT df_ban_giao_con_no DEFAULT 0,
    nguoi_duyet_con_no_id bigint NULL,
    ly_do_con_no nvarchar(500) NULL,
    CONSTRAINT fk_bgiao_phieu FOREIGN KEY(phieu_id) REFERENCES dbo.phieu_sua_chua(id),
    CONSTRAINT fk_bgiao_nguoi_giao FOREIGN KEY(nguoi_giao_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT fk_bgiao_nguoi_duyet FOREIGN KEY(nguoi_duyet_con_no_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT ck_bgiao_ten CHECK (LEN(LTRIM(RTRIM(ten_nguoi_nhan)))>0),
    CONSTRAINT ck_bgiao_con_no CHECK (con_no_luc_giao>=0 AND (con_no_luc_giao=0 OR
        (nguoi_duyet_con_no_id IS NOT NULL AND ly_do_con_no IS NOT NULL AND LEN(LTRIM(RTRIM(ly_do_con_no)))>0)))
);';

    -- Khối 24/30
    EXEC sys.sp_executesql N'CREATE OR ALTER VIEW dbo.v_v3_cong_no_phieu AS
SELECT p.id AS phieu_id, h.id AS hoa_don_id, h.tong_thanh_toan,
       COALESCE(t.da_thu,0) AS da_thu, COALESCE(t.da_hoan,0) AS da_hoan,
       COALESCE(t.da_thu,0)-COALESCE(t.da_hoan,0) AS da_thu_rong,
       CASE WHEN h.id IS NULL THEN NULL ELSE h.tong_thanh_toan-COALESCE(t.da_thu,0)+COALESCE(t.da_hoan,0) END AS con_no
FROM dbo.phieu_sua_chua p
LEFT JOIN dbo.hoa_don h ON h.phieu_id=p.id AND h.trang_thai<>''HUY''
OUTER APPLY (SELECT SUM(CASE WHEN g.loai=''THU'' THEN g.so_tien ELSE 0 END) AS da_thu,
                    SUM(CASE WHEN g.loai=''HOAN'' THEN g.so_tien ELSE 0 END) AS da_hoan
             FROM dbo.giao_dich_thanh_toan g WHERE g.phieu_id=p.id) t;';

    -- Khối 25/30
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
        INSERT dbo.chi_tiet_hoa_don_v3(hoa_don_id,phieu_id,loai,dich_vu_id,ten_hang_muc,so_luong,don_gia)
        SELECT @hoa_don_id,c.phieu_id,''DICH_VU'',c.dich_vu_id,d.ten_dich_vu,c.so_luong,c.don_gia_thuc_te
          FROM dbo.chi_tiet_dich_vu c JOIN dbo.dich_vu d ON d.id=c.dich_vu_id WHERE c.phieu_id=@phieu_id;
        INSERT dbo.chi_tiet_hoa_don_v3(hoa_don_id,phieu_id,loai,chi_tiet_xuat_kho_id,ten_hang_muc,so_luong,don_gia)
        SELECT @hoa_don_id,c.phieu_id,''LINH_KIEN'',c.id,l.ten_linh_kien,c.so_luong_xuat-COALESCE(t.da_tra,0),c.don_gia_xuat
          FROM dbo.chi_tiet_xuat_kho c JOIN dbo.linh_kien l ON l.id=c.linh_kien_id
          OUTER APPLY (SELECT SUM(r.so_luong) AS da_tra FROM dbo.tra_linh_kien_kho_v3 r WHERE r.chi_tiet_xuat_kho_id=c.id) t
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

    -- Khối 26/30
    EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.usp_v3_ghi_nhan_thanh_toan
    @phieu_id bigint, @hoa_don_id bigint = NULL, @loai varchar(10), @so_tien decimal(14,2),
    @phuong_thuc varchar(15), @nguoi_thuc_hien_id bigint, @ma_tham_chieu nvarchar(100)=NULL,
    @ghi_chu nvarchar(500)=NULL, @giao_dich_id bigint OUTPUT
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @own bit=CASE WHEN @@TRANCOUNT=0 THEN 1 ELSE 0 END;
    DECLARE @r decimal(19,2), @tong decimal(14,2), @active_hd bigint;
    BEGIN TRY
        IF @loai IS NULL OR @loai NOT IN (''THU'',''HOAN'') OR @so_tien IS NULL OR @so_tien<=0
            THROW 51020, N''Loại giao dịch hoặc số tiền không hợp lệ.'', 1;
        IF @own=1 BEGIN TRANSACTION;
        IF NOT EXISTS (SELECT 1 FROM dbo.phieu_sua_chua WITH (UPDLOCK,HOLDLOCK) WHERE id=@phieu_id)
            THROW 51021, N''Không tìm thấy phiếu sửa chữa.'', 1;
        SELECT @active_hd=id, @tong=tong_thanh_toan FROM dbo.hoa_don WHERE phieu_id=@phieu_id AND trang_thai<>''HUY'';
        IF @hoa_don_id IS NOT NULL AND (@active_hd IS NULL OR @hoa_don_id<>@active_hd)
            THROW 51022, N''Hóa đơn không thuộc phiếu hoặc đã hủy.'', 1;
        IF @hoa_don_id IS NULL SET @hoa_don_id=@active_hd;
        SELECT @r=COALESCE(SUM(CASE WHEN loai=''THU'' THEN so_tien ELSE -so_tien END),0)
          FROM dbo.giao_dich_thanh_toan WHERE phieu_id=@phieu_id;
        IF @loai=''HOAN'' AND @so_tien>@r THROW 51023, N''Tiền hoàn vượt số tiền đã thu.'', 1;
        IF @loai=''THU'' AND @tong IS NOT NULL AND @r+@so_tien>@tong
            THROW 51024, N''Tiền thu vượt tổng hóa đơn.'', 1;
        INSERT INTO dbo.giao_dich_thanh_toan(phieu_id,hoa_don_id,loai,so_tien,phuong_thuc,nguoi_thuc_hien_id,ma_tham_chieu,ghi_chu)
        VALUES (@phieu_id,@hoa_don_id,@loai,@so_tien,@phuong_thuc,@nguoi_thuc_hien_id,@ma_tham_chieu,@ghi_chu);
        SET @giao_dich_id=SCOPE_IDENTITY();
        IF @active_hd IS NOT NULL
        BEGIN
            SET @r=@r+CASE WHEN @loai=''THU'' THEN @so_tien ELSE -@so_tien END;
            UPDATE dbo.hoa_don SET trang_thai=CASE WHEN @r>=@tong THEN ''DA_THANH_TOAN'' ELSE ''CHUA_THANH_TOAN'' END,
                phuong_thuc_tt=CASE WHEN @r>=@tong THEN @phuong_thuc ELSE NULL END,
                ngay_thanh_toan=CASE WHEN @r>=@tong THEN SYSUTCDATETIME() ELSE NULL END,
                updated_at=SYSUTCDATETIME() WHERE id=@active_hd;
        END;
        IF @own=1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own=1 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';

    -- Khối 27/30
    EXEC sys.sp_executesql N'CREATE OR ALTER PROCEDURE dbo.usp_v3_ban_giao
    @phieu_id bigint, @nguoi_giao_id bigint, @ten_nguoi_nhan nvarchar(150),
    @tinh_trang_may nvarchar(1000)=NULL, @phu_kien_da_tra nvarchar(1000)=NULL,
    @xac_nhan nvarchar(1000)=NULL, @nguoi_duyet_con_no_id bigint=NULL, @ly_do_con_no nvarchar(500)=NULL
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @own bit=CASE WHEN @@TRANCOUNT=0 THEN 1 ELSE 0 END;
    DECLARE @trang_thai varchar(15), @tong decimal(14,2), @da_thu decimal(19,2), @con_no decimal(14,2), @now datetime2(3)=SYSUTCDATETIME();
    BEGIN TRY
        IF @own=1 BEGIN TRANSACTION;
        SELECT @trang_thai=trang_thai FROM dbo.phieu_sua_chua WITH (UPDLOCK,HOLDLOCK) WHERE id=@phieu_id;
        IF @trang_thai IS NULL OR @trang_thai<>''HOAN_THANH'' THROW 51030, N''Chỉ bàn giao phiếu đã hoàn thành.'', 1;
        SELECT @tong=tong_thanh_toan FROM dbo.hoa_don WHERE phieu_id=@phieu_id AND trang_thai<>''HUY'';
        IF @tong IS NULL THROW 51031, N''Cần lập hóa đơn trước khi bàn giao.'', 1;
        SELECT @da_thu=COALESCE(SUM(CASE WHEN loai=''THU'' THEN so_tien ELSE -so_tien END),0)
          FROM dbo.giao_dich_thanh_toan WHERE phieu_id=@phieu_id;
        SET @con_no=CASE WHEN @tong>@da_thu THEN @tong-@da_thu ELSE 0 END;
        IF @con_no>0 AND (@nguoi_duyet_con_no_id IS NULL OR NULLIF(LTRIM(RTRIM(@ly_do_con_no)),N'''') IS NULL)
            THROW 51032, N''Còn công nợ: cần người duyệt và lý do bàn giao.'', 1;
        INSERT INTO dbo.ban_giao_thiet_bi(phieu_id,nguoi_giao_id,ten_nguoi_nhan,thoi_gian,tinh_trang_may,phu_kien_da_tra,xac_nhan,con_no_luc_giao,nguoi_duyet_con_no_id,ly_do_con_no)
        VALUES(@phieu_id,@nguoi_giao_id,@ten_nguoi_nhan,@now,@tinh_trang_may,@phu_kien_da_tra,@xac_nhan,@con_no,@nguoi_duyet_con_no_id,@ly_do_con_no);
        UPDATE dbo.phieu_sua_chua SET trang_thai=''DA_TRA'',ngay_tra=@now,updated_at=@now WHERE id=@phieu_id;
        INSERT INTO dbo.lich_su_trang_thai_phieu(phieu_id,trang_thai_cu,trang_thai_moi,nguoi_thuc_hien_id,thoi_gian,ghi_chu)
        VALUES(@phieu_id,''HOAN_THANH'',''DA_TRA'',@nguoi_giao_id,@now,N''Bàn giao thiết bị theo V3'');
        IF @own=1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own=1 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';

    -- Khối 28/30
    EXEC sys.sp_executesql N'CREATE TABLE dbo.cam_ket_bao_hanh (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_cam_ket_bao_hanh PRIMARY KEY,
    phieu_goc_id bigint NOT NULL,
    loai varchar(15) NOT NULL,
    chi_tiet_xuat_kho_id bigint NULL,
    dich_vu_id bigint NULL,
    ngay_bat_dau date NOT NULL,
    ngay_ket_thuc date NOT NULL,
    dieu_kien nvarchar(1000) NULL,
    created_at datetime2(3) NOT NULL CONSTRAINT df_ckbh_created DEFAULT SYSUTCDATETIME(),
    CONSTRAINT fk_ckbh_phieu FOREIGN KEY(phieu_goc_id) REFERENCES dbo.phieu_sua_chua(id),
    CONSTRAINT fk_ckbh_ctxk FOREIGN KEY(chi_tiet_xuat_kho_id,phieu_goc_id) REFERENCES dbo.chi_tiet_xuat_kho(id,phieu_id),
    CONSTRAINT fk_ckbh_ctdv FOREIGN KEY(phieu_goc_id,dich_vu_id) REFERENCES dbo.chi_tiet_dich_vu(phieu_id,dich_vu_id),
    CONSTRAINT uq_ckbh_id_phieu UNIQUE(id,phieu_goc_id),
    CONSTRAINT ck_ckbh_loai CHECK ((loai=''LINH_KIEN'' AND chi_tiet_xuat_kho_id IS NOT NULL AND dich_vu_id IS NULL)
                                OR (loai=''DICH_VU'' AND dich_vu_id IS NOT NULL AND chi_tiet_xuat_kho_id IS NULL)),
    CONSTRAINT ck_ckbh_ngay CHECK (ngay_ket_thuc>=ngay_bat_dau)
);
CREATE UNIQUE INDEX uq_ckbh_ctxk ON dbo.cam_ket_bao_hanh(chi_tiet_xuat_kho_id) WHERE chi_tiet_xuat_kho_id IS NOT NULL;
CREATE UNIQUE INDEX uq_ckbh_dich_vu ON dbo.cam_ket_bao_hanh(phieu_goc_id,dich_vu_id) WHERE dich_vu_id IS NOT NULL;

CREATE TABLE dbo.phieu_bao_hanh (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_phieu_bao_hanh PRIMARY KEY,
    phieu_goc_id bigint NOT NULL,
    thiet_bi_id bigint NOT NULL,
    cam_ket_id bigint NULL,
    phieu_sua_bao_hanh_id bigint NULL,
    nguoi_tiep_nhan_id bigint NOT NULL,
    ngay_tiep_nhan datetime2(3) NOT NULL CONSTRAINT df_pbh_ngay DEFAULT SYSUTCDATETIME(),
    loi_khach_bao nvarchar(1000) NOT NULL,
    trang_thai varchar(20) NOT NULL CONSTRAINT df_pbh_status DEFAULT ''TIEP_NHAN'',
    ket_luan nvarchar(1000) NULL,
    ket_qua_xu_ly nvarchar(1000) NULL,
    ngay_hoan_thanh datetime2(3) NULL,
    CONSTRAINT fk_pbh_phieu_goc_thiet_bi FOREIGN KEY(phieu_goc_id,thiet_bi_id) REFERENCES dbo.phieu_sua_chua(id,thiet_bi_id),
    CONSTRAINT fk_pbh_cam_ket_phieu FOREIGN KEY(cam_ket_id,phieu_goc_id) REFERENCES dbo.cam_ket_bao_hanh(id,phieu_goc_id),
    CONSTRAINT fk_pbh_phieu_sua_thiet_bi FOREIGN KEY(phieu_sua_bao_hanh_id,thiet_bi_id) REFERENCES dbo.phieu_sua_chua(id,thiet_bi_id),
    CONSTRAINT fk_pbh_tiep_nhan FOREIGN KEY(nguoi_tiep_nhan_id) REFERENCES dbo.nhan_vien(id),
    CONSTRAINT ck_pbh_phieu_khac CHECK (phieu_sua_bao_hanh_id IS NULL OR phieu_sua_bao_hanh_id<>phieu_goc_id),
    CONSTRAINT ck_pbh_loi CHECK (LEN(LTRIM(RTRIM(loi_khach_bao)))>0),
    CONSTRAINT ck_pbh_status CHECK (trang_thai IN (''TIEP_NHAN'',''DANG_KIEM_TRA'',''CHAP_NHAN'',''TU_CHOI'',''DANG_XU_LY'',''HOAN_THANH'',''DA_TRA'')),
    CONSTRAINT ck_pbh_ket_luan CHECK (trang_thai<>''TU_CHOI'' OR NULLIF(LTRIM(RTRIM(ket_luan)),N'''') IS NOT NULL),
    CONSTRAINT ck_pbh_hoan_thanh CHECK (trang_thai NOT IN (''HOAN_THANH'',''DA_TRA'') OR (ngay_hoan_thanh IS NOT NULL AND ket_qua_xu_ly IS NOT NULL)),
    CONSTRAINT ck_pbh_ngay CHECK (ngay_hoan_thanh IS NULL OR ngay_hoan_thanh>=ngay_tiep_nhan)
);
CREATE INDEX ix_pbh_phieu_goc ON dbo.phieu_bao_hanh(phieu_goc_id,ngay_tiep_nhan);

CREATE TABLE dbo.phieu_bao_hanh_ncc (
    id bigint IDENTITY(1,1) NOT NULL CONSTRAINT pk_phieu_bao_hanh_ncc PRIMARY KEY,
    phieu_bao_hanh_id bigint NULL,
    nha_cung_cap_id bigint NOT NULL,
    linh_kien_id bigint NOT NULL,
    linh_kien_serial_id bigint NULL,
    so_luong int NOT NULL,
    trang_thai varchar(20) NOT NULL CONSTRAINT df_pbh_ncc_status DEFAULT ''CHO_GUI'',
    ngay_gui datetime2(3) NULL,
    ngay_nhan_lai datetime2(3) NULL,
    ket_qua nvarchar(1000) NULL,
    CONSTRAINT fk_pbh_ncc_pbh FOREIGN KEY(phieu_bao_hanh_id) REFERENCES dbo.phieu_bao_hanh(id),
    CONSTRAINT fk_pbh_ncc_ncc FOREIGN KEY(nha_cung_cap_id) REFERENCES dbo.nha_cung_cap(id),
    CONSTRAINT fk_pbh_ncc_lk FOREIGN KEY(linh_kien_id) REFERENCES dbo.linh_kien(id),
    CONSTRAINT fk_pbh_ncc_serial FOREIGN KEY(linh_kien_serial_id,linh_kien_id) REFERENCES dbo.linh_kien_serial(id,linh_kien_id),
    CONSTRAINT ck_pbh_ncc_sl CHECK (so_luong>0 AND (linh_kien_serial_id IS NULL OR so_luong=1)),
    CONSTRAINT ck_pbh_ncc_tt CHECK (trang_thai IN (''CHO_GUI'',''DA_GUI'',''DA_NHAN_LAI'',''HUY'')),
    CONSTRAINT ck_pbh_ncc_ngay CHECK ((trang_thai NOT IN (''DA_GUI'',''DA_NHAN_LAI'') OR ngay_gui IS NOT NULL)
        AND (trang_thai<>''DA_NHAN_LAI'' OR ngay_nhan_lai IS NOT NULL)
        AND (ngay_nhan_lai IS NULL OR (ngay_gui IS NOT NULL AND ngay_nhan_lai>=ngay_gui)))
);';

    -- Khối 29/30
    EXEC sys.sp_executesql N'-- Gọi cùng transaction với usp_xuat_kho, không gọi tách rời khi vận hành.
CREATE OR ALTER PROCEDURE dbo.usp_xuat_kho_serial
    @chi_tiet_xuat_kho_id bigint, @linh_kien_serial_id bigint, @nhan_vien_id bigint
AS
BEGIN
    SET NOCOUNT ON; SET XACT_ABORT ON;
    DECLARE @own bit=CASE WHEN @@TRANCOUNT=0 THEN 1 ELSE 0 END;
    DECLARE @kho bigint,@lk bigint,@sl int,@da_gan bigint;
    BEGIN TRY
        IF @own=1 BEGIN TRANSACTION;
        SELECT @kho=kho_id,@lk=linh_kien_id,@sl=so_luong_xuat,@da_gan=linh_kien_serial_id
          FROM dbo.chi_tiet_xuat_kho WITH (UPDLOCK,HOLDLOCK) WHERE id=@chi_tiet_xuat_kho_id;
        IF @kho IS NULL OR @sl<>1 OR @da_gan IS NOT NULL
            THROW 51040, N''Dòng xuất không tồn tại, không phải một chiếc hoặc đã gắn serial.'', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.thu_kho WHERE nhan_vien_id=@nhan_vien_id)
            THROW 51041, N''Người thực hiện không phải thủ kho.'', 1;
        UPDATE dbo.linh_kien_serial SET trang_thai=''DA_XUAT'',chi_tiet_xuat_kho_id=@chi_tiet_xuat_kho_id,
               kho_id=NULL,updated_at=SYSUTCDATETIME()
         WHERE id=@linh_kien_serial_id AND trang_thai=''TRONG_KHO'' AND kho_id=@kho AND linh_kien_id=@lk;
        IF @@ROWCOUNT<>1 THROW 51042, N''Serial không còn trong kho hoặc không thuộc linh kiện này.'', 1;
        UPDATE dbo.chi_tiet_xuat_kho SET linh_kien_serial_id=@linh_kien_serial_id WHERE id=@chi_tiet_xuat_kho_id;
        IF @own=1 COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @own=1 AND @@TRANCOUNT>0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;
END;';

    -- Khối 30/30
    EXEC sys.sp_executesql N'INSERT INTO dbo.v3_schema_migration(version) VALUES (''V3_20261104_01''),(''V3_20261104_02'');';
    COMMIT TRANSACTION;
    PRINT N'Đã áp dụng V3. Các tính năng mới cần backend kết nối trước khi sử dụng.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
-- Chạy một lần trên CSDL V3 hiện có. File tạo mới V3 đã bao gồm phần này.
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
    THROW 51200, N'Hãy chọn CSDL ứng dụng V3.', 1;
IF OBJECT_ID(N'dbo.v3_schema_migration', N'U') IS NULL
    THROW 51201, N'CSDL chưa có schema V3.', 1;
IF EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261106_API')
    THROW 51202, N'Bản bổ sung API đã được áp dụng.', 1;
BEGIN TRY
    BEGIN TRANSACTION;

    CREATE TABLE dbo.ma_lien_ket_khach_hang_v3 (
        id bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        khach_hang_id bigint NOT NULL REFERENCES dbo.khach_hang(id),
        ma_hash varchar(64) NOT NULL,
        het_han datetime2(3) NOT NULL,
        da_dung_luc datetime2(3) NULL,
        nguoi_cap_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        created_at datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX ix_mlkkh_khach ON dbo.ma_lien_ket_khach_hang_v3(khach_hang_id, id DESC);

    CREATE TABLE dbo.lich_hen_v3 (
        id bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        khach_hang_id bigint NOT NULL REFERENCES dbo.khach_hang(id),
        thoi_gian_mong_muon datetime2(3) NOT NULL,
        ten_thiet_bi nvarchar(150) NOT NULL,
        mo_ta_loi nvarchar(1000) NOT NULL,
        trang_thai varchar(20) NOT NULL DEFAULT 'CHO_XAC_NHAN',
        ghi_chu nvarchar(500) NULL,
        phieu_id bigint NULL REFERENCES dbo.phieu_sua_chua(id),
        created_at datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        updated_at datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT ck_lich_hen_v3_status CHECK (trang_thai IN
            ('CHO_XAC_NHAN','DA_XAC_NHAN','TU_CHOI','DA_TIEP_NHAN','HUY'))
    );
    CREATE UNIQUE INDEX uq_lich_hen_phieu ON dbo.lich_hen_v3(phieu_id) WHERE phieu_id IS NOT NULL;
    CREATE INDEX ix_lich_hen_khach ON dbo.lich_hen_v3(khach_hang_id, created_at DESC);

    ALTER TABLE dbo.bao_gia ADD khach_hang_phan_hoi_id bigint NULL;
    ALTER TABLE dbo.bao_gia ADD CONSTRAINT fk_bg_khach_phan_hoi
        FOREIGN KEY(khach_hang_phan_hoi_id) REFERENCES dbo.khach_hang(id);
    ALTER TABLE dbo.bao_gia DROP CONSTRAINT ck_bg_phan_hoi;
    -- SQL Server biên dịch cả batch trước khi chạy: ràng buộc dùng cột vừa thêm cần batch riêng.
    EXEC sys.sp_executesql N'ALTER TABLE dbo.bao_gia ADD CONSTRAINT ck_bg_phan_hoi CHECK
        (trang_thai NOT IN (''DA_DUYET'',''TU_CHOI'') OR
         (ngay_phan_hoi IS NOT NULL AND kenh_phan_hoi IS NOT NULL
          AND LEN(LTRIM(RTRIM(kenh_phan_hoi))) > 0
          AND (nguoi_ghi_nhan_id IS NOT NULL OR khach_hang_phan_hoi_id IS NOT NULL)))';

    ALTER TABLE dbo.phieu_bao_hanh_ncc ADD kho_gui_id bigint NULL;
    ALTER TABLE dbo.phieu_bao_hanh_ncc ADD da_tru_ton bit NOT NULL
        CONSTRAINT df_pbh_ncc_da_tru_ton DEFAULT 0;
    ALTER TABLE dbo.phieu_bao_hanh_ncc ADD nguon_hang varchar(10) NOT NULL
        CONSTRAINT df_pbh_ncc_nguon_hang DEFAULT 'CU';
    EXEC sys.sp_executesql N'ALTER TABLE dbo.phieu_bao_hanh_ncc ADD CONSTRAINT ck_pbh_ncc_nguon_hang
        CHECK (nguon_hang IN (''CU'',''KHO'',''KHACH''))';
    ALTER TABLE dbo.phieu_bao_hanh_ncc ADD CONSTRAINT fk_pbh_ncc_kho_gui
        FOREIGN KEY(kho_gui_id) REFERENCES dbo.kho(id);

    CREATE TABLE dbo.xu_ly_bao_hanh_ncc_v3 (
        id bigint IDENTITY(1,1) NOT NULL PRIMARY KEY,
        phieu_bao_hanh_ncc_id bigint NOT NULL,
        loai varchar(20) NOT NULL,
        kho_id bigint NULL REFERENCES dbo.kho(id),
        phieu_nhap_id bigint NULL REFERENCES dbo.phieu_nhap(id),
        so_luong int NOT NULL,
        nguoi_thuc_hien_id bigint NOT NULL REFERENCES dbo.nhan_vien(id),
        thoi_gian datetime2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        ghi_chu nvarchar(500) NULL,
        CONSTRAINT uq_xlbh_ncc_ho_so UNIQUE (phieu_bao_hanh_ncc_id),
        CONSTRAINT fk_xlbh_ncc_ho_so FOREIGN KEY (phieu_bao_hanh_ncc_id)
            REFERENCES dbo.phieu_bao_hanh_ncc(id),
        CONSTRAINT ck_xlbh_ncc_loai CHECK (loai IN ('NHAP_LAI','NHAP_THAY_THE','KHONG_NHAP')),
        CONSTRAINT ck_xlbh_ncc_sl CHECK (so_luong > 0)
    );
    CREATE UNIQUE INDEX uq_xlbh_ncc_phieu_nhap ON dbo.xu_ly_bao_hanh_ncc_v3(phieu_nhap_id)
        WHERE phieu_nhap_id IS NOT NULL;

    INSERT dbo.v3_schema_migration(version) VALUES ('V3_20261106_API');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
-- Chạy sau BO_SUNG_API_20261106.sql trên CSDL V3 hiện có.
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
    THROW 51300, N'Hãy chọn CSDL ứng dụng V3.', 1;
IF OBJECT_ID(N'dbo.v3_schema_migration', N'U') IS NULL
    THROW 51301, N'CSDL chưa có schema V3.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261106_API')
    THROW 51302, N'Cần áp dụng BO_SUNG_API_20261106.sql trước.', 1;
IF EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261107_KTV')
    THROW 51303, N'Bản bổ sung quy trình KTV đã được áp dụng.', 1;
IF COL_LENGTH(N'dbo.phieu_sua_chua', N'ktv_nhan_id') IS NOT NULL
    OR COL_LENGTH(N'dbo.phieu_sua_chua', N'ktv_nhan_luc') IS NOT NULL
    OR COL_LENGTH(N'dbo.phieu_sua_chua', N'ket_qua_kiem_tra') IS NOT NULL
    THROW 51304, N'Schema KTV đã thay đổi một phần; kiểm tra trước khi chạy migration.', 1;
BEGIN TRY
    BEGIN TRANSACTION;

    ALTER TABLE dbo.phieu_sua_chua ADD
        ktv_nhan_id bigint NULL,
        ktv_nhan_luc datetime2(3) NULL,
        ket_qua_kiem_tra nvarchar(1000) NULL;
    -- SQL Server biên dịch cả batch; ràng buộc dùng cột vừa thêm cần batch riêng.
    EXEC sys.sp_executesql N'ALTER TABLE dbo.phieu_sua_chua ADD CONSTRAINT fk_psc_ktv_nhan
        FOREIGN KEY (ktv_nhan_id) REFERENCES dbo.nhan_vien(id)';
    EXEC sys.sp_executesql N'ALTER TABLE dbo.phieu_sua_chua ADD CONSTRAINT ck_psc_ktv_nhan_du
        CHECK ((ktv_nhan_id IS NULL AND ktv_nhan_luc IS NULL)
            OR (ktv_nhan_id IS NOT NULL AND ktv_nhan_luc IS NOT NULL))';

    INSERT dbo.v3_schema_migration(version) VALUES ('V3_20261107_KTV');
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

GO
SET NOEXEC OFF;
GO
