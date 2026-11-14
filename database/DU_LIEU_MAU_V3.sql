-- Dữ liệu nghiệp vụ giả định cho CSDL QLSCTBDT_V3. Chỉ dùng ở môi trường phát triển/kiểm thử.
-- Chạy sau BO_SUNG_API_20261106.sql và BO_SUNG_QUY_TRINH_KTV_20261107.sql.
-- Chạy lại an toàn: nếu bộ dữ liệu đã tồn tại, script chỉ hiển thị dữ liệu hiện có.
-- Mật khẩu của bốn tài khoản thử nghiệm: Demo@123456 (BCrypt work factor 11).
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
    THROW 51400, N'Chỉ nạp dữ liệu mẫu vào QLSCTBDT_V3.', 1;
IF OBJECT_ID(N'dbo.v3_schema_migration', N'U') IS NULL
    THROW 51401, N'CSDL chưa có schema V3.', 1;
IF NOT EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261106_API')
    OR NOT EXISTS (SELECT 1 FROM dbo.v3_schema_migration WHERE version = 'V3_20261107_KTV')
    THROW 51402, N'Cần chạy đủ hai script bổ sung API và quy trình KTV trước khi nạp dữ liệu.', 1;
IF @@TRANCOUNT <> 0
    THROW 51403, N'Hãy chạy seed khi không có giao dịch đang mở.', 1;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.tai_khoan WITH (UPDLOCK, HOLDLOCK)
               WHERE ten_dang_nhap IN ('nguyenthimai', 'tranvanbinh', 'phamquocan', 'nguyenminhanh'))
    BEGIN
        IF (SELECT COUNT(*) FROM dbo.tai_khoan
            WHERE ten_dang_nhap IN ('nguyenthimai', 'tranvanbinh', 'phamquocan', 'nguyenminhanh')) <> 4
            OR NOT EXISTS (SELECT 1 FROM dbo.phieu_nhap WHERE so_chung_tu_ncc = 'MP-2026-0418')
            THROW 51404, N'Đã có một phần dữ liệu giả định; cần kiểm tra thủ công trước khi nạp tiếp.', 1;
        PRINT N'Bộ dữ liệu giả định đã có. Không tạo thêm hoặc đặt lại mật khẩu.';
        COMMIT TRANSACTION;
    END
    ELSE
    BEGIN
        IF EXISTS (SELECT 1 FROM dbo.khach_hang WHERE so_dien_thoai IN ('0906842715', '0973184625'))
            OR EXISTS (SELECT 1 FROM dbo.nhan_vien WHERE so_dien_thoai IN ('0917624385', '0984317652', '0935271846'))
            OR EXISTS (SELECT 1 FROM dbo.phieu_nhap WHERE so_chung_tu_ncc = 'MP-2026-0418')
            OR EXISTS (SELECT 1 FROM dbo.linh_kien_serial WHERE so_serial LIKE 'BOE-NV156-%')
            THROW 51405, N'Dữ liệu trùng mã của bộ giả định đã tồn tại; không tự ghi đè.', 1;
        IF NOT EXISTS (SELECT 1 FROM dbo.loai_linh_kien WHERE ma = 'RAM')
            OR NOT EXISTS (SELECT 1 FROM dbo.loai_linh_kien WHERE ma = 'QUAT_LAPTOP')
            OR NOT EXISTS (SELECT 1 FROM dbo.loai_linh_kien WHERE ma = 'MAN_HINH')
            THROW 51406, N'Thiếu danh mục loại linh kiện RAM, QUAT_LAPTOP hoặc MAN_HINH.', 1;

        DECLARE @now datetime2(3) = SYSUTCDATETIME();
        DECLARE @hash varchar(255) = '$2a$11$m0.vnS0SN7VTWLKS..ygxucmBWw504KYCtN./hJJrWn8wEurmUAgu';
        DECLARE @ltTk bigint, @khoTk bigint, @ktvTk bigint, @khTk bigint;
        DECLARE @lt bigint, @thuKho bigint, @ktv bigint, @kh1 bigint, @kh2 bigint;
        DECLARE @khoId bigint, @ncc bigint, @ram bigint, @quat bigint, @manHinh bigint;
        DECLARE @dvVeSinh bigint, @dvCaiWin bigint, @dvThayKeo bigint, @pn bigint;
        DECLARE @tbA bigint, @tbB bigint, @tbC bigint, @tbD bigint, @tbE bigint;
        DECLARE @pA bigint, @pB bigint, @pC bigint, @pD bigint, @pE bigint;
        DECLARE @bg bigint, @yc bigint, @hd bigint, @gd bigint, @camKet bigint;

        -- Tài khoản và hồ sơ theo đúng vai trò của API.
        INSERT dbo.tai_khoan(ten_dang_nhap, mat_khau_hash, vai_tro)
        VALUES ('nguyenthimai', @hash, 'LE_TAN'), ('tranvanbinh', @hash, 'THU_KHO'),
               ('phamquocan', @hash, 'KY_THUAT_VIEN'), ('nguyenminhanh', @hash, 'KHACH_HANG');
        SELECT @ltTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'nguyenthimai';
        SELECT @khoTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'tranvanbinh';
        SELECT @ktvTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'phamquocan';
        SELECT @khTk = id FROM dbo.tai_khoan WHERE ten_dang_nhap = 'nguyenminhanh';

        INSERT dbo.nhan_vien(ho_ten, so_dien_thoai, email, chuc_vu, tai_khoan_id)
        VALUES (N'Nguyễn Thị Mai', '0917624385', 'mai.nguyen@techfix.test', 'LE_TAN', @ltTk);
        SET @lt = SCOPE_IDENTITY();
        INSERT dbo.le_tan(nhan_vien_id) VALUES (@lt);
        INSERT dbo.nhan_vien(ho_ten, so_dien_thoai, email, chuc_vu, tai_khoan_id)
        VALUES (N'Trần Văn Bình', '0984317652', 'binh.tran@techfix.test', 'THU_KHO', @khoTk);
        SET @thuKho = SCOPE_IDENTITY();
        INSERT dbo.thu_kho(nhan_vien_id) VALUES (@thuKho);
        INSERT dbo.nhan_vien(ho_ten, so_dien_thoai, email, chuc_vu, tai_khoan_id)
        VALUES (N'Phạm Quốc An', '0935271846', 'an.pham@techfix.test', 'KY_THUAT_VIEN', @ktvTk);
        SET @ktv = SCOPE_IDENTITY();
        INSERT dbo.ky_thuat_vien(nhan_vien_id, chuyen_mon) VALUES (@ktv, N'Laptop và màn hình');

        INSERT dbo.khach_hang(ho_ten, so_dien_thoai, email, tai_khoan_id)
        VALUES (N'Nguyễn Minh Anh', '0906842715', 'minhanh.nguyen@khachhang.test', @khTk);
        SET @kh1 = SCOPE_IDENTITY();
        INSERT dbo.khach_hang(ho_ten, so_dien_thoai, email)
        VALUES (N'Trần Hải Nam', '0973184625', 'hainam.tran@khachhang.test');
        SET @kh2 = SCOPE_IDENTITY();

        INSERT dbo.thiet_bi(khach_hang_id, ten_thiet_bi, loai_may, hang_san_xuat, model, so_serial)
        VALUES (@kh1, N'Dell Latitude 5420', N'Laptop', N'Dell', N'Latitude 5420', 'DL5420A9F2K7');
        SET @tbA = SCOPE_IDENTITY();
        INSERT dbo.thiet_bi(khach_hang_id, ten_thiet_bi, loai_may, hang_san_xuat, model, so_serial)
        VALUES (@kh1, N'Asus Vivobook 15', N'Laptop', N'Asus', N'Vivobook 15', 'ASV15M3P8Q2');
        SET @tbB = SCOPE_IDENTITY();
        INSERT dbo.thiet_bi(khach_hang_id, ten_thiet_bi, loai_may, hang_san_xuat, model, so_serial)
        VALUES (@kh1, N'Lenovo ThinkPad T14', N'Laptop', N'Lenovo', N'ThinkPad T14', 'LNT14R6K3H9');
        SET @tbC = SCOPE_IDENTITY();
        INSERT dbo.thiet_bi(khach_hang_id, ten_thiet_bi, loai_may, hang_san_xuat, model, so_serial)
        VALUES (@kh2, N'HP ProBook 440', N'Laptop', N'HP', N'ProBook 440', 'HP440G8C5V2N');
        SET @tbD = SCOPE_IDENTITY();
        INSERT dbo.thiet_bi(khach_hang_id, ten_thiet_bi, loai_may, hang_san_xuat, model, so_serial)
        VALUES (@kh2, N'Acer Aspire 5', N'Laptop', N'Acer', N'Aspire 5', 'ACASP5Q7M4L2');
        SET @tbE = SCOPE_IDENTITY();

        -- Danh mục dịch vụ, kho, nhà cung cấp và ba mẫu linh kiện V3.
        INSERT dbo.dich_vu(ten_dich_vu, mo_ta, gia_niem_yet)
        VALUES (N'Vệ sinh laptop', N'Vệ sinh quạt và khe tản nhiệt', 150000);
        SET @dvVeSinh = SCOPE_IDENTITY();
        INSERT dbo.dich_vu(ten_dich_vu, mo_ta, gia_niem_yet)
        VALUES (N'Cài đặt hệ điều hành', N'Cài đặt và kiểm tra trình điều khiển', 120000);
        SET @dvCaiWin = SCOPE_IDENTITY();
        INSERT dbo.dich_vu(ten_dich_vu, mo_ta, gia_niem_yet)
        VALUES (N'Thay keo tản nhiệt', N'Vệ sinh và thay keo', 200000);
        SET @dvThayKeo = SCOPE_IDENTITY();
        INSERT dbo.nha_cung_cap(ten_ncc, so_dien_thoai, email)
        VALUES (N'Công ty TNHH Linh kiện Minh Phát', '02838274618', 'kinhdoanh@minhphat.test');
        SET @ncc = SCOPE_IDENTITY();
        INSERT dbo.kho(ten_kho, dia_diem, thu_kho_id)
        VALUES (N'Kho linh kiện chính', N'Tầng 1, khu kỹ thuật', @thuKho);
        SET @khoId = SCOPE_IDENTITY();

        INSERT dbo.linh_kien(ten_linh_kien, hang_san_xuat, don_gia_ban,
                             thoi_han_bao_hanh_thang, loai_linh_kien_id, ma_model, theo_doi_serial)
        SELECT N'RAM 8GB DDR4 SO-DIMM', N'Kingston', 450000, 12, id, N'KVR32S22S8/8', 0
        FROM dbo.loai_linh_kien WHERE ma = 'RAM';
        SET @ram = SCOPE_IDENTITY();
        INSERT dbo.linh_kien(ten_linh_kien, hang_san_xuat, don_gia_ban,
                             thoi_han_bao_hanh_thang, loai_linh_kien_id, ma_model, theo_doi_serial)
        SELECT N'Quạt tản nhiệt Lenovo ThinkPad T14', N'Lenovo', 280000, 6, id, N'T14-G1-FAN-4P', 0
        FROM dbo.loai_linh_kien WHERE ma = 'QUAT_LAPTOP';
        SET @quat = SCOPE_IDENTITY();
        INSERT dbo.linh_kien(ten_linh_kien, hang_san_xuat, don_gia_ban,
                             thoi_han_bao_hanh_thang, loai_linh_kien_id, ma_model, theo_doi_serial)
        SELECT N'Màn hình 15.6 FHD 30 pin', N'BOE', 1450000, 12, id, N'NV156FHM-N4K', 1
        FROM dbo.loai_linh_kien WHERE ma = 'MAN_HINH';
        SET @manHinh = SCOPE_IDENTITY();
        INSERT dbo.thong_so_linh_kien(linh_kien_id, thuoc_tinh_id, lua_chon_id)
        SELECT @ram, t.id, c.id FROM dbo.thuoc_tinh_loai_linh_kien t
        JOIN dbo.loai_linh_kien l ON l.id = t.loai_linh_kien_id
        JOIN dbo.lua_chon_thuoc_tinh_linh_kien c ON c.thuoc_tinh_id = t.id
        WHERE l.ma = 'RAM' AND ((t.ma = 'DUNG_LUONG' AND c.gia_tri = N'8')
            OR (t.ma = 'CHUAN_DDR' AND c.gia_tri = N'DDR4')
            OR (t.ma = 'KIEU_KHE' AND c.gia_tri = N'SO-DIMM'));
        INSERT dbo.thong_so_linh_kien(linh_kien_id, thuoc_tinh_id, gia_tri_so)
        SELECT @ram, t.id, 3200 FROM dbo.thuoc_tinh_loai_linh_kien t
        JOIN dbo.loai_linh_kien l ON l.id = t.loai_linh_kien_id
        WHERE l.ma = 'RAM' AND t.ma = 'TOC_DO';
        INSERT dbo.thong_so_linh_kien(linh_kien_id, thuoc_tinh_id, gia_tri_van_ban)
        SELECT @quat, t.id, CASE t.ma WHEN 'MA_PART' THEN N'T14-G1-FAN-4P'
            WHEN 'DONG_MAY' THEN N'ThinkPad T14' ELSE N'4 chân' END
        FROM dbo.thuoc_tinh_loai_linh_kien t JOIN dbo.loai_linh_kien l ON l.id = t.loai_linh_kien_id
        WHERE l.ma = 'QUAT_LAPTOP' AND t.ma IN ('MA_PART', 'DONG_MAY', 'DAU_CAM');
        INSERT dbo.thong_so_linh_kien(linh_kien_id, thuoc_tinh_id, gia_tri_so)
        SELECT @manHinh, t.id, CASE t.ma WHEN 'KICH_THUOC' THEN 15.6 ELSE 30 END
        FROM dbo.thuoc_tinh_loai_linh_kien t JOIN dbo.loai_linh_kien l ON l.id = t.loai_linh_kien_id
        WHERE l.ma = 'MAN_HINH' AND t.ma IN ('KICH_THUOC', 'SO_CHAN_CAP');
        INSERT dbo.thong_so_linh_kien(linh_kien_id, thuoc_tinh_id, gia_tri_van_ban)
        SELECT @manHinh, t.id, N'1920 x 1080'
        FROM dbo.thuoc_tinh_loai_linh_kien t JOIN dbo.loai_linh_kien l ON l.id = t.loai_linh_kien_id
        WHERE l.ma = 'MAN_HINH' AND t.ma = 'DO_PHAN_GIAI';
        EXEC dbo.usp_v3_kiem_tra_linh_kien @linh_kien_id = @ram;
        EXEC dbo.usp_v3_kiem_tra_linh_kien @linh_kien_id = @quat;
        EXEC dbo.usp_v3_kiem_tra_linh_kien @linh_kien_id = @manHinh;

        -- Phiếu nhập đã xác nhận: thủ tục tạo tồn và sổ kho, serial tạo cùng giao dịch.
        INSERT dbo.phieu_nhap(nha_cung_cap_id, kho_id, thu_kho_id, so_chung_tu_ncc, ghi_chu)
        VALUES (@ncc, @khoId, @thuKho, 'MP-2026-0418', N'Nhập bổ sung RAM, quạt và màn hình theo chứng từ nhà cung cấp.');
        SET @pn = SCOPE_IDENTITY();
        INSERT dbo.chi_tiet_nhap_kho(phieu_nhap_id, linh_kien_id, so_luong_nhap, don_gia_nhap, danh_sach_serial)
        VALUES (@pn, @ram, 8, 300000, NULL),
               (@pn, @quat, 2, 180000, NULL),
               (@pn, @manHinh, 2, 950000, N'["BOE-NV156-2408A61","BOE-NV156-2408A62"]');
        EXEC dbo.usp_xac_nhan_nhap_kho @phieu_nhap_id = @pn, @nhan_vien_id = @thuKho;
        INSERT dbo.linh_kien_serial(linh_kien_id, so_serial, kho_id, phieu_nhap_id)
        VALUES (@manHinh, 'BOE-NV156-2408A61', @khoId, @pn),
               (@manHinh, 'BOE-NV156-2408A62', @khoId, @pn);
        UPDATE dbo.ton_kho SET muc_ton_toi_thieu = CASE linh_kien_id
            WHEN @ram THEN 3 WHEN @quat THEN 3 ELSE 1 END
        WHERE kho_id = @khoId AND linh_kien_id IN (@ram, @quat, @manHinh);

        -- Phiếu nhập nháp riêng để thử nút xác nhận nhập kho.
        INSERT dbo.phieu_nhap(nha_cung_cap_id, kho_id, thu_kho_id, so_chung_tu_ncc, ghi_chu)
        VALUES (@ncc, @khoId, @thuKho, 'MP-2026-0419', N'Đang đối chiếu số lượng và chứng từ trước khi xác nhận nhập kho.');
        SET @pn = SCOPE_IDENTITY();
        INSERT dbo.chi_tiet_nhap_kho(phieu_nhap_id, linh_kien_id, so_luong_nhap, don_gia_nhap)
        VALUES (@pn, @quat, 4, 180000);

        -- A: mới tiếp nhận, KTV chưa xác nhận nhận máy.
        INSERT dbo.phieu_sua_chua(thiet_bi_id, le_tan_id, ky_thuat_vien_id, ngay_nhan,
                                   tinh_trang_loi, tinh_trang_ngoai_quan, phu_kien_kem_theo, ghi_chu)
        VALUES (@tbA, @lt, @ktv, DATEADD(HOUR,-5,@now), N'Máy nóng, quạt kêu to', N'Vỏ có vết xước nhẹ',
                N'Sạc laptop', N'Khách phản ánh máy nóng sau khoảng 20 phút sử dụng; chờ kỹ thuật viên kiểm tra.');
        SET @pA = SCOPE_IDENTITY();

        -- B: đã kiểm tra, báo giá đang chờ khách phản hồi.
        INSERT dbo.phieu_sua_chua(thiet_bi_id, le_tan_id, ky_thuat_vien_id, ngay_nhan,
                                   tinh_trang_loi, ktv_nhan_id, ktv_nhan_luc, ket_qua_kiem_tra, ghi_chu)
        VALUES (@tbB, @lt, @ktv, DATEADD(HOUR,-5,@now), N'Máy khởi động chậm', @ktv, DATEADD(MINUTE,-35,@now),
                N'RAM dung lượng thấp; đề nghị nâng cấp RAM 8GB.', N'Đã kiểm tra sơ bộ, chờ khách xác nhận phương án nâng cấp RAM.');
        SET @pB = SCOPE_IDENTITY();
        INSERT dbo.bao_gia(phieu_id, phien_ban, nguoi_tao_id, ngay_tao)
        VALUES (@pB, 1, @lt, DATEADD(MINUTE,-30,@now));
        SET @bg = SCOPE_IDENTITY();
        INSERT dbo.chi_tiet_bao_gia(bao_gia_id, loai, linh_kien_id, ten_hang_muc, so_luong, don_gia)
        VALUES (@bg, 'LINH_KIEN', @ram, N'RAM 8GB DDR4 SO-DIMM', 1, 450000);
        UPDATE dbo.bao_gia SET trang_thai = 'CHO_DUYET', ngay_gui = DATEADD(MINUTE,-20,@now)
        WHERE id = @bg;

        -- C: đang sửa, có yêu cầu vật tư chờ thủ kho duyệt.
        INSERT dbo.phieu_sua_chua(thiet_bi_id, le_tan_id, ky_thuat_vien_id, ngay_nhan,
                                   tinh_trang_loi, ktv_nhan_id, ktv_nhan_luc, ket_qua_kiem_tra, ghi_chu)
        VALUES (@tbC, @lt, @ktv, DATEADD(HOUR,-5,@now), N'Quạt tản nhiệt không quay', @ktv, DATEADD(HOUR,-2,@now),
                N'Quạt hỏng, cần thay quạt mới.', N'Khách đã đồng ý thay quạt; đang chờ cấp linh kiện để sửa chữa.');
        SET @pC = SCOPE_IDENTITY();
        INSERT dbo.bao_gia(phieu_id, phien_ban, nguoi_tao_id, ngay_tao)
        VALUES (@pC, 1, @lt, DATEADD(MINUTE,-30,@now));
        SET @bg = SCOPE_IDENTITY();
        INSERT dbo.chi_tiet_bao_gia(bao_gia_id, loai, dich_vu_id, ten_hang_muc, so_luong, don_gia)
        VALUES (@bg, 'DICH_VU', @dvVeSinh, N'Vệ sinh laptop', 1, 150000);
        INSERT dbo.chi_tiet_bao_gia(bao_gia_id, loai, linh_kien_id, ten_hang_muc, so_luong, don_gia)
        VALUES (@bg, 'LINH_KIEN', @quat, N'Quạt tản nhiệt Lenovo ThinkPad T14', 1, 280000);
        UPDATE dbo.bao_gia SET trang_thai = 'CHO_DUYET', ngay_gui = DATEADD(MINUTE,-20,@now) WHERE id = @bg;
        UPDATE dbo.bao_gia SET trang_thai = 'DA_DUYET', ngay_phan_hoi = DATEADD(MINUTE,-10,@now),
            kenh_phan_hoi = N'Điện thoại', nguoi_ghi_nhan_id = @lt,
            noi_dung_phan_hoi = N'Khách đồng ý thay quạt theo báo giá qua điện thoại.' WHERE id = @bg;
        UPDATE dbo.phieu_sua_chua SET trang_thai = 'DANG_SUA', updated_at = @now WHERE id = @pC;
        INSERT dbo.yeu_cau_linh_kien(phieu_id, nguoi_yeu_cau_id, ghi_chu)
        VALUES (@pC, @ktv, N'Cần cấp một quạt tản nhiệt cho Lenovo ThinkPad T14.');
        SET @yc = SCOPE_IDENTITY();
        INSERT dbo.chi_tiet_yeu_cau_linh_kien(yeu_cau_id, phieu_id, linh_kien_id, so_luong_yeu_cau)
        VALUES (@yc, @pC, @quat, 1);

        -- D: đã hoàn thành, có hóa đơn và một lần thu tiền; còn công nợ.
        INSERT dbo.phieu_sua_chua(thiet_bi_id, le_tan_id, ky_thuat_vien_id, ngay_nhan,
                                   tinh_trang_loi, ktv_nhan_id, ktv_nhan_luc, ket_qua_kiem_tra, ghi_chu)
        VALUES (@tbD, @lt, @ktv, DATEADD(HOUR,-5,@now), N'Máy bụi, nhiệt độ cao', @ktv, DATEADD(HOUR,-3,@now),
                N'Cần vệ sinh hệ thống tản nhiệt.', N'Đã vệ sinh hệ thống tản nhiệt; chờ thanh toán phần còn lại và bàn giao.');
        SET @pD = SCOPE_IDENTITY();
        INSERT dbo.bao_gia(phieu_id, phien_ban, nguoi_tao_id, ngay_tao)
        VALUES (@pD, 1, @lt, DATEADD(MINUTE,-30,@now));
        SET @bg = SCOPE_IDENTITY();
        INSERT dbo.chi_tiet_bao_gia(bao_gia_id, loai, dich_vu_id, ten_hang_muc, so_luong, don_gia)
        VALUES (@bg, 'DICH_VU', @dvVeSinh, N'Vệ sinh laptop', 1, 150000);
        UPDATE dbo.bao_gia SET trang_thai = 'CHO_DUYET', ngay_gui = DATEADD(MINUTE,-20,@now) WHERE id = @bg;
        UPDATE dbo.bao_gia SET trang_thai = 'DA_DUYET', ngay_phan_hoi = DATEADD(MINUTE,-10,@now),
            kenh_phan_hoi = N'Điện thoại', nguoi_ghi_nhan_id = @lt,
            noi_dung_phan_hoi = N'Khách xác nhận thực hiện vệ sinh và kiểm tra tản nhiệt.' WHERE id = @bg;
        UPDATE dbo.phieu_sua_chua SET trang_thai = 'DANG_SUA', updated_at = @now WHERE id = @pD;
        INSERT dbo.chi_tiet_dich_vu(phieu_id, dich_vu_id, so_luong, don_gia_thuc_te)
        VALUES (@pD, @dvVeSinh, 1, 150000);
        UPDATE dbo.phieu_sua_chua SET trang_thai = 'HOAN_THANH', ngay_hoan_thanh = @now,
            ket_qua_sua_chua = N'Đã vệ sinh và kiểm tra nhiệt độ ổn định.', updated_at = @now WHERE id = @pD;
        EXEC dbo.usp_v3_lap_hoa_don @phieu_id = @pD, @le_tan_id = @lt,
            @giam_gia = 0, @hoa_don_id = @hd OUTPUT;
        EXEC dbo.usp_v3_ghi_nhan_thanh_toan @phieu_id = @pD, @hoa_don_id = @hd,
            @loai = 'THU', @so_tien = 50000, @phuong_thuc = 'TIEN_MAT',
            @nguoi_thuc_hien_id = @lt, @ghi_chu = N'Khách thanh toán trước 50.000 đồng, số còn lại thanh toán khi nhận máy.', @giao_dich_id = @gd OUTPUT;

        -- E: đã thanh toán và bàn giao; có cam kết cùng một phiếu bảo hành mới.
        INSERT dbo.phieu_sua_chua(thiet_bi_id, le_tan_id, ky_thuat_vien_id, ngay_nhan,
                                   tinh_trang_loi, ktv_nhan_id, ktv_nhan_luc, ket_qua_kiem_tra, ghi_chu)
        VALUES (@tbE, @lt, @ktv, DATEADD(HOUR,-5,@now), N'Máy quá nhiệt khi sử dụng', @ktv, DATEADD(HOUR,-4,@now),
                N'Keo tản nhiệt đã khô.', N'Máy đã bàn giao; tiếp tục theo dõi phản hồi trong thời gian bảo hành.');
        SET @pE = SCOPE_IDENTITY();
        INSERT dbo.bao_gia(phieu_id, phien_ban, nguoi_tao_id, ngay_tao)
        VALUES (@pE, 1, @lt, DATEADD(MINUTE,-30,@now));
        SET @bg = SCOPE_IDENTITY();
        INSERT dbo.chi_tiet_bao_gia(bao_gia_id, loai, dich_vu_id, ten_hang_muc, so_luong, don_gia)
        VALUES (@bg, 'DICH_VU', @dvThayKeo, N'Thay keo tản nhiệt', 1, 200000);
        UPDATE dbo.bao_gia SET trang_thai = 'CHO_DUYET', ngay_gui = DATEADD(MINUTE,-20,@now) WHERE id = @bg;
        UPDATE dbo.bao_gia SET trang_thai = 'DA_DUYET', ngay_phan_hoi = DATEADD(MINUTE,-10,@now),
            kenh_phan_hoi = N'Điện thoại', nguoi_ghi_nhan_id = @lt,
            noi_dung_phan_hoi = N'Khách đồng ý thay keo tản nhiệt theo báo giá.' WHERE id = @bg;
        UPDATE dbo.phieu_sua_chua SET trang_thai = 'DANG_SUA', updated_at = @now WHERE id = @pE;
        INSERT dbo.chi_tiet_dich_vu(phieu_id, dich_vu_id, so_luong, don_gia_thuc_te)
        VALUES (@pE, @dvThayKeo, 1, 200000);
        UPDATE dbo.phieu_sua_chua SET trang_thai = 'HOAN_THANH', ngay_hoan_thanh = @now,
            ket_qua_sua_chua = N'Đã thay keo và chạy kiểm tra tải.', updated_at = @now WHERE id = @pE;
        EXEC dbo.usp_v3_lap_hoa_don @phieu_id = @pE, @le_tan_id = @lt,
            @giam_gia = 0, @hoa_don_id = @hd OUTPUT;
        EXEC dbo.usp_v3_ghi_nhan_thanh_toan @phieu_id = @pE, @hoa_don_id = @hd,
            @loai = 'THU', @so_tien = 200000, @phuong_thuc = 'CHUYEN_KHOAN',
            @nguoi_thuc_hien_id = @lt, @ghi_chu = N'Khách đã thanh toán đủ bằng chuyển khoản.', @giao_dich_id = @gd OUTPUT;
        INSERT dbo.lich_su_trang_thai_phieu(phieu_id, trang_thai_cu, trang_thai_moi,
                                            nguoi_thuc_hien_id, thoi_gian, ghi_chu)
        SELECT id, NULL, 'TIEP_NHAN', @lt, DATEADD(HOUR,-5,@now), N'Tiếp nhận thiết bị và ghi nhận tình trạng ban đầu.'
        FROM dbo.phieu_sua_chua WHERE id IN (@pA, @pB, @pC, @pD, @pE);
        INSERT dbo.lich_su_trang_thai_phieu(phieu_id, trang_thai_cu, trang_thai_moi,
                                            nguoi_thuc_hien_id, thoi_gian, ghi_chu)
        SELECT id, 'TIEP_NHAN', 'DANG_SUA', @ktv, DATEADD(MINUTE,-5,@now),
            N'Khách đã xác nhận báo giá; chuyển kỹ thuật viên xử lý.'
        FROM dbo.phieu_sua_chua WHERE id IN (@pC, @pD, @pE);
        INSERT dbo.lich_su_trang_thai_phieu(phieu_id, trang_thai_cu, trang_thai_moi,
                                            nguoi_thuc_hien_id, thoi_gian, ghi_chu)
        SELECT id, 'DANG_SUA', 'HOAN_THANH', @ktv, @now, N'Hoàn tất sửa chữa và kiểm tra hoạt động của thiết bị.'
        FROM dbo.phieu_sua_chua WHERE id IN (@pD, @pE);
        EXEC dbo.usp_v3_ban_giao @phieu_id = @pE, @nguoi_giao_id = @lt,
            @ten_nguoi_nhan = N'Trần Hải Nam', @tinh_trang_may = N'Máy hoạt động bình thường',
            @phu_kien_da_tra = N'Sạc laptop', @xac_nhan = N'Khách đã nhận máy';
        INSERT dbo.cam_ket_bao_hanh(phieu_goc_id, loai, dich_vu_id, ngay_bat_dau, ngay_ket_thuc, dieu_kien)
        VALUES (@pE, 'DICH_VU', @dvThayKeo, CONVERT(date,DATEADD(HOUR,7,@now)),
            DATEADD(DAY,90,CONVERT(date,DATEADD(HOUR,7,@now))), N'Bảo hành hạng mục thay keo tản nhiệt trong 90 ngày kể từ ngày bàn giao.');
        SET @camKet = SCOPE_IDENTITY();
        INSERT dbo.phieu_bao_hanh(phieu_goc_id, thiet_bi_id, cam_ket_id, nguoi_tiep_nhan_id, loi_khach_bao)
        VALUES (@pE, @tbE, @camKet, @lt, N'Máy vẫn nóng khi chạy tác vụ nặng sau khi thay keo tản nhiệt.');

        -- Lịch hẹn tính theo ngày Việt Nam tiếp theo, luôn là giờ tương lai.
        DECLARE @ngayMai date = DATEADD(DAY,1,CONVERT(date,DATEADD(HOUR,7,@now)));
        INSERT dbo.lich_hen(khach_hang_id, thoi_gian_mong_muon, ten_thiet_bi, mo_ta_loi)
        VALUES (@kh1, DATEADD(HOUR,3,CONVERT(datetime2(3),@ngayMai)),
            N'Dell Latitude 5420', N'Pin tụt nhanh, khách muốn kiểm tra tình trạng pin.');
        INSERT dbo.lich_hen(khach_hang_id, thoi_gian_mong_muon, ten_thiet_bi, mo_ta_loi, trang_thai)
        VALUES (@kh2, DATEADD(HOUR,4,CONVERT(datetime2(3),@ngayMai)),
            N'HP ProBook 440', N'Khách yêu cầu vệ sinh và kiểm tra quạt tản nhiệt.', 'DA_XAC_NHAN');

        COMMIT TRANSACTION;
        PRINT N'Đã nạp dữ liệu nghiệp vụ giả định V3.';
    END
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;

SELECT ten_dang_nhap, vai_tro FROM dbo.tai_khoan
WHERE ten_dang_nhap IN ('nguyenthimai', 'tranvanbinh', 'phamquocan', 'nguyenminhanh')
ORDER BY ten_dang_nhap;
SELECT p.ma_phieu, p.trang_thai, k.ho_ten AS khach_hang, p.ghi_chu
FROM dbo.phieu_sua_chua p
JOIN dbo.thiet_bi t ON t.id = p.thiet_bi_id
JOIN dbo.khach_hang k ON k.id = t.khach_hang_id
WHERE k.so_dien_thoai IN ('0906842715', '0973184625')
ORDER BY p.id;
SELECT lk.ma_lk, lk.ten_linh_kien, tk.so_luong_ton, tk.muc_ton_toi_thieu
FROM dbo.ton_kho tk JOIN dbo.linh_kien lk ON lk.id = tk.linh_kien_id
WHERE lk.ma_model IN (N'KVR32S22S8/8', N'T14-G1-FAN-4P', N'NV156FHM-N4K')
ORDER BY lk.id;
