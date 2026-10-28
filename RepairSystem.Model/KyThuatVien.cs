using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class KyThuatVien
{
    public long NhanVienId { get; set; }

    public string ChucVu { get; set; } = null!;

    public string? ChuyenMon { get; set; }

    public virtual NhanVien NhanVien { get; set; } = null!;

    public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
}
