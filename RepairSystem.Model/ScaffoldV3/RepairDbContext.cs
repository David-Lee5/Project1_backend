using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class RepairDbContext : DbContext
{
    public RepairDbContext(DbContextOptions<RepairDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AnhLinhKien> AnhLinhKiens { get; set; }

    public virtual DbSet<AnhPhieuSuaChua> AnhPhieuSuaChuas { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<BanGiaoThietBi> BanGiaoThietBis { get; set; }

    public virtual DbSet<BaoGium> BaoGia { get; set; }

    public virtual DbSet<BiMatThietBi> BiMatThietBiV3s { get; set; }

    public virtual DbSet<BienDongKho> BienDongKhos { get; set; }

    public virtual DbSet<CamKetBaoHanh> CamKetBaoHanhs { get; set; }

    public virtual DbSet<ChiTietBaoGium> ChiTietBaoGia { get; set; }

    public virtual DbSet<ChiTietDichVu> ChiTietDichVus { get; set; }

    public virtual DbSet<ChiTietHoaDon> ChiTietHoaDonV3s { get; set; }

    public virtual DbSet<ChiTietNhapKho> ChiTietNhapKhos { get; set; }

    public virtual DbSet<ChiTietXuatKho> ChiTietXuatKhos { get; set; }

    public virtual DbSet<ChiTietYeuCauLinhKien> ChiTietYeuCauLinhKienV3s { get; set; }

    public virtual DbSet<DichVu> DichVus { get; set; }

    public virtual DbSet<GiaoDichThanhToan> GiaoDichThanhToans { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<Kho> Khos { get; set; }

    public virtual DbSet<KyThuatVien> KyThuatViens { get; set; }

    public virtual DbSet<LeTan> LeTans { get; set; }

    public virtual DbSet<LichSuTrangThaiPhieu> LichSuTrangThaiPhieus { get; set; }

    public virtual DbSet<LinhKien> LinhKiens { get; set; }

    public virtual DbSet<LinhKienSerial> LinhKienSerials { get; set; }

    public virtual DbSet<LoaiLinhKien> LoaiLinhKiens { get; set; }

    public virtual DbSet<LuaChonThuocTinhLinhKien> LuaChonThuocTinhLinhKiens { get; set; }

    public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhieuBaoHanh> PhieuBaoHanhs { get; set; }

    public virtual DbSet<PhieuBaoHanhNcc> PhieuBaoHanhNccs { get; set; }

    public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; }

    public virtual DbSet<PhieuSuaChua> PhieuSuaChuas { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThietBi> ThietBis { get; set; }

    public virtual DbSet<ThongSoLinhKien> ThongSoLinhKiens { get; set; }

    public virtual DbSet<ThuKho> ThuKhos { get; set; }

    public virtual DbSet<ThuocTinhLoaiLinhKien> ThuocTinhLoaiLinhKiens { get; set; }

    public virtual DbSet<TonKho> TonKhos { get; set; }

    public virtual DbSet<TraLinhKienKho> TraLinhKienKhoV3s { get; set; }

    public virtual DbSet<V3SchemaMigration> V3SchemaMigrations { get; set; }

    public virtual DbSet<VLinhKienHienThi> VLinhKienHienThis { get; set; }

    public virtual DbSet<VPhieuChiPhi> VPhieuChiPhis { get; set; }

    public virtual DbSet<VPhieuNhapTong> VPhieuNhapTongs { get; set; }

    public virtual DbSet<VTonKhoSapHet> VTonKhoSapHets { get; set; }

    public virtual DbSet<VV3BaoGium> VV3BaoGia { get; set; }

    public virtual DbSet<VV3CongNoPhieu> VV3CongNoPhieus { get; set; }

    public virtual DbSet<YeuCauLinhKien> YeuCauLinhKienV3s { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnhLinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__anh_linh__3213E83F76EB7D6E");

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

            entity.HasOne(d => d.LinhKien).WithOne(p => p.AnhLinhKien)
                .HasForeignKey<AnhLinhKien>(d => d.LinhKienId)
                .HasConstraintName("fk_anh_linh_kien_lk");
        });

        modelBuilder.Entity<AnhPhieuSuaChua>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__anh_phie__3213E83FF7051113");

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
            entity.HasKey(e => e.Id).HasName("PK__audit_lo__3213E83FCD57BBB9");

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

        modelBuilder.Entity<BanGiaoThietBi>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ban_giao_thiet_bi");

            entity.ToTable("ban_giao_thiet_bi");

            entity.HasIndex(e => e.PhieuId, "uq_ban_giao_phieu").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConNoLucGiao)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("con_no_luc_giao");
            entity.Property(e => e.LyDoConNo)
                .HasMaxLength(500)
                .HasColumnName("ly_do_con_no");
            entity.Property(e => e.NguoiDuyetConNoId).HasColumnName("nguoi_duyet_con_no_id");
            entity.Property(e => e.NguoiGiaoId).HasColumnName("nguoi_giao_id");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.PhuKienDaTra)
                .HasMaxLength(1000)
                .HasColumnName("phu_kien_da_tra");
            entity.Property(e => e.TenNguoiNhan)
                .HasMaxLength(150)
                .HasColumnName("ten_nguoi_nhan");
            entity.Property(e => e.ThoiGian)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoi_gian");
            entity.Property(e => e.TinhTrangMay)
                .HasMaxLength(1000)
                .HasColumnName("tinh_trang_may");
            entity.Property(e => e.XacNhan)
                .HasMaxLength(1000)
                .HasColumnName("xac_nhan");

            entity.HasOne(d => d.NguoiDuyetConNo).WithMany(p => p.BanGiaoThietBiNguoiDuyetConNos)
                .HasForeignKey(d => d.NguoiDuyetConNoId)
                .HasConstraintName("fk_bgiao_nguoi_duyet");

            entity.HasOne(d => d.NguoiGiao).WithMany(p => p.BanGiaoThietBiNguoiGiaos)
                .HasForeignKey(d => d.NguoiGiaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bgiao_nguoi_giao");

            entity.HasOne(d => d.Phieu).WithOne(p => p.BanGiaoThietBi)
                .HasForeignKey<BanGiaoThietBi>(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bgiao_phieu");
        });

        modelBuilder.Entity<BaoGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_bao_gia");

            entity.ToTable("bao_gia", tb => tb.HasTrigger("trg_v3_bao_gia_chuyen_trang_thai"));

            entity.HasIndex(e => new { e.PhieuId, e.PhienBan }, "uq_bao_gia_phien_ban").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GiamGia)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("giam_gia");
            entity.Property(e => e.KenhPhanHoi)
                .HasMaxLength(100)
                .HasColumnName("kenh_phan_hoi");
            entity.Property(e => e.NgayGui)
                .HasPrecision(3)
                .HasColumnName("ngay_gui");
            entity.Property(e => e.NgayPhanHoi)
                .HasPrecision(3)
                .HasColumnName("ngay_phan_hoi");
            entity.Property(e => e.NgayTao)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.NguoiGhiNhanId).HasColumnName("nguoi_ghi_nhan_id");
            entity.Property(e => e.NguoiTaoId).HasColumnName("nguoi_tao_id");
            entity.Property(e => e.NoiDungPhanHoi)
                .HasMaxLength(1000)
                .HasColumnName("noi_dung_phan_hoi");
            entity.Property(e => e.PhienBan).HasColumnName("phien_ban");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("NHAP")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.NguoiGhiNhan).WithMany(p => p.BaoGiumNguoiGhiNhans)
                .HasForeignKey(d => d.NguoiGhiNhanId)
                .HasConstraintName("fk_bg_nguoi_ghi_nhan");

            entity.HasOne(d => d.NguoiTao).WithMany(p => p.BaoGiumNguoiTaos)
                .HasForeignKey(d => d.NguoiTaoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bg_nguoi_tao");

            entity.HasOne(d => d.Phieu).WithMany(p => p.BaoGia)
                .HasForeignKey(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bg_phieu");
        });

        modelBuilder.Entity<BiMatThietBi>(entity =>
        {
            entity.HasKey(e => e.PhieuId).HasName("pk_bi_mat_thiet_bi_v3");

            entity.ToTable("bi_mat_thiet_bi");

            entity.Property(e => e.PhieuId)
                .ValueGeneratedNever()
                .HasColumnName("phieu_id");
            entity.Property(e => e.DuLieuMaHoa).HasColumnName("du_lieu_ma_hoa");
            entity.Property(e => e.HetHan)
                .HasPrecision(3)
                .HasColumnName("het_han");
            entity.Property(e => e.MaKhoa)
                .HasMaxLength(200)
                .HasColumnName("ma_khoa");
            entity.Property(e => e.NgayTao)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_tao");

            entity.HasOne(d => d.Phieu).WithOne(p => p.BiMatThietBi)
                .HasForeignKey<BiMatThietBi>(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bi_mat_phieu");
        });

        modelBuilder.Entity<BienDongKho>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bien_don__3213E83FFE7CAB00");

            entity.ToTable("bien_dong_kho");

            entity.HasIndex(e => new { e.KhoId, e.LinhKienId, e.ThoiGian }, "ix_bien_dong_kho_lk").IsDescending(false, false, true);

            entity.HasIndex(e => e.PhieuNhapId, "ix_bien_dong_pn").HasFilter("([phieu_nhap_id] IS NOT NULL)");

            entity.HasIndex(e => e.ChiTietXuatKhoId, "ix_bien_dong_xuat").HasFilter("([chi_tiet_xuat_kho_id] IS NOT NULL)");

            entity.HasIndex(e => e.TraLinhKienId, "uq_bdk_tra_lk_v3")
                .IsUnique()
                .HasFilter("([tra_linh_kien_id] IS NOT NULL)");

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
            entity.Property(e => e.TraLinhKienId).HasColumnName("tra_linh_kien_id");

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

            entity.HasOne(d => d.TraLinhKien).WithOne(p => p.BienDongKho)
                .HasForeignKey<BienDongKho>(d => d.TraLinhKienId)
                .HasConstraintName("fk_bdk_tra_lk_v3");

            entity.HasOne(d => d.TonKho).WithMany(p => p.BienDongKhos)
                .HasForeignKey(d => new { d.KhoId, d.LinhKienId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bdk_ton_kho");
        });

        modelBuilder.Entity<CamKetBaoHanh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_cam_ket_bao_hanh");

            entity.ToTable("cam_ket_bao_hanh");

            entity.HasIndex(e => e.ChiTietXuatKhoId, "uq_ckbh_ctxk")
                .IsUnique()
                .HasFilter("([chi_tiet_xuat_kho_id] IS NOT NULL)");

            entity.HasIndex(e => new { e.PhieuGocId, e.DichVuId }, "uq_ckbh_dich_vu")
                .IsUnique()
                .HasFilter("([dich_vu_id] IS NOT NULL)");

            entity.HasIndex(e => new { e.Id, e.PhieuGocId }, "uq_ckbh_id_phieu").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChiTietXuatKhoId).HasColumnName("chi_tiet_xuat_kho_id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.DichVuId).HasColumnName("dich_vu_id");
            entity.Property(e => e.DieuKien)
                .HasMaxLength(1000)
                .HasColumnName("dieu_kien");
            entity.Property(e => e.Loai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("loai");
            entity.Property(e => e.NgayBatDau).HasColumnName("ngay_bat_dau");
            entity.Property(e => e.NgayKetThuc).HasColumnName("ngay_ket_thuc");
            entity.Property(e => e.PhieuGocId).HasColumnName("phieu_goc_id");

            entity.HasOne(d => d.PhieuGoc).WithMany(p => p.CamKetBaoHanhs)
                .HasForeignKey(d => d.PhieuGocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ckbh_phieu");

            entity.HasOne(d => d.ChiTietXuatKho).WithMany(p => p.CamKetBaoHanhs)
                .HasPrincipalKey(p => new { p.Id, p.PhieuId })
                .HasForeignKey(d => new { d.ChiTietXuatKhoId, d.PhieuGocId })
                .HasConstraintName("fk_ckbh_ctxk");

            entity.HasOne(d => d.ChiTietDichVu).WithOne(p => p.CamKetBaoHanh)
                .HasForeignKey<CamKetBaoHanh>(d => new { d.PhieuGocId, d.DichVuId })
                .HasConstraintName("fk_ckbh_ctdv");
        });

        modelBuilder.Entity<ChiTietBaoGium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_chi_tiet_bao_gia");

            entity.ToTable("chi_tiet_bao_gia", tb => tb.HasTrigger("trg_v3_chi_tiet_bao_gia_khoa"));

            entity.HasIndex(e => e.BaoGiaId, "ix_ctbg_bao_gia");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BaoGiaId).HasColumnName("bao_gia_id");
            entity.Property(e => e.DichVuId).HasColumnName("dich_vu_id");
            entity.Property(e => e.DonGia)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.Loai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("loai");
            entity.Property(e => e.SoLuong).HasColumnName("so_luong");
            entity.Property(e => e.TenHangMuc)
                .HasMaxLength(250)
                .HasColumnName("ten_hang_muc");
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("(CONVERT([decimal](19,2),CONVERT([decimal](19,2),[so_luong])*[don_gia]))", true)
                .HasColumnType("decimal(19, 2)")
                .HasColumnName("thanh_tien");

            entity.HasOne(d => d.BaoGia).WithMany(p => p.ChiTietBaoGia)
                .HasForeignKey(d => d.BaoGiaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ctbg_bg");

            entity.HasOne(d => d.DichVu).WithMany(p => p.ChiTietBaoGia)
                .HasForeignKey(d => d.DichVuId)
                .HasConstraintName("fk_ctbg_dv");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.ChiTietBaoGia)
                .HasForeignKey(d => d.LinhKienId)
                .HasConstraintName("fk_ctbg_lk");
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

        modelBuilder.Entity<ChiTietHoaDon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_chi_tiet_hoa_don_v3");

            entity.ToTable("chi_tiet_hoa_don", tb => tb.HasTrigger("trg_v3_chi_tiet_hoa_don_khong_sua"));

            entity.HasIndex(e => new { e.HoaDonId, e.DichVuId }, "uq_cthd_dv")
                .IsUnique()
                .HasFilter("([dich_vu_id] IS NOT NULL)");

            entity.HasIndex(e => new { e.HoaDonId, e.ChiTietXuatKhoId }, "uq_cthd_xuat")
                .IsUnique()
                .HasFilter("([chi_tiet_xuat_kho_id] IS NOT NULL)");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChiTietXuatKhoId).HasColumnName("chi_tiet_xuat_kho_id");
            entity.Property(e => e.DichVuId).HasColumnName("dich_vu_id");
            entity.Property(e => e.DonGia)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia");
            entity.Property(e => e.HoaDonId).HasColumnName("hoa_don_id");
            entity.Property(e => e.Loai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("loai");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.SoLuong).HasColumnName("so_luong");
            entity.Property(e => e.TenHangMuc)
                .HasMaxLength(250)
                .HasColumnName("ten_hang_muc");
            entity.Property(e => e.ThanhTien)
                .HasComputedColumnSql("(CONVERT([decimal](19,2),CONVERT([decimal](19,2),[so_luong])*[don_gia]))", true)
                .HasColumnType("decimal(19, 2)")
                .HasColumnName("thanh_tien");

            entity.HasOne(d => d.ChiTietXuatKho).WithMany(p => p.ChiTietHoaDonV3s)
                .HasPrincipalKey(p => new { p.Id, p.PhieuId })
                .HasForeignKey(d => new { d.ChiTietXuatKhoId, d.PhieuId })
                .HasConstraintName("fk_cthd_xuat_phieu");

            entity.HasOne(d => d.HoaDon).WithMany(p => p.ChiTietHoaDonV3s)
                .HasPrincipalKey(p => new { p.Id, p.PhieuId })
                .HasForeignKey(d => new { d.HoaDonId, d.PhieuId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cthd_hd_phieu");

            entity.HasOne(d => d.ChiTietDichVu).WithMany(p => p.ChiTietHoaDonV3s)
                .HasForeignKey(d => new { d.PhieuId, d.DichVuId })
                .HasConstraintName("fk_cthd_dv_phieu");
        });

        modelBuilder.Entity<ChiTietNhapKho>(entity =>
        {
            entity.HasKey(e => new { e.PhieuNhapId, e.LinhKienId }).HasName("pk_chi_tiet_nhap_kho");

            entity.ToTable("chi_tiet_nhap_kho");

            entity.HasIndex(e => e.LinhKienId, "ix_ctnk_linh_kien");

            entity.Property(e => e.PhieuNhapId).HasColumnName("phieu_nhap_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.DanhSachSerial).HasColumnName("danh_sach_serial");
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
            entity.HasKey(e => e.Id).HasName("PK__chi_tiet__3213E83F00641EA6");

            entity.ToTable("chi_tiet_xuat_kho");

            entity.HasIndex(e => new { e.KhoId, e.LinhKienId }, "ix_ctxk_kho_lk");

            entity.HasIndex(e => e.PhieuId, "ix_ctxk_phieu");

            entity.HasIndex(e => e.ThuKhoId, "ix_ctxk_thu_kho");

            entity.HasIndex(e => new { e.Id, e.PhieuId }, "uq_ctxk_id_phieu_v3").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BaoHanhDen).HasColumnName("bao_hanh_den");
            entity.Property(e => e.DonGiaXuat)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("don_gia_xuat");
            entity.Property(e => e.KhoId).HasColumnName("kho_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.LinhKienSerialId).HasColumnName("linh_kien_serial_id");
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
            entity.Property(e => e.YeuCauChiTietId).HasColumnName("yeu_cau_chi_tiet_id");

            entity.HasOne(d => d.LinhKienSerial).WithMany(p => p.ChiTietXuatKhos)
                .HasForeignKey(d => d.LinhKienSerialId)
                .HasConstraintName("fk_ctxk_serial");

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

            entity.HasOne(d => d.ChiTietYeuCauLinhKien).WithMany(p => p.ChiTietXuatKhos)
                .HasPrincipalKey(p => new { p.Id, p.PhieuId, p.LinhKienId })
                .HasForeignKey(d => new { d.YeuCauChiTietId, d.PhieuId, d.LinhKienId })
                .HasConstraintName("fk_ctxk_yeu_cau_v3");
        });

        modelBuilder.Entity<ChiTietYeuCauLinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_ct_yclk_v3");

            entity.ToTable("chi_tiet_yeu_cau_linh_kien");

            entity.HasIndex(e => new { e.Id, e.PhieuId, e.LinhKienId }, "uq_ct_yclk_id_phieu_lk").IsUnique();

            entity.HasIndex(e => new { e.YeuCauId, e.LinhKienId }, "uq_ct_yclk_lk").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(500)
                .HasColumnName("ghi_chu");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.SoLuongDuyet).HasColumnName("so_luong_duyet");
            entity.Property(e => e.SoLuongYeuCau).HasColumnName("so_luong_yeu_cau");
            entity.Property(e => e.YeuCauId).HasColumnName("yeu_cau_id");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.ChiTietYeuCauLinhKienV3s)
                .HasForeignKey(d => d.LinhKienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ct_yclk_lk");

            entity.HasOne(d => d.YeuCauLinhKien).WithMany(p => p.ChiTietYeuCauLinhKienV3s)
                .HasPrincipalKey(p => new { p.Id, p.PhieuId })
                .HasForeignKey(d => new { d.YeuCauId, d.PhieuId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ct_yclk_phieu");
        });

        modelBuilder.Entity<DichVu>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__dich_vu__3213E83F3A78B990");

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

        modelBuilder.Entity<GiaoDichThanhToan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_giao_dich_thanh_toan");

            entity.ToTable("giao_dich_thanh_toan", tb => tb.HasTrigger("trg_v3_giao_dich_khong_sua"));

            entity.HasIndex(e => new { e.PhieuId, e.ThoiGian }, "ix_gdtt_phieu");

            entity.HasIndex(e => e.HoaDonId, "uq_gdtt_hoa_don_cu")
                .IsUnique()
                .HasFilter("([nguon]='HOA_DON_CU')");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(500)
                .HasColumnName("ghi_chu");
            entity.Property(e => e.HoaDonId).HasColumnName("hoa_don_id");
            entity.Property(e => e.Loai)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("loai");
            entity.Property(e => e.MaThamChieu)
                .HasMaxLength(100)
                .HasColumnName("ma_tham_chieu");
            entity.Property(e => e.NguoiThucHienId).HasColumnName("nguoi_thuc_hien_id");
            entity.Property(e => e.Nguon)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("MOI")
                .HasColumnName("nguon");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.PhuongThuc)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("phuong_thuc");
            entity.Property(e => e.SoTien)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("so_tien");
            entity.Property(e => e.ThoiGian)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoi_gian");

            entity.HasOne(d => d.NguoiThucHien).WithMany(p => p.GiaoDichThanhToans)
                .HasForeignKey(d => d.NguoiThucHienId)
                .HasConstraintName("fk_gdtt_nguoi");

            entity.HasOne(d => d.Phieu).WithMany(p => p.GiaoDichThanhToans)
                .HasForeignKey(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_gdtt_phieu");

            entity.HasOne(d => d.HoaDon).WithMany(p => p.GiaoDichThanhToans)
                .HasPrincipalKey(p => new { p.Id, p.PhieuId })
                .HasForeignKey(d => new { d.HoaDonId, d.PhieuId })
                .HasConstraintName("fk_gdtt_hd_phieu");
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__hoa_don__3213E83FE261DB45");

            entity.ToTable("hoa_don");

            entity.HasIndex(e => e.LeTanId, "ix_hoa_don_le_tan");

            entity.HasIndex(e => new { e.TrangThai, e.NgayLap }, "ix_hoa_don_trang_thai").IsDescending(false, true);

            entity.HasIndex(e => new { e.Id, e.PhieuId }, "uq_hoa_don_id_phieu_v3").IsUnique();

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

            entity.HasOne(d => d.Phieu).WithOne(p => p.HoaDon)
                .HasForeignKey<HoaDon>(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_hoa_don_phieu");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__khach_ha__3213E83FFBE792F9");

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
            entity.HasKey(e => e.Id).HasName("PK__kho__3213E83F5C9F549B");

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
            entity.HasKey(e => e.NhanVienId).HasName("PK__ky_thuat__81F98323B9F3B5FB");

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
            entity.HasKey(e => e.NhanVienId).HasName("PK__le_tan__81F98323A110E25D");

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
            entity.HasKey(e => e.Id).HasName("PK__lich_su___3213E83F6DF81635");

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
            entity.HasKey(e => e.Id).HasName("PK__linh_kie__3213E83F6C7BECC0");

            entity.ToTable("linh_kien", tb => tb.HasTrigger("trg_v3_linh_kien_khoa_loai"));

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
            entity.Property(e => e.LoaiLinhKienId).HasColumnName("loai_linh_kien_id");
            entity.Property(e => e.MaLk)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('LK'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_lk");
            entity.Property(e => e.MaModel)
                .HasMaxLength(100)
                .HasColumnName("ma_model");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.TenLinhKien)
                .HasMaxLength(150)
                .HasColumnName("ten_linh_kien");
            entity.Property(e => e.TheoDoiSerial).HasColumnName("theo_doi_serial");
            entity.Property(e => e.ThoiHanBaoHanhThang).HasColumnName("thoi_han_bao_hanh_thang");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.LoaiLinhKien).WithMany(p => p.LinhKiens)
                .HasForeignKey(d => d.LoaiLinhKienId)
                .HasConstraintName("fk_linh_kien_loai_v3");
        });

        modelBuilder.Entity<LinhKienSerial>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__linh_kie__3213E83F67F6BFE5");

            entity.ToTable("linh_kien_serial");

            entity.HasIndex(e => e.KhoId, "ix_lks_kho_id");

            entity.HasIndex(e => e.LinhKienId, "ix_lks_linh_kien_id");

            entity.HasIndex(e => e.PhieuNhapId, "ix_lks_phieu_nhap_id");

            entity.HasIndex(e => e.TrangThai, "ix_lks_trang_thai");

            entity.HasIndex(e => e.SoSerial, "uq_linh_kien_serial_so_serial").IsUnique();

            entity.HasIndex(e => new { e.Id, e.LinhKienId }, "uq_lks_id_lk_v3").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChiTietXuatKhoId).HasColumnName("chi_tiet_xuat_kho_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("created_at");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(500)
                .HasColumnName("ghi_chu");
            entity.Property(e => e.KhoId).HasColumnName("kho_id");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.PhieuNhapId).HasColumnName("phieu_nhap_id");
            entity.Property(e => e.SoSerial)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("so_serial");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("TRONG_KHO")
                .HasColumnName("trang_thai");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getutcdate())")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ChiTietXuatKho).WithMany(p => p.LinhKienSerials)
                .HasForeignKey(d => d.ChiTietXuatKhoId)
                .HasConstraintName("fk_lks_chi_tiet_xuat_kho");

            entity.HasOne(d => d.Kho).WithMany(p => p.LinhKienSerials)
                .HasForeignKey(d => d.KhoId)
                .HasConstraintName("fk_lks_kho");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.LinhKienSerials)
                .HasForeignKey(d => d.LinhKienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lks_linh_kien");

            entity.HasOne(d => d.PhieuNhap).WithMany(p => p.LinhKienSerials)
                .HasForeignKey(d => d.PhieuNhapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lks_phieu_nhap");
        });

        modelBuilder.Entity<LoaiLinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_loai_linh_kien");

            entity.ToTable("loai_linh_kien");

            entity.HasIndex(e => e.Ma, "uq_loai_linh_kien_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.HoatDong)
                .HasDefaultValue(true)
                .HasColumnName("hoat_dong");
            entity.Property(e => e.Ma)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("ma");
            entity.Property(e => e.Ten)
                .HasMaxLength(100)
                .HasColumnName("ten");
        });

        modelBuilder.Entity<LuaChonThuocTinhLinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_lua_chon_tt_lk");

            entity.ToTable("lua_chon_thuoc_tinh_linh_kien");

            entity.HasIndex(e => new { e.ThuocTinhId, e.GiaTri }, "uq_lua_chon_tt_gia_tri").IsUnique();

            entity.HasIndex(e => new { e.Id, e.ThuocTinhId }, "uq_lua_chon_tt_id_tt").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GiaTri)
                .HasMaxLength(150)
                .HasColumnName("gia_tri");
            entity.Property(e => e.HoatDong)
                .HasDefaultValue(true)
                .HasColumnName("hoat_dong");
            entity.Property(e => e.ThuTu).HasColumnName("thu_tu");
            entity.Property(e => e.ThuocTinhId).HasColumnName("thuoc_tinh_id");

            entity.HasOne(d => d.ThuocTinh).WithMany(p => p.LuaChonThuocTinhLinhKiens)
                .HasForeignKey(d => d.ThuocTinhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_lua_chon_tt");
        });

        modelBuilder.Entity<NhaCungCap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__nha_cung__3213E83FEB052FF8");

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
            entity.HasKey(e => e.Id).HasName("PK__nhan_vie__3213E83F8DCE304A");

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

        modelBuilder.Entity<PhieuBaoHanh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_phieu_bao_hanh");

            entity.ToTable("phieu_bao_hanh");

            entity.HasIndex(e => new { e.PhieuGocId, e.NgayTiepNhan }, "ix_pbh_phieu_goc");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CamKetId).HasColumnName("cam_ket_id");
            entity.Property(e => e.KetLuan)
                .HasMaxLength(1000)
                .HasColumnName("ket_luan");
            entity.Property(e => e.KetQuaXuLy)
                .HasMaxLength(1000)
                .HasColumnName("ket_qua_xu_ly");
            entity.Property(e => e.LoiKhachBao)
                .HasMaxLength(1000)
                .HasColumnName("loi_khach_bao");
            entity.Property(e => e.NgayHoanThanh)
                .HasPrecision(3)
                .HasColumnName("ngay_hoan_thanh");
            entity.Property(e => e.NgayTiepNhan)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_tiep_nhan");
            entity.Property(e => e.NguoiTiepNhanId).HasColumnName("nguoi_tiep_nhan_id");
            entity.Property(e => e.PhieuGocId).HasColumnName("phieu_goc_id");
            entity.Property(e => e.PhieuSuaBaoHanhId).HasColumnName("phieu_sua_bao_hanh_id");
            entity.Property(e => e.ThietBiId).HasColumnName("thiet_bi_id");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("TIEP_NHAN")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.NguoiTiepNhan).WithMany(p => p.PhieuBaoHanhs)
                .HasForeignKey(d => d.NguoiTiepNhanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pbh_tiep_nhan");

            entity.HasOne(d => d.CamKetBaoHanh).WithMany(p => p.PhieuBaoHanhs)
                .HasPrincipalKey(p => new { p.Id, p.PhieuGocId })
                .HasForeignKey(d => new { d.CamKetId, d.PhieuGocId })
                .HasConstraintName("fk_pbh_cam_ket_phieu");

            entity.HasOne(d => d.PhieuSuaChua).WithMany(p => p.PhieuBaoHanhPhieuSuaChuas)
                .HasPrincipalKey(p => new { p.Id, p.ThietBiId })
                .HasForeignKey(d => new { d.PhieuGocId, d.ThietBiId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pbh_phieu_goc_thiet_bi");

            entity.HasOne(d => d.PhieuSuaChuaNavigation).WithMany(p => p.PhieuBaoHanhPhieuSuaChuaNavigations)
                .HasPrincipalKey(p => new { p.Id, p.ThietBiId })
                .HasForeignKey(d => new { d.PhieuSuaBaoHanhId, d.ThietBiId })
                .HasConstraintName("fk_pbh_phieu_sua_thiet_bi");
        });

        modelBuilder.Entity<PhieuBaoHanhNcc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_phieu_bao_hanh_ncc");

            entity.ToTable("phieu_bao_hanh_ncc");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.KetQua)
                .HasMaxLength(1000)
                .HasColumnName("ket_qua");
            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.LinhKienSerialId).HasColumnName("linh_kien_serial_id");
            entity.Property(e => e.NgayGui)
                .HasPrecision(3)
                .HasColumnName("ngay_gui");
            entity.Property(e => e.NgayNhanLai)
                .HasPrecision(3)
                .HasColumnName("ngay_nhan_lai");
            entity.Property(e => e.NhaCungCapId).HasColumnName("nha_cung_cap_id");
            entity.Property(e => e.PhieuBaoHanhId).HasColumnName("phieu_bao_hanh_id");
            entity.Property(e => e.SoLuong).HasColumnName("so_luong");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("CHO_GUI")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.PhieuBaoHanhNccs)
                .HasForeignKey(d => d.LinhKienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pbh_ncc_lk");

            entity.HasOne(d => d.NhaCungCap).WithMany(p => p.PhieuBaoHanhNccs)
                .HasForeignKey(d => d.NhaCungCapId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pbh_ncc_ncc");

            entity.HasOne(d => d.PhieuBaoHanh).WithMany(p => p.PhieuBaoHanhNccs)
                .HasForeignKey(d => d.PhieuBaoHanhId)
                .HasConstraintName("fk_pbh_ncc_pbh");

            entity.HasOne(d => d.LinhKienSerial).WithMany(p => p.PhieuBaoHanhNccs)
                .HasPrincipalKey(p => new { p.Id, p.LinhKienId })
                .HasForeignKey(d => new { d.LinhKienSerialId, d.LinhKienId })
                .HasConstraintName("fk_pbh_ncc_serial");
        });

        modelBuilder.Entity<PhieuNhap>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__phieu_nh__3213E83F829715A1");

            entity.ToTable("phieu_nhap", tb => tb.HasTrigger("trg_v3_phieu_nhap_serial_bat_buoc"));

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
            entity.HasKey(e => e.Id).HasName("PK__phieu_su__3213E83FD40B8B1E");

            entity.ToTable("phieu_sua_chua");

            entity.HasIndex(e => new { e.KyThuatVienId, e.TrangThai }, "ix_phieu_ktv");

            entity.HasIndex(e => e.LeTanId, "ix_phieu_le_tan");

            entity.HasIndex(e => e.ThietBiId, "ix_phieu_thiet_bi");

            entity.HasIndex(e => new { e.TrangThai, e.NgayNhan }, "ix_phieu_trang_thai").IsDescending(false, true);

            entity.HasIndex(e => new { e.Id, e.ThietBiId }, "uq_phieu_id_thiet_bi_v3").IsUnique();

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
            entity.Property(e => e.MatKhauMay)
                .HasMaxLength(100)
                .HasColumnName("mat_khau_may");
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
            entity.Property(e => e.PhuKienKemTheo)
                .HasMaxLength(500)
                .HasColumnName("phu_kien_kem_theo");
            entity.Property(e => e.ThietBiId).HasColumnName("thiet_bi_id");
            entity.Property(e => e.TinhTrangLoi).HasColumnName("tinh_trang_loi");
            entity.Property(e => e.TinhTrangNgoaiQuan)
                .HasMaxLength(500)
                .HasColumnName("tinh_trang_ngoai_quan");
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
            entity.HasKey(e => e.Id).HasName("PK__refresh___3213E83FFEE89266");

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
            entity.HasKey(e => e.Id).HasName("PK__tai_khoa__3213E83FE4FD318B");

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
            entity.HasKey(e => e.Id).HasName("PK__thiet_bi__3213E83F92213F43");

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
            entity.Property(e => e.HangSanXuat)
                .HasMaxLength(100)
                .HasColumnName("hang_san_xuat");
            entity.Property(e => e.KhachHangId).HasColumnName("khach_hang_id");
            entity.Property(e => e.LoaiMay)
                .HasMaxLength(50)
                .HasColumnName("loai_may");
            entity.Property(e => e.MaThietBi)
                .HasMaxLength(22)
                .IsUnicode(false)
                .HasComputedColumnSql("('TB'+case when [id]<(1000000) then right('000000'+CONVERT([varchar](20),[id]),(6)) else CONVERT([varchar](20),[id]) end)", true)
                .HasColumnName("ma_thiet_bi");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
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

        modelBuilder.Entity<ThongSoLinhKien>(entity =>
        {
            entity.HasKey(e => new { e.LinhKienId, e.ThuocTinhId }).HasName("pk_thong_so_linh_kien");

            entity.ToTable("thong_so_linh_kien", tb => tb.HasTrigger("trg_v3_thong_so_kieu"));

            entity.Property(e => e.LinhKienId).HasColumnName("linh_kien_id");
            entity.Property(e => e.ThuocTinhId).HasColumnName("thuoc_tinh_id");
            entity.Property(e => e.GiaTriDungSai).HasColumnName("gia_tri_dung_sai");
            entity.Property(e => e.GiaTriSo)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("gia_tri_so");
            entity.Property(e => e.GiaTriVanBan)
                .HasMaxLength(500)
                .HasColumnName("gia_tri_van_ban");
            entity.Property(e => e.LuaChonId).HasColumnName("lua_chon_id");

            entity.HasOne(d => d.LinhKien).WithMany(p => p.ThongSoLinhKiens)
                .HasForeignKey(d => d.LinhKienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ts_lk");

            entity.HasOne(d => d.ThuocTinh).WithMany(p => p.ThongSoLinhKiens)
                .HasForeignKey(d => d.ThuocTinhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ts_tt");

            entity.HasOne(d => d.LuaChonThuocTinhLinhKien).WithMany(p => p.ThongSoLinhKiens)
                .HasPrincipalKey(p => new { p.Id, p.ThuocTinhId })
                .HasForeignKey(d => new { d.LuaChonId, d.ThuocTinhId })
                .HasConstraintName("fk_ts_lua_chon");
        });

        modelBuilder.Entity<ThuKho>(entity =>
        {
            entity.HasKey(e => e.NhanVienId).HasName("PK__thu_kho__81F98323DCEEF958");

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

        modelBuilder.Entity<ThuocTinhLoaiLinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_thuoc_tinh_loai_lk");

            entity.ToTable("thuoc_tinh_loai_linh_kien", tb => tb.HasTrigger("trg_v3_thuoc_tinh_khoa_kieu"));

            entity.HasIndex(e => new { e.LoaiLinhKienId, e.Ma }, "uq_tt_loai_ma").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BatBuoc).HasColumnName("bat_buoc");
            entity.Property(e => e.DonVi)
                .HasMaxLength(30)
                .HasColumnName("don_vi");
            entity.Property(e => e.HoatDong)
                .HasDefaultValue(true)
                .HasColumnName("hoat_dong");
            entity.Property(e => e.KieuDuLieu)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("kieu_du_lieu");
            entity.Property(e => e.LoaiLinhKienId).HasColumnName("loai_linh_kien_id");
            entity.Property(e => e.Ma)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("ma");
            entity.Property(e => e.Ten)
                .HasMaxLength(100)
                .HasColumnName("ten");
            entity.Property(e => e.ThuTu).HasColumnName("thu_tu");

            entity.HasOne(d => d.LoaiLinhKien).WithMany(p => p.ThuocTinhLoaiLinhKiens)
                .HasForeignKey(d => d.LoaiLinhKienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tt_loai_lk");
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

        modelBuilder.Entity<TraLinhKienKho>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_tra_lk_v3");

            entity.ToTable("tra_linh_kien_kho", tb => tb.HasTrigger("trg_v3_tra_linh_kien_khong_sua"));

            entity.HasIndex(e => e.ChiTietXuatKhoId, "ix_tra_lk_ctxk");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChiTietXuatKhoId).HasColumnName("chi_tiet_xuat_kho_id");
            entity.Property(e => e.LyDo)
                .HasMaxLength(500)
                .HasColumnName("ly_do");
            entity.Property(e => e.NguoiThucHienId).HasColumnName("nguoi_thuc_hien_id");
            entity.Property(e => e.SoLuong).HasColumnName("so_luong");
            entity.Property(e => e.ThoiGian)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("thoi_gian");

            entity.HasOne(d => d.ChiTietXuatKho).WithMany(p => p.TraLinhKienKhoV3s)
                .HasForeignKey(d => d.ChiTietXuatKhoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tra_lk_ctxk");

            entity.HasOne(d => d.NguoiThucHien).WithMany(p => p.TraLinhKienKhoV3s)
                .HasForeignKey(d => d.NguoiThucHienId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_tra_lk_nguoi");
        });

        modelBuilder.Entity<V3SchemaMigration>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("pk_v3_schema_migration");

            entity.ToTable("v3_schema_migration");

            entity.Property(e => e.Version)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("version");
            entity.Property(e => e.AppliedAt)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("applied_at");
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
                .HasColumnType("decimal(38, 4)")
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

        modelBuilder.Entity<VV3BaoGium>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_v3_bao_gia");

            entity.Property(e => e.GiamGia)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("giam_gia");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.KenhPhanHoi)
                .HasMaxLength(100)
                .HasColumnName("kenh_phan_hoi");
            entity.Property(e => e.NgayGui)
                .HasPrecision(3)
                .HasColumnName("ngay_gui");
            entity.Property(e => e.NgayPhanHoi)
                .HasPrecision(3)
                .HasColumnName("ngay_phan_hoi");
            entity.Property(e => e.NgayTao)
                .HasPrecision(3)
                .HasColumnName("ngay_tao");
            entity.Property(e => e.NguoiGhiNhanId).HasColumnName("nguoi_ghi_nhan_id");
            entity.Property(e => e.PhienBan).HasColumnName("phien_ban");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.TongSauGiam)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("tong_sau_giam");
            entity.Property(e => e.TongTruocGiam)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("tong_truoc_giam");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("trang_thai");
        });

        modelBuilder.Entity<VV3CongNoPhieu>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_v3_cong_no_phieu");

            entity.Property(e => e.ConNo)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("con_no");
            entity.Property(e => e.DaHoan)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("da_hoan");
            entity.Property(e => e.DaThu)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("da_thu");
            entity.Property(e => e.DaThuRong)
                .HasColumnType("decimal(38, 2)")
                .HasColumnName("da_thu_rong");
            entity.Property(e => e.HoaDonId).HasColumnName("hoa_don_id");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.TongThanhToan)
                .HasColumnType("decimal(14, 2)")
                .HasColumnName("tong_thanh_toan");
        });

        modelBuilder.Entity<YeuCauLinhKien>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_yeu_cau_lk_v3");

            entity.ToTable("yeu_cau_linh_kien");

            entity.HasIndex(e => new { e.Id, e.PhieuId }, "uq_yclk_id_phieu").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.GhiChu)
                .HasMaxLength(500)
                .HasColumnName("ghi_chu");
            entity.Property(e => e.NgayDuyet)
                .HasPrecision(3)
                .HasColumnName("ngay_duyet");
            entity.Property(e => e.NgayYeuCau)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("ngay_yeu_cau");
            entity.Property(e => e.NguoiDuyetId).HasColumnName("nguoi_duyet_id");
            entity.Property(e => e.NguoiYeuCauId).HasColumnName("nguoi_yeu_cau_id");
            entity.Property(e => e.PhieuId).HasColumnName("phieu_id");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasDefaultValue("CHO_DUYET")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.NguoiDuyet).WithMany(p => p.YeuCauLinhKienV3NguoiDuyets)
                .HasForeignKey(d => d.NguoiDuyetId)
                .HasConstraintName("fk_yclk_duyet");

            entity.HasOne(d => d.NguoiYeuCau).WithMany(p => p.YeuCauLinhKienV3NguoiYeuCaus)
                .HasForeignKey(d => d.NguoiYeuCauId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_yclk_nguoi");

            entity.HasOne(d => d.Phieu).WithMany(p => p.YeuCauLinhKienV3s)
                .HasForeignKey(d => d.PhieuId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_yclk_phieu");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
