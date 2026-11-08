using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/phieu-sua-chua/{phieuId:long}/xac-nhan-tiep-nhan")]
[Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
public sealed class XacNhanTiepNhanV3Controller(AppDbContextV3 db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(long phieuId)
    {
        var row = await db.XacNhanTiepNhanV3s.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PhieuId == phieuId);
        return row == null ? NotFound() : Ok(ApiResponse<object>.SuccessResponse(row));
    }

    // Ảnh biên nhận đã ký được lưu bằng API ảnh hiện có với loại KHAC.
    // Đây là ghi nhận chứng từ giấy; không phải chữ ký số hay xác thực OTP.
    [HttpPost]
    public async Task<IActionResult> Post(long phieuId, [FromBody] XacNhanTiepNhanRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NguoiXacNhan) || req.NguoiXacNhan.Length > 150
            || req.AnhBienNhanId <= 0)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần người xác nhận và ảnh biên nhận đã ký."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var ticket = await db.PhieuSuaChuas.Include(x => x.ThietBi)
            .FirstOrDefaultAsync(x => x.Id == phieuId);
        if (ticket == null) return NotFound();
        if (!ticket.ApDungKiemSoatTiepNhan || ticket.TrangThai != "TIEP_NHAN"
            || ticket.KtvNhanLuc != null)
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu không còn ở giai đoạn xác nhận tiếp nhận."));
        if (await db.XacNhanTiepNhanV3s.AnyAsync(x => x.PhieuId == phieuId))
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu đã được xác nhận tiếp nhận."));
        if (string.IsNullOrWhiteSpace(ticket.TinhTrangNgoaiQuan)
            || string.IsNullOrWhiteSpace(ticket.PhuKienKemTheo)
            || string.IsNullOrWhiteSpace(ticket.ChecklistTiepNhanJson)
            || (string.IsNullOrWhiteSpace(ticket.ThietBi.SoSerial)
                && string.IsNullOrWhiteSpace(ticket.LyDoKhongSerial)))
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu thiếu thông tin tình trạng, phụ kiện hoặc định danh thiết bị."));
        var minPhotos = await db.QuyTacVanHanhV3s.AsNoTracking()
            .Where(x => x.Id == 1).Select(x => x.AnhTiepNhanToiThieu).SingleAsync();
        if (await db.AnhPhieuSuaChuas.CountAsync(x => x.PhieuId == phieuId && x.Loai == "LUC_TIEP_NHAN") < minPhotos)
            return Conflict(ApiResponse<object>.ErrorResponse($"Cần ít nhất {minPhotos} ảnh lúc tiếp nhận."));
        if (!await db.AnhPhieuSuaChuas.AnyAsync(x => x.Id == req.AnhBienNhanId
            && x.PhieuId == phieuId && x.Loai == "KHAC"
            && x.NguoiTaiLenId == actor.Value && x.Sha256 != null))
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Ảnh biên nhận phải thuộc phiếu này, do người ghi nhận tải lên và có loại KHAC."));
        var row = new XacNhanTiepNhanV3 {
            PhieuId = phieuId, NguoiXacNhan = req.NguoiXacNhan.Trim(),
            AnhBienNhanId = req.AnhBienNhanId, NhanVienGhiNhanId = actor.Value,
            XacNhanLuc = DateTime.UtcNow
        };
        db.XacNhanTiepNhanV3s.Add(row);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(row));
    }
}

public sealed record XacNhanTiepNhanRequest(string? NguoiXacNhan, long AnhBienNhanId);
