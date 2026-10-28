using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class HoaDon
{
    public long Id { get; set; }

    public string MaHd { get; set; } = null!;

    public long PhieuId { get; set; }

    public long LeTanId { get; set; }

    public DateTime NgayLap { get; set; }

    public decimal TienDichVu { get; set; }

    public decimal TienLinhKien { get; set; }

    public decimal GiamGia { get; set; }

    public decimal TongThanhToan { get; set; }

    public string? PhuongThucTt { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime? NgayThanhToan { get; set; }

    public string? GhiChu { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual LeTan LeTan { get; set; } = null!;

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
