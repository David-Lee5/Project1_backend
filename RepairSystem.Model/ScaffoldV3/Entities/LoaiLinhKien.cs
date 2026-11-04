using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class LoaiLinhKien
{
    public long Id { get; set; }

    public string Ma { get; set; } = null!;

    public string Ten { get; set; } = null!;

    public bool HoatDong { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<LinhKien> LinhKiens { get; set; } = new List<LinhKien>();

    public virtual ICollection<ThuocTinhLoaiLinhKien> ThuocTinhLoaiLinhKiens { get; set; } = new List<ThuocTinhLoaiLinhKien>();
}
