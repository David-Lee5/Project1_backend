using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class CamKetBaoHanh
{
    public long Id { get; set; }

    public long PhieuGocId { get; set; }

    public string Loai { get; set; } = null!;

    public long? ChiTietXuatKhoId { get; set; }

    public long? DichVuId { get; set; }

    public DateOnly NgayBatDau { get; set; }

    public DateOnly NgayKetThuc { get; set; }

    public string? DieuKien { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ChiTietDichVu? ChiTietDichVu { get; set; }

    public virtual ChiTietXuatKho? ChiTietXuatKho { get; set; }

    public virtual ICollection<PhieuBaoHanh> PhieuBaoHanhs { get; set; } = new List<PhieuBaoHanh>();

    public virtual PhieuSuaChua PhieuGoc { get; set; } = null!;
}
