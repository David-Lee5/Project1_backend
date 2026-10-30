using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class PhieuSuaChuaDto
{
    public long Id { get; set; }
    public string MaPhieu { get; set; } = null!;
    public long ThietBiId { get; set; }
    public string TenThietBi { get; set; } = null!;
    public long KhachHangId { get; set; }
    public string HoTenKhachHang { get; set; } = null!;
    public long LeTanId { get; set; }
    public long? KyThuatVienId { get; set; }
    public string? TenKyThuatVien { get; set; }
    public DateTime NgayNhan { get; set; }
    public DateTime? NgayHenTra { get; set; }
    public DateTime? NgayHoanThanh { get; set; }
    public DateTime? NgayTra { get; set; }
    public string TinhTrangLoi { get; set; } = null!;
    public string? KetQuaSuaChua { get; set; }
    public string TrangThai { get; set; } = null!;
    public string? LyDoHuy { get; set; }
    public string? GhiChu { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RepairPageDto
{
    public IReadOnlyList<PhieuSuaChuaDto> Items { get; set; } = Array.Empty<PhieuSuaChuaDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class RepairDetailDto
{
    public PhieuSuaChuaDto Phieu { get; set; } = null!;
    public IReadOnlyList<RepairServiceLineDto> DichVu { get; set; } = Array.Empty<RepairServiceLineDto>();
    public IReadOnlyList<RepairPartLineDto> LinhKien { get; set; } = Array.Empty<RepairPartLineDto>();
    public IReadOnlyList<RepairHistoryDto> LichSu { get; set; } = Array.Empty<RepairHistoryDto>();
    public IReadOnlyList<RepairPhotoDto> HinhAnh { get; set; } = Array.Empty<RepairPhotoDto>();
    public decimal TienDichVu { get; set; }
    public decimal TienLinhKien { get; set; }
    public decimal TongChiPhi => TienDichVu + TienLinhKien;
}

public class RepairServiceLineDto
{
    public long DichVuId { get; set; }
    public string TenDichVu { get; set; } = null!;
    public int SoLuong { get; set; }
    public decimal DonGiaThucTe { get; set; }
    public decimal ThanhTien { get; set; }
}

public class RepairPartLineDto
{
    public long Id { get; set; }
    public long KhoId { get; set; }
    public long LinhKienId { get; set; }
    public string TenLinhKien { get; set; } = null!;
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien { get; set; }
    public DateOnly? BaoHanhDen { get; set; }
}

public class RepairHistoryDto
{
    public long Id { get; set; }
    public string? TrangThaiCu { get; set; }
    public string TrangThaiMoi { get; set; } = null!;
    public long? NguoiThucHienId { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? GhiChu { get; set; }
}

public class RepairPhotoDto
{
    public long Id { get; set; }
    public string Loai { get; set; } = null!;
    public string DuongDan { get; set; } = null!;
    public string? MoTa { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateRepairDto
{
    [Range(1, long.MaxValue)] public long ThietBiId { get; set; }
    public DateTime? NgayHenTra { get; set; }
    [Required, MaxLength(4000)] public string TinhTrangLoi { get; set; } = null!;
    [MaxLength(4000)] public string? GhiChu { get; set; }
}

public class UpdateRepairDto
{
    public DateTime? NgayHenTra { get; set; }
    [Required, MaxLength(4000)] public string TinhTrangLoi { get; set; } = null!;
    [MaxLength(4000)] public string? GhiChu { get; set; }
}

public class AssignRepairDto
{
    [Range(1, long.MaxValue)] public long KyThuatVienId { get; set; }
    [MaxLength(1000)] public string? GhiChu { get; set; }
}

public class ChangeRepairStatusDto
{
    [Required, MaxLength(15)] public string TrangThai { get; set; } = null!;
    [MaxLength(4000)] public string? KetQuaSuaChua { get; set; }
    [MaxLength(4000)] public string? LyDoHuy { get; set; }
    [MaxLength(1000)] public string? GhiChu { get; set; }
}

public class UpsertRepairServiceDto
{
    [Range(1, 1000000)] public int SoLuong { get; set; } = 1;
    // Only administrators may override the catalog price. Existing line prices remain snapshots.
    [Range(typeof(decimal), "0", "999999999999.99", ParseLimitsInInvariantCulture = true)] public decimal? DonGiaThucTe { get; set; }
}

public class AddRepairPhotoDto
{
    [Required, MaxLength(15)] public string Loai { get; set; } = "KHAC";
    [Required, MaxLength(500)] public string DuongDan { get; set; } = null!;
    [MaxLength(255)] public string? MoTa { get; set; }
}
