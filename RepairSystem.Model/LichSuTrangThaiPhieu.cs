using System;
using System.Collections.Generic;

namespace RepairSystem.Model;

public partial class LichSuTrangThaiPhieu
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public string? TrangThaiCu { get; set; }

    public string TrangThaiMoi { get; set; } = null!;

    public long? NguoiThucHienId { get; set; }

    public DateTime ThoiGian { get; set; }

    public string? GhiChu { get; set; }

    public virtual NhanVien? NguoiThucHien { get; set; }

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
