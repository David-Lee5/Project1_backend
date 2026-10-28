using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Model;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

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
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<Permission> Permissions { get; set; }
    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }
    public virtual DbSet<ThietBi> ThietBis { get; set; }
    public virtual DbSet<ThuKho> ThuKhos { get; set; }
    public virtual DbSet<TonKho> TonKhos { get; set; }
    public virtual DbSet<VLinhKienHienThi> VLinhKienHienThis { get; set; }
    public virtual DbSet<VPhieuChiPhi> VPhieuChiPhis { get; set; }
    public virtual DbSet<VPhieuNhapTong> VPhieuNhapTongs { get; set; }
    public virtual DbSet<VTonKhoSapHet> VTonKhoSapHets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=DESKTOP-04P8719\\SQLEXPRESS;Database=QLSCTBDT;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // EF will use conventions and navigation properties to create relationships
        base.OnModelCreating(modelBuilder);
    }
}
