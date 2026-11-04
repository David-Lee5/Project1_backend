using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class VV3BaoGium
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public int PhienBan { get; set; }

    public string TrangThai { get; set; } = null!;

    public decimal GiamGia { get; set; }

    public decimal? TongTruocGiam { get; set; }

    public decimal? TongSauGiam { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayGui { get; set; }

    public DateTime? NgayPhanHoi { get; set; }

    public string? KenhPhanHoi { get; set; }

    public long? NguoiGhiNhanId { get; set; }
}
