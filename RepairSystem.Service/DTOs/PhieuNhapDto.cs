using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>Một dòng linh kiện trong yêu cầu tạo phiếu nhập.</summary>
public class DongNhapKhoRequestDto
{
    [Required]
    public long LinhKienId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng nhập phải lớn hơn 0.")]
    public int SoLuongNhap { get; set; }

    /// <summary>Đơn giá nhập (giá vốn). Tối đa 999.999.999.999,99 theo kiểu decimal(14,2) của DB.</summary>
    [Range(0, 999_999_999_999.99, ErrorMessage = "Đơn giá nhập phải lớn hơn hoặc bằng 0.")]
    public decimal DonGiaNhap { get; set; }
}

/// <summary>Yêu cầu tạo phiếu nhập kho.</summary>
public class TaoPhieuNhapDto
{
    [Required]
    public long NhaCungCapId { get; set; }

    [Required]
    public long KhoId { get; set; }

    /// <summary>
    /// Thủ kho chịu trách nhiệm phiếu. Thủ kho đăng nhập thì tự lấy chính họ.
    /// Admin tạo phiếu: nếu bỏ trống sẽ lấy thủ kho phụ trách kho đó.
    /// </summary>
    public long? ThuKhoId { get; set; }

    [MaxLength(50)]
    public string? SoChungTuNcc { get; set; }

    /// <summary>Ngày nhập hàng. Bỏ trống = thời điểm hiện tại.</summary>
    public DateTime? NgayNhap { get; set; }

    public string? GhiChu { get; set; }

    /// <summary>
    /// true (mặc định): lưu phiếu và nhập kho luôn (cộng tồn) trong một transaction.
    /// false: chỉ lưu nháp (trạng thái NHAP), chưa cộng tồn; sau đó gọi API xác nhận.
    /// </summary>
    public bool XacNhanNgay { get; set; } = true;

    [Required]
    [MinLength(1, ErrorMessage = "Phiếu nhập phải có ít nhất một dòng linh kiện.")]
    public List<DongNhapKhoRequestDto> ChiTiet { get; set; } = new();
}

/// <summary>Một dòng chi tiết của phiếu nhập (đọc).</summary>
public class DongNhapKhoDto
{
    public long LinhKienId { get; set; }
    public string MaLk { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public string DonViTinh { get; set; } = null!;
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }
}

/// <summary>Thông tin chung của phiếu nhập (dùng cho danh sách).</summary>
public class PhieuNhapDto
{
    public long Id { get; set; }
    public string MaPn { get; set; } = null!;
    public long NhaCungCapId { get; set; }
    public string TenNcc { get; set; } = null!;
    public long KhoId { get; set; }
    public string TenKho { get; set; } = null!;
    public long ThuKhoId { get; set; }
    public string? TenThuKho { get; set; }
    public string? SoChungTuNcc { get; set; }
    public DateTime NgayNhap { get; set; }

    /// <summary>NHAP (nháp) | DA_NHAP_KHO (đã cộng tồn) | HUY</summary>
    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }
    public decimal TongTienNhap { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Phiếu nhập kèm các dòng chi tiết.</summary>
public class PhieuNhapChiTietDto
{
    public PhieuNhapDto Phieu { get; set; } = null!;
    public List<DongNhapKhoDto> ChiTiet { get; set; } = new();
}
