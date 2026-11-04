using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class PhieuBaoHanhNcc
{
    public long Id { get; set; }

    public long? PhieuBaoHanhId { get; set; }

    public long NhaCungCapId { get; set; }

    public long LinhKienId { get; set; }

    public long? LinhKienSerialId { get; set; }

    public int SoLuong { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime? NgayGui { get; set; }

    public DateTime? NgayNhanLai { get; set; }

    public string? KetQua { get; set; }

    public virtual LinhKien LinhKien { get; set; } = null!;

    public virtual LinhKienSerial? LinhKienSerial { get; set; }

    public virtual NhaCungCap NhaCungCap { get; set; } = null!;

    public virtual PhieuBaoHanh? PhieuBaoHanh { get; set; }
}
