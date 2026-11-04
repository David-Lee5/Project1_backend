using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class GiaoDichThanhToan
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public long? HoaDonId { get; set; }

    public string Loai { get; set; } = null!;

    public decimal SoTien { get; set; }

    public string PhuongThuc { get; set; } = null!;

    public DateTime ThoiGian { get; set; }

    public long? NguoiThucHienId { get; set; }

    public string Nguon { get; set; } = null!;

    public string? MaThamChieu { get; set; }

    public string? GhiChu { get; set; }

    public virtual HoaDon? HoaDon { get; set; }

    public virtual NhanVien? NguoiThucHien { get; set; }

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
