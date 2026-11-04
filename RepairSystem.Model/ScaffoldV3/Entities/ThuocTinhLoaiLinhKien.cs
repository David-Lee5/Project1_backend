using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class ThuocTinhLoaiLinhKien
{
    public long Id { get; set; }

    public long LoaiLinhKienId { get; set; }

    public string Ma { get; set; } = null!;

    public string Ten { get; set; } = null!;

    public string KieuDuLieu { get; set; } = null!;

    public string? DonVi { get; set; }

    public bool BatBuoc { get; set; }

    public int ThuTu { get; set; }

    public bool HoatDong { get; set; }

    public virtual LoaiLinhKien LoaiLinhKien { get; set; } = null!;

    public virtual ICollection<LuaChonThuocTinhLinhKien> LuaChonThuocTinhLinhKiens { get; set; } = new List<LuaChonThuocTinhLinhKien>();

    public virtual ICollection<ThongSoLinhKien> ThongSoLinhKiens { get; set; } = new List<ThongSoLinhKien>();
}
