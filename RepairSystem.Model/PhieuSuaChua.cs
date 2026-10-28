using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class PhieuSuaChua
{
    public long Id { get; set; }

    public string MaPhieu { get; set; } = null!;

    public long ThietBiId { get; set; }

    public long LeTanId { get; set; }

    public long? KyThuatVienId { get; set; }

    public DateTime NgayNhan { get; set; }

    public DateTime? NgayHenTra { get; set; }

    public DateTime? NgayHoanThanh { get; set; }

    public DateTime? NgayTra { get; set; }

    public string TinhTrangLoi { get; set; } = null!;

    public string? KetQuaSuaChua { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? LyDoHuy { get; set; }

    public string? GhiChu { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AnhPhieuSuaChua> AnhPhieuSuaChuas { get; set; } = new List<AnhPhieuSuaChua>();

    public virtual ICollection<ChiTietDichVu> ChiTietDichVus { get; set; } = new List<ChiTietDichVu>();

    public virtual ICollection<ChiTietXuatKho> ChiTietXuatKhos { get; set; } = new List<ChiTietXuatKho>();

    public virtual HoaDon? HoaDon { get; set; }

    public virtual KyThuatVien? KyThuatVien { get; set; }

    public virtual LeTan LeTan { get; set; } = null!;

    public virtual ICollection<LichSuTrangThaiPhieu> LichSuTrangThaiPhieus { get; set; } = new List<LichSuTrangThaiPhieu>();

    public virtual ThietBi ThietBi { get; set; } = null!;
}
