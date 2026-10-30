using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class BillingInvoiceDto
{
    public long Id { get; set; }
    public string MaHd { get; set; } = null!;
    public long PhieuId { get; set; }
    public string MaPhieu { get; set; } = null!;
    public long LeTanId { get; set; }
    public DateTime NgayLap { get; set; }
    public decimal TienDichVu { get; set; }
    public decimal TienLinhKien { get; set; }
    public decimal GiamGia { get; set; }
    public decimal TongThanhToan { get; set; }
    public string TrangThai { get; set; } = null!;
    public string? PhuongThucTt { get; set; }
    public DateTime? NgayThanhToan { get; set; }
    public string? GhiChu { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateInvoiceDto
{
    [Range(1, long.MaxValue)]
    public long PhieuId { get; set; }

    [Range(typeof(decimal), "0", "999999999999.99")]
    public decimal GiamGia { get; set; }
}

public class PayInvoiceDto
{
    [Required]
    [RegularExpression("^(TIEN_MAT|CHUYEN_KHOAN|THE|VI_DIEN_TU)$", ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
    public string PhuongThucTt { get; set; } = null!;
}

public class CancelInvoiceDto
{
    [Required]
    [StringLength(2000, MinimumLength = 3)]
    public string LyDo { get; set; } = null!;
}
