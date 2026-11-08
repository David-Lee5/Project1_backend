using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

/// <summary>
/// Serial từng linh kiện: danh sách để chọn khi xuất kho, tra cứu vòng đời, tra cứu bảo hành công khai.
/// Phân quyền: danh sách (Admin, Thủ kho, Kỹ thuật viên), vòng đời (mọi nhân viên), bảo hành (công khai).
/// </summary>
[ApiController]
[Route("api/linh-kien-serial")]
public class LinhKienSerialController : ControllerBase
{
    private readonly ILinhKienSerialService _service;

    public LinhKienSerialController(ILinhKienSerialService service)
    {
        _service = service;
    }

    /// <summary>
    /// Danh sách serial. Query: linhKienId, trangThai
    /// (TRONG_KHO | DA_XUAT | BAO_HANH | THANH_LY | THIEU_KIEM_KE | CACH_LY), khoId.
    /// Dùng trangThai=TRONG_KHO để lấy các serial có thể xuất kho.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin + "," + Roles.ThuKho + "," + Roles.KyThuatVien)]
    public async Task<ActionResult<ApiResponse<IEnumerable<LinhKienSerialDto>>>> GetDanhSach(
        [FromQuery] long? linhKienId = null,
        [FromQuery] string? trangThai = null,
        [FromQuery] long? khoId = null)
    {
        try
        {
            var result = await _service.GetDanhSachAsync(linhKienId, trangThai, khoId);
            return Ok(ApiResponse<IEnumerable<LinhKienSerialDto>>.SuccessResponse(result));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<IEnumerable<LinhKienSerialDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>Tra cứu vòng đời một serial: nhập kho từ phiếu/NCC nào, xuất cho phiếu sửa chữa nào.</summary>
    [HttpGet("{soSerial}/tracuu")]
    [Authorize(Roles = Roles.NhanVien)]
    public async Task<ActionResult<ApiResponse<SerialVongDoiDto>>> TraCuu(string soSerial)
    {
        var result = await _service.TraCuuAsync(soSerial);
        if (result == null)
            return NotFound(ApiResponse<SerialVongDoiDto>.ErrorResponse($"Không tìm thấy serial '{soSerial}'."));

        return Ok(ApiResponse<SerialVongDoiDto>.SuccessResponse(result));
    }

    /// <summary>
    /// Tra cứu bảo hành công khai (không cần đăng nhập). Không trả về thông tin khách hàng.
    /// </summary>
    [HttpGet("bao-hanh/{soSerial}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<BaoHanhSerialDto>>> TraCuuBaoHanh(string soSerial)
    {
        var result = await _service.TraCuuBaoHanhAsync(soSerial);
        if (result == null)
            return NotFound(ApiResponse<BaoHanhSerialDto>.ErrorResponse(
                "Không tìm thấy serial này trong hệ thống. Linh kiện có thể không phải hàng do cửa hàng bán ra."));

        return Ok(ApiResponse<BaoHanhSerialDto>.SuccessResponse(result));
    }
}
