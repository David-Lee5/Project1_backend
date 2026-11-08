using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class AppDbContextV3
{
    public DbSet<LichHenV3> LichHenV3s => Set<LichHenV3>();
    public DbSet<XuLyBaoHanhNccV3> XuLyBaoHanhNccV3s => Set<XuLyBaoHanhNccV3>();
    public DbSet<QuyTacVanHanhV3> QuyTacVanHanhV3s => Set<QuyTacVanHanhV3>();
    public DbSet<XacNhanTiepNhanV3> XacNhanTiepNhanV3s => Set<XacNhanTiepNhanV3>();
    public DbSet<DuyetBaoHanhLapV3> DuyetBaoHanhLapV3s => Set<DuyetBaoHanhLapV3>();
    public DbSet<KiemKeKhoV3> KiemKeKhoV3s => Set<KiemKeKhoV3>();
    public DbSet<DongKiemKeKhoV3> DongKiemKeKhoV3s => Set<DongKiemKeKhoV3>();
    public DbSet<BienDongSerialKiemKeV3> BienDongSerialKiemKeV3s => Set<BienDongSerialKiemKeV3>();
    public DbSet<CachLyVatTuV3> CachLyVatTuV3s => Set<CachLyVatTuV3>();
    public DbSet<CongViecSuaChuaV3> CongViecSuaChuaV3s => Set<CongViecSuaChuaV3>();
    public DbSet<CongViecVatTuV3> CongViecVatTuV3s => Set<CongViecVatTuV3>();
    public DbSet<BaoHanhPhanTichV3> BaoHanhPhanTichV3s => Set<BaoHanhPhanTichV3>();
    public DbSet<HoSoThanhLyV3> HoSoThanhLyV3s => Set<HoSoThanhLyV3>();
    public DbSet<TaiLieuThanhLyV3> TaiLieuThanhLyV3s => Set<TaiLieuThanhLyV3>();
    public DbSet<HoaDonDauVaoV3> HoaDonDauVaoV3s => Set<HoaDonDauVaoV3>();
    public DbSet<HoaDonDienTuV3> HoaDonDienTuV3s => Set<HoaDonDienTuV3>();
    public DbSet<DieuChinhHoaDonV3> DieuChinhHoaDonV3s => Set<DieuChinhHoaDonV3>();

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // SQL datetime2 không lưu múi giờ. Các giá trị của API lịch hẹn được lưu bằng UTC;
        // gắn lại DateTimeKind khi đọc để JSON có hậu tố Z và FE hiển thị giờ địa phương.
        var utcDateTime = new ValueConverter<DateTime, DateTime>(
            value => value,
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        modelBuilder.Entity<PhieuSuaChua>(entity =>
        {
            entity.Property(x => x.KtvNhanId).HasColumnName("ktv_nhan_id");
            entity.Property(x => x.KtvNhanLuc).HasColumnName("ktv_nhan_luc")
                .HasPrecision(3).HasConversion(utcDateTime);
            entity.Property(x => x.KetQuaKiemTra).HasColumnName("ket_qua_kiem_tra").HasMaxLength(1000);
            entity.Property(x => x.LyDoKhongSerial).HasColumnName("ly_do_khong_serial").HasMaxLength(250);
            entity.Property(x => x.ApDungKiemSoatTiepNhan).HasColumnName("ap_dung_kiem_soat_tiep_nhan");
            entity.Property(x => x.ChecklistTiepNhanJson).HasColumnName("checklist_tiep_nhan_json");
        });
        modelBuilder.Entity<AnhPhieuSuaChua>(e =>
        {
            e.Property(x => x.NguoiTaiLenId).HasColumnName("nguoi_tai_len_id");
            e.Property(x => x.Sha256).HasColumnName("sha256").HasMaxLength(64).IsUnicode(false).IsFixedLength();
        });
        modelBuilder.Entity<QuyTacVanHanhV3>(e =>
        {
            e.ToTable("quy_tac_van_hanh_v3");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.BaoHanhLapDuyetTuLan).HasColumnName("bao_hanh_lap_duyet_tu_lan");
            e.Property(x => x.AnhTiepNhanToiThieu).HasColumnName("anh_tiep_nhan_toi_thieu");
            e.Property(x => x.QuyTacTraCong).HasColumnName("quy_tac_tra_cong").HasMaxLength(500);
            e.Property(x => x.ChinhSachSerial).HasColumnName("chinh_sach_serial").HasMaxLength(500);
            e.Property(x => x.LoaiHinhKinhDoanh).HasColumnName("loai_hinh_kinh_doanh").HasMaxLength(100);
            e.Property(x => x.PhuongPhapThue).HasColumnName("phuong_phap_thue").HasMaxLength(100);
            e.Property(x => x.DonViHoaDonDienTu).HasColumnName("don_vi_hoa_don_dien_tu").HasMaxLength(150);
            e.Property(x => x.NguoiCapNhatId).HasColumnName("nguoi_cap_nhat_id");
            e.Property(x => x.CapNhatLuc).HasColumnName("cap_nhat_luc").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<XacNhanTiepNhanV3>(e =>
        {
            e.ToTable("xac_nhan_tiep_nhan_v3");
            e.HasKey(x => x.PhieuId);
            e.Property(x => x.PhieuId).HasColumnName("phieu_id").ValueGeneratedNever();
            e.Property(x => x.NguoiXacNhan).HasColumnName("nguoi_xac_nhan").HasMaxLength(150);
            e.Property(x => x.AnhBienNhanId).HasColumnName("anh_bien_nhan_id");
            e.Property(x => x.NhanVienGhiNhanId).HasColumnName("nhan_vien_ghi_nhan_id");
            e.Property(x => x.XacNhanLuc).HasColumnName("xac_nhan_luc").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<DuyetBaoHanhLapV3>(e =>
        {
            e.ToTable("duyet_bao_hanh_lap_v3");
            e.HasKey(x => x.PhieuBaoHanhId);
            e.Property(x => x.PhieuBaoHanhId).HasColumnName("phieu_bao_hanh_id").ValueGeneratedNever();
            e.Property(x => x.LanThu).HasColumnName("lan_thu");
            e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.NguoiDeXuatId).HasColumnName("nguoi_de_xuat_id");
            e.Property(x => x.NguoiDuyetId).HasColumnName("nguoi_duyet_id");
            e.Property(x => x.NguyenNhanSoBo).HasColumnName("nguyen_nhan_so_bo").HasMaxLength(1000);
            e.Property(x => x.LyDoQuyetDinh).HasColumnName("ly_do_quyet_dinh").HasMaxLength(1000);
            e.Property(x => x.NgayDuyet).HasColumnName("ngay_duyet").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<KiemKeKhoV3>(e =>
        {
            e.ToTable("kiem_ke_kho_v3");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.KhoId).HasColumnName("kho_id");
            e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.NguoiKiemId).HasColumnName("nguoi_kiem_id");
            e.Property(x => x.NguoiDuyetId).HasColumnName("nguoi_duyet_id");
            e.Property(x => x.TaoLuc).HasColumnName("tao_luc").HasConversion(utcDateTime);
            e.Property(x => x.DuyetLuc).HasColumnName("duyet_luc").HasConversion(utcDateTime);
            e.Property(x => x.GhiChu).HasColumnName("ghi_chu").HasMaxLength(500);
            e.Property(x => x.LyDoQuyetDinh).HasColumnName("ly_do_quyet_dinh").HasMaxLength(500);
            e.HasMany(x => x.ChiTiet).WithOne().HasForeignKey(x => x.KiemKeId);
        });
        modelBuilder.Entity<DongKiemKeKhoV3>(e =>
        {
            e.ToTable("dong_kiem_ke_kho_v3");
            e.HasKey(x => new { x.KiemKeId, x.LinhKienId });
            e.Property(x => x.KiemKeId).HasColumnName("kiem_ke_id");
            e.Property(x => x.LinhKienId).HasColumnName("linh_kien_id");
            e.Property(x => x.SoLuongSoSach).HasColumnName("so_luong_so_sach");
            e.Property(x => x.SoLuongThucTe).HasColumnName("so_luong_thuc_te");
            e.Property(x => x.SerialSoSachJson).HasColumnName("serial_so_sach_json");
            e.Property(x => x.SerialThucTeJson).HasColumnName("serial_thuc_te_json");
        });
        modelBuilder.Entity<BienDongSerialKiemKeV3>(e =>
        {
            e.ToTable("bien_dong_serial_kiem_ke_v3");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.KiemKeId).HasColumnName("kiem_ke_id");
            e.Property(x => x.LinhKienSerialId).HasColumnName("linh_kien_serial_id");
            e.Property(x => x.TrangThaiCu).HasColumnName("trang_thai_cu").HasMaxLength(30);
            e.Property(x => x.TrangThaiMoi).HasColumnName("trang_thai_moi").HasMaxLength(30);
            e.Property(x => x.ThoiGian).HasColumnName("thoi_gian").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<CachLyVatTuV3>(e =>
        {
            e.ToTable("cach_ly_vat_tu_v3");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.KhoId).HasColumnName("kho_id");
            e.Property(x => x.LinhKienId).HasColumnName("linh_kien_id");
            e.Property(x => x.LinhKienSerialId).HasColumnName("linh_kien_serial_id");
            e.Property(x => x.SoLuong).HasColumnName("so_luong");
            e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.LyDo).HasColumnName("ly_do").HasMaxLength(500);
            e.Property(x => x.NguoiTaoId).HasColumnName("nguoi_tao_id");
            e.Property(x => x.TaoLuc).HasColumnName("tao_luc").HasConversion(utcDateTime);
            e.Property(x => x.NguoiXuLyId).HasColumnName("nguoi_xu_ly_id");
            e.Property(x => x.XuLyLuc).HasColumnName("xu_ly_luc").HasConversion(utcDateTime);
            e.Property(x => x.LyDoXuLy).HasColumnName("ly_do_xu_ly").HasMaxLength(500);
        });
        modelBuilder.Entity<CongViecSuaChuaV3>(e =>
        {
            e.ToTable("cong_viec_sua_chua_v3"); e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.PhieuId).HasColumnName("phieu_id");
            e.Property(x => x.PhieuBaoHanhId).HasColumnName("phieu_bao_hanh_id");
            e.Property(x => x.MoTa).HasColumnName("mo_ta").HasMaxLength(500);
            e.Property(x => x.KyThuatVienId).HasColumnName("ky_thuat_vien_id");
            e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.TaoBoiId).HasColumnName("tao_boi_id");
            e.Property(x => x.TaoLuc).HasColumnName("tao_luc").HasConversion(utcDateTime);
            e.Property(x => x.BatDauLuc).HasColumnName("bat_dau_luc").HasConversion(utcDateTime);
            e.Property(x => x.HoanThanhLuc).HasColumnName("hoan_thanh_luc").HasConversion(utcDateTime);
            e.Property(x => x.KetQua).HasColumnName("ket_qua").HasMaxLength(1000);
            e.Property(x => x.CongDeXuat).HasColumnName("cong_de_xuat").HasPrecision(14, 2);
            e.Property(x => x.CongDeXuatBoiId).HasColumnName("cong_de_xuat_boi_id");
            e.Property(x => x.CongDuyet).HasColumnName("cong_duyet").HasPrecision(14, 2);
            e.Property(x => x.CongDuyetBoiId).HasColumnName("cong_duyet_boi_id");
            e.Property(x => x.CongDuyetLuc).HasColumnName("cong_duyet_luc").HasConversion(utcDateTime);
            e.Property(x => x.LyDoDuyetCong).HasColumnName("ly_do_duyet_cong").HasMaxLength(500);
            e.Property(x => x.ChiTraLuc).HasColumnName("chi_tra_luc").HasConversion(utcDateTime);
            e.Property(x => x.ChiTraBoiId).HasColumnName("chi_tra_boi_id");
            e.Property(x => x.MaThamChieuChiTra).HasColumnName("ma_tham_chieu_chi_tra").HasMaxLength(100);
            e.Property(x => x.PhuongThucChiTra).HasColumnName("phuong_thuc_chi_tra").HasMaxLength(20).IsUnicode(false);
            e.HasMany(x => x.VatTu).WithOne().HasForeignKey(x => x.CongViecId);
        });
        modelBuilder.Entity<CongViecVatTuV3>(e =>
        {
            e.ToTable("cong_viec_vat_tu_v3");
            e.HasKey(x => new { x.CongViecId, x.ChiTietXuatKhoId });
            e.HasIndex(x => x.ChiTietXuatKhoId).IsUnique();
            e.Property(x => x.CongViecId).HasColumnName("cong_viec_id");
            e.Property(x => x.ChiTietXuatKhoId).HasColumnName("chi_tiet_xuat_kho_id");
        });
        modelBuilder.Entity<BaoHanhPhanTichV3>(e =>
        {
            e.ToTable("bao_hanh_phan_tich_v3"); e.HasKey(x => x.PhieuBaoHanhId);
            e.Property(x => x.PhieuBaoHanhId).HasColumnName("phieu_bao_hanh_id").ValueGeneratedNever();
            e.Property(x => x.NguyenNhan).HasColumnName("nguyen_nhan").HasMaxLength(1000);
            e.Property(x => x.BenChiuChiPhi).HasColumnName("ben_chiu_chi_phi").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.ChiPhiDuKien).HasColumnName("chi_phi_du_kien").HasPrecision(14, 2);
            e.Property(x => x.ChiPhiThucTe).HasColumnName("chi_phi_thuc_te").HasPrecision(14, 2);
            e.Property(x => x.NguoiGhiId).HasColumnName("nguoi_ghi_id");
            e.Property(x => x.CapNhatLuc).HasColumnName("cap_nhat_luc").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<HoSoThanhLyV3>(e =>
        {
            e.ToTable("ho_so_thanh_ly_v3"); e.HasKey(x => x.Id);
            e.HasIndex(x => x.CachLyId);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.CachLyId).HasColumnName("cach_ly_id");
            e.Property(x => x.KetQuaKiemTra).HasColumnName("ket_qua_kiem_tra").HasMaxLength(1000);
            e.Property(x => x.HuongXuLy).HasColumnName("huong_xu_ly").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.GiaTriUocTinh).HasColumnName("gia_tri_uoc_tinh").HasPrecision(14, 2);
            e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.NguoiDeXuatId).HasColumnName("nguoi_de_xuat_id");
            e.Property(x => x.DeXuatLuc).HasColumnName("de_xuat_luc").HasConversion(utcDateTime);
            e.Property(x => x.NguoiDuyetId).HasColumnName("nguoi_duyet_id");
            e.Property(x => x.DuyetLuc).HasColumnName("duyet_luc").HasConversion(utcDateTime);
            e.Property(x => x.LyDoQuyetDinh).HasColumnName("ly_do_quyet_dinh").HasMaxLength(500);
            e.Property(x => x.GiaTriDuyet).HasColumnName("gia_tri_duyet").HasPrecision(14, 2);
            e.Property(x => x.GiaTriThuVe).HasColumnName("gia_tri_thu_ve").HasPrecision(14, 2);
        });
        modelBuilder.Entity<TaiLieuThanhLyV3>(e =>
        {
            e.ToTable("tai_lieu_thanh_ly_v3"); e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.HoSoId).HasColumnName("ho_so_id");
            e.Property(x => x.Loai).HasColumnName("loai").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.TenGoc).HasColumnName("ten_goc").HasMaxLength(200);
            e.Property(x => x.DuongDan).HasColumnName("duong_dan").HasMaxLength(400);
            e.Property(x => x.Sha256).HasColumnName("sha256").HasMaxLength(64).IsUnicode(false).IsFixedLength();
            e.Property(x => x.NguoiTaiId).HasColumnName("nguoi_tai_id");
            e.Property(x => x.TaiLuc).HasColumnName("tai_luc").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<HoaDonDauVaoV3>(e =>
        {
            e.ToTable("hoa_don_dau_vao_v3"); e.HasKey(x => x.Id);
            e.HasIndex(x => x.PhieuNhapId).IsUnique();
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.PhieuNhapId).HasColumnName("phieu_nhap_id");
            e.Property(x => x.SoHoaDon).HasColumnName("so_hoa_don").HasMaxLength(100);
            e.Property(x => x.NgayHoaDon).HasColumnName("ngay_hoa_don");
            e.Property(x => x.MaSoThueNcc).HasColumnName("ma_so_thue_ncc").HasMaxLength(30);
            e.Property(x => x.TienHang).HasColumnName("tien_hang").HasPrecision(14, 2);
            e.Property(x => x.TienThue).HasColumnName("tien_thue").HasPrecision(14, 2);
            e.Property(x => x.TongTien).HasColumnName("tong_tien").HasPrecision(14, 2);
            e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.ChenhLech).HasColumnName("chenh_lech").HasPrecision(14, 2);
            e.Property(x => x.GhiChu).HasColumnName("ghi_chu").HasMaxLength(500);
            e.Property(x => x.NguoiGhiId).HasColumnName("nguoi_ghi_id");
            e.Property(x => x.GhiLuc).HasColumnName("ghi_luc").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<HoaDonDienTuV3>(e =>
        {
            e.ToTable("hoa_don_dien_tu_v3"); e.HasKey(x => x.Id);
            e.HasIndex(x => x.HoaDonId).IsUnique();
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.HoaDonId).HasColumnName("hoa_don_id");
            e.Property(x => x.NhaCungCap).HasColumnName("nha_cung_cap").HasMaxLength(150);
            e.Property(x => x.SoHoaDon).HasColumnName("so_hoa_don").HasMaxLength(100);
            e.Property(x => x.NgayPhatHanh).HasColumnName("ngay_phat_hanh").HasConversion(utcDateTime);
            e.Property(x => x.MaTraCuu).HasColumnName("ma_tra_cuu").HasMaxLength(150);
            e.Property(x => x.TongTien).HasColumnName("tong_tien").HasPrecision(14, 2);
            e.Property(x => x.TrangThai).HasColumnName("trang_thai").HasMaxLength(20).IsUnicode(false);
            e.Property(x => x.NguoiGhiId).HasColumnName("nguoi_ghi_id");
            e.Property(x => x.GhiLuc).HasColumnName("ghi_luc").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<DieuChinhHoaDonV3>(e =>
        {
            e.ToTable("dieu_chinh_hoa_don_v3"); e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.HoaDonDienTuId).HasColumnName("hoa_don_dien_tu_id");
            e.Property(x => x.SoChungTu).HasColumnName("so_chung_tu").HasMaxLength(100);
            e.Property(x => x.Loai).HasColumnName("loai").HasMaxLength(15).IsUnicode(false);
            e.Property(x => x.LyDo).HasColumnName("ly_do").HasMaxLength(1000);
            e.Property(x => x.ChenhLechTien).HasColumnName("chenh_lech_tien").HasPrecision(14, 2);
            e.Property(x => x.NgayChungTu).HasColumnName("ngay_chung_tu").HasConversion(utcDateTime);
            e.Property(x => x.NguoiGhiId).HasColumnName("nguoi_ghi_id");
            e.Property(x => x.GhiLuc).HasColumnName("ghi_luc").HasConversion(utcDateTime);
        });
        modelBuilder.Entity<LichHenV3>(e =>
        {
            e.ToTable("lich_hen_v3");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.KhachHangId).HasColumnName("khach_hang_id");
            e.Property(x => x.ThoiGianMongMuon).HasColumnName("thoi_gian_mong_muon")
                .HasConversion(utcDateTime);
            e.Property(x => x.TenThietBi).HasColumnName("ten_thiet_bi");
            e.Property(x => x.MoTaLoi).HasColumnName("mo_ta_loi");
            e.Property(x => x.TrangThai).HasColumnName("trang_thai");
            e.Property(x => x.GhiChu).HasColumnName("ghi_chu");
            e.Property(x => x.PhieuId).HasColumnName("phieu_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at")
                .HasConversion(utcDateTime).HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(x => x.UpdatedAt).HasColumnName("updated_at")
                .HasConversion(utcDateTime).HasDefaultValueSql("SYSUTCDATETIME()");
        });
        modelBuilder.Entity<XuLyBaoHanhNccV3>(e =>
        {
            e.ToTable("xu_ly_bao_hanh_ncc_v3");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PhieuBaoHanhNccId).IsUnique();
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.PhieuBaoHanhNccId).HasColumnName("phieu_bao_hanh_ncc_id");
            e.Property(x => x.Loai).HasColumnName("loai");
            e.Property(x => x.KhoId).HasColumnName("kho_id");
            e.Property(x => x.PhieuNhapId).HasColumnName("phieu_nhap_id");
            e.Property(x => x.SoLuong).HasColumnName("so_luong");
            e.Property(x => x.NguoiThucHienId).HasColumnName("nguoi_thuc_hien_id");
            e.Property(x => x.ThoiGian).HasColumnName("thoi_gian").HasDefaultValueSql("SYSUTCDATETIME()");
            e.Property(x => x.GhiChu).HasColumnName("ghi_chu");
        });
        modelBuilder.Entity<PhieuBaoHanhNcc>(e =>
        {
            e.Property(x => x.KhoGuiId).HasColumnName("kho_gui_id");
            e.Property(x => x.DaTruTon).HasColumnName("da_tru_ton");
            e.Property(x => x.NguonHang).HasColumnName("nguon_hang").HasDefaultValue("CU");
        });
        modelBuilder.Entity<BaoGium>(e =>
            e.Property(x => x.KhachHangPhanHoiId).HasColumnName("khach_hang_phan_hoi_id"));
    }
}
