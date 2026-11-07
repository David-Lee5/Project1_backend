using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class AppDbContextV3
{
    public DbSet<LichHenV3> LichHenV3s => Set<LichHenV3>();
    public DbSet<XuLyBaoHanhNccV3> XuLyBaoHanhNccV3s => Set<XuLyBaoHanhNccV3>();

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
