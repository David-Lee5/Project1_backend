using RepairSystem.Service.DTOs;

namespace RepairSystem.Service.IServices;

public interface ILinhKienSerialService
{
    /// <summary>Danh sách serial, mặc định xếp theo ngày nhập cũ trước (FIFO). Lọc theo linh kiện, trạng thái, kho.</summary>
    Task<IEnumerable<LinhKienSerialDto>> GetDanhSachAsync(long? linhKienId, string? trangThai, long? khoId);

    /// <summary>Vòng đời một serial (nhập kho từ đâu, xuất cho phiếu nào). Null nếu không tồn tại.</summary>
    Task<SerialVongDoiDto?> TraCuuAsync(string soSerial);

    /// <summary>Tra cứu bảo hành công khai. Null nếu serial không phải hàng của cửa hàng.</summary>
    Task<BaoHanhSerialDto?> TraCuuBaoHanhAsync(string soSerial);
}
