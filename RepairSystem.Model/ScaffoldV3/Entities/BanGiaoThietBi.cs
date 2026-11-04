using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class BanGiaoThietBi
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public long NguoiGiaoId { get; set; }

    public string TenNguoiNhan { get; set; } = null!;

    public DateTime ThoiGian { get; set; }

    public string? TinhTrangMay { get; set; }

    public string? PhuKienDaTra { get; set; }

    public string? XacNhan { get; set; }

    public decimal ConNoLucGiao { get; set; }

    public long? NguoiDuyetConNoId { get; set; }

    public string? LyDoConNo { get; set; }

    public virtual NhanVien? NguoiDuyetConNo { get; set; }

    public virtual NhanVien NguoiGiao { get; set; } = null!;

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
