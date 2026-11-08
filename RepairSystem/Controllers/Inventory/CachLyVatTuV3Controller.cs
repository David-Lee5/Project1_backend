using System.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Controllers.V3;

[ApiController]
[Route("api/v3/cach-ly-vat-tu")]
[Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
public sealed class CachLyVatTuV3Controller(AppDbContextV3 db, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DanhSach([FromQuery] long? khoId, [FromQuery] string? trangThai)
    {
        var query = db.CachLyVatTuV3s.AsNoTracking().AsQueryable();
        if (khoId.HasValue) query = query.Where(x => x.KhoId == khoId);
        if (!string.IsNullOrWhiteSpace(trangThai)) query = query.Where(x => x.TrangThai == trangThai);
        var rows = await query.OrderByDescending(x => x.TaoLuc).Take(300)
            .Select(x => new {
                x.Id, x.KhoId, x.LinhKienId, x.LinhKienSerialId,
                x.SoLuong, x.TrangThai, x.LyDo, x.NguoiTaoId, x.TaoLuc,
                x.NguoiXuLyId, x.XuLyLuc, x.LyDoXuLy,
                HoSo = db.HoSoThanhLyV3s.Where(h => h.CachLyId == x.Id)
                    .OrderByDescending(h => h.Id).Select(h => new {
                        h.Id, h.KetQuaKiemTra, h.HuongXuLy, h.GiaTriUocTinh,
                        h.GiaTriDuyet, h.GiaTriThuVe, h.TrangThai,
                        h.NguoiDeXuatId, h.DeXuatLuc, h.NguoiDuyetId,
                        h.DuyetLuc, h.LyDoQuyetDinh
                    }).FirstOrDefault()
            }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("{id:long}/de-xuat")]
    public async Task<IActionResult> DeXuat(long id, [FromBody] DeXuatXuLyVatTuRequest req)
    {
        if (req.HuongXuLy is not ("NHAP_LAI" or "THANH_LY" or "TIEU_HUY")
            || string.IsNullOrWhiteSpace(req.KetQuaKiemTra) || req.KetQuaKiemTra.Length > 1000
            || req.GiaTriUocTinh < 0 || req.GiaTriUocTinh > 999_999_999_999.99m)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần kết quả kiểm tra, hướng xử lý và giá trị ước tính."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.CachLyVatTuV3s.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "CACH_LY" || await db.HoSoThanhLyV3s.AnyAsync(x =>
                x.CachLyId == id && x.TrangThai != "TU_CHOI"))
            return Conflict(ApiResponse<object>.ErrorResponse("Vật tư đã xử lý hoặc đang có đề xuất chưa kết thúc."));
        var caseFile = new HoSoThanhLyV3 {
            CachLyId = id, KetQuaKiemTra = req.KetQuaKiemTra.Trim(),
            HuongXuLy = req.HuongXuLy, GiaTriUocTinh = req.GiaTriUocTinh,
            TrangThai = "CHO_DUYET", NguoiDeXuatId = actor.Value,
            DeXuatLuc = DateTime.UtcNow
        };
        db.HoSoThanhLyV3s.Add(caseFile);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { caseFile.Id, caseFile.TrangThai }));
    }

    [HttpGet("{id:long}/tai-lieu")]
    public async Task<IActionResult> DanhSachTaiLieu(long id)
    {
        var rows = await db.TaiLieuThanhLyV3s.AsNoTracking().Where(x => x.HoSoId == id)
            .OrderBy(x => x.TaiLuc).Select(x => new {
                x.Id, x.HoSoId, x.Loai, x.TenGoc, x.Sha256, x.NguoiTaiId, x.TaiLuc
            }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpPost("{id:long}/tai-lieu")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> TaiTaiLieu(long id, [FromForm] string loai, IFormFile file)
    {
        if (loai is not ("ANH" or "BIEN_BAN") || file == null || file.Length is <= 0 or > 10_000_000)
            return BadRequest(ApiResponse<object>.ErrorResponse("Tài liệu không hợp lệ hoặc vượt 10 MB."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        var caseFile = await db.HoSoThanhLyV3s.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (caseFile == null) return NotFound();
        if (caseFile.TrangThai != "CHO_DUYET")
            return Conflict(ApiResponse<object>.ErrorResponse("Chỉ bổ sung chứng cứ trước khi duyệt."));
        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory);
        var bytes = memory.ToArray();
        var isJpeg = bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        var isPng = bytes.Length >= 8 && bytes.AsSpan(0, 8)
            .SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var isPdf = bytes.Length >= 4 && bytes.AsSpan(0, 4)
            .SequenceEqual(new byte[] { 37, 80, 68, 70 });
        if (!(isJpeg || isPng || (loai == "BIEN_BAN" && isPdf)))
            return BadRequest(ApiResponse<object>.ErrorResponse("Chỉ nhận ảnh JPEG/PNG hoặc biên bản PDF."));
        var ext = isJpeg ? ".jpg" : isPng ? ".png" : ".pdf";
        var filename = Guid.NewGuid().ToString("N") + ext;
        var folder = Path.Combine(env.ContentRootPath, "App_Data", "thanh-ly");
        Directory.CreateDirectory(folder);
        var path = Path.Combine(folder, filename);
        await System.IO.File.WriteAllBytesAsync(path, bytes);
        var originalName = Path.GetFileName(file.FileName);
        if (string.IsNullOrWhiteSpace(originalName)) originalName = filename;
        var doc = new TaiLieuThanhLyV3 {
            HoSoId = id, Loai = loai, TenGoc = originalName[..Math.Min(originalName.Length, 200)],
            DuongDan = filename, Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            NguoiTaiId = actor.Value, TaiLuc = DateTime.UtcNow
        };
        db.TaiLieuThanhLyV3s.Add(doc);
        try { await db.SaveChangesAsync(); }
        catch { System.IO.File.Delete(path); throw; }
        return Ok(ApiResponse<object>.SuccessResponse(new { doc.Id, doc.Loai, doc.Sha256 }));
    }

    [HttpGet("{id:long}/tai-lieu/{taiLieuId:long}")]
    public async Task<IActionResult> XemTaiLieu(long id, long taiLieuId)
    {
        var doc = await db.TaiLieuThanhLyV3s.AsNoTracking().FirstOrDefaultAsync(x =>
            x.Id == taiLieuId && x.HoSoId == id);
        if (doc == null) return NotFound();
        var path = Path.Combine(env.ContentRootPath, "App_Data", "thanh-ly", doc.DuongDan);
        if (!System.IO.File.Exists(path)) return NotFound();
        var bytes = await System.IO.File.ReadAllBytesAsync(path);
        if (!string.Equals(Convert.ToHexString(SHA256.HashData(bytes)), doc.Sha256,
                StringComparison.OrdinalIgnoreCase))
            return Conflict(ApiResponse<object>.ErrorResponse("Tài liệu đã thay đổi; cần kiểm tra lưu trữ."));
        var contentType = Path.GetExtension(doc.DuongDan) switch {
            ".jpg" => "image/jpeg", ".png" => "image/png", _ => "application/pdf" };
        return File(bytes, contentType);
    }

    [HttpPost("{id:long}/duyet")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Duyet(long id, [FromBody] DuyetXuLyVatTuRequest req)
    {
        if (req.QuyetDinh is not ("DA_DUYET" or "TU_CHOI")
            || string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 500
            || req.GiaTriDuyet < 0 || req.GiaTriDuyet > 999_999_999_999.99m)
            return BadRequest(ApiResponse<object>.ErrorResponse("Quyết định, lý do hoặc giá trị duyệt không hợp lệ."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var caseFile = await db.HoSoThanhLyV3s.FirstOrDefaultAsync(x => x.Id == id);
        if (caseFile == null) return NotFound();
        if (caseFile.TrangThai != "CHO_DUYET")
            return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ không chờ duyệt."));
        var quarantine = await db.CachLyVatTuV3s.AsNoTracking().FirstAsync(x => x.Id == caseFile.CachLyId);
        if (quarantine.TrangThai != "CACH_LY")
            return Conflict(ApiResponse<object>.ErrorResponse("Vật tư không còn cách ly."));
        if (caseFile.NguoiDeXuatId == actor || quarantine.NguoiTaoId == actor)
            return Conflict(ApiResponse<object>.ErrorResponse("Người cách ly/đề xuất không được tự duyệt."));
        if (req.QuyetDinh == "DA_DUYET")
        {
            var docs = await db.TaiLieuThanhLyV3s.AsNoTracking()
                .Where(x => x.HoSoId == id).ToListAsync();
            var types = docs.Select(x => x.Loai).Distinct().ToList();
            if (!types.Contains("ANH") || !types.Contains("BIEN_BAN"))
                return Conflict(ApiResponse<object>.ErrorResponse("Cần ảnh hiện trạng và biên bản kiểm tra trước khi duyệt."));
            foreach (var doc in docs)
            {
                var filePath = Path.Combine(env.ContentRootPath, "App_Data", "thanh-ly", doc.DuongDan);
                if (!System.IO.File.Exists(filePath) || !string.Equals(
                    Convert.ToHexString(SHA256.HashData(await System.IO.File.ReadAllBytesAsync(filePath))),
                    doc.Sha256, StringComparison.OrdinalIgnoreCase))
                    return Conflict(ApiResponse<object>.ErrorResponse("Chứng cứ thiếu hoặc đã thay đổi; cần kiểm tra lưu trữ."));
            }
        }
        caseFile.TrangThai = req.QuyetDinh;
        caseFile.NguoiDuyetId = actor;
        caseFile.DuyetLuc = DateTime.UtcNow;
        caseFile.LyDoQuyetDinh = req.LyDo.Trim();
        caseFile.GiaTriDuyet = req.QuyetDinh == "DA_DUYET" ? req.GiaTriDuyet : null;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { caseFile.Id, caseFile.TrangThai }));
    }

    [HttpPost]
    public async Task<IActionResult> Tao([FromBody] TaoCachLyVatTuRequest req)
    {
        if (req.KhoId <= 0 || req.LinhKienId <= 0 || req.SoLuong <= 0
            || string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần kho, linh kiện, số lượng và lý do cách ly."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var tracked = await db.LinhKiens.AsNoTracking().Where(x => x.Id == req.LinhKienId
            && x.DeletedAt == null).Select(x => x.TheoDoiSerial).FirstOrDefaultAsync();
        if (tracked == null)
            return BadRequest(ApiResponse<object>.ErrorResponse("Linh kiện chưa có chính sách serial hợp lệ."));
        if ((tracked.Value && (req.LinhKienSerialId == null || req.SoLuong != 1))
            || (!tracked.Value && req.LinhKienSerialId != null))
            return BadRequest(ApiResponse<object>.ErrorResponse("Serial và số lượng không đúng chính sách linh kiện."));
        var stock = await db.TonKhos.FirstOrDefaultAsync(x =>
            x.KhoId == req.KhoId && x.LinhKienId == req.LinhKienId);
        if (stock == null || stock.SoLuongTon < req.SoLuong)
            return Conflict(ApiResponse<object>.ErrorResponse("Không đủ tồn có thể xuất để cách ly."));
        LinhKienSerial? serial = null;
        if (req.LinhKienSerialId.HasValue)
        {
            if (await db.LinhKienSerials.CountAsync(x => x.KhoId == req.KhoId
                    && x.LinhKienId == req.LinhKienId && x.TrangThai == "TRONG_KHO")
                != stock.SoLuongTon)
                return Conflict(ApiResponse<object>.ErrorResponse(
                    "Tồn sổ và serial chưa khớp; cần kiểm kê trước khi cách ly."));
            serial = await db.LinhKienSerials.FirstOrDefaultAsync(x => x.Id == req.LinhKienSerialId
                && x.LinhKienId == req.LinhKienId && x.KhoId == req.KhoId
                && x.TrangThai == "TRONG_KHO");
            if (serial == null)
                return Conflict(ApiResponse<object>.ErrorResponse("Serial không còn trong kho hoặc đã bị cách ly/xuất."));
        }
        var now = DateTime.UtcNow;
        stock.SoLuongTon -= req.SoLuong;
        stock.NgayCapNhat = now;
        if (serial != null)
        {
            serial.TrangThai = "CACH_LY";
            serial.UpdatedAt = now;
        }
        var row = new CachLyVatTuV3 {
            KhoId = req.KhoId, LinhKienId = req.LinhKienId,
            LinhKienSerialId = req.LinhKienSerialId, SoLuong = req.SoLuong,
            TrangThai = "CACH_LY", LyDo = req.LyDo.Trim(),
            NguoiTaoId = actor.Value, TaoLuc = now
        };
        db.CachLyVatTuV3s.Add(row);
        db.BienDongKhos.Add(new BienDongKho {
            KhoId = req.KhoId, LinhKienId = req.LinhKienId,
            Loai = "DIEU_CHINH", SoLuongThayDoi = -req.SoLuong,
            SoLuongSau = stock.SoLuongTon, NguoiThucHienId = actor.Value,
            ThoiGian = now, LyDo = $"Cách ly hàng lỗi: {req.LyDo.Trim()}"
        });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }

    [HttpPost("{id:long}/xu-ly")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> XuLy(long id, [FromBody] XuLyCachLyVatTuRequest req)
    {
        if (req.QuyetDinh is not ("NHAP_LAI" or "THANH_LY" or "TIEU_HUY")
            || string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần quyết định và lý do xử lý."));
        var actor = await V3Auth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var row = await db.CachLyVatTuV3s.FirstOrDefaultAsync(x => x.Id == id);
        if (row == null) return NotFound();
        if (row.TrangThai != "CACH_LY")
            return Conflict(ApiResponse<object>.ErrorResponse("Hồ sơ cách ly đã được xử lý."));
        var caseFile = await db.HoSoThanhLyV3s.FirstOrDefaultAsync(x =>
            x.CachLyId == id && x.TrangThai == "DA_DUYET");
        if (caseFile == null || caseFile.HuongXuLy != req.QuyetDinh)
            return Conflict(ApiResponse<object>.ErrorResponse(
                "Cần hồ sơ kiểm tra, ảnh, biên bản và quyết định duyệt đúng hướng xử lý trước."));
        if (req.GiaTriThuVe < 0 || req.GiaTriThuVe > 999_999_999_999.99m
            || (req.QuyetDinh != "THANH_LY" && req.GiaTriThuVe != 0))
            return BadRequest(ApiResponse<object>.ErrorResponse("Giá trị thu về không hợp lệ."));
        if (row.NguoiTaoId == actor)
            return Conflict(ApiResponse<object>.ErrorResponse("Người cách ly không được tự quyết định xử lý."));
        var now = DateTime.UtcNow;
        if (row.LinhKienSerialId.HasValue)
        {
            var serial = await db.LinhKienSerials.FirstOrDefaultAsync(x =>
                x.Id == row.LinhKienSerialId && x.KhoId == row.KhoId && x.TrangThai == "CACH_LY");
            if (serial == null)
                return Conflict(ApiResponse<object>.ErrorResponse("Trạng thái serial đã thay đổi; cần đối soát."));
            serial.TrangThai = req.QuyetDinh == "NHAP_LAI" ? "TRONG_KHO" : "THANH_LY";
            if (req.QuyetDinh != "NHAP_LAI") serial.KhoId = null;
            serial.UpdatedAt = now;
        }
        if (req.QuyetDinh == "NHAP_LAI")
        {
            var stock = await db.TonKhos.FirstAsync(x => x.KhoId == row.KhoId
                && x.LinhKienId == row.LinhKienId);
            if (row.LinhKienSerialId.HasValue &&
                await db.LinhKienSerials.CountAsync(x => x.KhoId == row.KhoId
                    && x.LinhKienId == row.LinhKienId && x.TrangThai == "TRONG_KHO")
                != stock.SoLuongTon)
                return Conflict(ApiResponse<object>.ErrorResponse(
                    "Tồn sổ và serial chưa khớp; cần đối soát trước khi nhập lại."));
            stock.SoLuongTon = checked(stock.SoLuongTon + row.SoLuong);
            stock.NgayCapNhat = now;
            db.BienDongKhos.Add(new BienDongKho {
                KhoId = row.KhoId, LinhKienId = row.LinhKienId,
                Loai = "DIEU_CHINH", SoLuongThayDoi = row.SoLuong,
                SoLuongSau = stock.SoLuongTon, NguoiThucHienId = actor.Value,
                ThoiGian = now, LyDo = $"Nhập lại sau cách ly #{id}: {req.LyDo.Trim()}"
            });
        }
        row.TrangThai = req.QuyetDinh == "TIEU_HUY" ? "THANH_LY" : req.QuyetDinh;
        row.NguoiXuLyId = actor;
        row.XuLyLuc = now;
        row.LyDoXuLy = req.LyDo.Trim();
        caseFile.GiaTriThuVe = req.GiaTriThuVe;
        caseFile.TrangThai = "DA_XU_LY";
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { row.Id, row.TrangThai }));
    }
}

public sealed record TaoCachLyVatTuRequest(long KhoId, long LinhKienId,
    long? LinhKienSerialId, int SoLuong, string? LyDo);
public sealed record DeXuatXuLyVatTuRequest(string? KetQuaKiemTra,
    string? HuongXuLy, decimal GiaTriUocTinh);
public sealed record DuyetXuLyVatTuRequest(string? QuyetDinh, string? LyDo, decimal GiaTriDuyet);
public sealed record XuLyCachLyVatTuRequest(string QuyetDinh, string? LyDo, decimal GiaTriThuVe);
