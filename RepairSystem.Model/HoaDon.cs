using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class HoaDon
{
    public string MaHd { get; set; } = null!;

    public DateTime? NgayLap { get; set; }

    public decimal TongThanhToan { get; set; }

    public string? PhuongThucTt { get; set; }

    public string? TrangThai { get; set; }

    public string MaPhieu { get; set; } = null!;

    public virtual PhieuSuaChua MaPhieuNavigation { get; set; } = null!;
}
