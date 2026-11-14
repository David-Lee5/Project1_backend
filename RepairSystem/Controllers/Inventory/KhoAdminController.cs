using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/kho")]
[Authorize(Roles = Roles.Admin)]
public class KhoAdminController(RepairDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Tao([FromBody] TaoKhoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.TenKho) || req.TenKho.Trim().Length > 100
            || req.DiaDiem?.Length > 255)
            return BadRequest(ApiResponse<object>.ErrorResponse("Tên kho hoặc địa điểm không hợp lệ."));
        if (req.ThuKhoId.HasValue && !await db.ThuKhos.AnyAsync(x => x.NhanVienId == req.ThuKhoId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Nhân viên được chọn không phải thủ kho."));
        var kho = new Kho {
            TenKho = req.TenKho.Trim(), DiaDiem = req.DiaDiem?.Trim(),
            ThuKhoId = req.ThuKhoId
        };
        db.Khos.Add(kho);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { kho.Id, kho.MaKho, kho.TenKho }));
    }

    /// <summary>Chỉ xóa mềm kho khi đã hết hàng và không còn nghiệp vụ kho đang xử lý.</summary>
    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> Xoa(long id)
    {
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (!await db.Khos.AnyAsync(k => k.Id == id && k.DeletedAt == null))
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy kho id={id}."));

        if (await db.TonKhos.AnyAsync(t => t.KhoId == id && t.SoLuongTon > 0)
            || await db.LinhKienSerials.AnyAsync(s => s.KhoId == id
                && (s.TrangThai == "TRONG_KHO" || s.TrangThai == "CACH_LY" || s.TrangThai == "THIEU_KIEM_KE")))
            return Conflict(ApiResponse<bool>.ErrorResponse(
                "Kho vẫn còn hàng hoặc serial chưa xử lý. Hãy xử lý hết trước khi xóa kho."));

        if (await db.PhieuNhaps.AnyAsync(p => p.KhoId == id && p.TrangThai == "NHAP"))
            return Conflict(ApiResponse<bool>.ErrorResponse(
                "Kho còn phiếu nhập nháp. Hãy xác nhận hoặc hủy phiếu trước khi xóa kho."));

        if (await db.KiemKeKhos.AnyAsync(p => p.KhoId == id && p.TrangThai == "CHO_DUYET"))
            return Conflict(ApiResponse<bool>.ErrorResponse(
                "Kho còn phiếu kiểm kê chờ duyệt. Hãy xử lý phiếu trước khi xóa kho."));

        if (await db.CachLyVatTus.AnyAsync(p => p.KhoId == id && p.TrangThai == "CACH_LY"))
            return Conflict(ApiResponse<bool>.ErrorResponse(
                "Kho còn vật tư cách ly chưa xử lý. Hãy xử lý vật tư trước khi xóa kho."));

        if (await db.PhieuBaoHanhNccs.AnyAsync(p => p.KhoGuiId == id
            && (p.TrangThai == "DA_GUI" || (p.TrangThai == "DA_NHAN_LAI"
                && !db.XuLyBaoHanhNccs.Any(x => x.PhieuBaoHanhNccId == p.Id)))))
            return Conflict(ApiResponse<bool>.ErrorResponse(
                "Kho còn hồ sơ bảo hành nhà cung cấp đang xử lý. Hãy hoàn tất trước khi xóa kho."));

        var now = DateTime.UtcNow;
        var affected = await db.Khos
            .Where(k => k.Id == id && k.DeletedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(k => k.DeletedAt, now)
                .SetProperty(k => k.UpdatedAt, now));

        if (affected == 0)
            return NotFound(ApiResponse<bool>.ErrorResponse($"Không tìm thấy kho id={id}."));

        await tx.CommitAsync();
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Đã xóa kho khỏi danh sách sử dụng."));
    }
}

public sealed record TaoKhoRequest(string? TenKho, string? DiaDiem, long? ThuKhoId);
