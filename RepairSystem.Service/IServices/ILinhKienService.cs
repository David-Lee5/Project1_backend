using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface ILinhKienService
{
    /// <param name="keyword">Tìm theo mã, tên hoặc hãng sản xuất.</param>
    /// <param name="includeStock">true: kèm tổng tồn kho (chỉ dành cho nhân viên).</param>
    Task<IEnumerable<LinhKienDto>> GetAllAsync(string? keyword = null, bool includeStock = false, bool includeDeleted = false);

    Task<LinhKienDto?> GetByIdAsync(long id, bool includeStock = false);
    Task<LinhKienDto> CreateAsync(UpsertLinhKienDto dto);
    Task<LinhKienDto> UpdateAsync(long id, UpsertLinhKienDto dto);

    /// <summary>Soft-delete linh kiện (đặt deleted_at).</summary>
    Task<bool> DeleteAsync(long id);
}
