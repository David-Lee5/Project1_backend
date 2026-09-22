using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class KyThuatVien
{
    public string MaKtv { get; set; } = null!;

    public int? TaiKhoanId { get; set; }

    public string HoTen { get; set; } = null!;

    public string? ChuyenMon { get; set; }

    public string SoDienThoai { get; set; } = null!;

    public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();

    public virtual TaiKhoan? TaiKhoan { get; set; }
}
