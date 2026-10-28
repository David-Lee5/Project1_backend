using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class LeTan
{
    public long NhanVienId { get; set; }

    public string ChucVu { get; set; } = null!;

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual NhanVien NhanVien { get; set; } = null!;

    public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
}
