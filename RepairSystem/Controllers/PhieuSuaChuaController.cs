using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

/// <summary>
/// Phiếu tiếp nhận sửa chữa thiết bị. Chỉ nhân viên truy cập (khách hàng không gọi trực tiếp API này).
/// - Lễ tân / Admin: tạo phiếu, xem liên hệ khách và mật khẩu máy.
/// - Kỹ thuật viên: chỉ thấy phiếu mình phụ trách, đổi trạng thái sửa chữa.
/// - Thủ kho: xem phiếu để xuất linh kiện (không thấy SĐT khách và mật khẩu máy).
/// </summary>
[ApiController]
[Route("api/phieu-sua-chua")]
[Authorize(Roles = Roles.NhanVien)]
public class PhieuSuaChuaController : ControllerBase
{
    // Giá trị cột phieu_sua_chua.trang_thai (có CHECK trong DB)
    private const string TiepNhan = "TIEP_NHAN";
    private const string DangSua = "DANG_SUA";
    private const string ChoLinhKien = "CHO_LINH_KIEN";
    private const string HoanThanh = "HOAN_THANH";
    private const string DaTra = "DA_TRA";
    private const string Huy = "HUY";

    private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^\+?[0-9]{8,14}$", RegexOptions.Compiled);

    /// <summary>Các bước chuyển trạng thái hợp lệ.</summary>
    private static readonly Dictionary<string, string[]> ChuyenTrangThai = new()
    {
        [TiepNhan] = new[] { DangSua, Huy },
        [DangSua] = new[] { ChoLinhKien, HoanThanh, Huy },
        [ChoLinhKien] = new[] { DangSua, Huy },
        [HoanThanh] = new[] { DaTra },
    };

    private readonly AppDbContext _context;
    private readonly IXuatKhoService _xuatKhoService;

    public PhieuSuaChuaController(AppDbContext context, IXuatKhoService xuatKhoService)
    {
        _context = context;
        _xuatKhoService = xuatKhoService;
    }

    // ── Đọc dữ liệu ────────────────────────────────────────────────

    /// <summary>
    /// Danh sách phiếu. Query: trangThai (có thể nhiều giá trị, cách nhau bằng dấu phẩy), search.
    /// Kỹ thuật viên chỉ thấy phiếu mình phụ trách.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<PhieuSuaChuaViewDto>>>> GetAll(
        [FromQuery] string? trangThai = null,
        [FromQuery] string? search = null)
    {
        var query = _context.PhieuSuaChuas.AsNoTracking().AsQueryable();

        if (User.IsInRole(Roles.KyThuatVien))
        {
            var nvId = await GetNhanVienIdAsync();
            if (nvId == null)
                return Unauthorized(ApiResponse<IEnumerable<PhieuSuaChuaViewDto>>.ErrorResponse(
                    "Tài khoản chưa gắn với hồ sơ nhân viên."));
            query = query.Where(p => p.KyThuatVienId == nvId);
        }

        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            var ds = trangThai.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToUpperInvariant())
                .ToList();
            query = query.Where(p => ds.Contains(p.TrangThai));
        }

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
            .Select(ChieuXem(XemLienHe(), XemMatKhau()))
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<PhieuSuaChuaViewDto>>.SuccessResponse(list));
    }

    /// <summary>Chi tiết một phiếu.</summary>
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<PhieuSuaChuaViewDto>>> GetById(long id)
    {
        var dto = await _context.PhieuSuaChuas
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ChieuXem(XemLienHe(), XemMatKhau()))
            .FirstOrDefaultAsync();

        if (dto == null)
            return NotFound(ApiResponse<PhieuSuaChuaViewDto>.ErrorResponse($"Không tìm thấy phiếu sửa chữa id={id}."));

        if (!await DuocXemPhieuAsync(dto.KyThuatVienId))
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<PhieuSuaChuaViewDto>.ErrorResponse("Bạn không phụ trách phiếu này."));

        return Ok(ApiResponse<PhieuSuaChuaViewDto>.SuccessResponse(dto));
    }

    /// <summary>Danh sách kỹ thuật viên đang hoạt động kèm số ca đang làm, để lễ tân phân công.</summary>
    [HttpGet("ky-thuat-vien")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<IEnumerable<KyThuatVienPhanCongDto>>>> GetKyThuatVien()
    {
        var dangLam = new[] { TiepNhan, DangSua, ChoLinhKien };

        var list = await _context.KyThuatViens
            .AsNoTracking()
            .Where(k => k.NhanVien.DeletedAt == null && k.NhanVien.TaiKhoan.HoatDong)
            .Select(k => new KyThuatVienPhanCongDto
            {
                Id = k.NhanVienId,
                HoTen = k.NhanVien.HoTen,
                ChuyenMon = k.ChuyenMon,
                SoCaDangLam = k.PhieuSuaChuas.Count(p => dangLam.Contains(p.TrangThai))
            })
            .OrderBy(x => x.SoCaDangLam)
            .ThenBy(x => x.HoTen)
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<KyThuatVienPhanCongDto>>.SuccessResponse(list));
    }

    // ── Tiếp nhận máy ──────────────────────────────────────────────

    /// <summary>
    /// Tạo phiếu tiếp nhận (form Intake). Lễ tân được lấy từ tài khoản đăng nhập.
    /// Mã phiếu (SCxxxxxxx) do database tự sinh.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<object>>> Create([FromBody] IntakeRequestDto dto)
    {
        // ── Kiểm tra dữ liệu đầu vào ──
        if (string.IsNullOrWhiteSpace(dto.TinhTrangLoi))
            return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng nhập mô tả tình trạng lỗi."));
        if (Dai(dto.TinhTrangNgoaiQuan, 500) || Dai(dto.PhuKienKemTheo, 500))
            return BadRequest(ApiResponse<object>.ErrorResponse("Tình trạng ngoại quan và phụ kiện tối đa 500 ký tự."));
        if (Dai(dto.MatKhauMay, 100))
            return BadRequest(ApiResponse<object>.ErrorResponse("Mật khẩu máy tối đa 100 ký tự."));

        var nhanVienId = await GetNhanVienIdAsync();
        if (nhanVienId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Tài khoản chưa gắn với hồ sơ nhân viên."));

        // Lễ tân: chính người đăng nhập. Admin: phải chỉ định một lễ tân.
        var leTanId = User.IsInRole(Roles.LeTan) ? nhanVienId.Value : dto.LeTanId ?? 0;
        if (leTanId <= 0 || !await _context.LeTans.AnyAsync(l => l.NhanVienId == leTanId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Không xác định được lễ tân tiếp nhận (cần chỉ định leTanId hợp lệ)."));

        if (dto.KyThuatVienId.HasValue &&
            !await _context.KyThuatViens.AnyAsync(k => k.NhanVienId == dto.KyThuatVienId.Value && k.NhanVien.DeletedAt == null))
            return BadRequest(ApiResponse<object>.ErrorResponse("Kỹ thuật viên được chọn không tồn tại."));

        var bayGio = DateTime.UtcNow;
        if (dto.NgayHenTra.HasValue && dto.NgayHenTra.Value.ToUniversalTime() < bayGio)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thời gian hẹn trả máy phải sau thời điểm tiếp nhận."));

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // ── Thiết bị: dùng thiết bị có sẵn hoặc tìm/tạo khách + thiết bị mới ──
            long thietBiId;
            if (dto.ThietBiId is > 0)
            {
                var tonTai = await _context.ThietBis.AnyAsync(t => t.Id == dto.ThietBiId.Value && t.DeletedAt == null);
                if (!tonTai)
                    return BadRequest(ApiResponse<object>.ErrorResponse($"Thiết bị id={dto.ThietBiId} không tồn tại."));
                thietBiId = dto.ThietBiId.Value;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.CustomerName))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng nhập tên khách hàng."));
                if (string.IsNullOrWhiteSpace(dto.DeviceName))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng nhập model thiết bị."));
                if (Dai(dto.CustomerName, 100) || Dai(dto.DeviceName, 150) || Dai(dto.DeviceType, 50) || Dai(dto.SoSerial, 100))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Một số thông tin vượt quá độ dài cho phép."));

                var phone = Regex.Replace(dto.Phone ?? string.Empty, @"[\s.\-()]", "");
                if (!PhoneRegex.IsMatch(phone))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Số điện thoại không hợp lệ (8–14 chữ số)."));

                var email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
                if (email != null && !EmailRegex.IsMatch(email))
                    return BadRequest(ApiResponse<object>.ErrorResponse("Email không hợp lệ."));

                var khach = await _context.KhachHangs
                    .FirstOrDefaultAsync(k => k.SoDienThoai == phone && k.DeletedAt == null);
                if (khach == null)
                {
                    if (email != null && await _context.KhachHangs.AnyAsync(k => k.Email == email && k.DeletedAt == null))
                        return Conflict(ApiResponse<object>.ErrorResponse("Email này đã thuộc về một khách hàng khác."));

                    khach = new KhachHang
                    {
                        HoTen = dto.CustomerName.Trim(),
                        SoDienThoai = phone,
                        Email = email,
                        VaiTroTk = "KHACH_HANG"
                    };
                    _context.KhachHangs.Add(khach);
                    await _context.SaveChangesAsync();
                }

                var serial = string.IsNullOrWhiteSpace(dto.SoSerial) ? null : dto.SoSerial.Trim().ToUpperInvariant();
                ThietBi? thietBi = null;
                if (serial != null)
                {
                    thietBi = await _context.ThietBis.FirstOrDefaultAsync(t => t.SoSerial == serial && t.DeletedAt == null);
                    if (thietBi != null && thietBi.KhachHangId != khach.Id)
                        return Conflict(ApiResponse<object>.ErrorResponse(
                            $"Số serial {serial} đã được đăng ký cho một khách hàng khác. Vui lòng kiểm tra lại."));
                }

                if (thietBi == null)
                {
                    thietBi = new ThietBi
                    {
                        KhachHangId = khach.Id,
                        LoaiMay = string.IsNullOrWhiteSpace(dto.DeviceType) ? "Laptop" : dto.DeviceType.Trim(),
                        TenThietBi = dto.DeviceName.Trim(),
                        SoSerial = serial
                    };
                    _context.ThietBis.Add(thietBi);
                    await _context.SaveChangesAsync();
                }

                thietBiId = thietBi.Id;
            }

            // ── Phiếu sửa chữa (không gán MaPhieu: đây là cột tính toán của DB) ──
            var phieu = new PhieuSuaChua
            {
                ThietBiId = thietBiId,
                LeTanId = leTanId,
                KyThuatVienId = dto.KyThuatVienId,
                NgayNhan = bayGio,
                NgayHenTra = dto.NgayHenTra?.ToUniversalTime(),
                TinhTrangLoi = dto.TinhTrangLoi.Trim(),
                TinhTrangNgoaiQuan = NullIfBlank(dto.TinhTrangNgoaiQuan),
                PhuKienKemTheo = NullIfBlank(dto.PhuKienKemTheo),
                MatKhauMay = NullIfBlank(dto.MatKhauMay),
                TrangThai = TiepNhan,
                GhiChu = NullIfBlank(dto.GhiChu)
            };
            _context.PhieuSuaChuas.Add(phieu);
            await _context.SaveChangesAsync();

            _context.LichSuTrangThaiPhieus.Add(new LichSuTrangThaiPhieu
            {
                PhieuId = phieu.Id,
                TrangThaiCu = null,
                TrangThaiMoi = TiepNhan,
                NguoiThucHienId = nhanVienId,
                GhiChu = "Tiếp nhận thiết bị"
            });
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            var resData = new
            {
                phieu.Id,
                phieu.MaPhieu, // đã được EF đọc lại từ cột tính toán sau khi lưu
                phieu.ThietBiId,
                phieu.NgayNhan,
                phieu.NgayHenTra,
                phieu.TinhTrangLoi,
                phieu.TinhTrangNgoaiQuan,
                phieu.PhuKienKemTheo,
                phieu.TrangThai
            };
            return Ok(ApiResponse<object>.SuccessResponse(resData, "Tiếp nhận thiết bị thành công."));
        }
        catch (DbUpdateException)
        {
            await tx.RollbackAsync();
            return Conflict(ApiResponse<object>.ErrorResponse(
                "Không thể lưu phiếu do vi phạm ràng buộc dữ liệu (trùng số điện thoại/email/serial hoặc dữ liệu không hợp lệ)."));
        }
    }

    // ── Chuyển trạng thái ──────────────────────────────────────────

    /// <summary>
    /// Đổi trạng thái phiếu: TIEP_NHAN → DANG_SUA → (CHO_LINH_KIEN ↔ DANG_SUA) → HOAN_THANH → DA_TRA, hoặc HUY.
    /// KTV: DANG_SUA / CHO_LINH_KIEN / HOAN_THANH trên phiếu của mình. Lễ tân: HUY / DA_TRA. Admin: tất cả.
    /// </summary>
    [HttpPut("{id:long}/trang-thai")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<object>>> CapNhatTrangThai(long id, [FromBody] CapNhatTrangThaiPhieuDto dto)
    {
        var dich = dto.TrangThai?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(dich))
            return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng chọn trạng thái mới."));

        var nhanVienId = await GetNhanVienIdAsync();
        if (nhanVienId == null)
            return Unauthorized(ApiResponse<object>.ErrorResponse("Tài khoản chưa gắn với hồ sơ nhân viên."));

        var phieu = await _context.PhieuSuaChuas.FirstOrDefaultAsync(p => p.Id == id);
        if (phieu == null)
            return NotFound(ApiResponse<object>.ErrorResponse($"Không tìm thấy phiếu sửa chữa id={id}."));

        if (!ChuyenTrangThai.TryGetValue(phieu.TrangThai, out var hopLe) || !hopLe.Contains(dich))
            return Conflict(ApiResponse<object>.ErrorResponse($"Không thể chuyển phiếu từ {phieu.TrangThai} sang {dich}."));

        var laAdmin = User.IsInRole(Roles.Admin);
        var laKtv = User.IsInRole(Roles.KyThuatVien);
        if (!laAdmin)
        {
            var duocPhep = laKtv ? new[] { DangSua, ChoLinhKien, HoanThanh } : new[] { Huy, DaTra };
            if (!duocPhep.Contains(dich))
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.ErrorResponse("Vai trò của bạn không được phép chuyển sang trạng thái này."));

            if (laKtv && phieu.KyThuatVienId != null && phieu.KyThuatVienId != nhanVienId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.ErrorResponse("Bạn không phụ trách phiếu này."));
        }

        // DB bắt buộc phiếu đã sửa phải có KTV: KTV nhận việc thì tự gán, vai trò khác phải có KTV sẵn
        if (dich != Huy && phieu.KyThuatVienId == null)
        {
            if (laKtv) phieu.KyThuatVienId = nhanVienId;
            else
                return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu chưa có kỹ thuật viên phụ trách."));
        }

        if (dich == Huy)
        {
            if (string.IsNullOrWhiteSpace(dto.LyDoHuy))
                return BadRequest(ApiResponse<object>.ErrorResponse("Vui lòng nhập lý do hủy phiếu."));
            phieu.LyDoHuy = dto.LyDoHuy.Trim();
        }

        var cu = phieu.TrangThai;
        var bayGio = DateTime.UtcNow;
        phieu.TrangThai = dich;
        phieu.UpdatedAt = bayGio;
        if (dich == HoanThanh)
        {
            phieu.NgayHoanThanh = bayGio;
            if (!string.IsNullOrWhiteSpace(dto.KetQuaSuaChua)) phieu.KetQuaSuaChua = dto.KetQuaSuaChua.Trim();
        }
        if (dich == DaTra) phieu.NgayTra = bayGio;

        _context.LichSuTrangThaiPhieus.Add(new LichSuTrangThaiPhieu
        {
            PhieuId = phieu.Id,
            TrangThaiCu = cu,
            TrangThaiMoi = dich,
            NguoiThucHienId = nhanVienId,
            GhiChu = NullIfBlank(dto.GhiChu)
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(ApiResponse<object>.ErrorResponse("Không thể cập nhật trạng thái do vi phạm ràng buộc dữ liệu."));
        }

        return Ok(ApiResponse<object>.SuccessResponse(
            new { phieu.Id, phieu.MaPhieu, phieu.TrangThai }, "Đã cập nhật trạng thái phiếu."));
    }

    // ── Xuất kho cho phiếu ─────────────────────────────────────────

    /// <summary>
    /// Thủ kho xuất linh kiện cho phiếu (phiếu phải ở DANG_SUA hoặc CHO_LINH_KIEN).
    /// Nếu linh kiện theo dõi serial thì phải chọn serial cụ thể.
    /// </summary>
    [HttpPost("{id:long}/xuat-kho")]
    [Authorize(Roles = Roles.ThuKho)]
    public async Task<ActionResult<ApiResponse<List<DongXuatKhoDto>>>> XuatKho(long id, [FromBody] XuatKhoRequestDto dto)
    {
        var taiKhoanId = User.GetTaiKhoanId();
        if (taiKhoanId == null)
            return Unauthorized(ApiResponse<List<DongXuatKhoDto>>.ErrorResponse("Không xác định được người dùng."));

        try
        {
            var result = await _xuatKhoService.XuatKhoAsync(id, dto, taiKhoanId.Value);
            return Ok(ApiResponse<List<DongXuatKhoDto>>.SuccessResponse(result, "Xuất kho thành công."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<List<DongXuatKhoDto>>.ErrorResponse(ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<List<DongXuatKhoDto>>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiResponse<List<DongXuatKhoDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Các linh kiện đã xuất cho phiếu (kèm serial).</summary>
    [HttpGet("{id:long}/xuat-kho")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<List<DongXuatKhoDto>>>> GetXuatKho(long id)
    {
        var kyThuatVienId = await _context.PhieuSuaChuas
            .Where(p => p.Id == id)
            .Select(p => new { p.KyThuatVienId })
            .FirstOrDefaultAsync();
        if (kyThuatVienId == null)
            return NotFound(ApiResponse<List<DongXuatKhoDto>>.ErrorResponse($"Không tìm thấy phiếu sửa chữa id={id}."));

        if (!await DuocXemPhieuAsync(kyThuatVienId.KyThuatVienId))
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<List<DongXuatKhoDto>>.ErrorResponse("Bạn không phụ trách phiếu này."));

        var result = await _xuatKhoService.GetXuatKhoCuaPhieuAsync(id);
        return Ok(ApiResponse<List<DongXuatKhoDto>>.SuccessResponse(result));
    }

    // ── Nội bộ ─────────────────────────────────────────────────────

    private bool XemLienHe() => User.IsInRole(Roles.Admin) || User.IsInRole(Roles.LeTan);

    private bool XemMatKhau() =>
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.LeTan) || User.IsInRole(Roles.KyThuatVien);

    /// <summary>Kỹ thuật viên chỉ được xem phiếu mình phụ trách; các vai trò còn lại xem được mọi phiếu.</summary>
    private async Task<bool> DuocXemPhieuAsync(long? kyThuatVienId)
    {
        if (!User.IsInRole(Roles.KyThuatVien)) return true;
        var nvId = await GetNhanVienIdAsync();
        return nvId != null && kyThuatVienId == nvId;
    }

    private async Task<long?> GetNhanVienIdAsync()
    {
        var taiKhoanId = User.GetTaiKhoanId();
        if (taiKhoanId == null) return null;

        return await _context.NhanViens
            .Where(n => n.TaiKhoanId == taiKhoanId.Value && n.DeletedAt == null)
            .Select(n => (long?)n.Id)
            .FirstOrDefaultAsync();
    }

    /// <summary>Phép chiếu dùng chung cho danh sách và chi tiết; ẩn SĐT khách / mật khẩu máy theo vai trò.</summary>
    private static Expression<Func<PhieuSuaChua, PhieuSuaChuaViewDto>> ChieuXem(bool xemLienHe, bool xemMatKhau) =>
        p => new PhieuSuaChuaViewDto
        {
            Id = p.Id,
            MaPhieu = p.MaPhieu,
            ThietBiId = p.ThietBiId,
            TenThietBi = p.ThietBi.TenThietBi,
            SoSerialThietBi = p.ThietBi.SoSerial,
            TenKhachHang = p.ThietBi.KhachHang.HoTen,
            SoDienThoaiKhach = xemLienHe ? p.ThietBi.KhachHang.SoDienThoai : null,
            TenLeTan = p.LeTan.NhanVien.HoTen,
            KyThuatVienId = p.KyThuatVienId,
            TenKyThuatVien = p.KyThuatVien != null ? p.KyThuatVien.NhanVien.HoTen : null,
            NgayNhan = p.NgayNhan,
            NgayHenTra = p.NgayHenTra,
            TinhTrangLoi = p.TinhTrangLoi,
            TinhTrangNgoaiQuan = p.TinhTrangNgoaiQuan,
            PhuKienKemTheo = p.PhuKienKemTheo,
            MatKhauMay = xemMatKhau ? p.MatKhauMay : null,
            TrangThai = p.TrangThai,
            GhiChu = p.GhiChu
        };

    private static bool Dai(string? s, int max) => s != null && s.Trim().Length > max;

    private static string? NullIfBlank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}

/// <summary>Dữ liệu phiếu trả về cho giao diện.</summary>
public class PhieuSuaChuaViewDto
{
    public long Id { get; set; }
    public string MaPhieu { get; set; } = null!;
    public long ThietBiId { get; set; }
    public string TenThietBi { get; set; } = null!;
    public string? SoSerialThietBi { get; set; }
    public string TenKhachHang { get; set; } = null!;
    /// <summary>Chỉ Admin và Lễ tân thấy.</summary>
    public string? SoDienThoaiKhach { get; set; }
    public string TenLeTan { get; set; } = null!;
    public long? KyThuatVienId { get; set; }
    public string? TenKyThuatVien { get; set; }
    public DateTime NgayNhan { get; set; }
    public DateTime? NgayHenTra { get; set; }
    public string TinhTrangLoi { get; set; } = null!;
    public string? TinhTrangNgoaiQuan { get; set; }
    public string? PhuKienKemTheo { get; set; }
    /// <summary>Chỉ Admin, Lễ tân và Kỹ thuật viên phụ trách thấy.</summary>
    public string? MatKhauMay { get; set; }
    public string TrangThai { get; set; } = null!;
    public string? GhiChu { get; set; }
}

/// <summary>Kỹ thuật viên kèm số ca đang làm, dùng để phân công.</summary>
public class KyThuatVienPhanCongDto
{
    public long Id { get; set; }
    public string HoTen { get; set; } = null!;
    public string? ChuyenMon { get; set; }
    public int SoCaDangLam { get; set; }
}

/// <summary>DTO tiếp nhận thiết bị từ giao diện Lễ tân.</summary>
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
    public DateTime? NgayHenTra { get; set; }
    public string TinhTrangLoi { get; set; } = null!;
    public string? TinhTrangNgoaiQuan { get; set; }
    public string? PhuKienKemTheo { get; set; }
    public string? MatKhauMay { get; set; }
    public string? GhiChu { get; set; }
}

public class CapNhatTrangThaiPhieuDto
{
    public string TrangThai { get; set; } = null!;
    public string? GhiChu { get; set; }
    /// <summary>Bắt buộc khi chuyển sang HUY.</summary>
    public string? LyDoHuy { get; set; }
    /// <summary>Tuỳ chọn khi chuyển sang HOAN_THANH.</summary>
    public string? KetQuaSuaChua { get; set; }
}
