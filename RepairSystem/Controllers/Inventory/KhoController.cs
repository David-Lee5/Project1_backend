using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/kho")]
[Authorize(Roles = Roles.NhanVien)]
public class KhoController : ControllerBase
{
    /// <summary>Admin và Thủ kho được xem cảnh báo và đặt ngưỡng tồn.</summary>
    private const string QuanLyKhoRoles = Roles.Admin + "," + Roles.ThuKho;

    private readonly IKhoService _service;

    public KhoController(IKhoService service)
    {
        _service = service;
    }

    /// <summary>Danh sách kho (để chọn kho khi lập phiếu nhập).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<KhoDto>>>> GetAllKho()
    {
        var result = await _service.GetAllKhoAsync();
        return Ok(ApiResponse<IEnumerable<KhoDto>>.SuccessResponse(result));
    }

    /// <summary>
    /// Xem tồn kho hiện hành của mọi linh kiện. Query: khoId, keyword (mã hoặc tên), chiSapHet.
    /// Mọi nhân viên đều xem được (không chứa giá vốn).
    /// </summary>
    [HttpGet("ton-kho")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TonKhoDto>>>> GetTonKho(
        [FromQuery] long? khoId = null,
        [FromQuery] string? keyword = null,
        [FromQuery] bool chiSapHet = false)
    {
        var result = await _service.GetTonKhoAsync(khoId, keyword, chiSapHet);
        return Ok(ApiResponse<IEnumerable<TonKhoDto>>.SuccessResponse(result));
    }

    /// <summary>Cảnh báo các linh kiện có tồn dưới hoặc bằng mức tối thiểu. Yêu cầu role Admin hoặc Thủ kho.</summary>
    [HttpGet("canh-bao-het-hang")]
    [Authorize(Roles = QuanLyKhoRoles)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CanhBaoTonKhoDto>>>> GetCanhBao([FromQuery] long? khoId = null)
    {
        var result = await _service.GetCanhBaoAsync(khoId);
        return Ok(ApiResponse<IEnumerable<CanhBaoTonKhoDto>>.SuccessResponse(result));
    }

    /// <summary>Đặt mức tồn tối thiểu (ngưỡng cảnh báo) cho một linh kiện trong một kho. Yêu cầu role Admin hoặc Thủ kho.</summary>
    [HttpPut("ton-kho/{khoId:long}/{linhKienId:long}/muc-ton-toi-thieu")]
    [Authorize(Roles = QuanLyKhoRoles)]
    public async Task<ActionResult<ApiResponse<TonKhoDto>>> CapNhatMucTonToiThieu(
        long khoId, long linhKienId, [FromBody] CapNhatMucTonToiThieuDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<TonKhoDto>.ErrorResponse("Dữ liệu không hợp lệ."));

        try
        {
            var result = await _service.CapNhatMucTonToiThieuAsync(khoId, linhKienId, dto.MucTonToiThieu);
            return Ok(ApiResponse<TonKhoDto>.SuccessResponse(result, "Cập nhật mức tồn tối thiểu thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TonKhoDto>.ErrorResponse(ex.Message));
        }
    }
}
