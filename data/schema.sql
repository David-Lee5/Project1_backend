USE [QLSCTBDT]

GO

/****** Object:  Table [dbo].[phieu_sua_chua]    Script Date: 10/26/2026 3:38:01 PM ******/
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
PRIMARY KEY CLUSTERED
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[chi_tiet_dich_vu]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[chi_tiet_xuat_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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
PRIMARY KEY CLUSTERED
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  View [dbo].[v_phieu_chi_phi]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[phieu_nhap]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[chi_tiet_nhap_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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
 CONSTRAINT [pk_chi_tiet_nhap_kho] PRIMARY KEY CLUSTERED
(
	[phieu_nhap_id] ASC,
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  View [dbo].[v_phieu_nhap_tong]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[linh_kien]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[anh_linh_kien]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[ton_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  View [dbo].[v_linh_kien_hien_thi]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  View [dbo].[v_ton_kho_sap_het]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[anh_phieu_sua_chua]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[audit_log]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[bien_dong_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[dich_vu]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[hoa_don]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[khach_hang]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[ky_thuat_vien]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[le_tan]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[lich_su_trang_thai_phieu]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Table [dbo].[nha_cung_cap]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[nhan_vien]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[refresh_token]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[tai_khoan]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[thiet_bi]    Script Date: 10/26/2026 3:38:01 PM ******/
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[thu_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  Index [ix_anh_linh_kien_lk]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_anh_linh_kien_lk] ON [dbo].[anh_linh_kien]
(
	[linh_kien_id] ASC,
	[thu_tu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [uq_anh_linh_kien_chinh]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_anh_linh_kien_chinh] ON [dbo].[anh_linh_kien]
(
	[linh_kien_id] ASC
)
WHERE ([la_anh_chinh]=(1))
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_anh_phieu]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_anh_phieu] ON [dbo].[anh_phieu_sua_chua]
(
	[phieu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_audit_log_bang]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_audit_log_bang] ON [dbo].[audit_log]
(
	[bang] ASC,
	[ban_ghi_id] ASC,
	[thoi_gian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_bien_dong_kho_lk]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_bien_dong_kho_lk] ON [dbo].[bien_dong_kho]
(
	[kho_id] ASC,
	[linh_kien_id] ASC,
	[thoi_gian] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_bien_dong_pn]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_bien_dong_pn] ON [dbo].[bien_dong_kho]
(
	[phieu_nhap_id] ASC
)
WHERE ([phieu_nhap_id] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_bien_dong_xuat]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_bien_dong_xuat] ON [dbo].[bien_dong_kho]
(
	[chi_tiet_xuat_kho_id] ASC
)
WHERE ([chi_tiet_xuat_kho_id] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_ctdv_dich_vu]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_ctdv_dich_vu] ON [dbo].[chi_tiet_dich_vu]
(
	[dich_vu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_ctnk_linh_kien]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_ctnk_linh_kien] ON [dbo].[chi_tiet_nhap_kho]
(
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_ctxk_kho_lk]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_ctxk_kho_lk] ON [dbo].[chi_tiet_xuat_kho]
(
	[kho_id] ASC,
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_ctxk_phieu]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_ctxk_phieu] ON [dbo].[chi_tiet_xuat_kho]
(
	[phieu_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_ctxk_thu_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_ctxk_thu_kho] ON [dbo].[chi_tiet_xuat_kho]
(
	[thu_kho_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_dich_vu_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[dich_vu] ADD  CONSTRAINT [uq_dich_vu_ma] UNIQUE NONCLUSTERED
(
	[ma_dv] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_dich_vu_ten]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_dich_vu_ten] ON [dbo].[dich_vu]
(
	[ten_dich_vu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_hoa_don_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[hoa_don] ADD  CONSTRAINT [uq_hoa_don_ma] UNIQUE NONCLUSTERED
(
	[ma_hd] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_hoa_don_le_tan]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_hoa_don_le_tan] ON [dbo].[hoa_don]
(
	[le_tan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_hoa_don_trang_thai]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_hoa_don_trang_thai] ON [dbo].[hoa_don]
(
	[trang_thai] ASC,
	[ngay_lap] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [uq_hoa_don_phieu_hieu_luc]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_hoa_don_phieu_hieu_luc] ON [dbo].[hoa_don]
(
	[phieu_id] ASC
)
WHERE ([trang_thai]<>'HUY')
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_khach_hang_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[khach_hang] ADD  CONSTRAINT [uq_khach_hang_ma] UNIQUE NONCLUSTERED
(
	[ma_kh] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_khach_hang_ho_ten]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_khach_hang_ho_ten] ON [dbo].[khach_hang]
(
	[ho_ten] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_khach_hang_sdt]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_khach_hang_sdt] ON [dbo].[khach_hang]
(
	[so_dien_thoai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_khach_hang_email]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_khach_hang_email] ON [dbo].[khach_hang]
(
	[email] ASC
)
WHERE ([email] IS NOT NULL AND [deleted_at] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [uq_khach_hang_tai_khoan]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_khach_hang_tai_khoan] ON [dbo].[khach_hang]
(
	[tai_khoan_id] ASC
)
WHERE ([tai_khoan_id] IS NOT NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_kho_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[kho] ADD  CONSTRAINT [uq_kho_ma] UNIQUE NONCLUSTERED
(
	[ma_kho] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_kho_thu_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_kho_thu_kho] ON [dbo].[kho]
(
	[thu_kho_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_lich_su_phieu]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_lich_su_phieu] ON [dbo].[lich_su_trang_thai_phieu]
(
	[phieu_id] ASC,
	[thoi_gian] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_linh_kien_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[linh_kien] ADD  CONSTRAINT [uq_linh_kien_ma] UNIQUE NONCLUSTERED
(
	[ma_lk] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_linh_kien_ten]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_linh_kien_ten] ON [dbo].[linh_kien]
(
	[ten_linh_kien] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_nha_cung_cap_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[nha_cung_cap] ADD  CONSTRAINT [uq_nha_cung_cap_ma] UNIQUE NONCLUSTERED
(
	[ma_ncc] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_nhan_vien_id_chuc_vu]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[nhan_vien] ADD  CONSTRAINT [uq_nhan_vien_id_chuc_vu] UNIQUE NONCLUSTERED
(
	[id] ASC,
	[chuc_vu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_nhan_vien_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[nhan_vien] ADD  CONSTRAINT [uq_nhan_vien_ma] UNIQUE NONCLUSTERED
(
	[ma_nv] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [uq_nhan_vien_tai_khoan]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[nhan_vien] ADD  CONSTRAINT [uq_nhan_vien_tai_khoan] UNIQUE NONCLUSTERED
(
	[tai_khoan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_phieu_nhap_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[phieu_nhap] ADD  CONSTRAINT [uq_phieu_nhap_ma] UNIQUE NONCLUSTERED
(
	[ma_pn] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_phieu_nhap_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_nhap_kho] ON [dbo].[phieu_nhap]
(
	[kho_id] ASC,
	[ngay_nhap] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_phieu_nhap_ncc]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_nhap_ncc] ON [dbo].[phieu_nhap]
(
	[nha_cung_cap_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_phieu_nhap_thu_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_nhap_thu_kho] ON [dbo].[phieu_nhap]
(
	[thu_kho_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_phieu_sua_chua_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[phieu_sua_chua] ADD  CONSTRAINT [uq_phieu_sua_chua_ma] UNIQUE NONCLUSTERED
(
	[ma_phieu] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_phieu_ktv]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_ktv] ON [dbo].[phieu_sua_chua]
(
	[ky_thuat_vien_id] ASC,
	[trang_thai] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_phieu_le_tan]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_le_tan] ON [dbo].[phieu_sua_chua]
(
	[le_tan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_phieu_thiet_bi]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_thiet_bi] ON [dbo].[phieu_sua_chua]
(
	[thiet_bi_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [ix_phieu_trang_thai]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_phieu_trang_thai] ON [dbo].[phieu_sua_chua]
(
	[trang_thai] ASC,
	[ngay_nhan] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_refresh_token_hash]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[refresh_token] ADD  CONSTRAINT [uq_refresh_token_hash] UNIQUE NONCLUSTERED
(
	[token_hash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_refresh_token_tai_khoan]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_refresh_token_tai_khoan] ON [dbo].[refresh_token]
(
	[tai_khoan_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_tai_khoan_id_vai_tro]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[tai_khoan] ADD  CONSTRAINT [uq_tai_khoan_id_vai_tro] UNIQUE NONCLUSTERED
(
	[id] ASC,
	[vai_tro] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_tai_khoan_ten_dang_nhap]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[tai_khoan] ADD  CONSTRAINT [uq_tai_khoan_ten_dang_nhap] UNIQUE NONCLUSTERED
(
	[ten_dang_nhap] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_thiet_bi_ma]    Script Date: 10/26/2026 3:38:01 PM ******/
ALTER TABLE [dbo].[thiet_bi] ADD  CONSTRAINT [uq_thiet_bi_ma] UNIQUE NONCLUSTERED
(
	[ma_thiet_bi] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_thiet_bi_khach_hang]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_thiet_bi_khach_hang] ON [dbo].[thiet_bi]
(
	[khach_hang_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

SET ANSI_PADDING ON

GO

/****** Object:  Index [uq_thiet_bi_so_serial]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [uq_thiet_bi_so_serial] ON [dbo].[thiet_bi]
(
	[so_serial] ASC
)
WHERE ([so_serial] IS NOT NULL AND [deleted_at] IS NULL)
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

GO

/****** Object:  Index [ix_ton_kho_linh_kien]    Script Date: 10/26/2026 3:38:01 PM ******/
CREATE NONCLUSTERED INDEX [ix_ton_kho_linh_kien] ON [dbo].[ton_kho]
(
	[linh_kien_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]

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

ALTER TABLE [dbo].[ky_thuat_vien] ADD  DEFAULT ('KY_THUAT_VIEN') FOR [chuc_vu]

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

ALTER TABLE [dbo].[thiet_bi] ADD  DEFAULT (sysutcdatetime()) FOR [created_at]

GO

ALTER TABLE [dbo].[thiet_bi] ADD  DEFAULT (sysutcdatetime()) FOR [updated_at]

GO

ALTER TABLE [dbo].[thu_kho] ADD  DEFAULT ('THU_KHO') FOR [chuc_vu]

GO

ALTER TABLE [dbo].[ton_kho] ADD  DEFAULT ((0)) FOR [so_luong_ton]

GO

ALTER TABLE [dbo].[ton_kho] ADD  DEFAULT ((0)) FOR [muc_ton_toi_thieu]

GO

ALTER TABLE [dbo].[ton_kho] ADD  DEFAULT (sysutcdatetime()) FOR [ngay_cap_nhat]

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

ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctxk_thu_kho] FOREIGN KEY([thu_kho_id])
REFERENCES [dbo].[thu_kho] ([nhan_vien_id])

GO

ALTER TABLE [dbo].[chi_tiet_xuat_kho] CHECK CONSTRAINT [fk_ctxk_thu_kho]

GO

ALTER TABLE [dbo].[chi_tiet_xuat_kho]  WITH CHECK ADD  CONSTRAINT [fk_ctxk_ton_kho] FOREIGN KEY([kho_id], [linh_kien_id])
REFERENCES [dbo].[ton_kho] ([kho_id], [linh_kien_id])

GO

ALTER TABLE [dbo].[chi_tiet_xuat_kho] CHECK CONSTRAINT [fk_ctxk_ton_kho]

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

ALTER TABLE [dbo].[ky_thuat_vien]  WITH CHECK ADD  CONSTRAINT [fk_ky_thuat_vien_nhan_vien] FOREIGN KEY([nhan_vien_id], [chuc_vu])
REFERENCES [dbo].[nhan_vien] ([id], [chuc_vu])

GO

ALTER TABLE [dbo].[ky_thuat_vien] CHECK CONSTRAINT [fk_ky_thuat_vien_nhan_vien]

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

ALTER TABLE [dbo].[khach_hang]  WITH CHECK ADD CHECK  (([email] like '%_@_%._%' AND NOT [email] like '% %'))

GO

ALTER TABLE [dbo].[khach_hang]  WITH CHECK ADD CHECK  ((NOT [so_dien_thoai] like '%[^0-9+ ]%' AND (len([so_dien_thoai])>=(8) AND len([so_dien_thoai])<=(15))))

GO

ALTER TABLE [dbo].[khach_hang]  WITH CHECK ADD CHECK  (([vai_tro_tk]='KHACH_HANG'))

GO

ALTER TABLE [dbo].[ky_thuat_vien]  WITH CHECK ADD CHECK  (([chuc_vu]='KY_THUAT_VIEN'))

GO

ALTER TABLE [dbo].[le_tan]  WITH CHECK ADD CHECK  (([chuc_vu]='LE_TAN'))

GO

ALTER TABLE [dbo].[linh_kien]  WITH CHECK ADD CHECK  (([don_gia_ban]>=(0)))

GO

ALTER TABLE [dbo].[linh_kien]  WITH CHECK ADD CHECK  (([thoi_han_bao_hanh_thang]>=(0)))

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

ALTER TABLE [dbo].[thu_kho]  WITH CHECK ADD CHECK  (([chuc_vu]='THU_KHO'))

GO

ALTER TABLE [dbo].[ton_kho]  WITH CHECK ADD CHECK  (([muc_ton_toi_thieu]>=(0)))

GO

ALTER TABLE [dbo].[ton_kho]  WITH CHECK ADD CHECK  (([so_luong_ton]>=(0)))

GO

/****** Object:  StoredProcedure [dbo].[usp_lap_hoa_don]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  StoredProcedure [dbo].[usp_xac_nhan_nhap_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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

/****** Object:  StoredProcedure [dbo].[usp_xuat_kho]    Script Date: 10/26/2026 3:38:01 PM ******/
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

USE [master]

GO

ALTER DATABASE [QLSCTBDT] SET  READ_WRITE

GO
