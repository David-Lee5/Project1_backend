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
}

public sealed record TaoKhoRequest(string? TenKho, string? DiaDiem, long? ThuKhoId);
