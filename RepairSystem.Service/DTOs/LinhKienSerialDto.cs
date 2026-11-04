namespace RepairSystem.Service.DTOs;

/// <summary>Một serial linh kiện (dùng cho danh sách chọn khi xuất kho).</summary>
public class LinhKienSerialDto
{
    public long Id { get; set; }
    public long LinhKienId { get; set; }
    public string MaLk { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public string SoSerial { get; set; } = null!;

    /// <summary>TRONG_KHO | DA_XUAT | BAO_HANH | THANH_LY</summary>
    public string TrangThai { get; set; } = null!;

    public long? KhoId { get; set; }
    public string? TenKho { get; set; }

    public long PhieuNhapId { get; set; }
    public string MaPn { get; set; } = null!;
    public DateTime NgayNhap { get; set; }
    public string TenNcc { get; set; } = null!;

    public string? GhiChu { get; set; }
}

/// <summary>Vòng đời đầy đủ của một serial (chỉ dành cho nhân viên).</summary>
public class SerialVongDoiDto : LinhKienSerialDto
{
    public long? PhieuSuaChuaId { get; set; }
    public string? MaPhieuSuaChua { get; set; }
    public DateTime? NgayXuat { get; set; }
    public DateOnly? BaoHanhDen { get; set; }
    public string? TenThietBi { get; set; }
    public string? TenKhachHang { get; set; }
}

/// <summary>
/// Kết quả tra cứu bảo hành công khai. Cố ý KHÔNG chứa tên/số điện thoại khách hàng
/// vì endpoint này không yêu cầu đăng nhập.
/// </summary>
public class BaoHanhSerialDto
{
    public string SoSerial { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public string? HangSanXuat { get; set; }
    public string TrangThai { get; set; } = null!;

    public DateTime NgayNhapKho { get; set; }
    public string TenNcc { get; set; } = null!;

    public DateTime? NgayXuat { get; set; }
    public string? MaPhieuSuaChua { get; set; }
    public string? TenThietBi { get; set; }

    public DateOnly? BaoHanhDen { get; set; }
    public DateOnly? BaoHanhTu { get; set; }

    /// <summary>true nếu đã bán ra và còn trong thời hạn bảo hành.</summary>
    public bool ConBaoHanh { get; set; }

    public string ThongBao { get; set; } = null!;
}
