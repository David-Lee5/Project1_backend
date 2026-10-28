using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface IDichVuService
{
    Task<IEnumerable<DichVuDto>> GetAllAsync(bool includeDeleted = false);
    Task<DichVuDto?> GetByIdAsync(long id);
    Task<DichVuDto> CreateAsync(UpsertDichVuDto dto);
    Task<DichVuDto> UpdateAsync(long id, UpsertDichVuDto dto);
    /// <summary>Soft-delete dịch vụ (đặt deleted_at).</summary>
    Task<bool> DeleteAsync(long id);
}
