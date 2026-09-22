using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class PhieuSuaChua
{
    public string MaPhieu { get; set; } = null!;

    public DateTime? NgayNhan { get; set; }

    public DateTime? NgayHenTra { get; set; }

    public string TinhTrangLoi { get; set; } = null!;

    public string? TrangThai { get; set; }

    public string MaThietBi { get; set; } = null!;

    public string? MaKtv { get; set; }

    public virtual ICollection<CtDichVu> CtDichVus { get; set; } = new List<CtDichVu>();

    public virtual ICollection<CtXuatKho> CtXuatKhos { get; set; } = new List<CtXuatKho>();

    public virtual HoaDon? HoaDon { get; set; }

    public virtual KyThuatVien? MaKtvNavigation { get; set; }

    public virtual ThietBi MaThietBiNavigation { get; set; } = null!;
}
