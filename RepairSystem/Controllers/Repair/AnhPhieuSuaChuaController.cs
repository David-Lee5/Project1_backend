using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/phieu-sua-chua/{phieuId:long}/anh")]
[Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
public class AnhPhieuSuaChuaController(AppDbContextV3 db, IWebHostEnvironment env) : ControllerBase
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private string Folder => Path.Combine(env.ContentRootPath, "App_Data", "repair-photos");

    [HttpGet]
    public async Task<IActionResult> DanhSach(long phieuId)
    {
        if (!await DuocXem(phieuId)) return Forbid();
        var rows = await db.AnhPhieuSuaChuas.AsNoTracking().Where(x => x.PhieuId == phieuId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new { x.Id, x.Loai, x.MoTa, x.CreatedAt }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Xem(long phieuId, long id)
    {
        if (!await DuocXem(phieuId)) return Forbid();
        var row = await db.AnhPhieuSuaChuas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.PhieuId == phieuId);
        if (row == null) return NotFound();
        var name = Path.GetFileName(row.DuongDan);
        var path = Path.Combine(Folder, name);
        if (!System.IO.File.Exists(path)) return NotFound();
        var mime = Path.GetExtension(name) switch {
            ".png" => "image/png", ".webp" => "image/webp", _ => "image/jpeg"
        };
        return PhysicalFile(path, mime);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> TaiLen(long phieuId, IFormFile? file,
        [FromForm] string loai = "KHAC", [FromForm] string? moTa = null)
    {
        if (!await DuocXem(phieuId)) return Forbid();
        if (loai is not ("KHAC" or "LUC_TIEP_NHAN" or "SAU_SUA") || moTa?.Length > 255
            || file == null || file.Length == 0 || file.Length > MaxBytes)
            return BadRequest(ApiResponse<object>.ErrorResponse("Ảnh, loại hoặc mô tả không hợp lệ (tối đa 5 MB)."));
        if (await db.AnhPhieuSuaChuas.CountAsync(x => x.PhieuId == phieuId) >= 20)
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu đã có tối đa 20 ảnh."));
        if (User.IsInRole(Roles.KyThuatVien) && loai == "LUC_TIEP_NHAN")
            return Forbid();

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream((int)file.Length);
        await input.CopyToAsync(buffer);
        var bytes = buffer.ToArray();
        if (bytes.Length < 12)
            return BadRequest(ApiResponse<object>.ErrorResponse("File ảnh không hợp lệ."));
        var extension = SignatureExtension(bytes);
        if (extension == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ nhận JPEG, PNG hoặc WebP."));
        Directory.CreateDirectory(Folder);
        var name = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(Folder, name);
        try
        {
            await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                await output.WriteAsync(bytes);
            var row = new AnhPhieuSuaChua {
                PhieuId = phieuId, Loai = loai, MoTa = moTa?.Trim(), DuongDan = name
            };
            db.AnhPhieuSuaChuas.Add(row);
            await db.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.Loai, row.MoTa }));
        }
        catch
        {
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
            throw;
        }
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> Xoa(long phieuId, long id)
    {
        var row = await db.AnhPhieuSuaChuas.FirstOrDefaultAsync(x => x.Id == id && x.PhieuId == phieuId);
        if (row == null) return NotFound();
        if (await db.PhieuSuaChuas.AnyAsync(x => x.Id == phieuId && x.TrangThai == "DA_TRA"))
            return Conflict(ApiResponse<object>.ErrorResponse("Không xóa ảnh sau khi bàn giao."));
        db.AnhPhieuSuaChuas.Remove(row);
        await db.SaveChangesAsync();
        var path = Path.Combine(Folder, Path.GetFileName(row.DuongDan));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id }));
    }

    private async Task<bool> DuocXem(long phieuId)
    {
        var ticket = await db.PhieuSuaChuas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == phieuId);
        if (ticket == null) return false;
        if (!User.IsInRole(Roles.KyThuatVien)) return true;
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        return ticket.KyThuatVienId == actor;
    }

    private static string? SignatureExtension(byte[] h)
    {
        if (h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF) return ".jpg";
        if (h.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })) return ".png";
        if (System.Text.Encoding.ASCII.GetString(h, 0, 4) == "RIFF"
            && System.Text.Encoding.ASCII.GetString(h, 8, 4) == "WEBP") return ".webp";
        return null;
    }
}
