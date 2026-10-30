using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Model;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnhLinhKien> AnhLinhKiens { get; set; }

    public virtual DbSet<AnhPhieuSuaChua> AnhPhieuSuaChuas { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BienDongKho> BienDongKhos { get; set; }

    public virtual DbSet<ChiTietDichVu> ChiTietDichVus { get; set; }

    public virtual DbSet<ChiTietNhapKho> ChiTietNhapKhos { get; set; }

    public virtual DbSet<ChiTietXuatKho> ChiTietXuatKhos { get; set; }

    public virtual DbSet<DichVu> DichVus { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<Kho> Khos { get; set; }

    public virtual DbSet<KyThuatVien> KyThuatViens { get; set; }

    public virtual DbSet<LeTan> LeTans { get; set; }

    public virtual DbSet<LichSuTrangThaiPhieu> LichSuTrangThaiPhieus { get; set; }

    public virtual DbSet<LinhKien> LinhKiens { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; }

    public virtual DbSet<PhieuSuaChua> PhieuSuaChuas { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThietBi> ThietBis { get; set; }

    public virtual DbSet<ThuKho> ThuKhos { get; set; }

    public virtual DbSet<TonKho> TonKhos { get; set; }

    public virtual DbSet<VLinhKienHienThi> VLinhKienHienThis { get; set; }

    public virtual DbSet<VPhieuChiPhi> VPhieuChiPhis { get; set; }

    public virtual DbSet<VPhieuNhapTong> VPhieuNhapTongs { get; set; }

    public virtual DbSet<VTonKhoSapHet> VTonKhoSapHets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnhLinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__anh_linh__3213E83F316F74EA");

            entity.ToTable("anh_linh_kien");

            entity.HasIndex(e => new { e.LinhKienId, e.ThuTu }, "ix_anh_linh_kien_lk");

            entity.HasIndex(e => e.LinhKienId, "uq_anh_linh_kien_chinh")
                .IsUnique()
                .HasFilter("([la_anh_chinh]=(1))");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DuongDan)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("duong_dan");
            entity.Property(e => e.LaAnhChinh).HasColumnName("la_anh_chinh");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.ThuTu).HasColumnName("thu_tu");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.AnhLinhKiens)
                .HasForeignKey(d => d.LinhKienId)
                .HasConstraintName("fk_anh_linh_kien_lk");
        });

        modelBuilder.Entity<AnhPhieuSuaChua>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__anh_phie__3213E83FF5080BA0");

            entity.ToTable("anh_phieu_sua_chua");

            entity.HasIndex(e => e.PhieuId, "ix_anh_phieu");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DuongDan)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("duong_dan");
            entity.Property(e => e.Loai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("KHAC")
                .HasColumnName("loai");
            entity.Property(e => e.MoTa)
                .HasMaxLength(255)
                .HasColumnName("mo_ta");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");

            entity.HasOne(d => d.Phieu).WithMany(p => p.AnhPhieuSuaChuas)
                .HasForeignKey(d => d.PhieuId)
                .HasConstraintName("fk_anh_phieu");
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audit_lo__3213E83FB68894CC");

            entity.ToTable("audit_log");

            entity.HasIndex(e => new { e.Bang, e.BanGhiId, e.ThoiGian }, "ix_audit_log_bang").IsDescending(false, false, true);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BanGhiId).HasColumnName("ban_ghi_id");
            entity.Property(e => e.Bang)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("bang");
            entity.Property(e => e.DuLieuCu).HasColumnName("du_lieu_cu");
            entity.Property(e => e.DuLieuMoi).HasColumnName("du_lieu_moi");
            entity.Property(e => e.HanhDong)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("hanh_dong");
            entity.Property(e => e.NguoiThucHienId).HasColumnName("nguoi_thuc_hien_id");
            entity.Property(e => e.ThoiGian)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoi_gian");

            entity.HasOne(d => d.NguoiThucHien).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.NguoiThucHienId)
                .HasConstraintName("fk_audit_log_tai_khoan");
        });

        modelBuilder.Entity<BienDongKho>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bien_don__3213E83F93D3BBFF");

            entity.ToTable("bien_dong_kho");

            entity.HasIndex(e => new { e.KhoId, e.LinhKienId, e.ThoiGian }, "ix_bien_dong_kho_lk").IsDescending(false, false, true);

            entity.HasIndex(e => e.PhieuNhapId, "ix_bien_dong_pn").HasFilter("([phieu_nhap_id] IS NOT NULL)");

            entity.HasIndex(e => e.ChiTietXuatKhoId, "ix_bien_dong_xuat").HasFilter("([chi_tiet_xuat_kho_id] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChiTietXuatKhoId).HasColumnName("chi_tiet_xuat_kho_id");
            entity.Property(e => e.KhoId).HasColumnName("kho_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.Loai)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("loai");
            entity.Property(e => e.LyDo).HasColumnName("ly_do");
            entity.Property(e => e.NguoiThucHienId).HasColumnName("nguoi_thuc_hien_id");
            entity.Property(e => e.PhieuNhapId).HasColumnName("phieu_nhap_id");
            entity.Property(e => e.SoLuongSau).HasColumnName("so_luong_sau");
            entity.Property(e => e.SoLuongThayDoi).HasColumnName("so_luong_thay_doi");
            entity.Property(e => e.ThoiGian)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoi_gian");

            entity.HasOne(d => d.ChiTietXuatKho).WithMany(p => p.BienDongKhos)
                .HasForeignKey(d => d.ChiTietXuatKhoId)
                .HasConstraintName("fk_bdk_xuat_kho");

            entity.HasOne(d => d.NguoiThucHien).WithMany(p => p.BienDongKhos)
                .HasForeignKey(d => d.NguoiThucHienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bdk_nguoi");

            entity.HasOne(d => d.PhieuNhap).WithMany(p => p.BienDongKhos)
                .HasForeignKey(d => d.PhieuNhapId)
                .HasConstraintName("fk_bdk_phieu_nhap");

            entity.HasOne(d => d.TonKho).WithMany(p => p.BienDongKhos)
                .HasForeignKey(d => new { d.KhoId, d.LinhKienId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bdk_ton_kho");
        });

        modelBuilder.Entity<ChiTietDichVu>(entity =>
        {
            entity.HasKey(e => new { e.PhieuId, e.DichVuId }).HasName("pk_chi_tiet_dich_vu");

            entity.ToTable("chi_tiet_dich_vu");

            entity.HasIndex(e => e.DichVuId, "ix_ctdv_dich_vu");

            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.DichVuId).HasColumnName("dich_vu_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DonGiaThucTe)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia_thuc_te");
            entity.Property(e => e.SoLuong)
                .HasDefaultValue(1)
                .HasColumnName("so_luong");
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("(CONVERT([decimal](14,2),[so_luong]*[don_gia_thuc_te]))", true)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("thanh_tien");

            entity.HasOne(d => d.DichVu).WithMany(p => p.ChiTietDichVus)
                .HasForeignKey(d => d.DichVuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ctdv_dich_vu");

            entity.HasOne(d => d.Phieu).WithMany(p => p.ChiTietDichVus)
                .HasForeignKey(d => d.PhieuId)
                .HasConstraintName("fk_ctdv_phieu");
        });

        modelBuilder.Entity<ChiTietNhapKho>(entity =>
        {
            entity.HasKey(e => new { e.PhieuNhapId, e.LinhKienId }).HasName("pk_chi_tiet_nhap_kho");

            entity.ToTable("chi_tiet_nhap_kho");

            entity.HasIndex(e => e.LinhKienId, "ix_ctnk_linh_kien");

            entity.Property(e => e.PhieuNhapId).HasColumnName("phieu_nhap_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.DonGiaNhap)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia_nhap");
            entity.Property(e => e.SoLuongNhap).HasColumnName("so_luong_nhap");
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("(CONVERT([decimal](14,2),[so_luong_nhap]*[don_gia_nhap]))", true)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("thanh_tien");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.ChiTietNhapKhos)
                .HasForeignKey(d => d.LinhKienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ctnk_linh_kien");

            entity.HasOne(d => d.PhieuNhap).WithMany(p => p.ChiTietNhapKhos)
                .HasForeignKey(d => d.PhieuNhapId)
                .HasConstraintName("fk_ctnk_phieu_nhap");
        });

        modelBuilder.Entity<ChiTietXuatKho>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__chi_tiet__3213E83F733DA2AB");

            entity.ToTable("chi_tiet_xuat_kho");

            entity.HasIndex(e => new { e.KhoId, e.LinhKienId }, "ix_ctxk_kho_lk");

            entity.HasIndex(e => e.PhieuId, "ix_ctxk_phieu");

            entity.HasIndex(e => e.ThuKhoId, "ix_ctxk_thu_kho");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BaoHanhDen).HasColumnName("bao_hanh_den");
            entity.Property(e => e.DonGiaXuat)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia_xuat");
            entity.Property(e => e.KhoId).HasColumnName("kho_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.NgayXuat)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_xuat");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.SoLuongXuat).HasColumnName("so_luong_xuat");
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("(CONVERT([decimal](14,2),[so_luong_xuat]*[don_gia_xuat]))", true)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("thanh_tien");
            entity.Property(e => e.ThuKhoId).HasColumnName("thu_kho_id");

            entity.HasOne(d => d.Phieu).WithMany(p => p.ChiTietXuatKhos)
                .HasForeignKey(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ctxk_phieu");

            entity.HasOne(d => d.ThuKho).WithMany(p => p.ChiTietXuatKhos)
                .HasForeignKey(d => d.ThuKhoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ctxk_thu_kho");

            entity.HasOne(d => d.TonKho).WithMany(p => p.ChiTietXuatKhos)
                .HasForeignKey(d => new { d.KhoId, d.LinhKienId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ctxk_ton_kho");
        });

        modelBuilder.Entity<DichVu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__dich_vu__3213E83FC46F0764");

            entity.ToTable("dich_vu");

            entity.HasIndex(e => e.TenDichVu, "ix_dich_vu_ten");

            entity.HasIndex(e => e.MaDv, "uq_dich_vu_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(3)
                .HasColumnName("deleted_at");
            entity.Property(e => e.GiaNiemYet)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("gia_niem_yet");
            entity.Property(e => e.HinhAnh)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("hinh_anh");
            entity.Property(e => e.MaDv)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('DV'+case when [id]<(100000) then right('00000'+CONVERT([varchar](20),[id]),(5)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_dv");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.TenDichVu)
                .HasMaxLength(150)
                .HasColumnName("ten_dich_vu");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__hoa_don__3213E83FAF0161A8");

            entity.ToTable("hoa_don");

            entity.HasIndex(e => e.LeTanId, "ix_hoa_don_le_tan");

            entity.HasIndex(e => new { e.TrangThai, e.NgayLap }, "ix_hoa_don_trang_thai").IsDescending(false, true);

            entity.HasIndex(e => e.MaHd, "uq_hoa_don_ma").IsUnique();

            entity.HasIndex(e => e.PhieuId, "uq_hoa_don_phieu_hieu_luc")
                .IsUnique()
                .HasFilter("([trang_thai]<>'HUY')");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GhiChu).HasColumnName("ghi_chu");
            entity.Property(e => e.GiamGia)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("giam_gia");
            entity.Property(e => e.LeTanId).HasColumnName("le_tan_id");
            entity.Property(e => e.MaHd)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('HD'+case when [id]<(10000000) then right('0000000'+CONVERT([varchar](20),[id]),(7)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_hd");
            entity.Property(e => e.NgayLap)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_lap");
            entity.Property(e => e.NgayThanhToan)
                .HasPrecision(3)
                .HasColumnName("ngay_thanh_toan");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.PhuongThucTt)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phuong_thuc_tt");
            entity.Property(e => e.TienDichVu)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("tien_dich_vu");
            entity.Property(e => e.TienLinhKien)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("tien_linh_kien");
            entity.Property(e => e.TongThanhToan)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("tong_thanh_toan");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("CHUA_THANH_TOAN")
                .HasColumnName("trang_thai");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LeTan).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.LeTanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hoa_don_le_tan");

            entity.HasOne(d => d.Phieu).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hoa_don_phieu");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__khach_ha__3213E83F68373380");

            entity.ToTable("khach_hang");

            entity.HasIndex(e => e.HoTen, "ix_khach_hang_ho_ten");

            entity.HasIndex(e => e.SoDienThoai, "ix_khach_hang_sdt");

            entity.HasIndex(e => e.Email, "uq_khach_hang_email")
                .IsUnique()
                .HasFilter("([email] IS NOT NULL AND [deleted_at] IS NULL)");

            entity.HasIndex(e => e.MaKh, "uq_khach_hang_ma").IsUnique();

            entity.HasIndex(e => e.TaiKhoanId, "uq_khach_hang_tai_khoan")
                .IsUnique()
                .HasFilter("([tai_khoan_id] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(3)
                .HasColumnName("deleted_at");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(255)
                .HasColumnName("dia_chi");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.MaKh)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('KH'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_kh");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("so_dien_thoai");
            entity.Property(e => e.TaiKhoanId).HasColumnName("tai_khoan_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.VaiTroTk)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("KHACH_HANG")
                .HasColumnName("vai_tro_tk");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.KhachHangs)
                .HasPrincipalKey(p => new { p.Id, p.VaiTro })
                .HasForeignKey(d => new { d.TaiKhoanId, d.VaiTroTk })
                .HasConstraintName("fk_khach_hang_tai_khoan");
        });

        modelBuilder.Entity<Kho>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__kho__3213E83F0DBEF7FC");

            entity.ToTable("kho");

            entity.HasIndex(e => e.ThuKhoId, "ix_kho_thu_kho");

            entity.HasIndex(e => e.MaKho, "uq_kho_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(3)
                .HasColumnName("deleted_at");
            entity.Property(e => e.DiaDiem)
                .HasMaxLength(255)
                .HasColumnName("dia_diem");
            entity.Property(e => e.MaKho)
                .HasMaxLength(23)
                .IsUnicode(false)
                .HasComputedColumnSql("('KHO'+case when [id]<(1000) then right('000'+CONVERT([varchar](20),[id]),(3)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_kho");
            entity.Property(e => e.TenKho)
                .HasMaxLength(100)
                .HasColumnName("ten_kho");
            entity.Property(e => e.ThuKhoId).HasColumnName("thu_kho_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ThuKho).WithMany(p => p.Khos)
                .HasForeignKey(d => d.ThuKhoId)
                .HasConstraintName("fk_kho_thu_kho");
        });

        modelBuilder.Entity<KyThuatVien>(entity =>
        {
            entity.HasKey(e => e.NhanVienId).HasName("PK__ky_thuat__81F983235F9AF8EB");

            entity.ToTable("ky_thuat_vien");

            entity.Property(e => e.NhanVienId)
                .ValueGeneratedNever()
                .HasColumnName("nhan_vien_id");
            entity.Property(e => e.ChucVu)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("KY_THUAT_VIEN")
                .HasColumnName("chuc_vu");
            entity.Property(e => e.ChuyenMon)
                .HasMaxLength(100)
                .HasColumnName("chuyen_mon");

            entity.HasOne(d => d.NhanVien).WithMany(p => p.KyThuatViens)
                .HasPrincipalKey(p => new { p.Id, p.ChucVu })
                .HasForeignKey(d => new { d.NhanVienId, d.ChucVu })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ky_thuat_vien_nhan_vien");
        });

        modelBuilder.Entity<LeTan>(entity =>
        {
            entity.HasKey(e => e.NhanVienId).HasName("PK__le_tan__81F98323DE78836F");

            entity.ToTable("le_tan");

            entity.Property(e => e.NhanVienId)
                .ValueGeneratedNever()
                .HasColumnName("nhan_vien_id");
            entity.Property(e => e.ChucVu)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("LE_TAN")
                .HasColumnName("chuc_vu");

            entity.HasOne(d => d.NhanVien).WithMany(p => p.LeTans)
                .HasPrincipalKey(p => new { p.Id, p.ChucVu })
                .HasForeignKey(d => new { d.NhanVienId, d.ChucVu })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_le_tan_nhan_vien");
        });

        modelBuilder.Entity<LichSuTrangThaiPhieu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__lich_su___3213E83F44A3A39F");

            entity.ToTable("lich_su_trang_thai_phieu");

            entity.HasIndex(e => new { e.PhieuId, e.ThoiGian }, "ix_lich_su_phieu");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GhiChu).HasColumnName("ghi_chu");
            entity.Property(e => e.NguoiThucHienId).HasColumnName("nguoi_thuc_hien_id");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.ThoiGian)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoi_gian");
            entity.Property(e => e.TrangThaiCu)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("trang_thai_cu");
            entity.Property(e => e.TrangThaiMoi)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("trang_thai_moi");

            entity.HasOne(d => d.NguoiThucHien).WithMany(p => p.LichSuTrangThaiPhieus)
                .HasForeignKey(d => d.NguoiThucHienId)
                .HasConstraintName("fk_lich_su_nguoi");

            entity.HasOne(d => d.Phieu).WithMany(p => p.LichSuTrangThaiPhieus)
                .HasForeignKey(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lich_su_phieu");
        });

        modelBuilder.Entity<LinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__linh_kie__3213E83F80ED3131");

            entity.ToTable("linh_kien");

            entity.HasIndex(e => e.TenLinhKien, "ix_linh_kien_ten");

            entity.HasIndex(e => e.MaLk, "uq_linh_kien_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(3)
                .HasColumnName("deleted_at");
            entity.Property(e => e.DonGiaBan)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia_ban");
            entity.Property(e => e.DonViTinh)
                .HasMaxLength(20)
                .HasDefaultValue("cái")
                .HasColumnName("don_vi_tinh");
            entity.Property(e => e.HangSanXuat)
                .HasMaxLength(100)
                .HasColumnName("hang_san_xuat");
            entity.Property(e => e.MaLk)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('LK'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_lk");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.TenLinhKien)
                .HasMaxLength(150)
                .HasColumnName("ten_linh_kien");
            entity.Property(e => e.ThoiHanBaoHanhThang).HasColumnName("thoi_han_bao_hanh_thang");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__nha_cung__3213E83FD04CFCA3");

            entity.ToTable("nha_cung_cap");

            entity.HasIndex(e => e.MaNcc, "uq_nha_cung_cap_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(3)
                .HasColumnName("deleted_at");
            entity.Property(e => e.DiaChi)
                .HasMaxLength(255)
                .HasColumnName("dia_chi");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.MaNcc)
                .HasMaxLength(23)
                .IsUnicode(false)
                .HasComputedColumnSql("('NCC'+case when [id]<(10000) then right('0000'+CONVERT([varchar](20),[id]),(4)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_ncc");
            entity.Property(e => e.MaSoThue)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("ma_so_thue");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("so_dien_thoai");
            entity.Property(e => e.TenNcc)
                .HasMaxLength(150)
                .HasColumnName("ten_ncc");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__nhan_vie__3213E83FAD558156");

            entity.ToTable("nhan_vien");

            entity.HasIndex(e => new { e.Id, e.ChucVu }, "uq_nhan_vien_id_chuc_vu").IsUnique();

            entity.HasIndex(e => e.MaNv, "uq_nhan_vien_ma").IsUnique();

            entity.HasIndex(e => e.TaiKhoanId, "uq_nhan_vien_tai_khoan").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChucVu)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("chuc_vu");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(3)
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.MaNv)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('NV'+case when [id]<(100000) then right('00000'+CONVERT([varchar](20),[id]),(5)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_nv");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("so_dien_thoai");
            entity.Property(e => e.TaiKhoanId).HasColumnName("tai_khoan_id");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.NhanViens)
                .HasPrincipalKey(p => new { p.Id, p.VaiTro })
                .HasForeignKey(d => new { d.TaiKhoanId, d.ChucVu })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_nhan_vien_tai_khoan");
        });

        modelBuilder.Entity<PhieuNhap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__phieu_nh__3213E83FC6CB0768");

            entity.ToTable("phieu_nhap");

            entity.HasIndex(e => new { e.KhoId, e.NgayNhap }, "ix_phieu_nhap_kho").IsDescending(false, true);

            entity.HasIndex(e => e.NhaCungCapId, "ix_phieu_nhap_ncc");

            entity.HasIndex(e => e.ThuKhoId, "ix_phieu_nhap_thu_kho");

            entity.HasIndex(e => e.MaPn, "uq_phieu_nhap_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GhiChu).HasColumnName("ghi_chu");
            entity.Property(e => e.KhoId).HasColumnName("kho_id");
            entity.Property(e => e.MaPn)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('PN'+case when [id]<(10000000) then right('0000000'+CONVERT([varchar](20),[id]),(7)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_pn");
            entity.Property(e => e.NgayNhap)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_nhap");
            entity.Property(e => e.NhaCungCapId).HasColumnName("nha_cung_cap_id");
            entity.Property(e => e.SoChungTuNcc)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("so_chung_tu_ncc");
            entity.Property(e => e.ThuKhoId).HasColumnName("thu_kho_id");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("NHAP")
                .HasColumnName("trang_thai");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Kho).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.KhoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_phieu_nhap_kho");

            entity.HasOne(d => d.NhaCungCap).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.NhaCungCapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_phieu_nhap_ncc");

            entity.HasOne(d => d.ThuKho).WithMany(p => p.PhieuNhaps)
                .HasForeignKey(d => d.ThuKhoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_phieu_nhap_thu_kho");
        });

        modelBuilder.Entity<PhieuSuaChua>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__phieu_su__3213E83F0CCD252F");

            entity.ToTable("phieu_sua_chua");

            entity.HasIndex(e => new { e.KyThuatVienId, e.TrangThai }, "ix_phieu_ktv");

            entity.HasIndex(e => e.LeTanId, "ix_phieu_le_tan");

            entity.HasIndex(e => e.ThietBiId, "ix_phieu_thiet_bi");

            entity.HasIndex(e => new { e.TrangThai, e.NgayNhan }, "ix_phieu_trang_thai").IsDescending(false, true);

            entity.HasIndex(e => e.MaPhieu, "uq_phieu_sua_chua_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.GhiChu).HasColumnName("ghi_chu");
            entity.Property(e => e.KetQuaSuaChua).HasColumnName("ket_qua_sua_chua");
            entity.Property(e => e.KyThuatVienId).HasColumnName("ky_thuat_vien_id");
            entity.Property(e => e.LeTanId).HasColumnName("le_tan_id");
            entity.Property(e => e.LyDoHuy).HasColumnName("ly_do_huy");
            entity.Property(e => e.MaPhieu)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('SC'+case when [id]<(10000000) then right('0000000'+CONVERT([varchar](20),[id]),(7)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_phieu");
            entity.Property(e => e.NgayHenTra)
                .HasPrecision(3)
                .HasColumnName("ngay_hen_tra");
            entity.Property(e => e.NgayHoanThanh)
                .HasPrecision(3)
                .HasColumnName("ngay_hoan_thanh");
            entity.Property(e => e.NgayNhan)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_nhan");
            entity.Property(e => e.NgayTra)
                .HasPrecision(3)
                .HasColumnName("ngay_tra");
            entity.Property(e => e.ThietBiId).HasColumnName("thiet_bi_id");
            entity.Property(e => e.TinhTrangLoi).HasColumnName("tinh_trang_loi");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("TIEP_NHAN")
                .HasColumnName("trang_thai");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.KyThuatVien).WithMany(p => p.PhieuSuaChuas)
                .HasForeignKey(d => d.KyThuatVienId)
                .HasConstraintName("fk_phieu_ktv");

            entity.HasOne(d => d.LeTan).WithMany(p => p.PhieuSuaChuas)
                .HasForeignKey(d => d.LeTanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_phieu_le_tan");

            entity.HasOne(d => d.ThietBi).WithMany(p => p.PhieuSuaChuas)
                .HasForeignKey(d => d.ThietBiId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_phieu_thiet_bi");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__refresh___3213E83FA9418C7A");

            entity.ToTable("refresh_token");

            entity.HasIndex(e => e.TaiKhoanId, "ix_refresh_token_tai_khoan");

            entity.HasIndex(e => e.TokenHash, "uq_refresh_token_hash").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.HetHan)
                .HasPrecision(3)
                .HasColumnName("het_han");
            entity.Property(e => e.TaiKhoanId).HasColumnName("tai_khoan_id");
            entity.Property(e => e.ThuHoiLuc)
                .HasPrecision(3)
                .HasColumnName("thu_hoi_luc");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(128)
                .IsUnicode(false)
                .HasColumnName("token_hash");

            entity.HasOne(d => d.TaiKhoan).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.TaiKhoanId)
                .HasConstraintName("fk_refresh_token_tai_khoan");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tai_khoa__3213E83FD9F1C361");

            entity.ToTable("tai_khoan");

            entity.HasIndex(e => new { e.Id, e.VaiTro }, "uq_tai_khoan_id_vai_tro").IsUnique();

            entity.HasIndex(e => e.TenDangNhap, "uq_tai_khoan_ten_dang_nhap").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DangNhapCuoi)
                .HasPrecision(3)
                .HasColumnName("dang_nhap_cuoi");
            entity.Property(e => e.HoatDong)
                .HasDefaultValue(true)
                .HasColumnName("hoat_dong");
            entity.Property(e => e.KhoaDen)
                .HasPrecision(3)
                .HasColumnName("khoa_den");
            entity.Property(e => e.MatKhauHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("mat_khau_hash");
            entity.Property(e => e.SoLanDangNhapSai).HasColumnName("so_lan_dang_nhap_sai");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ten_dang_nhap");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");
            entity.Property(e => e.VaiTro)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("vai_tro");
        });

        modelBuilder.Entity<ThietBi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__thiet_bi__3213E83FD8EEB24D");

            entity.ToTable("thiet_bi");

            entity.HasIndex(e => e.KhachHangId, "ix_thiet_bi_khach_hang");

            entity.HasIndex(e => e.MaThietBi, "uq_thiet_bi_ma").IsUnique();

            entity.HasIndex(e => e.SoSerial, "uq_thiet_bi_so_serial")
                .IsUnique()
                .HasFilter("([so_serial] IS NOT NULL AND [deleted_at] IS NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasPrecision(3)
                .HasColumnName("deleted_at");
            entity.Property(e => e.GhiChu).HasColumnName("ghi_chu");
            entity.Property(e => e.KhachHangId).HasColumnName("khach_hang_id");
            entity.Property(e => e.LoaiMay)
                .HasMaxLength(50)
                .HasColumnName("loai_may");
            entity.Property(e => e.MaThietBi)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('TB'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_thiet_bi");
            entity.Property(e => e.NgayMua).HasColumnName("ngay_mua");
            entity.Property(e => e.SoSerial)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("so_serial");
            entity.Property(e => e.TenThietBi)
                .HasMaxLength(150)
                .HasColumnName("ten_thiet_bi");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.KhachHang).WithMany(p => p.ThietBis)
                .HasForeignKey(d => d.KhachHangId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_thiet_bi_khach_hang");
        });

        modelBuilder.Entity<ThuKho>(entity =>
        {
            entity.HasKey(e => e.NhanVienId).HasName("PK__thu_kho__81F983236CB3C589");

            entity.ToTable("thu_kho");

            entity.Property(e => e.NhanVienId)
                .ValueGeneratedNever()
                .HasColumnName("nhan_vien_id");
            entity.Property(e => e.ChucVu)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("THU_KHO")
                .HasColumnName("chuc_vu");

            entity.HasOne(d => d.NhanVien).WithMany(p => p.ThuKhos)
                .HasPrincipalKey(p => new { p.Id, p.ChucVu })
                .HasForeignKey(d => new { d.NhanVienId, d.ChucVu })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_thu_kho_nhan_vien");
        });

        modelBuilder.Entity<TonKho>(entity =>
        {
            entity.HasKey(e => new { e.KhoId, e.LinhKienId }).HasName("pk_ton_kho");

            entity.ToTable("ton_kho");

            entity.HasIndex(e => e.LinhKienId, "ix_ton_kho_linh_kien");

            entity.Property(e => e.KhoId).HasColumnName("kho_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.MucTonToiThieu).HasColumnName("muc_ton_toi_thieu");
            entity.Property(e => e.NgayCapNhat)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_cap_nhat");
            entity.Property(e => e.SoLuongTon).HasColumnName("so_luong_ton");

            entity.HasOne(d => d.Kho).WithMany(p => p.TonKhos)
                .HasForeignKey(d => d.KhoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ton_kho_kho");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.TonKhos)
                .HasForeignKey(d => d.LinhKienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ton_kho_linh_kien");
        });

        modelBuilder.Entity<VLinhKienHienThi>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_linh_kien_hien_thi");

            entity.Property(e => e.AnhChinh)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("anh_chinh");
            entity.Property(e => e.DonGiaBan)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia_ban");
            entity.Property(e => e.DonViTinh)
                .HasMaxLength(20)
                .HasColumnName("don_vi_tinh");
            entity.Property(e => e.HangSanXuat)
                .HasMaxLength(100)
                .HasColumnName("hang_san_xuat");
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.MaLk)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasColumnName("ma_lk");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.TenLinhKien)
                .HasMaxLength(150)
                .HasColumnName("ten_linh_kien");
            entity.Property(e => e.ThoiHanBaoHanhThang).HasColumnName("thoi_han_bao_hanh_thang");
            entity.Property(e => e.TongTon).HasColumnName("tong_ton");
        });

        modelBuilder.Entity<VPhieuChiPhi>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_phieu_chi_phi");

            entity.Property(e => e.PhieuId)
                .ValueGeneratedOnAdd()
                .HasColumnName("phieu_id");
            entity.Property(e => e.TienDichVu)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("tien_dich_vu");
            entity.Property(e => e.TienLinhKien)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("tien_linh_kien");
            entity.Property(e => e.TongTamTinh)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("tong_tam_tinh");
        });

        modelBuilder.Entity<VPhieuNhapTong>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_phieu_nhap_tong");

            entity.Property(e => e.PhieuNhapId).HasColumnName("phieu_nhap_id");
            entity.Property(e => e.TongTienNhap)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("tong_tien_nhap");
        });

        modelBuilder.Entity<VTonKhoSapHet>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_ton_kho_sap_het");

            entity.Property(e => e.KhoId).HasColumnName("kho_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.MucTonToiThieu).HasColumnName("muc_ton_toi_thieu");
            entity.Property(e => e.SoLuongTon).HasColumnName("so_luong_ton");
            entity.Property(e => e.TenKho)
                .HasMaxLength(100)
                .HasColumnName("ten_kho");
            entity.Property(e => e.TenLinhKien)
                .HasMaxLength(150)
                .HasColumnName("ten_linh_kien");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
