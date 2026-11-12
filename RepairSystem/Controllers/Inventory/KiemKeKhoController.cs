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
[Route("api/v3/kiem-ke-kho")]
[Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
public sealed class KiemKeKhoController(RepairDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> DanhSach([FromQuery] long? khoId)
    {
        var query = db.KiemKeKhos.AsNoTracking().AsQueryable();
        if (khoId.HasValue) query = query.Where(x => x.KhoId == khoId);
        var rows = await query.OrderByDescending(x => x.TaoLuc).Select(x => new {
            x.Id, x.KhoId, x.TrangThai, x.NguoiKiemId, x.NguoiDuyetId,
            x.TaoLuc, x.DuyetLuc, x.GhiChu, x.LyDoQuyetDinh
        }).ToListAsync();
        return Ok(ApiResponse<object>.SuccessResponse(rows));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> ChiTiet(long id)
    {
        var row = await db.KiemKeKhos.AsNoTracking().Include(x => x.ChiTiet)
            .FirstOrDefaultAsync(x => x.Id == id);
        return row == null ? NotFound() : Ok(ApiResponse<object>.SuccessResponse(row));
    }

    [HttpPost]
    public async Task<IActionResult> Tao([FromBody] TaoKiemKeKhoRequest req)
    {
        if (req.KhoId <= 0 || req.ChiTiet == null || req.ChiTiet.Count == 0
            || req.GhiChu?.Length > 500
            || req.ChiTiet.Any(x => x.LinhKienId <= 0 || x.SoLuongThucTe < 0)
            || req.ChiTiet.Select(x => x.LinhKienId).Distinct().Count() != req.ChiTiet.Count)
            return BadRequest(ApiResponse<object>.ErrorResponse("Phiếu kiểm kê không hợp lệ."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        if (!await db.Khos.AnyAsync(x => x.Id == req.KhoId && x.DeletedAt == null))
            return NotFound(ApiResponse<object>.ErrorResponse("Kho không tồn tại."));
        if (await db.KiemKeKhos.AnyAsync(x => x.KhoId == req.KhoId && x.TrangThai == "CHO_DUYET"))
            return Conflict(ApiResponse<object>.ErrorResponse("Kho đã có phiếu kiểm kê chờ duyệt."));
        var stock = await db.TonKhos.AsNoTracking().Where(x => x.KhoId == req.KhoId)
            .ToDictionaryAsync(x => x.LinhKienId);
        if (!stock.Keys.Order().SequenceEqual(req.ChiTiet.Select(x => x.LinhKienId).Order()))
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Cần kiểm kê đủ mọi mẫu linh kiện đang có dòng tồn kho, kể cả số lượng 0."));
        var stockIds = stock.Keys.ToArray();
        var policies = await db.LinhKiens.AsNoTracking()
            .Where(x => stockIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.TheoDoiSerial);
        var serials = await db.LinhKienSerials.AsNoTracking()
            .Where(x => x.KhoId == req.KhoId &&
                (x.TrangThai == "TRONG_KHO" || x.TrangThai == "THIEU_KIEM_KE"))
            .Select(x => new { x.LinhKienId, x.SoSerial, x.TrangThai }).ToListAsync();
        var count = new KiemKeKho {
            KhoId = req.KhoId, TrangThai = "CHO_DUYET", NguoiKiemId = actor.Value,
            TaoLuc = DateTime.UtcNow, GhiChu = req.GhiChu?.Trim()
        };
        foreach (var item in req.ChiTiet)
        {
            if (!policies.TryGetValue(item.LinhKienId, out var tracked) || tracked == null)
                return Conflict(ApiResponse<object>.ErrorResponse(
                    $"Linh kiện #{item.LinhKienId} chưa chọn chính sách serial."));
            var requested = ChuanHoa(item.SerialThucTe);
            if (requested == null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Danh sách serial có giá trị trống hoặc trùng."));
            var current = serials.Where(x => x.LinhKienId == item.LinhKienId
                    && x.TrangThai == "TRONG_KHO")
                .Select(x => x.SoSerial).Order(StringComparer.Ordinal).ToList();
            if (tracked.Value)
            {
                if (current.Count != stock[item.LinhKienId].SoLuongTon)
                    return Conflict(ApiResponse<object>.ErrorResponse(
                        $"Tồn sổ và serial của linh kiện #{item.LinhKienId} chưa khớp; cần đối soát trước."));
                if (requested.Count != item.SoLuongThucTe)
                    return BadRequest(ApiResponse<object>.ErrorResponse("Số lượng thực tế phải bằng số serial đã quét."));
                var allowed = serials.Where(x => x.LinhKienId == item.LinhKienId)
                    .Select(x => x.SoSerial).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (requested.Any(x => !allowed.Contains(x)))
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        "Có serial chưa thuộc kho hoặc chưa được ghi nhận; cần đối soát nguồn gốc trước."));
            }
            else if (requested.Count != 0 || serials.Any(x => x.LinhKienId == item.LinhKienId))
                return Conflict(ApiResponse<object>.ErrorResponse(
                    "Linh kiện quản lý theo số lượng không được có serial trong kho/phiếu kiểm kê."));
            count.ChiTiet.Add(new DongKiemKeKho {
                LinhKienId = item.LinhKienId,
                SoLuongSoSach = stock[item.LinhKienId].SoLuongTon,
                SoLuongThucTe = item.SoLuongThucTe,
                SerialSoSachJson = tracked.Value ? JsonSerializer.Serialize(current) : null,
                SerialThucTeJson = tracked.Value ? JsonSerializer.Serialize(requested) : null
            });
        }
        db.KiemKeKhos.Add(count);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { count.Id, count.TrangThai }));
    }

    [HttpPost("{id:long}/duyet")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Duyet(long id, [FromBody] DuyetKiemKeKhoRequest req)
    {
        if (req.QuyetDinh is not ("DA_DUYET" or "TU_CHOI")
            || string.IsNullOrWhiteSpace(req.LyDo) || req.LyDo.Length > 500)
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần quyết định và lý do."));
        var actor = await NhanVienAuth.NhanVienIdAsync(db, User);
        if (actor == null) return Unauthorized();
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var count = await db.KiemKeKhos.Include(x => x.ChiTiet)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (count == null) return NotFound();
        if (count.TrangThai != "CHO_DUYET")
            return Conflict(ApiResponse<object>.ErrorResponse("Phiếu kiểm kê đã được xử lý."));
        if (count.NguoiKiemId == actor)
            return Conflict(ApiResponse<object>.ErrorResponse("Người kiểm kê không được tự duyệt."));
        var now = DateTime.UtcNow;
        if (req.QuyetDinh == "DA_DUYET")
        {
            var stock = await db.TonKhos.Where(x => x.KhoId == count.KhoId)
                .ToDictionaryAsync(x => x.LinhKienId);
            if (!stock.Keys.Order().SequenceEqual(count.ChiTiet.Select(x => x.LinhKienId).Order()))
                return Conflict(ApiResponse<object>.ErrorResponse("Danh mục tồn kho đã đổi; cần kiểm kê lại."));
            var serials = await db.LinhKienSerials.Where(x => x.KhoId == count.KhoId
                && (x.TrangThai == "TRONG_KHO" || x.TrangThai == "THIEU_KIEM_KE"))
                .ToListAsync();
            foreach (var line in count.ChiTiet)
            {
                var currentStock = stock[line.LinhKienId];
                if (currentStock.SoLuongTon != line.SoLuongSoSach)
                    return Conflict(ApiResponse<object>.ErrorResponse("Tồn kho đã đổi sau khi đếm; cần kiểm kê lại."));
                if (line.SerialSoSachJson != null)
                {
                    var before = JsonSerializer.Deserialize<List<string>>(line.SerialSoSachJson)!;
                    var actual = JsonSerializer.Deserialize<List<string>>(line.SerialThucTeJson!)!;
                    var current = serials.Where(x => x.LinhKienId == line.LinhKienId
                            && x.TrangThai == "TRONG_KHO")
                        .Select(x => x.SoSerial).Order(StringComparer.Ordinal).ToList();
                    if (!current.SequenceEqual(before))
                        return Conflict(ApiResponse<object>.ErrorResponse("Serial trong kho đã đổi; cần kiểm kê lại."));
                    var physical = actual.ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var candidates = serials.Where(x => x.LinhKienId == line.LinhKienId).ToList();
                    if (actual.Any(x => !candidates.Any(s =>
                            string.Equals(s.SoSerial, x, StringComparison.OrdinalIgnoreCase))))
                        return Conflict(ApiResponse<object>.ErrorResponse("Serial thực tế không còn thuộc kho; cần kiểm kê lại."));
                    foreach (var serial in candidates)
                    {
                        var next = physical.Contains(serial.SoSerial) ? "TRONG_KHO" : "THIEU_KIEM_KE";
                        if (serial.TrangThai == next) continue;
                        db.BienDongSerialKiemKes.Add(new BienDongSerialKiemKe {
                            KiemKeId = count.Id, LinhKienSerialId = serial.Id,
                            TrangThaiCu = serial.TrangThai, TrangThaiMoi = next, ThoiGian = now
                        });
                        serial.TrangThai = next;
                        serial.UpdatedAt = now;
                    }
                }
                var delta = line.SoLuongThucTe - line.SoLuongSoSach;
                if (delta == 0) continue;
                currentStock.SoLuongTon = line.SoLuongThucTe;
                currentStock.NgayCapNhat = now;
                db.BienDongKhos.Add(new BienDongKho {
                    KhoId = count.KhoId, LinhKienId = line.LinhKienId,
                    Loai = "DIEU_CHINH", SoLuongThayDoi = delta,
                    SoLuongSau = line.SoLuongThucTe, NguoiThucHienId = actor.Value,
                    ThoiGian = now, LyDo = $"Kiểm kê #{count.Id}: {req.LyDo.Trim()}"
                });
            }
        }
        count.TrangThai = req.QuyetDinh;
        count.NguoiDuyetId = actor;
        count.DuyetLuc = now;
        count.LyDoQuyetDinh = req.LyDo.Trim();
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return Ok(ApiResponse<object>.SuccessResponse(new { count.Id, count.TrangThai }));
    }

    private static List<string>? ChuanHoa(List<string>? serials)
    {
        var rows = (serials ?? []).Select(x => x?.Trim().ToUpperInvariant()).ToList();
        if (rows.Any(string.IsNullOrWhiteSpace)
            || rows.Distinct(StringComparer.OrdinalIgnoreCase).Count() != rows.Count)
            return null;
        return rows.Select(x => x!).Order(StringComparer.Ordinal).ToList();
    }
}

public sealed record TaoKiemKeKhoRequest(long KhoId, List<DongTaoKiemKeKhoRequest>? ChiTiet, string? GhiChu);
public sealed record DongTaoKiemKeKhoRequest(long LinhKienId, int SoLuongThucTe,
    List<string>? SerialThucTe);
public sealed record DuyetKiemKeKhoRequest(string QuyetDinh, string? LyDo);
