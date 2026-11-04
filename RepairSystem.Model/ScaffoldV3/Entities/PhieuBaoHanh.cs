using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class PhieuBaoHanh
{
    public long Id { get; set; }

    public long PhieuGocId { get; set; }

    public long ThietBiId { get; set; }

    public long? CamKetId { get; set; }

    public long? PhieuSuaBaoHanhId { get; set; }

    public long NguoiTiepNhanId { get; set; }

    public DateTime NgayTiepNhan { get; set; }

    public string LoiKhachBao { get; set; } = null!;

    public string TrangThai { get; set; } = null!;

    public string? KetLuan { get; set; }

    public string? KetQuaXuLy { get; set; }

    public DateTime? NgayHoanThanh { get; set; }

    public virtual CamKetBaoHanh? CamKetBaoHanh { get; set; }

    public virtual NhanVien NguoiTiepNhan { get; set; } = null!;

    public virtual ICollection<PhieuBaoHanhNcc> PhieuBaoHanhNccs { get; set; } = new List<PhieuBaoHanhNcc>();

    public virtual PhieuSuaChua PhieuSuaChua { get; set; } = null!;

    public virtual PhieuSuaChua? PhieuSuaChuaNavigation { get; set; }
}
