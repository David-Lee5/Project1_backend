using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface INhaCungCapService
{
    Task<IEnumerable<NhaCungCapDto>> GetAllAsync(bool includeDeleted = false);
    Task<NhaCungCapDto?> GetByIdAsync(long id);
    Task<NhaCungCapDto> CreateAsync(UpsertNhaCungCapDto dto);
    Task<NhaCungCapDto> UpdateAsync(long id, UpsertNhaCungCapDto dto);
    /// <summary>Soft-delete nhà cung cấp (đặt deleted_at).</summary>
    Task<bool> DeleteAsync(long id);
}
