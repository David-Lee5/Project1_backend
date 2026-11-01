using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

/// <summary>
/// Yêu cầu tạo phiếu sửa chữa (tiếp nhận máy). Gồm cả tình trạng đầu vào để minh bạch khi nhận máy.
/// </summary>
public class TaoPhieuSuaChuaDto
{
    [Required]
    public long ThietBiId { get; set; }

    /// <summary>Lễ tân đăng nhập thì tự lấy chính họ. Admin tạo phiếu thì phải chỉ định.</summary>
    public long? LeTanId { get; set; }

    /// <summary>Kỹ thuật viên phụ trách. Bỏ trống = chưa phân công (phiếu ở trạng thái TIEP_NHAN).</summary>
    public long? KyThuatVienId { get; set; }

    [Required(ErrorMessage = "Vui lòng mô tả tình trạng lỗi.")]
    public string TinhTrangLoi { get; set; } = null!;

    public DateTime? NgayHenTra { get; set; }

    /// <summary>Tình trạng ngoại quan lúc nhận, ví dụ "Xước nhẹ mặt đáy, móp góc trái".</summary>
    [MaxLength(500)]
    public string? TinhTrangNgoaiQuan { get; set; }

    /// <summary>Phụ kiện kèm theo, ví dụ "Củ sạc, túi chống sốc".</summary>
    [MaxLength(500)]
    public string? PhuKienKemTheo { get; set; }

    /// <summary>Mật khẩu/mã mở máy để kỹ thuật viên kiểm tra sau sửa. Nên chỉ hiển thị cho KTV phụ trách.</summary>
    [MaxLength(100)]
    public string? MatKhauMay { get; set; }

    public string? GhiChu { get; set; }
}
