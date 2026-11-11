using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RepairSystem.Model;

public partial class AppDbContext
{
    public DbSet<MaLienKetKhachHang> MaLienKetKhachHangs => Set<MaLienKetKhachHang>();

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
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
        modelBuilder.Entity<MaLienKetKhachHang>(entity =>
        {
            entity.ToTable("ma_lien_ket_khach_hang");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.KhachHangId).HasColumnName("khach_hang_id");
            entity.Property(x => x.MaHash).HasColumnName("ma_hash").HasMaxLength(64).IsUnicode(false);
            entity.Property(x => x.HetHan).HasColumnName("het_han");
            entity.Property(x => x.DaDungLuc).HasColumnName("da_dung_luc");
            entity.Property(x => x.NguoiCapId).HasColumnName("nguoi_cap_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at")
                .HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
