using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController, Route("api/xuat-kho")]
[Authorize(Roles = Roles.NhanVien)]
public class XuatKhoController : ControllerBase
{
    private readonly InventoryService _service;
    public XuatKhoController(InventoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DispatchDto>>>> GetAll(long? phieuId = null, int skip = 0, int take = 100) =>
        Ok(ApiResponse<List<DispatchDto>>.SuccessResponse(await _service.GetDispatchesAsync(phieuId, skip, take)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<DispatchDto>>> GetById(long id) =>
        Ok(ApiResponse<DispatchDto>.SuccessResponse(await _service.GetDispatchAsync(id)));

    /// <summary>Xuất linh kiện cho phiếu đang sửa/chờ linh kiện; thủ tục SQL bảo vệ tồn không âm.</summary>
    [HttpPost, Authorize(Roles = Roles.ThuKho)]
    public async Task<ActionResult<ApiResponse<DispatchDto>>> Create(DispatchRequestDto dto)
    {
        var result = await _service.DispatchAsync(dto, AccountId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<DispatchDto>.SuccessResponse(result));
    }

    private long AccountId() => long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : throw new UnauthorizedAccessException("Không xác định được tài khoản.");
}
