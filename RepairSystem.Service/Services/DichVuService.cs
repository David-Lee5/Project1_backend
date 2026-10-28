using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;

namespace RepairSystem.Service.Services;

public class DichVuService : IDichVuService
{
    private readonly AppDbContext _context;

    public DichVuService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DichVuDto>> GetAllAsync(bool includeDeleted = false)
    {
        var query = _context.DichVus.AsQueryable();

        if (!includeDeleted)
            query = query.Where(dv => dv.DeletedAt == null);

        var list = await query.OrderBy(dv => dv.TenDichVu).ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<DichVuDto?> GetByIdAsync(long id)
    {
        var dv = await _context.DichVus
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);

        return dv == null ? null : ToDto(dv);
    }

    public async Task<DichVuDto> CreateAsync(UpsertDichVuDto dto)
    {
        var dv = new DichVu
        {
            TenDichVu = dto.TenDichVu,
            MoTa = dto.MoTa,
            HinhAnh = dto.HinhAnh,
            GiaNiemYet = dto.GiaNiemYet
        };

        _context.DichVus.Add(dv);
        await _context.SaveChangesAsync();
        return ToDto(dv);
    }

    public async Task<DichVuDto> UpdateAsync(long id, UpsertDichVuDto dto)
    {
        var dv = await _context.DichVus
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ id={id}.");

        dv.TenDichVu = dto.TenDichVu;
        dv.MoTa = dto.MoTa;
        dv.HinhAnh = dto.HinhAnh;
        dv.GiaNiemYet = dto.GiaNiemYet;
        dv.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToDto(dv);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var dv = await _context.DichVus
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);

        if (dv == null) return false;

        dv.DeletedAt = DateTime.UtcNow;
        dv.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ── helper ─────────────────────────────────────────────────────
    private static DichVuDto ToDto(DichVu dv) => new()
    {
        Id = dv.Id,
        MaDv = dv.MaDv,
        TenDichVu = dv.TenDichVu,
        MoTa = dv.MoTa,
        HinhAnh = dv.HinhAnh,
        GiaNiemYet = dv.GiaNiemYet,
        CreatedAt = dv.CreatedAt,
        UpdatedAt = dv.UpdatedAt
    };
}
