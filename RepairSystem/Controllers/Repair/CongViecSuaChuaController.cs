using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/cong-viec-sua-chua")]
[Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
public sealed class CongViecSuaChuaController(RepairDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DanhSach([FromQuery] long? phieuId, [FromQuery] long? phieuBaoHanhId)
    {
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var q = db.CongViecSuaChuas.AsNoTracking().Include(x => x.VatTu).AsQueryable();
        if (phieuId.HasValue) q = q.Where(x => x.PhieuId == phieuId);
        if (phieuBaoHanhId.HasValue) q = q.Where(x => x.PhieuBaoHanhId == phieuBaoHanhId);
        if (User.IsInRole(Roles.KyThuatVien)) q = q.Where(x => x.KyThuatVienId == actor);
        var rows = await q.OrderByDescending(x => x.TaoLuc).Take(300).ToListAsync();
        var showPay = User.IsInRole(Roles.Admin) || User.IsInRole(Roles.KyThuatVien);
        return Ok(ApiResponse<object>.SuccessResponse(rows.Select(x => new {
            x.Id, x.PhieuId, x.PhieuBaoHanhId, x.MoTa, x.KyThuatVienId,
            x.TrangThai, x.TaoBoiId, x.TaoLuc, x.BatDauLuc, x.HoanThanhLuc,
            x.KetQua, VatTu = x.VatTu.Select(v => v.ChiTietXuatKhoId),
            CongDeXuat = showPay ? x.CongDeXuat : null,
            CongDuyet = showPay ? x.CongDuyet : null,
            CongDuyetBoiId = showPay ? x.CongDuyetBoiId : null,
            CongDuyetLuc = showPay ? x.CongDuyetLuc : null,
            LyDoDuyetCong = showPay ? x.LyDoDuyetCong : null,
            ChiTraLuc = showPay ? x.ChiTraLuc : null,
            MaThamChieuChiTra = showPay ? x.MaThamChieuChiTra : null
        })));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> Tao([FromBody] TaoCongViecRequest req)
    {
        if (req.PhieuId <= 0 || req.KyThuatVienId <= 0
            || string.IsNullOrWhiteSpace(req.MoTa) || req.MoTa.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần phiếu, kỹ thuật viên và mô tả công việc."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var ticket = await db.PhieuSuaChuas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.PhieuId);
        if (ticket == null || ticket.TrangThai is "DA_TRA" or "HUY" or "HOAN_THANH")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu không còn nhận công việc mới."));
        if (!await db.KyThuatViens.AsNoTracking().AnyAsync(x => x.NhanVienId == req.KyThuatVienId
            && x.NhanVien.DeletedAt == null && x.NhanVien.TaiKhoan.HoatDong))
            return BadRequest(ApiResponse<object>.ErrorResponse("Kỹ thuật viên không hoạt động."));
        if (req.PhieuBaoHanhId.HasValue && !await db.PhieuBaoHanhs.AsNoTracking().AnyAsync(x =>
            x.Id == req.PhieuBaoHanhId && (x.PhieuGocId == req.PhieuId || x.PhieuSuaBaoHanhId == req.PhieuId)))
            return BadRequest(ApiResponse<object>.ErrorResponse("Hồ sơ bảo hành không liên kết với phiếu sửa này."));
        var row = new CongViecSuaChua {
            PhieuId = req.PhieuId, PhieuBaoHanhId = req.PhieuBaoHanhId,
            MoTa = req.MoTa.Trim(), KyThuatVienId = req.KyThuatVienId,
            TrangThai = "CHO_LAM", TaoBoiId = actor.Value, TaoLuc = DateTime.UtcNow
        };
        db.CongViecSuaChuas.Add(row);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("{id:long}/huy")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> Huy(long id, [FromBody] HuyCongViecRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần lý do hủy công việc."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.CongViecSuaChuas.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai is not ("CHO_LAM" or "DANG_LAM") || row.CongDeXuat != null)
            return Conflict(ApiResponse<object>.ErrorResponse("Chỉ hủy việc chưa hoàn thành và chưa đề xuất công."));
        row.TrangThai = "HUY";
        row.KetQua = $"Hủy: {req.LyDo.Trim()}";
        db.AuditLogs.Add(new AuditLog {
            Bang = "cong_viec_sua_chua", BanGhiId = id, HanhDong = "HUY",
            DuLieuMoi = JsonSerializer.Serialize(new { LyDo = req.LyDo.Trim(), NguoiHuyId = actor }),
            NguoiThucHienId = User.GetTaiKhoanId()
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("{id:long}/bat-dau")]
    [Authorize(Roles = Roles.KyThuatVien)]
    public async Task<IActionResult> BatDau(long id)
    {
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.CongViecSuaChuas.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.KyThuatVienId != actor) return Forbid();
        if (row.TrangThai != "CHO_LAM")
            return Conflict(ApiResponse<object>.ErrorResponse("Công việc không còn ở trạng thái chờ làm."));
        var ticket = await db.PhieuSuaChuas.AsNoTracking().FirstAsync(x => x.Id == row.PhieuId);
        if (ticket.TrangThai != "DANG_SUA")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu phải ở trạng thái đang sửa."));
        row.TrangThai = "DANG_LAM";
        row.BatDauLuc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("{id:long}/hoan-thanh")]
    [Authorize(Roles = Roles.KyThuatVien)]
    public async Task<IActionResult> HoanThanh(long id, [FromBody] HoanThanhCongViecRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.KetQua) || req.KetQua.Length > 1000
            || (req.ChiTietXuatKhoIds?.Count ?? 0) > 50
            || (req.ChiTietXuatKhoIds?.Distinct().Count() ?? 0) != (req.ChiTietXuatKhoIds?.Count ?? 0))
            return BadRequest(ApiResponse<object>.ErrorResponse("Kết quả hoặc danh sách linh kiện không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.CongViecSuaChuas.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.KyThuatVienId != actor) return Forbid();
        if (row.TrangThai != "DANG_LAM")
            return Conflict(ApiResponse<object>.ErrorResponse("Công việc chưa bắt đầu hoặc đã kết thúc."));
        var ticket = await db.PhieuSuaChuas.AsNoTracking().FirstAsync(x => x.Id == row.PhieuId);
        if (ticket.TrangThai is not ("DANG_SUA" or "CHO_LINH_KIEN"))
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu không còn ở giai đoạn sửa."));
        var ids = req.ChiTietXuatKhoIds ?? [];
        if (ids.Count > 0)
        {
            var issued = await db.ChiTietXuatKhos.AsNoTracking().Where(x =>
                ids.Contains(x.Id) && x.PhieuId == row.PhieuId)
                .Select(x => new { x.Id, x.SoLuongXuat }).ToListAsync();
            var returned = await db.TraLinhKienKhos.AsNoTracking()
                .Where(x => ids.Contains(x.ChiTietXuatKhoId))
                .GroupBy(x => x.ChiTietXuatKhoId)
                .Select(g => new { Id = g.Key, Quantity = g.Sum(x => x.SoLuong) }).ToListAsync();
            if (issued.Count != ids.Count || issued.Any(x => x.SoLuongXuat <=
                    (returned.FirstOrDefault(r => r.Id == x.Id)?.Quantity ?? 0))
                || await db.CongViecVatTus.AsNoTracking()
                .AnyAsync(x => ids.Contains(x.ChiTietXuatKhoId)))
                return Conflict(ApiResponse<object>.ErrorResponse(
                    "Linh kiện không thuộc phiếu, đã trả hết hoặc đã gắn công việc khác."));
            foreach (var partId in ids)
                db.CongViecVatTus.Add(new CongViecVatTu {
                    CongViecId = id, ChiTietXuatKhoId = partId });
        }
        row.KetQua = req.KetQua.Trim();
        row.HoanThanhLuc = DateTime.UtcNow;
        row.TrangThai = "HOAN_THANH";
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("{id:long}/de-xuat-cong")]
    [Authorize(Roles = Roles.KyThuatVien)]
    public async Task<IActionResult> DeXuatCong(long id, [FromBody] DeXuatCongRequest req)
    {
        if (req.SoTien < 0 || req.SoTien > 999_999_999_999.99m)
            return BadRequest(ApiResponse<object>.ErrorResponse("Số tiền công không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var row = await db.CongViecSuaChuas.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.KyThuatVienId != actor) return Forbid();
        if (row.TrangThai != "HOAN_THANH" || row.CongDuyet != null)
            return Conflict(ApiResponse<object>.ErrorResponse("Công việc chưa hoàn thành hoặc đã duyệt công."));
        row.CongDeXuat = req.SoTien;
        row.CongDeXuatBoiId = actor;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.CongDeXuat }));
    }

    [HttpPost("{id:long}/duyet-cong")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DuyetCong(long id, [FromBody] DuyetCongRequest req)
    {
        if (req.SoTien < 0 || req.SoTien > 999_999_999_999.99m
            || string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần số công được duyệt và lý do."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.CongViecSuaChuas.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "HOAN_THANH" || row.CongDeXuat == null || row.CongDuyet != null)
            return Conflict(ApiResponse<object>.ErrorResponse("Chưa có đề xuất công hợp lệ hoặc đã duyệt."));
        if (row.CongDeXuatBoiId == actor || row.KyThuatVienId == actor)
            return Conflict(ApiResponse<object>.ErrorResponse("Người đề xuất hoặc người làm không được tự duyệt công."));
        row.CongDuyet = req.SoTien;
        row.CongDuyetBoiId = actor;
        row.CongDuyetLuc = DateTime.UtcNow;
        row.LyDoDuyetCong = req.LyDo.Trim();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.CongDuyet }));
    }

    [HttpPost("{id:long}/chi-tra")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ChiTra(long id, [FromBody] ChiTraCongRequest req)
    {
        if (req.PhuongThuc is not ("TIEN_MAT" or "CHUYEN_KHOAN")
            || string.IsNullOrWhiteSpace(req.MaThamChieu) || req.MaThamChieu.Length > 100)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần phương thức và mã chứng từ chi trả."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.CongViecSuaChuas.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.CongDuyet == null || row.ChiTraLuc != null)
            return Conflict(ApiResponse<object>.ErrorResponse("Công chưa duyệt hoặc đã ghi chi trả."));
        row.ChiTraLuc = DateTime.UtcNow;
        row.ChiTraBoiId = actor;
        row.PhuongThucChiTra = req.PhuongThuc;
        row.MaThamChieuChiTra = req.MaThamChieu.Trim();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.ChiTraLuc }));
    }
}

public sealed record TaoCongViecRequest(long PhieuId, long KyThuatVienId,
    long? PhieuBaoHanhId, string? MoTa);
public sealed record HoanThanhCongViecRequest(string? KetQua, List<long>? ChiTietXuatKhoIds);
public sealed record DeXuatCongRequest(decimal SoTien);
public sealed record DuyetCongRequest(decimal SoTien, string? LyDo);
public sealed record ChiTraCongRequest(string? PhuongThuc, string? MaThamChieu);
public sealed record HuyCongViecRequest(string? LyDo);
