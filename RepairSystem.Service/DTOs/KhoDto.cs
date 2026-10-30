using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Thông tin một kho.</summary>
public class KhoDto
{
    public long Id { get; set; }
    public string MaKho { get; set; } = null!;
    public string TenKho { get; set; } = null!;
    public string? DiaDiem { get; set; }
    public long? ThuKhoId { get; set; }
}

/// <summary>Một dòng tồn kho: một linh kiện trong một kho.</summary>
public class TonKhoDto
{
    public long KhoId { get; set; }
    public string TenKho { get; set; } = null!;
    public long LinhKienId { get; set; }
    public string MaLk { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public string DonViTinh { get; set; } = null!;
    public int SoLuongTon { get; set; }
    public int MucTonToiThieu { get; set; }
    public DateTime NgayCapNhat { get; set; }

    /// <summary>true khi tồn chạm hoặc dưới mức tối thiểu.</summary>
    public bool SapHet => SoLuongTon <= MucTonToiThieu;
}

/// <summary>Cảnh báo tồn kho: linh kiện có tồn &lt;= mức tối thiểu.</summary>
public class CanhBaoTonKhoDto
{
    public long KhoId { get; set; }
    public string TenKho { get; set; } = null!;
    public long LinhKienId { get; set; }
    public string MaLk { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public string DonViTinh { get; set; } = null!;
    public int SoLuongTon { get; set; }
    public int MucTonToiThieu { get; set; }

    /// <summary>HET_HANG (tồn = 0, hiển thị màu đỏ) hoặc SAP_HET (còn hàng nhưng chạm ngưỡng, màu vàng).</summary>
    public string MucDo => SoLuongTon == 0 ? "HET_HANG" : "SAP_HET";

    /// <summary>Số lượng tối thiểu cần nhập thêm để quay lại mức tối thiểu.</summary>
    public int CanNhapThem => Math.Max(MucTonToiThieu - SoLuongTon, 0);
}

/// <summary>Đặt mức tồn tối thiểu (ngưỡng cảnh báo) cho một linh kiện trong một kho.</summary>
public class CapNhatMucTonToiThieuDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn tối thiểu phải lớn hơn hoặc bằng 0.")]
    public int MucTonToiThieu { get; set; }
}
