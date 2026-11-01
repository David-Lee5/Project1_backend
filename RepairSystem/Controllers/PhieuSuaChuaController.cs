using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers;

/// <summary>
/// Quản lý phiếu tiếp nhận sửa chữa thiết bị (Giai đoạn 2 & 3).
/// Minh bạch thông tin đầu vào: ngoại quan lúc nhận, phụ kiện gửi kèm, mật khẩu mở máy.
/// </summary>
[ApiController]
[Route("api/phieu-sua-chua")]
public class PhieuSuaChuaController : ControllerBase
{
    private readonly AppDbContext _context;

    public PhieuSuaChuaController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy danh sách phiếu sửa chữa (kèm tình trạng ngoại quan & phụ kiện).
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> GetAll(
        [FromQuery] string? trangThai = null,
        [FromQuery] string? search = null)
    {
        var query = _context.PhieuSuaChuas
            .Include(p => p.ThietBi).ThenInclude(t => t.KhachHang)
            .Include(p => p.LeTan).ThenInclude(l => l.NhanVien)
            .Include(p => p.KyThuatVien).ThenInclude(k => k.NhanVien)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(trangThai))
            query = query.Where(p => p.TrangThai == trangThai);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                p.MaPhieu.ToLower().Contains(s) ||
                p.ThietBi.TenThietBi.ToLower().Contains(s) ||
                p.ThietBi.KhachHang.HoTen.ToLower().Contains(s) ||
                p.ThietBi.KhachHang.SoDienThoai.Contains(s));
        }

        var list = await query
            .OrderByDescending(p => p.NgayNhan)
            .Select(p => new
            {
                p.Id,
                p.MaPhieu,
                p.ThietBiId,
                TenThietBi = p.ThietBi.TenThietBi,
                SoSerialThietBi = p.ThietBi.SoSerial,
                TenKhachHang = p.ThietBi.KhachHang.HoTen,
                SoDienThoaiKhach = p.ThietBi.KhachHang.SoDienThoai,
                TenLeTan = p.LeTan != null ? p.LeTan.NhanVien.HoTen : "Lễ tân",
                TenKyThuatVien = p.KyThuatVien != null ? p.KyThuatVien.NhanVien.HoTen : null,
                p.NgayNhan,
                p.NgayHenTra,
                p.TinhTrangLoi,
                p.TinhTrangNgoaiQuan,
                p.PhuKienKemTheo,
                p.MatKhauMay,
                p.TrangThai,
                p.GhiChu
            })
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(list));
    }

    /// <summary>
    /// Lấy chi tiết một phiếu sửa chữa theo id.
    /// </summary>
    [HttpGet("{id:long}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> GetById(long id)
    {
        var p = await _context.PhieuSuaChuas
            .Include(p => p.ThietBi).ThenInclude(t => t.KhachHang)
            .Include(p => p.LeTan).ThenInclude(l => l.NhanVien)
            .Include(p => p.KyThuatVien).ThenInclude(k => k.NhanVien)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (p == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy phiếu sửa chữa id={id}."));

        var result = new
        {
            p.Id,
            p.MaPhieu,
            p.ThietBiId,
            TenThietBi = p.ThietBi.TenThietBi,
            SoSerialThietBi = p.ThietBi.SoSerial,
            TenKhachHang = p.ThietBi.KhachHang.HoTen,
            SoDienThoaiKhach = p.ThietBi.KhachHang.SoDienThoai,
            TenLeTan = p.LeTan != null ? p.LeTan.NhanVien.HoTen : "Lễ tân",
            TenKyThuatVien = p.KyThuatVien != null ? p.KyThuatVien.NhanVien.HoTen : null,
            p.NgayNhan,
            p.NgayHenTra,
            p.TinhTrangLoi,
            p.TinhTrangNgoaiQuan,
            p.PhuKienKemTheo,
            p.MatKhauMay,
            p.TrangThai,
            p.GhiChu
        };

        return Ok(ApiResponse<object>.SuccessResponse(result));
    }

    /// <summary>
    /// Tạo phiếu tiếp nhận thiết bị mới (Form Intake của Lễ tân).
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] IntakeRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TinhTrangLoi))
            return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng nhập mô tả tình trạng lỗi."));

        long leTanId = dto.LeTanId ?? 0;
        if (leTanId <= 0)
        {
            var firstLeTan = await _context.LeTans.Select(l => l.NhanVienId).FirstOrDefaultAsync();
            if (firstLeTan > 0)
                leTanId = firstLeTan;
            else
            {
                var nv = await _context.NhanViens.Select(n => n.Id).FirstOrDefaultAsync();
                leTanId = nv;
            }
        }

        long thietBiId = dto.ThietBiId ?? 0;
        if (thietBiId <= 0)
        {
            // Tìm hoặc tạo khách hàng
            var phone = (dto.Phone ?? "0900000000").Trim();
            var khach = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SoDienThoai == phone);
            if (khach == null)
            {
                khach = new KhachHang
                {
                    HoTen = string.IsNullOrWhiteSpace(dto.CustomerName) ? "Khách vãng lai" : dto.CustomerName.Trim(),
                    SoDienThoai = phone,
                    Email = dto.Email?.Trim()
                };
                _context.KhachHangs.Add(khach);
                await _context.SaveChangesAsync();
            }

            // Tạo thiết bị
            var serial = string.IsNullOrWhiteSpace(dto.SoSerial)
                ? $"SN-{DateTime.UtcNow:yyyyMMddHHmmss}"
                : dto.SoSerial.Trim().ToUpperInvariant();

            var thietBi = await _context.ThietBis.FirstOrDefaultAsync(t => t.SoSerial == serial);
            if (thietBi == null)
            {
                var rndTb = Random.Shared.Next(1000, 9999);
                thietBi = new ThietBi
                {
                    MaThietBi = $"TB-{DateTime.UtcNow:yyMMdd}-{rndTb}",
                    KhachHangId = khach.Id,
                    LoaiMay = dto.DeviceType ?? "Laptop",
                    TenThietBi = string.IsNullOrWhiteSpace(dto.DeviceName) ? "Thiết bị khách hàng" : dto.DeviceName.Trim(),
                    SoSerial = serial
                };
                _context.ThietBis.Add(thietBi);
                await _context.SaveChangesAsync();
            }
            thietBiId = thietBi.Id;
        }

        // Tạo mã phiếu tiếp nhận duy nhất
        var rnd = Random.Shared.Next(1000, 9999);
        var maPhieu = $"PN-{DateTime.UtcNow:yyMMdd}-{rnd}";

        var phieu = new PhieuSuaChua
        {
            MaPhieu = maPhieu,
            ThietBiId = thietBiId,
            LeTanId = leTanId,
            KyThuatVienId = dto.KyThuatVienId,
            NgayNhan = DateTime.UtcNow,
            TinhTrangLoi = dto.TinhTrangLoi.Trim(),
            TinhTrangNgoaiQuan = dto.TinhTrangNgoaiQuan?.Trim(),
            PhuKienKemTheo = dto.PhuKienKemTheo?.Trim(),
            MatKhauMay = dto.MatKhauMay?.Trim(),
            TrangThai = "TIEP_NHAN",
            GhiChu = dto.GhiChu?.Trim()
        };

        _context.PhieuSuaChuas.Add(phieu);
        await _context.SaveChangesAsync();

        var resData = new
        {
            phieu.Id,
            phieu.MaPhieu,
            phieu.ThietBiId,
            phieu.NgayNhan,
            phieu.TinhTrangLoi,
            phieu.TinhTrangNgoaiQuan,
            phieu.PhuKienKemTheo,
            phieu.MatKhauMay,
            phieu.TrangThai
        };

        return Ok(ApiResponse<object>.SuccessResponse(resData, "Tiếp nhận thiết bị thành công."));
    }
}

/// <summary>
/// DTO tiếp nhận thiết bị từ giao diện Lễ tân.
/// </summary>
public class IntakeRequestDto
{
    public long? ThietBiId { get; set; }
    public string? CustomerName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? DeviceType { get; set; }
    public string? DeviceName { get; set; }
    public string? SoSerial { get; set; }
    public long? LeTanId { get; set; }
    public long? KyThuatVienId { get; set; }
    public string TinhTrangLoi { get; set; } = null!;
    public string? TinhTrangNgoaiQuan { get; set; }
    public string? PhuKienKemTheo { get; set; }
    public string? MatKhauMay { get; set; }
    public string? GhiChu { get; set; }
}
