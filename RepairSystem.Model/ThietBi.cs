using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class ThietBi
{
    public string MaThietBi { get; set; } = null!;

    public string TenThietBi { get; set; } = null!;

    public string? LoaiMay { get; set; }

    public string? SoSerial { get; set; }

    public string MaKh { get; set; } = null!;

    public virtual KhachHang MaKhNavigation { get; set; } = null!;

    public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
}
