using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class LuaChonThuocTinhLinhKien
{
    public long Id { get; set; }

    public long ThuocTinhId { get; set; }

    public string GiaTri { get; set; } = null!;

    public int ThuTu { get; set; }

    public bool HoatDong { get; set; }

    public virtual ICollection<ThongSoLinhKien> ThongSoLinhKiens { get; set; } = new List<ThongSoLinhKien>();

    public virtual ThuocTinhLoaiLinhKien ThuocTinh { get; set; } = null!;
}
