using System;
using System.Collections.Generic;

namespace RepairSystem.Model.ScaffoldV3.Entities;

public partial class YeuCauLinhKien
{
    public long Id { get; set; }

    public long PhieuId { get; set; }

    public long NguoiYeuCauId { get; set; }

    public DateTime NgayYeuCau { get; set; }

    public string TrangThai { get; set; } = null!;

    public long? NguoiDuyetId { get; set; }

    public DateTime? NgayDuyet { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<ChiTietYeuCauLinhKien> ChiTietYeuCauLinhKiens { get; set; } = new List<ChiTietYeuCauLinhKien>();

    public virtual NhanVien? NguoiDuyet { get; set; }

    public virtual NhanVien NguoiYeuCau { get; set; } = null!;

    public virtual PhieuSuaChua Phieu { get; set; } = null!;
}
