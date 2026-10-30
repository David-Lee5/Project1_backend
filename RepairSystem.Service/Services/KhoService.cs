using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class KhoService : IKhoService
{
    private readonly AppDbContext _context;

    public KhoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<KhoDto>> GetAllKhoAsync()
    {
        return await _context.Khos
            .AsNoTracking()
            .Where(k => k.DeletedAt == null)
            .OrderBy(k => k.Id)
            .Select(k => new KhoDto
            {
                Id = k.Id,
                MaKho = k.MaKho,
                TenKho = k.TenKho,
                DiaDiem = k.DiaDiem,
                ThuKhoId = k.ThuKhoId
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<TonKhoDto>> GetTonKhoAsync(long? khoId = null, string? keyword = null, bool chiSapHet = false)
    {
        var query = BaseTonKhoQuery();

        if (khoId.HasValue)
            query = query.Where(t => t.KhoId == khoId.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            query = query.Where(t => t.LinhKien.TenLinhKien.Contains(k) || t.LinhKien.MaLk.Contains(k));
        }

        if (chiSapHet)
            query = query.Where(t => t.SoLuongTon <= t.MucTonToiThieu);

        return await query
            .OrderBy(t => t.Kho.TenKho)
            .ThenBy(t => t.LinhKien.TenLinhKien)
            .Select(ToTonKhoDto)
            .ToListAsync();
    }

    public async Task<IEnumerable<CanhBaoTonKhoDto>> GetCanhBaoAsync(long? khoId = null)
    {
        var query = BaseTonKhoQuery().Where(t => t.SoLuongTon <= t.MucTonToiThieu);

        if (khoId.HasValue)
            query = query.Where(t => t.KhoId == khoId.Value);

        // Hết hàng xếp trước, sau đó đến các mặt hàng thiếu nhiều nhất
        return await query
            .OrderBy(t => t.SoLuongTon)
            .ThenBy(t => t.LinhKien.TenLinhKien)
            .Select(t => new CanhBaoTonKhoDto
            {
                KhoId = t.KhoId,
                TenKho = t.Kho.TenKho,
                LinhKienId = t.LinhKienId,
                MaLk = t.LinhKien.MaLk,
                TenLinhKien = t.LinhKien.TenLinhKien,
                DonViTinh = t.LinhKien.DonViTinh,
                SoLuongTon = t.SoLuongTon,
                MucTonToiThieu = t.MucTonToiThieu
            })
            .ToListAsync();
    }

    public async Task<TonKhoDto> CapNhatMucTonToiThieuAsync(long khoId, long linhKienId, int mucTonToiThieu)
    {
        var tonKho = await _context.TonKhos
            .FirstOrDefaultAsync(t => t.KhoId == khoId && t.LinhKienId == linhKienId)
            ?? throw new KeyNotFoundException(
                $"Chưa có dòng tồn kho cho linh kiện {linhKienId} ở kho {khoId}. Hãy nhập kho linh kiện này trước.");

        tonKho.MucTonToiThieu = mucTonToiThieu;
        await _context.SaveChangesAsync();

        return await BaseTonKhoQuery()
            .Where(t => t.KhoId == khoId && t.LinhKienId == linhKienId)
            .Select(ToTonKhoDto)
            .FirstAsync();
    }

    // ── helpers ────────────────────────────────────────────────────
    /// <summary>Chỉ lấy tồn của kho và linh kiện chưa bị xoá mềm.</summary>
    private IQueryable<TonKho> BaseTonKhoQuery() =>
        _context.TonKhos
            .AsNoTracking()
            .Where(t => t.Kho.DeletedAt == null && t.LinhKien.DeletedAt == null);

    private static readonly System.Linq.Expressions.Expression<Func<TonKho, TonKhoDto>> ToTonKhoDto = t => new TonKhoDto
    {
        KhoId = t.KhoId,
        TenKho = t.Kho.TenKho,
        LinhKienId = t.LinhKienId,
        MaLk = t.LinhKien.MaLk,
        TenLinhKien = t.LinhKien.TenLinhKien,
        DonViTinh = t.LinhKien.DonViTinh,
        SoLuongTon = t.SoLuongTon,
        MucTonToiThieu = t.MucTonToiThieu,
        NgayCapNhat = t.NgayCapNhat
    };
}
