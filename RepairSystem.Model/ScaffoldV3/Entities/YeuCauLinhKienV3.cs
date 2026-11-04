using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class YeuCauLinhKienV3
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public long NguoiYeuCauId { get; set; }

    public DateTime NgayYeuCau { get; set; }

    public string TrangThai { get; set; } = null!;

    public long? NguoiDuyetId { get; set; }

    public DateTime? NgayDuyet { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietYeuCauLinhKienV3> ChiTietYeuCauLinhKienV3s { get; set; } = new List<ChiTietYeuCauLinhKienV3>();

    public virtual NhanVien? NguoiDuyet { get; set; }

    public virtual NhanVien NguoiYeuCau { get; set; } = null!;

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
