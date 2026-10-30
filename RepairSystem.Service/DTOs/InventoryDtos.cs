using System.ComponentModel.DataAnnotations;

namespace RepairSystem.Service.DTOs;

public class WarehouseDto
{
    public long Id { get; set; }
    public string MaKho { get; set; } = null!;
    public string TenKho { get; set; } = null!;
    public string? DiaDiem { get; set; }
    public long? ThuKhoId { get; set; }
}

public class UpsertWarehouseDto
{
    [Required, MaxLength(100)] public string TenKho { get; set; } = null!;
    [MaxLength(255)] public string? DiaDiem { get; set; }
    [Range(1, long.MaxValue)] public long? ThuKhoId { get; set; }
}

public class StockDto
{
    public long KhoId { get; set; }
    public string TenKho { get; set; } = null!;
    public long LinhKienId { get; set; }
    public string MaLk { get; set; } = null!;
    public string TenLinhKien { get; set; } = null!;
    public int SoLuongTon { get; set; }
    public int MucTonToiThieu { get; set; }
    public DateTime NgayCapNhat { get; set; }
}

public class UpdateStockThresholdDto
{
    [Range(0, int.MaxValue)] public int MucTonToiThieu { get; set; }
}

public class StockMovementDto
{
    public long Id { get; set; }
    public long KhoId { get; set; }
    public long LinhKienId { get; set; }
    public string Loai { get; set; } = null!;
    public int SoLuongThayDoi { get; set; }
    public int SoLuongSau { get; set; }
    public long? PhieuNhapId { get; set; }
    public long? ChiTietXuatKhoId { get; set; }
    public long NguoiThucHienId { get; set; }
    public DateTime ThoiGian { get; set; }
    public string? LyDo { get; set; }
}

public class ReceiptLineRequestDto
{
    [Range(1, long.MaxValue)] public long LinhKienId { get; set; }
    [Range(1, int.MaxValue)] public int SoLuongNhap { get; set; }
    [Range(typeof(decimal), "0", "999999999999.99")] public decimal DonGiaNhap { get; set; }
}

public class UpsertReceiptDto
{
    [Range(1, long.MaxValue)] public long NhaCungCapId { get; set; }
    [Range(1, long.MaxValue)] public long KhoId { get; set; }
    [MaxLength(50)] public string? SoChungTuNcc { get; set; }
    [MaxLength(4000)] public string? GhiChu { get; set; }
    [Required, MinLength(1), MaxLength(200)] public List<ReceiptLineRequestDto> ChiTiet { get; set; } = new();
}

public class ReceiptLineDto
{
    public long LinhKienId { get; set; }
    public string TenLinhKien { get; set; } = null!;
    public int SoLuongNhap { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }
}

public class ReceiptDto
{
    public long Id { get; set; }
    public string MaPn { get; set; } = null!;
    public long NhaCungCapId { get; set; }
    public long KhoId { get; set; }
    public long ThuKhoId { get; set; }
    public string? SoChungTuNcc { get; set; }
    public DateTime NgayNhap { get; set; }
    public string TrangThai { get; set; } = null!;
    public string? GhiChu { get; set; }
    public decimal TongTien { get; set; }
    public List<ReceiptLineDto> ChiTiet { get; set; } = new();
}

public class DispatchRequestDto
{
    [Range(1, long.MaxValue)] public long PhieuId { get; set; }
    [Range(1, long.MaxValue)] public long KhoId { get; set; }
    [Range(1, long.MaxValue)] public long LinhKienId { get; set; }
    [Range(1, int.MaxValue)] public int SoLuong { get; set; }
}

public class DispatchDto
{
    public long Id { get; set; }
    public long PhieuId { get; set; }
    public long KhoId { get; set; }
    public long LinhKienId { get; set; }
    public long ThuKhoId { get; set; }
    public int SoLuongXuat { get; set; }
    public decimal DonGiaXuat { get; set; }
    public decimal ThanhTien { get; set; }
    public DateTime NgayXuat { get; set; }
    public DateOnly? BaoHanhDen { get; set; }
}
