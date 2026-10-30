using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.IServices;
using RepairSystem.Service.Helpers;

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
        var query = _context.DichVus.AsNoTracking().AsQueryable();

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
        CatalogValidation.Validate(dto);
        CatalogValidation.Money(dto.GiaNiemYet);
        var dv = new DichVu
        {
            TenDichVu = dto.TenDichVu.Trim(),
            MoTa = CatalogValidation.NullIfBlank(dto.MoTa),
            HinhAnh = CatalogValidation.NullIfBlank(dto.HinhAnh),
            GiaNiemYet = dto.GiaNiemYet,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DichVus.Add(dv);
        await _context.SaveChangesAsync();
        return ToDto(dv);
    }

    public async Task<DichVuDto> UpdateAsync(long id, UpsertDichVuDto dto)
    {
        CatalogValidation.Validate(dto);
        CatalogValidation.Money(dto.GiaNiemYet);
        var dv = await _context.DichVus
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null)
            ?? throw new KeyNotFoundException($"Không tìm thấy dịch vụ id={id}.");

        dv.TenDichVu = dto.TenDichVu.Trim();
        dv.MoTa = CatalogValidation.NullIfBlank(dto.MoTa);
        dv.HinhAnh = CatalogValidation.NullIfBlank(dto.HinhAnh);
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
