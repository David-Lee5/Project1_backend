using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/bao-hanh")]
[Authorize(Roles = Roles.NhanVien)]
public class BaoHanhV3Controller(AppDbContextV3 db) : ControllerBase
{
    private static readonly Dictionary<string, string[]> BuocKhach = new() {
        ["TIEP_NHAN"] = ["DANG_KIEM_TRA"],
        ["DANG_KIEM_TRA"] = ["CHAP_NHAN", "TU_CHOI"],
        ["CHAP_NHAN"] = ["DANG_XU_LY"],
        ["DANG_XU_LY"] = ["HOAN_THANH"],
        ["HOAN_THANH"] = ["DA_TRA"]
    };

    [HttpGet("cam-ket/{phieuGocId:long}")]
    public async Task<IActionResult> CamKet(long phieuGocId)
    {
        var rows = await db.CamKetBaoHanhs.AsNoTracking()
            .Where(x => x.PhieuGocId == phieuGocId).OrderBy(x => x.NgayKetThuc)
            .Select(x => new { x.Id, x.Loai, x.ChiTietXuatKhoId, x.DichVuId,
                x.NgayBatDau, x.NgayKetThuc, x.DieuKien }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("cam-ket")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> TaoCamKet([FromBody] TaoCamKetBaoHanhRequest req)
    {
        if (req.NgayKetThuc < req.NgayBatDau || req.DieuKien?.Length > 1000
            || !((req.Loai == "DICH_VU" && req.DichVuId > 0 && req.ChiTietXuatKhoId == null)
                 || (req.Loai == "LINH_KIEN" && req.ChiTietXuatKhoId > 0 && req.DichVuId == null)))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cam kết bảo hành không hợp lệ."));
        var phieu = await db.PhieuSuaChuas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == req.PhieuGocId);
        if (phieu == null || phieu.TrangThai != "DA_TRA")
            return Conflict(ApiResponse<object>.ErrorResponse("Chỉ tạo cam kết cho phiếu đã bàn giao."));
        var row = new CamKetBaoHanh {
            PhieuGocId = req.PhieuGocId, Loai = req.Loai,
            DichVuId = req.DichVuId, ChiTietXuatKhoId = req.ChiTietXuatKhoId,
            NgayBatDau = req.NgayBatDau, NgayKetThuc = req.NgayKetThuc,
            DieuKien = req.DieuKien?.Trim()
        };
        db.CamKetBaoHanhs.Add(row);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException) { return Conflict(ApiResponse<object>.ErrorResponse("Hạng mục bảo hành không thuộc phiếu hoặc đã có cam kết.")); }
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id }));
    }

    [HttpGet("khach")]
    public async Task<IActionResult> DanhSachKhach([FromQuery] long? phieuGocId)
    {
        var q = db.PhieuBaoHanhs.AsNoTracking().AsQueryable();
        if (phieuGocId.HasValue) q = q.Where(x => x.PhieuGocId == phieuGocId);
        var rows = await q.OrderByDescending(x => x.NgayTiepNhan)
            .Select(x => new { x.Id, x.PhieuGocId, x.ThietBiId, x.CamKetId,
                x.PhieuSuaBaoHanhId, x.NgayTiepNhan, x.LoiKhachBao,
                x.TrangThai, x.KetLuan, x.KetQuaXuLy, x.NgayHoanThanh }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("khach")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<IActionResult> TiepNhanKhach([FromBody] TiepNhanBaoHanhRequest req)
    {
        if (req.PhieuGocId <= 0 || string.IsNullOrWhiteSpace(req.LoiKhachBao)
            || req.LoiKhachBao.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Thông tin tiếp nhận bảo hành không hợp lệ."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var phieu = await db.PhieuSuaChuas.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == req.PhieuGocId);
        if (phieu == null || phieu.TrangThai != "DA_TRA")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu gốc chưa bàn giao hoặc không tồn tại."));
        if (req.CamKetId.HasValue)
        {
            var camKet = await db.CamKetBaoHanhs.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == req.CamKetId && x.PhieuGocId == req.PhieuGocId);
            if (camKet == null) return BadRequest(ApiResponse<object>.ErrorResponse("Cam kết không thuộc phiếu gốc."));
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            if (today < camKet.NgayBatDau || today > camKet.NgayKetThuc)
                return BadRequest(ApiResponse<object>.ErrorResponse("Cam kết chưa có hiệu lực hoặc đã hết hạn."));
        }
        if (req.PhieuSuaBaoHanhId.HasValue
            && !await db.PhieuSuaChuas.AnyAsync(x => x.Id == req.PhieuSuaBaoHanhId
                && x.ThietBiId == phieu.ThietBiId && x.Id != phieu.Id))
            return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu sửa bảo hành không cùng thiết bị."));
        var row = new PhieuBaoHanh {
            PhieuGocId = phieu.Id, ThietBiId = phieu.ThietBiId,
            CamKetId = req.CamKetId, PhieuSuaBaoHanhId = req.PhieuSuaBaoHanhId,
            NguoiTiepNhanId = actor.Value, LoiKhachBao = req.LoiKhachBao.Trim(),
            TrangThai = "TIEP_NHAN"
        };
        db.PhieuBaoHanhs.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("khach/{id:long}/trang-thai")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KyThuatVien)]
    public async Task<IActionResult> DoiTrangThaiKhach(long id, [FromBody] DoiTrangThaiBaoHanhRequest req)
    {
        var row = await db.PhieuBaoHanhs.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (!BuocKhach.TryGetValue(row.TrangThai, out var next) || !next.Contains(req.TrangThai))
            return Conflict(ApiResponse<object>.ErrorResponse("Bước chuyển trạng thái không hợp lệ."));
        if (req.KetLuan?.Length > 1000 || req.KetQuaXuLy?.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Nội dung quá dài."));
        if (req.TrangThai == "TU_CHOI" && string.IsNullOrWhiteSpace(req.KetLuan))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần kết luận khi từ chối bảo hành."));
        if (req.TrangThai == "HOAN_THANH" && string.IsNullOrWhiteSpace(req.KetQuaXuLy))
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần ghi kết quả xử lý."));
        row.TrangThai = req.TrangThai;
        if (req.KetLuan != null) row.KetLuan = req.KetLuan.Trim();
        if (req.KetQuaXuLy != null) row.KetQuaXuLy = req.KetQuaXuLy.Trim();
        if (req.TrangThai == "HOAN_THANH") row.NgayHoanThanh = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpGet("nha-cung-cap")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> DanhSachNcc([FromQuery] long? phieuBaoHanhId)
    {
        var q = db.PhieuBaoHanhNccs.AsNoTracking().AsQueryable();
        if (phieuBaoHanhId.HasValue) q = q.Where(x => x.PhieuBaoHanhId == phieuBaoHanhId);
        var rows = await q.OrderByDescending(x => x.Id).Select(x => new {
            x.Id, x.PhieuBaoHanhId, x.NhaCungCapId, x.LinhKienId, x.LinhKienSerialId,
            x.SoLuong, x.TrangThai, x.NgayGui, x.NgayNhanLai, x.KetQua
        }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("nha-cung-cap")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> TaoNcc([FromBody] TaoBaoHanhNccRequest req)
    {
        if (req.SoLuong <= 0 || (req.LinhKienSerialId.HasValue && req.SoLuong != 1))
            return BadRequest(ApiResponse<object>.ErrorResponse("Số lượng bảo hành nhà cung cấp không hợp lệ."));
        if (!await db.NhaCungCaps.AnyAsync(x => x.Id == req.NhaCungCapId && x.DeletedAt == null)
            || !await db.LinhKiens.AnyAsync(x => x.Id == req.LinhKienId && x.DeletedAt == null))
            return BadRequest(ApiResponse<object>.ErrorResponse("Nhà cung cấp hoặc linh kiện không tồn tại."));
        if (req.PhieuBaoHanhId.HasValue && !await db.PhieuBaoHanhs.AnyAsync(x => x.Id == req.PhieuBaoHanhId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu bảo hành khách không tồn tại."));
        if (req.LinhKienSerialId.HasValue && !await db.LinhKienSerials.AnyAsync(x =>
            x.Id == req.LinhKienSerialId && x.LinhKienId == req.LinhKienId))
            return BadRequest(ApiResponse<object>.ErrorResponse("Serial không thuộc linh kiện."));
        var row = new PhieuBaoHanhNcc {
            PhieuBaoHanhId = req.PhieuBaoHanhId, NhaCungCapId = req.NhaCungCapId,
            LinhKienId = req.LinhKienId, LinhKienSerialId = req.LinhKienSerialId,
            SoLuong = req.SoLuong, TrangThai = "CHO_GUI"
        };
        db.PhieuBaoHanhNccs.Add(row);
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("nha-cung-cap/{id:long}/gui")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> GuiNcc(long id)
    {
        var row = await db.PhieuBaoHanhNccs.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "CHO_GUI") return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ không ở trạng thái chờ gửi."));
        row.TrangThai = "DA_GUI";
        row.NgayGui = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("nha-cung-cap/{id:long}/nhan-lai")]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
    public async Task<IActionResult> NhanLaiNcc(long id, [FromBody] NhanLaiBaoHanhNccRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.KetQua) || req.KetQua.Length > 1000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần ghi kết quả nhà cung cấp."));
        var row = await db.PhieuBaoHanhNccs.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "DA_GUI") return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ chưa được gửi."));
        row.TrangThai = "DA_NHAN_LAI";
        row.NgayNhanLai = DateTime.UtcNow;
        row.KetQua = req.KetQua.Trim();
        await db.SaveChangesAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }
}

public sealed record TaoCamKetBaoHanhRequest(long PhieuGocId, string Loai,
    long? ChiTietXuatKhoId, long? DichVuId, DateOnly NgayBatDau, DateOnly NgayKetThuc, string? DieuKien);
public sealed record TiepNhanBaoHanhRequest(long PhieuGocId, long? CamKetId,
    long? PhieuSuaBaoHanhId, string? LoiKhachBao);
public sealed record DoiTrangThaiBaoHanhRequest(string TrangThai, string? KetLuan, string? KetQuaXuLy);
public sealed record TaoBaoHanhNccRequest(long? PhieuBaoHanhId, long NhaCungCapId,
    long LinhKienId, long? LinhKienSerialId, int SoLuong);
public sealed record NhanLaiBaoHanhNccRequest(string? KetQua);
