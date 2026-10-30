using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/thiet-bi")]
[Authorize(Roles = Roles.NhanVien + "," + Roles.KhachHang)]
public class ThietBiController : ControllerBase
{
    private readonly CustomerDeviceService _service;

    public ThietBiController(CustomerDeviceService service) => _service = service;

    /// <summary>Lọc thiết bị theo chủ sở hữu và tìm theo mã, tên hoặc serial.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CustomerDevicePage<ThietBiDto>>>> GetAll(
        [FromQuery] long? khachHangId = null, [FromQuery] string? search = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(ApiResponse<CustomerDevicePage<ThietBiDto>>.SuccessResponse(
            await _service.GetDevicesAsync(Actor(), khachHangId, search, page, pageSize)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<ThietBiDto>>> GetById(long id)
        => Ok(ApiResponse<ThietBiDto>.SuccessResponse(await _service.GetDeviceAsync(id, Actor())));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ThietBiDto>>> Create(UpsertThietBiDto input)
    {
        var device = await _service.CreateDeviceAsync(input, Actor());
        return CreatedAtAction(nameof(GetById), new { id = device.Id },
            ApiResponse<ThietBiDto>.SuccessResponse(device, "Tạo thiết bị thành công."));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<ThietBiDto>>> Update(long id, UpsertThietBiDto input)
        => Ok(ApiResponse<ThietBiDto>.SuccessResponse(
            await _service.UpdateDeviceAsync(id, input, Actor()), "Cập nhật thiết bị thành công."));

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        await _service.DeleteDeviceAsync(id, Actor());
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xoá thiết bị thành công."));
    }

    private CustomerDeviceActor Actor()
    {
        var id = User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!long.TryParse(id, out var accountId) || accountId <= 0)
            throw new UnauthorizedAccessException("Không xác định được tài khoản.");
        return new CustomerDeviceActor(accountId,
            Roles.NhanVien.Split(',').Any(User.IsInRole), User.IsInRole(Roles.Admin) || User.IsInRole(Roles.LeTan));
    }
}
