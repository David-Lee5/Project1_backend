-- Chuan hoa bo du lieu thu V3 dang co thanh du lieu nghiep vu gia dinh, de doc tren giao dien.
-- Khong sua ban ghi khac. Cac nhan vat, lien he va chung tu duoi day la gia dinh.
-- Sao luu QLSCTBDT_V3 truoc khi ap dung. Chay bang sqlcmd -b -f 65001 hoac SSMS.
USE [QLSCTBDT_V3];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;

IF DB_NAME() <> N'QLSCTBDT_V3'
    THROW 51700, N'Chi duoc chay tren QLSCTBDT_V3.', 1;
IF @@TRANCOUNT <> 0
    THROW 51701, N'Khong chay script trong giao dich dang mo.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.tai_khoan WHERE ten_dang_nhap LIKE 'demo[_]%')
BEGIN
    IF (SELECT COUNT(*) FROM dbo.tai_khoan WHERE ten_dang_nhap IN
        ('nguyenthimai', 'tranvanbinh', 'phamquocan', 'nguyenminhanh')) = 4
       AND EXISTS (SELECT 1 FROM dbo.phieu_nhap WHERE so_chung_tu_ncc = 'MP-2026-0418')
    BEGIN
        PRINT N'Bo du lieu da duoc chuan hoa truoc do.';
        RETURN;
    END;
    THROW 51702, N'Khong tim thay bo du lieu thu day du; khong thay doi CSDL.', 1;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    IF (SELECT COUNT(*) FROM dbo.tai_khoan WITH (UPDLOCK, HOLDLOCK)
        WHERE ten_dang_nhap IN ('demo_letan', 'demo_kho', 'demo_ktv', 'demo_khach')) <> 4
       OR (SELECT COUNT(*) FROM dbo.nhan_vien WHERE ho_ten LIKE N'[[]DEMO] %') <> 3
       OR (SELECT COUNT(*) FROM dbo.khach_hang WHERE ho_ten LIKE N'[[]DEMO] %') <> 2
       OR (SELECT COUNT(*) FROM dbo.thiet_bi WHERE so_serial LIKE 'DEMO-TB-%') <> 5
       OR (SELECT COUNT(*) FROM dbo.linh_kien_serial WHERE so_serial LIKE 'DEMO-SCR-%') <> 2
       OR (SELECT COUNT(*) FROM dbo.phieu_nhap WHERE so_chung_tu_ncc LIKE 'DEMO-NHAP-%') <> 2
       OR NOT EXISTS (SELECT 1 FROM dbo.linh_kien
                      WHERE ten_linh_kien = N'[DEMO] Quạt Dell Latitude 5420'
                        AND ma_model = N'DELL-5420-FAN')
       OR EXISTS (SELECT 1 FROM sys.triggers WHERE name IN
                 ('trg_v3_chi_tiet_bao_gia_khoa', 'trg_v3_chi_tiet_hoa_don_khong_sua',
                  'trg_v3_bao_gia_chuyen_trang_thai', 'trg_v3_giao_dich_khong_sua')
                  AND is_disabled = 1)
        THROW 51703, N'Bo du lieu thu da thay doi hoac bi thieu; can doi chieu truoc khi chay.', 1;

    IF EXISTS (SELECT 1 FROM dbo.tai_khoan WHERE ten_dang_nhap IN
        ('nguyenthimai', 'tranvanbinh', 'phamquocan', 'nguyenminhanh'))
       OR EXISTS (SELECT 1 FROM dbo.nhan_vien WHERE so_dien_thoai IN
        ('0917624385', '0984317652', '0935271846'))
       OR EXISTS (SELECT 1 FROM dbo.khach_hang WHERE so_dien_thoai IN
        ('0906842715', '0973184625'))
       OR EXISTS (SELECT 1 FROM dbo.thiet_bi WHERE so_serial IN
        ('DL5420A9F2K7', 'ASV15M3P8Q2', 'LNT14R6K3H9', 'HP440G8C5V2N', 'ACASP5Q7M4L2'))
       OR EXISTS (SELECT 1 FROM dbo.linh_kien_serial WHERE so_serial IN
        ('BOE-NV156-2408A61', 'BOE-NV156-2408A62'))
       OR EXISTS (SELECT 1 FROM dbo.phieu_nhap WHERE so_chung_tu_ncc IN
        ('MP-2026-0418', 'MP-2026-0419'))
        THROW 51704, N'Gia tri moi da ton tai; khong ghi de du lieu.', 1;

    DECLARE @letanTk bigint, @khoTk bigint, @ktvTk bigint, @khachTk bigint;
    DECLARE @kh1 bigint, @kh2 bigint, @quat bigint;
    SELECT @letanTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'demo_letan';
    SELECT @khoTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'demo_kho';
    SELECT @ktvTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'demo_ktv';
    SELECT @khachTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'demo_khach';
    SELECT @kh1 = id FROM dbo.khach_hang WHERE tai_khoan_id = @khachTk;
    SELECT @kh2 = id FROM dbo.khach_hang WHERE so_dien_thoai = '0900000102';
    SELECT @quat = id FROM dbo.linh_kien WHERE ma_model = N'DELL-5420-FAN';

    IF @kh1 IS NULL OR @kh2 IS NULL OR @quat IS NULL
        THROW 51705, N'Khong xac dinh duoc khach hoac mau quat thu.', 1;

    UPDATE dbo.tai_khoan SET ten_dang_nhap = CASE ten_dang_nhap
        WHEN 'demo_letan' THEN 'nguyenthimai'
        WHEN 'demo_kho' THEN 'tranvanbinh'
        WHEN 'demo_ktv' THEN 'phamquocan'
        WHEN 'demo_khach' THEN 'nguyenminhanh' END
    WHERE id IN (@letanTk, @khoTk, @ktvTk, @khachTk);

    UPDATE dbo.nhan_vien SET
        ho_ten = CASE tai_khoan_id
            WHEN @letanTk THEN N'Nguyễn Thị Mai'
            WHEN @khoTk THEN N'Trần Văn Bình'
            WHEN @ktvTk THEN N'Phạm Quốc An' END,
        so_dien_thoai = CASE tai_khoan_id
            WHEN @letanTk THEN '0917624385'
            WHEN @khoTk THEN '0984317652'
            WHEN @ktvTk THEN '0935271846' END,
        email = CASE tai_khoan_id
            WHEN @letanTk THEN 'mai.nguyen@techfix.test'
            WHEN @khoTk THEN 'binh.tran@techfix.test'
            WHEN @ktvTk THEN 'an.pham@techfix.test' END
    WHERE tai_khoan_id IN (@letanTk, @khoTk, @ktvTk);

    UPDATE dbo.khach_hang SET
        ho_ten = CASE id WHEN @kh1 THEN N'Nguyễn Minh Anh' ELSE N'Trần Hải Nam' END,
        so_dien_thoai = CASE id WHEN @kh1 THEN '0906842715' ELSE '0973184625' END,
        email = CASE id WHEN @kh1 THEN 'minhanh.nguyen@khachhang.test'
                             ELSE 'hainam.tran@khachhang.test' END
    WHERE id IN (@kh1, @kh2);

    UPDATE dbo.thiet_bi SET
        ten_thiet_bi = REPLACE(ten_thiet_bi, N'[DEMO] ', N''),
        so_serial = CASE so_serial
            WHEN 'DEMO-TB-A' THEN 'DL5420A9F2K7'
            WHEN 'DEMO-TB-B' THEN 'ASV15M3P8Q2'
            WHEN 'DEMO-TB-C' THEN 'LNT14R6K3H9'
            WHEN 'DEMO-TB-D' THEN 'HP440G8C5V2N'
            WHEN 'DEMO-TB-E' THEN 'ACASP5Q7M4L2' END
    WHERE so_serial IN ('DEMO-TB-A', 'DEMO-TB-B', 'DEMO-TB-C', 'DEMO-TB-D', 'DEMO-TB-E');

    UPDATE dbo.dich_vu SET ten_dich_vu = REPLACE(ten_dich_vu, N'[DEMO] ', N'')
    WHERE ten_dich_vu LIKE N'[[]DEMO] %';
    UPDATE dbo.nha_cung_cap SET
        ten_ncc = N'Công ty TNHH Linh kiện Minh Phát',
        so_dien_thoai = '02838274618',
        email = 'kinhdoanh@minhphat.test'
    WHERE ten_ncc = N'[DEMO] Nhà cung cấp linh kiện';
    UPDATE dbo.kho SET ten_kho = N'Kho linh kiện chính'
    WHERE ten_kho = N'[DEMO] Kho chính';

    UPDATE dbo.linh_kien SET ten_linh_kien = REPLACE(ten_linh_kien, N'[DEMO] ', N'')
    WHERE ten_linh_kien LIKE N'[[]DEMO] %';
    UPDATE dbo.linh_kien SET
        ten_linh_kien = N'Quạt tản nhiệt Lenovo ThinkPad T14',
        hang_san_xuat = N'Lenovo',
        ma_model = N'T14-G1-FAN-4P'
    WHERE id = @quat;
    UPDATE s SET gia_tri_van_ban = CASE a.ma
        WHEN 'MA_PART' THEN N'T14-G1-FAN-4P'
        WHEN 'DONG_MAY' THEN N'ThinkPad T14' END
    FROM dbo.thong_so_linh_kien AS s
    JOIN dbo.thuoc_tinh_loai_linh_kien AS a ON a.id = s.thuoc_tinh_id
    WHERE s.linh_kien_id = @quat AND a.ma IN ('MA_PART', 'DONG_MAY');

    UPDATE dbo.phieu_nhap SET
        so_chung_tu_ncc = CASE so_chung_tu_ncc
            WHEN 'DEMO-NHAP-01' THEN 'MP-2026-0418'
            ELSE 'MP-2026-0419' END,
        ghi_chu = CASE so_chung_tu_ncc
            WHEN 'DEMO-NHAP-01' THEN N'Nhập bổ sung RAM, quạt và màn hình theo chứng từ nhà cung cấp.'
            ELSE N'Đang đối chiếu số lượng và chứng từ trước khi xác nhận nhập kho.' END
    WHERE so_chung_tu_ncc IN ('DEMO-NHAP-01', 'DEMO-NHAP-NHAP');
    UPDATE dbo.chi_tiet_nhap_kho SET danh_sach_serial =
        N'["BOE-NV156-2408A61","BOE-NV156-2408A62"]'
    WHERE danh_sach_serial = N'["DEMO-SCR-001","DEMO-SCR-002"]';
    UPDATE dbo.linh_kien_serial SET so_serial = CASE so_serial
        WHEN 'DEMO-SCR-001' THEN 'BOE-NV156-2408A61'
        ELSE 'BOE-NV156-2408A62' END
    WHERE so_serial IN ('DEMO-SCR-001', 'DEMO-SCR-002');

    UPDATE dbo.phieu_sua_chua SET ghi_chu = CASE ghi_chu
        WHEN N'[DEMO] KTV có thể nhận máy và kiểm tra' THEN
            N'Khách phản ánh máy nóng sau khoảng 20 phút sử dụng; chờ kỹ thuật viên kiểm tra.'
        WHEN N'[DEMO] Khách có thể duyệt báo giá' THEN
            N'Đã kiểm tra sơ bộ, chờ khách xác nhận phương án nâng cấp RAM.'
        WHEN N'[DEMO] Thử duyệt yêu cầu vật tư và xuất kho' THEN
            N'Khách đã đồng ý thay quạt; đang chờ cấp linh kiện để sửa chữa.'
        WHEN N'[DEMO] Thử thu nốt tiền và bàn giao' THEN
            N'Đã vệ sinh hệ thống tản nhiệt; chờ thanh toán phần còn lại và bàn giao.'
        WHEN N'[DEMO] Phiếu đã bàn giao, dùng để thử bảo hành' THEN
            N'Máy đã bàn giao; tiếp tục theo dõi phản hồi trong thời gian bảo hành.' END
    WHERE ghi_chu LIKE N'[[]DEMO] %';
    -- Chi sua ten hien thi tren dong bao gia/hoa don cua bo du lieu thu.
    -- Cac trigger khoa lich su duoc tam dung va bat lai trong cung giao dich.
    DISABLE TRIGGER dbo.trg_v3_chi_tiet_bao_gia_khoa ON dbo.chi_tiet_bao_gia;
    DISABLE TRIGGER dbo.trg_v3_chi_tiet_hoa_don_khong_sua ON dbo.chi_tiet_hoa_don;
    DISABLE TRIGGER dbo.trg_v3_bao_gia_chuyen_trang_thai ON dbo.bao_gia;
    UPDATE dbo.chi_tiet_bao_gia
    SET ten_hang_muc = REPLACE(ten_hang_muc, N'[DEMO] ', N'')
    WHERE ten_hang_muc LIKE N'[[]DEMO] %';
    UPDATE dbo.chi_tiet_bao_gia SET ten_hang_muc = N'Quạt tản nhiệt Lenovo ThinkPad T14'
    WHERE linh_kien_id = @quat AND ten_hang_muc = N'Quạt Dell Latitude 5420';
    UPDATE dbo.chi_tiet_hoa_don
    SET ten_hang_muc = REPLACE(ten_hang_muc, N'[DEMO] ', N'')
    WHERE ten_hang_muc LIKE N'[[]DEMO] %';
    ENABLE TRIGGER dbo.trg_v3_chi_tiet_bao_gia_khoa ON dbo.chi_tiet_bao_gia;
    ENABLE TRIGGER dbo.trg_v3_chi_tiet_hoa_don_khong_sua ON dbo.chi_tiet_hoa_don;
    UPDATE dbo.bao_gia SET noi_dung_phan_hoi = CASE noi_dung_phan_hoi
        WHEN N'[DEMO] Đồng ý thay quạt' THEN N'Khách đồng ý thay quạt theo báo giá qua điện thoại.'
        WHEN N'[DEMO] Đồng ý vệ sinh' THEN N'Khách xác nhận thực hiện vệ sinh và kiểm tra tản nhiệt.'
        WHEN N'[DEMO] Đồng ý thay keo' THEN N'Khách đồng ý thay keo tản nhiệt theo báo giá.' END
    WHERE noi_dung_phan_hoi LIKE N'[[]DEMO] %';
    ENABLE TRIGGER dbo.trg_v3_bao_gia_chuyen_trang_thai ON dbo.bao_gia;
    UPDATE dbo.yeu_cau_linh_kien
    SET ghi_chu = N'Cần cấp một quạt tản nhiệt cho Lenovo ThinkPad T14.'
    WHERE ghi_chu = N'[DEMO] Cần quạt thay thế';
    DISABLE TRIGGER dbo.trg_v3_giao_dich_khong_sua ON dbo.giao_dich_thanh_toan;
    UPDATE dbo.giao_dich_thanh_toan SET ghi_chu = CASE ghi_chu
        WHEN N'[DEMO] Thanh toán một phần' THEN
            N'Khách thanh toán trước 50.000 đồng, số còn lại thanh toán khi nhận máy.'
        WHEN N'[DEMO] Đã thanh toán' THEN
            N'Khách đã thanh toán đủ bằng chuyển khoản.' END
    WHERE ghi_chu LIKE N'[[]DEMO] %';
    ENABLE TRIGGER dbo.trg_v3_giao_dich_khong_sua ON dbo.giao_dich_thanh_toan;
    UPDATE dbo.lich_su_trang_thai_phieu SET ghi_chu = CASE ghi_chu
        WHEN N'[DEMO] Tiếp nhận thiết bị' THEN N'Tiếp nhận thiết bị và ghi nhận tình trạng ban đầu.'
        WHEN N'[DEMO] Báo giá đã duyệt, bắt đầu sửa' THEN
            N'Khách đã xác nhận báo giá; chuyển kỹ thuật viên xử lý.'
        WHEN N'[DEMO] Sửa chữa hoàn thành' THEN
            N'Hoàn tất sửa chữa và kiểm tra hoạt động của thiết bị.' END
    WHERE ghi_chu LIKE N'[[]DEMO] %';
    UPDATE dbo.ban_giao_thiet_bi SET ten_nguoi_nhan = N'Trần Hải Nam'
    WHERE ten_nguoi_nhan = N'[DEMO] Trần Hải Nam';
    UPDATE dbo.cam_ket_bao_hanh SET dieu_kien =
        N'Bảo hành hạng mục thay keo tản nhiệt trong 90 ngày kể từ ngày bàn giao.'
    WHERE dieu_kien = N'[DEMO] Bảo hành dịch vụ 90 ngày';
    UPDATE dbo.phieu_bao_hanh SET loi_khach_bao =
        N'Máy vẫn nóng khi chạy tác vụ nặng sau khi thay keo tản nhiệt.'
    WHERE loi_khach_bao = N'[DEMO] Quạt vẫn chạy to sau khi sử dụng';
    DECLARE @ngayHen1 date = DATEADD(DAY, 3, CONVERT(date, DATEADD(HOUR, 7, SYSUTCDATETIME())));
    DECLARE @ngayHen2 date = DATEADD(DAY, 4, CONVERT(date, DATEADD(HOUR, 7, SYSUTCDATETIME())));
    UPDATE dbo.lich_hen SET ten_thiet_bi = N'Dell Latitude 5420',
        thoi_gian_mong_muon = DATEADD(HOUR, 3, CONVERT(datetime2(3), @ngayHen1)),
        mo_ta_loi = N'Pin tụt nhanh, khách muốn kiểm tra tình trạng pin.'
    WHERE khach_hang_id = @kh1 AND ten_thiet_bi = N'[DEMO] Laptop mới';
    UPDATE dbo.lich_hen SET ten_thiet_bi = N'HP ProBook 440',
        thoi_gian_mong_muon = DATEADD(HOUR, 4, CONVERT(datetime2(3), @ngayHen2)),
        mo_ta_loi = N'Khách yêu cầu vệ sinh và kiểm tra quạt tản nhiệt.'
    WHERE khach_hang_id = @kh2 AND ten_thiet_bi = N'[DEMO] Laptop văn phòng';

    IF EXISTS (SELECT 1 FROM dbo.tai_khoan WHERE ten_dang_nhap LIKE 'demo[_]%')
       OR EXISTS (SELECT 1 FROM dbo.phieu_nhap WHERE so_chung_tu_ncc LIKE 'DEMO-%')
       OR EXISTS (SELECT 1 FROM dbo.thiet_bi WHERE so_serial LIKE 'DEMO-%')
       OR EXISTS (SELECT 1 FROM dbo.linh_kien_serial WHERE so_serial LIKE 'DEMO-%')
       OR EXISTS (SELECT 1 FROM dbo.phieu_sua_chua WHERE ghi_chu LIKE N'[[]DEMO] %')
        THROW 51706, N'Con dau du lieu DEMO; da huy toan bo thay doi.', 1;

    COMMIT TRANSACTION;
    PRINT N'Da chuan hoa bo du lieu thu V3. Khong thay doi cac ban ghi khac.';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
