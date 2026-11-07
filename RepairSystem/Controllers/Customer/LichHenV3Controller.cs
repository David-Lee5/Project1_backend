using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/lich-hen")]
[Authorize]
public class LichHenV3Controller(AppDbContextV3 db) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = Roles.KhachHang)]
    public async Task<IActionResult> DatLich([FromBody] DatLichHenRequest req)
    {
        if (req.ThoiGianMongMuon.UtcDateTime <= DateTime.UtcNow.AddHours(1)
            || string.IsNullOrWhiteSpace(req.TenThietBi) || req.TenThietBi.Length > 150
            || string.IsNullOrWhiteSpace(req.MoTaLoi) || req.MoTaLoi.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin lịch hẹn không hợp lệ."));
        var customerId = await KhachHangId();
        if (customerId == null) return Forbid();
        var row = new LichHenV3 {
            KhachHangId = customerId.Value, ThoiGianMongMuon = req.ThoiGianMongMuon.UtcDateTime,
            TenThietBi = req.TenThietBi.Trim(), MoTaLoi = req.MoTaLoi.Trim(),
            TrangThai = "CHO_XAC_NHAN"
        };
        db.LichHenV3s.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpGet("me")]
    [Authorize(Roles = Roles.KhachHang)]
    public async Task<IActionResult> LichCuaToi()
    {
        var customerId = await KhachHangId();
        if (customerId == null) return Forbid();
        var rows = await db.LichHenV3s.AsNoTracking().Where(x => x.KhachHangId == customerId)
            .OrderByDescending(x => x.CreatedAt).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("{id:long}/huy")]
    [Authorize(Roles = Roles.KhachHang)]
    public async Task<IActionResult> Huy(long id)
    {
        var customerId = await KhachHangId();
        if (customerId == null) return Forbid();
        var row = await db.LichHenV3s.FirstOrDefaultAsync(x => x.Id == id && x.KhachHangId == customerId);
        if (row == null) return NotFound();
        if (row.TrangThai is not ("CHO_XAC_NHAN" or "DA_XAC_NHAN") || row.ThoiGianMongMuon <= DateTime.UtcNow)
            return Conflict(ApiResponse<object>.ErrorResponse("Lịch hẹn không thể hủy trực tuyến."));
        row.TrangThai = "HUY";
        row.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> DanhSach([FromQuery] string? trangThai)
    {
        var query = db.LichHenV3s.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(trangThai)) query = query.Where(x => x.TrangThai == trangThai);
        var rows = await query.OrderByDescending(x => x.CreatedAt)
            .Select(x => new { x.Id, x.KhachHangId, x.ThoiGianMongMuon, x.TenThietBi,
                x.MoTaLoi, x.TrangThai, x.GhiChu, x.PhieuId, x.CreatedAt,
                TenKhachHang = db.KhachHangs.Where(k => k.Id == x.KhachHangId).Select(k => k.HoTen).FirstOrDefault(),
                SoDienThoai = db.KhachHangs.Where(k => k.Id == x.KhachHangId).Select(k => k.SoDienThoai).FirstOrDefault()
            }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPatch("{id:long}/trang-thai")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> DoiTrangThai(long id, [FromBody] DoiTrangThaiLichHenRequest req)
    {
        if (req.TrangThai is not ("DA_XAC_NHAN" or "TU_CHOI" or "DA_TIEP_NHAN")
            || req.GhiChu?.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Trạng thái hoặc ghi chú không hợp lệ."));
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.LichHenV3s.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (!((row.TrangThai == "CHO_XAC_NHAN" && req.TrangThai is "DA_XAC_NHAN" or "TU_CHOI")
            || (row.TrangThai == "DA_XAC_NHAN" && req.TrangThai == "DA_TIEP_NHAN")))
            return Conflict(ApiResponse<object>.ErrorResponse("Bước chuyển trạng thái không hợp lệ."));
        if (req.TrangThai == "DA_TIEP_NHAN")
        {
            if (req.PhieuId is not > 0 || !await db.PhieuSuaChuas.AnyAsync(x =>
                x.Id == req.PhieuId && x.ThietBi.KhachHangId == row.KhachHangId))
                return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu sửa chữa không thuộc khách đặt lịch."));
            row.PhieuId = req.PhieuId;
        }
        else if (req.PhieuId != null)
            return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ gắn phiếu khi đã tiếp nhận."));
        row.TrangThai = req.TrangThai;
        row.GhiChu = req.GhiChu?.Trim();
        row.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai, row.PhieuId }));
    }

    private async Task<long?> KhachHangId()
    {
        var accountId = User.GetTaiKhoanId();
        if (accountId == null) return null;
        return await db.KhachHangs.AsNoTracking()
            .Where(x => x.TaiKhoanId == accountId && x.DeletedAt == null)
            .Select(x => (long?)x.Id).FirstOrDefaultAsync();
    }
}

public sealed record DatLichHenRequest(DateTimeOffset ThoiGianMongMuon, string? TenThietBi, string? MoTaLoi);
public sealed record DoiTrangThaiLichHenRequest(string TrangThai, string? GhiChu, long? PhieuId);
