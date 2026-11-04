using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class BaoGium
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public int PhienBan { get; set; }

    public string TrangThai { get; set; } = null!;

    public decimal GiamGia { get; set; }

    public long NguoiTaoId { get; set; }

    public DateTime NgayTao { get; set; }

    public DateTime? NgayGui { get; set; }

    public DateTime? NgayPhanHoi { get; set; }

    public string? KenhPhanHoi { get; set; }

    public long? NguoiGhiNhanId { get; set; }

    public string? NoiDungPhanHoi { get; set; }

    public virtual ICollection<ChiTietBaoGium> ChiTietBaoGia { get; set; } = new List<ChiTietBaoGium>();

    public virtual NhanVien? NguoiGhiNhan { get; set; }

    public virtual NhanVien NguoiTao { get; set; } = null!;

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
