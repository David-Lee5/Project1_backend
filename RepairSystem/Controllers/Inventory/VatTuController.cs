using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/phieu-sua-chua/{phieuId:long}/vat-tu")]
[Authorize(Roles = Roles.NhanVien)]
public class VatTuController(RepairDbContext db) : ControllerBase
{
    /// <summary>Phiếu có yêu cầu chờ duyệt hoặc số lượng đã duyệt chưa được xuất đủ.</summary>
    [HttpGet("/api/v3/vat-tu/phieu-can-xu-ly")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> PhieuCanXuLy()
    {
        var lines = await db.ChiTietYeuCauLinhKiens.AsNoTracking()
            .Where(c => (c.YeuCauLinhKien.Phieu.TrangThai == "DANG_SUA"
                    || c.YeuCauLinhKien.Phieu.TrangThai == "CHO_LINH_KIEN")
                && (c.YeuCauLinhKien.TrangThai == "CHO_DUYET"
                    || c.YeuCauLinhKien.TrangThai == "DA_DUYET"))
            .Select(c => new { c.Id, c.PhieuId, c.SoLuongDuyet,
                TrangThai = c.YeuCauLinhKien.TrangThai })
            .ToListAsync();
        var issued = await SoLuongDaXuatAsync(lines.Where(c => c.TrangThai == "DA_DUYET")
            .Select(c => c.Id).ToList());
        var pendingIds = lines.Where(c => c.TrangThai == "CHO_DUYET"
                || c.SoLuongDuyet.GetValueOrDefault() > issued.GetValueOrDefault(c.Id))
            .Select(c => c.PhieuId).Distinct().ToList();
        return Ok(ApiResponse<List<long>>.SuccessResponse(pendingIds));
    }

    [HttpGet("yeu-cau")]
    public async Task<IActionResult> DanhSach(long phieuId)
    {
        if (!await DuocXemPhieu(phieuId)) return Forbid();
        var rows = await db.YeuCauLinhKiens.AsNoTracking()
            .Where(x => x.PhieuId == phieuId).Include(x => x.ChiTietYeuCauLinhKiens)
            .OrderByDescending(x => x.NgayYeuCau).ToListAsync();
        var issued = await SoLuongDaXuatAsync(rows.SelectMany(x => x.ChiTietYeuCauLinhKiens)
            .Select(c => c.Id).ToList());
        return Ok(ApiResponse<object>.SuccessResponse(rows.Select(x => new {
            x.Id, x.PhieuId, x.NguoiYeuCauId, x.NgayYeuCau, x.TrangThai,
            x.NguoiDuyetId, x.NgayDuyet, x.GhiChu,
            ChiTiet = x.ChiTietYeuCauLinhKiens.Select(c => new {
                c.Id, c.LinhKienId, c.SoLuongYeuCau, c.SoLuongDuyet, c.GhiChu,
                SoLuongDaXuat = issued.GetValueOrDefault(c.Id)
            })
        }).ToList()));
    }

    private async Task<Dictionary<long, int>> SoLuongDaXuatAsync(List<long> detailIds)
    {
        if (detailIds.Count == 0) return new Dictionary<long, int>();
        var rows = await db.ChiTietXuatKhos.AsNoTracking()
            .Where(x => x.YeuCauChiTietId.HasValue
                && detailIds.Contains(x.YeuCauChiTietId.Value))
            .Select(x => new { DetailId = x.YeuCauChiTietId!.Value, x.SoLuongXuat })
            .ToListAsync();
        return rows.GroupBy(x => x.DetailId)
            .ToDictionary(group => group.Key, group => group.Sum(x => x.SoLuongXuat));
    }

    [HttpPost("yeu-cau")]
    [Authorize(Roles = Roles.Admin + "," + Roles.KyThuatVien)]
    public async Task<IActionResult> TaoYeuCau(long phieuId, [FromBody] TaoYeuCauVatTuRequest req)
    {
        if (req.ChiTiet == null || req.ChiTiet.Count == 0
            || req.ChiTiet.Any(x => x.LinhKienId <= 0 || x.SoLuong <= 0 || x.GhiChu?.Length > 500)
            || req.ChiTiet.Select(x => x.LinhKienId).Distinct().Count() != req.ChiTiet.Count
            || req.GhiChu?.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Chi tiết yêu cầu vật tư không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var phieu = await db.PhieuSuaChuas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == phieuId);
        if (phieu == null) return NotFound();
        if (!User.IsInRole(Roles.Admin) && phieu.KyThuatVienId != actor
            && !await db.CongViecSuaChuas.AsNoTracking().AnyAsync(x =>
                x.PhieuId == phieuId && x.KyThuatVienId == actor && x.TrangThai != "HUY"))
            return Forbid();
        if (phieu.TrangThai is not ("DANG_SUA" or "CHO_LINH_KIEN"))
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu không ở trạng thái được yêu cầu vật tư."));
        var ids = req.ChiTiet.Select(x => x.LinhKienId).ToList();
        if (await db.LinhKiens.CountAsync(x => ids.Contains(x.Id) && x.DeletedAt == null) != ids.Count)
            return BadRequest(ApiResponse<object>.ErrorResponse("Có linh kiện không tồn tại."));
        var row = new YeuCauLinhKien {
            PhieuId = phieuId, NguoiYeuCauId = actor.Value,
            TrangThai = "CHO_DUYET", GhiChu = req.GhiChu?.Trim()
        };
        foreach (var c in req.ChiTiet)
            row.ChiTietYeuCauLinhKiens.Add(new ChiTietYeuCauLinhKien {
                PhieuId = phieuId, LinhKienId = c.LinhKienId,
                SoLuongYeuCau = c.SoLuong, GhiChu = c.GhiChu?.Trim()
            });
        db.YeuCauLinhKiens.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("yeu-cau/{id:long}/duyet")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> Duyet(long phieuId, long id, [FromBody] DuyetYeuCauVatTuRequest req)
    {
        if (req.QuyetDinh is not ("DA_DUYET" or "TU_CHOI"))
            return BadRequest(ApiResponse<object>.ErrorResponse("Quyết định không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.YeuCauLinhKiens.Include(x => x.ChiTietYeuCauLinhKiens)
            .FirstOrDefaultAsync(x => x.Id == id && x.PhieuId == phieuId);
        if (row == null) return NotFound();
        if (row.TrangThai != "CHO_DUYET")
            return Conflict(ApiResponse<object>.ErrorResponse("Yêu cầu đã được xử lý."));
        if (row.NguoiYeuCauId == actor.Value)
            return Conflict(ApiResponse<object>.ErrorResponse("Người yêu cầu không được tự duyệt vật tư."));
        if (req.QuyetDinh == "DA_DUYET")
        {
            if (req.SoLuongDuyet == null || req.SoLuongDuyet.Count != row.ChiTietYeuCauLinhKiens.Count
                || req.SoLuongDuyet.Select(x => x.ChiTietId).Distinct().Count() != req.SoLuongDuyet.Count)
                return BadRequest(ApiResponse<object>.ErrorResponse("Cần duyệt số lượng từng dòng."));
            foreach (var detail in row.ChiTietYeuCauLinhKiens)
            {
                var selected = req.SoLuongDuyet.FirstOrDefault(x => x.ChiTietId == detail.Id);
                if (selected == null || selected.SoLuong < 0 || selected.SoLuong > detail.SoLuongYeuCau)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Số lượng duyệt không hợp lệ."));
                detail.SoLuongDuyet = selected.SoLuong;
            }
        }
        row.TrangThai = req.QuyetDinh;
        row.NguoiDuyetId = actor;
        row.NgayDuyet = DateTime.UtcNow;
        row.GhiChu = req.GhiChu?.Trim();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("tra-kho")]
    [Authorize(Roles = Roles.ThuKho)]
    public async Task<IActionResult> TraKho(long phieuId, [FromBody] TraVatTuRequest req)
    {
        if (req.ChiTietXuatKhoId <= 0 || req.SoLuong <= 0
            || string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin trả kho không hợp lệ."));
        var row = await db.ChiTietXuatKhos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.ChiTietXuatKhoId && x.PhieuId == phieuId);
        if (row == null) return NotFound();
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null || !await db.ThuKhos.AnyAsync(x => x.NhanVienId == actor)) return Forbid();
        var outId = new SqlParameter("@tra_id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        try
        {
            await db.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_v3_tra_linh_kien @chi_tiet_xuat_kho_id, @so_luong, @thu_kho_id, @ly_do, @tra_id OUTPUT",
                new SqlParameter("@chi_tiet_xuat_kho_id", req.ChiTietXuatKhoId),
                new SqlParameter("@so_luong", req.SoLuong),
                new SqlParameter("@thu_kho_id", actor.Value),
                new SqlParameter("@ly_do", req.LyDo.Trim()), outId);
        }
        catch (SqlException ex) when (ex.Number is >= 50000 and < 52000)
        {
            return Conflict(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        return Ok(ApiResponse<object>.SuccessResponse(new { TraKhoId = (long)outId.Value }));
    }

    private async Task<bool> DuocXemPhieu(long phieuId)
    {
        var phieu = await db.PhieuSuaChuas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == phieuId);
        if (phieu == null) return false;
        if (!User.IsInRole(Roles.KyThuatVien)) return true;
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        return phieu.KyThuatVienId == actor || await db.CongViecSuaChuas.AsNoTracking()
            .AnyAsync(x => x.PhieuId == phieuId && x.KyThuatVienId == actor && x.TrangThai != "HUY");
    }
}

public sealed record TaoYeuCauVatTuRequest(string? GhiChu, List<DongYeuCauVatTuRequest>? ChiTiet);
public sealed record DongYeuCauVatTuRequest(long LinhKienId, int SoLuong, string? GhiChu);
public sealed record DuyetYeuCauVatTuRequest(string QuyetDinh, string? GhiChu,
    List<DongDuyetVatTuRequest>? SoLuongDuyet);
public sealed record DongDuyetVatTuRequest(long ChiTietId, int SoLuong);
public sealed record TraVatTuRequest(long ChiTietXuatKhoId, int SoLuong, string? LyDo);
