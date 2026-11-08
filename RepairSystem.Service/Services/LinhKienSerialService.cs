using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class LinhKienSerialService : ILinhKienSerialService
{
    // Giá trị cột linh_kien_serial.trang_thai (có CHECK trong DB)
    private static readonly HashSet<string> TrangThaiHopLe =
        new() { "TRONG_KHO", "DA_XUAT", "BAO_HANH", "THANH_LY", "THIEU_KIEM_KE", "CACH_LY" };

    private readonly AppDbContext _context;
    private readonly AppDbContextV3 _v3Context;
    private readonly bool _v3Enabled;

    public LinhKienSerialService(AppDbContext context, AppDbContextV3 v3Context,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _context = context;
        _v3Context = v3Context;
        _v3Enabled = string.Equals(configuration["V3:Enabled"], "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IEnumerable<LinhKienSerialDto>> GetDanhSachAsync(long? linhKienId, string? trangThai, long? khoId)
    {
        var query = _context.LinhKienSerials.AsNoTracking().AsQueryable();

        if (linhKienId.HasValue)
            query = query.Where(s => s.LinhKienId == linhKienId.Value);

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            var tt = trangThai.Trim().ToUpperInvariant();
            if (!TrangThaiHopLe.Contains(tt))
                throw new ArgumentException(
                    $"Trạng thái '{trangThai}' không hợp lệ. Giá trị cho phép: {string.Join(", ", TrangThaiHopLe)}.");
            query = query.Where(s => s.TrangThai == tt);
        }

        if (khoId.HasValue)
            query = query.Where(s => s.KhoId == khoId.Value);

        return await query
            .OrderBy(s => s.PhieuNhap.NgayNhap)   // nhập trước xuất trước
            .ThenBy(s => s.Id)
            .Select(s => new LinhKienSerialDto
            {
                Id = s.Id,
                LinhKienId = s.LinhKienId,
                MaLk = s.LinhKien.MaLk,
                TenLinhKien = s.LinhKien.TenLinhKien,
                SoSerial = s.SoSerial,
                TrangThai = s.TrangThai,
                KhoId = s.KhoId,
                TenKho = s.Kho != null ? s.Kho.TenKho : null,
                PhieuNhapId = s.PhieuNhapId,
                MaPn = s.PhieuNhap.MaPn,
                NgayNhap = s.PhieuNhap.NgayNhap,
                TenNcc = s.PhieuNhap.NhaCungCap.TenNcc,
                GhiChu = s.GhiChu
            })
            .ToListAsync();
    }

    public async Task<SerialVongDoiDto?> TraCuuAsync(string soSerial)
    {
        var so = ChuanHoa(soSerial);

        return await _context.LinhKienSerials
            .AsNoTracking()
            .Where(s => s.SoSerial == so)
            .Select(s => new SerialVongDoiDto
            {
                Id = s.Id,
                LinhKienId = s.LinhKienId,
                MaLk = s.LinhKien.MaLk,
                TenLinhKien = s.LinhKien.TenLinhKien,
                SoSerial = s.SoSerial,
                TrangThai = s.TrangThai,
                KhoId = s.KhoId,
                TenKho = s.Kho != null ? s.Kho.TenKho : null,
                PhieuNhapId = s.PhieuNhapId,
                MaPn = s.PhieuNhap.MaPn,
                NgayNhap = s.PhieuNhap.NgayNhap,
                TenNcc = s.PhieuNhap.NhaCungCap.TenNcc,
                GhiChu = s.GhiChu,
                PhieuSuaChuaId = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.PhieuId : null,
                MaPhieuSuaChua = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.Phieu.MaPhieu : null,
                NgayXuat = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.NgayXuat : null,
                BaoHanhDen = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.BaoHanhDen : null,
                TenThietBi = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.Phieu.ThietBi.TenThietBi : null,
                TenKhachHang = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.Phieu.ThietBi.KhachHang.HoTen : null
            })
            .FirstOrDefaultAsync();
    }

    public async Task<BaoHanhSerialDto?> TraCuuBaoHanhAsync(string soSerial)
    {
        var so = ChuanHoa(soSerial);

        var dto = await _context.LinhKienSerials
            .AsNoTracking()
            .Where(s => s.SoSerial == so)
            .Select(s => new BaoHanhSerialDto
            {
                SoSerial = s.SoSerial,
                TenLinhKien = s.LinhKien.TenLinhKien,
                HangSanXuat = s.LinhKien.HangSanXuat,
                TrangThai = s.TrangThai,
                NgayNhapKho = s.PhieuNhap.NgayNhap,
                TenNcc = s.PhieuNhap.NhaCungCap.TenNcc,
                NgayXuat = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.NgayXuat : null,
                MaPhieuSuaChua = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.Phieu.MaPhieu : null,
                TenThietBi = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.Phieu.ThietBi.TenThietBi : null,
                BaoHanhDen = s.ChiTietXuatKho != null ? s.ChiTietXuatKho.BaoHanhDen : null
            })
            .FirstOrDefaultAsync();

        if (dto == null) return null;

        if (_v3Enabled && dto.TrangThai == "DA_XUAT")
        {
            var issueId = await _v3Context.LinhKienSerials.AsNoTracking()
                .Where(s => s.SoSerial == so)
                .Select(s => s.ChiTietXuatKhoId)
                .FirstOrDefaultAsync();
            var policy = issueId.HasValue
                ? await _v3Context.CamKetBaoHanhs.AsNoTracking()
                    .Where(x => x.Loai == "LINH_KIEN" && x.ChiTietXuatKhoId == issueId)
                    .OrderByDescending(x => x.NgayKetThuc)
                    .Select(x => new { x.NgayBatDau, x.NgayKetThuc })
                    .FirstOrDefaultAsync()
                : null;
            dto.BaoHanhTu = policy?.NgayBatDau;
            dto.BaoHanhDen = policy?.NgayKetThuc;
        }

        // Ngày hiện tại theo giờ Việt Nam (UTC+7)
        var homNay = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

        switch (dto.TrangThai)
        {
            case "TRONG_KHO":
                dto.ThongBao = "Linh kiện đang nằm trong kho của cửa hàng, chưa được bán ra.";
                break;
            case "BAO_HANH":
                dto.ThongBao = "Linh kiện đang trong quá trình bảo hành/đổi trả với nhà cung cấp.";
                break;
            case "THANH_LY":
                dto.ThongBao = "Linh kiện đã được thanh lý.";
                break;
            case "THIEU_KIEM_KE":
                dto.ThongBao = "Linh kiện đang thiếu sau kiểm kê; cần đối soát trước khi xuất.";
                break;
            case "CACH_LY":
                dto.ThongBao = "Linh kiện lỗi đang được cách ly, không thể xuất.";
                break;
            default: // DA_XUAT
                if (dto.BaoHanhDen == null)
                {
                    dto.ThongBao = _v3Enabled
                        ? "Linh kiện đã xuất nhưng chưa ghi nhận cam kết bảo hành sau bàn giao. Vui lòng liên hệ cửa hàng."
                        : "Linh kiện đã bán ra nhưng không có chính sách bảo hành.";
                }
                else if (dto.BaoHanhTu.HasValue && dto.BaoHanhTu.Value > homNay)
                {
                    dto.ThongBao = $"Bảo hành bắt đầu từ {dto.BaoHanhTu.Value:dd/MM/yyyy}.";
                }
                else if (dto.BaoHanhDen.Value >= homNay)
                {
                    dto.ConBaoHanh = true;
                    dto.ThongBao = $"Linh kiện do cửa hàng bán ra, còn bảo hành đến {dto.BaoHanhDen.Value:dd/MM/yyyy}.";
                }
                else
                {
                    dto.ThongBao = $"Linh kiện do cửa hàng bán ra, đã hết bảo hành từ {dto.BaoHanhDen.Value:dd/MM/yyyy}.";
                }
                break;
        }

        return dto;
    }

    private static string ChuanHoa(string soSerial) =>
        (soSerial ?? string.Empty).Trim().ToUpperInvariant();
}
