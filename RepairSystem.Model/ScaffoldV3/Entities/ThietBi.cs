using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class ThietBi
{
    public long Id { get; set; }

    public string MaThietBi { get; set; } = null!;

    public long KhachHangId { get; set; }

    public string TenThietBi { get; set; } = null!;

    public string? LoaiMay { get; set; }

    public string? SoSerial { get; set; }

    public DateOnly? NgayMua { get; set; }

    public string? GhiChu { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? HangSanXuat { get; set; }

    public string? Model { get; set; }

    public virtual KhachHang KhachHang { get; set; } = null!;

    public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
}
