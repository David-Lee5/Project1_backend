using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/khach-hang")]
[Authorize(Roles = Roles.NhanVien + "," + Roles.KhachHang)]
public class KhachHangController : ControllerBase
{
    private readonly CustomerDeviceService _service;

    public KhachHangController(CustomerDeviceService service) => _service = service;

    /// <summary>Nhân viên tìm khách hàng; khách hàng chỉ thấy hồ sơ của mình.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<CustomerDevicePage<KhachHangDto>>>> GetAll(
        [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(ApiResponse<CustomerDevicePage<KhachHangDto>>.SuccessResponse(
            await _service.GetCustomersAsync(Actor(), search, page, pageSize)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<KhachHangDto>>> GetById(long id)
        => Ok(ApiResponse<KhachHangDto>.SuccessResponse(await _service.GetCustomerAsync(id, Actor())));

    /// <summary>Tạo khách hàng chưa có tài khoản. Đăng ký tài khoản qua /api/auth/register.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<KhachHangDto>>> Create(UpsertKhachHangDto input)
    {
        var customer = await _service.CreateCustomerAsync(input, Actor());
        return CreatedAtAction(nameof(GetById), new { id = customer.Id },
            ApiResponse<KhachHangDto>.SuccessResponse(customer, "Tạo khách hàng thành công."));
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ApiResponse<KhachHangDto>>> Update(long id, UpsertKhachHangDto input)
        => Ok(ApiResponse<KhachHangDto>.SuccessResponse(
            await _service.UpdateCustomerAsync(id, input, Actor()), "Cập nhật khách hàng thành công."));

    [HttpDelete("{id:long}")]
    [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<bool>>> Delete(long id)
    {
        await _service.DeleteCustomerAsync(id, Actor());
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xoá khách hàng thành công."));
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
