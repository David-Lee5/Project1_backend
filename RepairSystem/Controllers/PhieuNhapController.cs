using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController, Route("api/phieu-nhap")]
[Authorize(Roles = Roles.Admin + "," + Roles.ThuKho)]
public class PhieuNhapController : ControllerBase
{
    private readonly InventoryService _service;
    public PhieuNhapController(InventoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ReceiptDto>>>> GetAll(string? status = null, int skip = 0, int take = 100) =>
        Ok(ApiResponse<List<ReceiptDto>>.SuccessResponse(await _service.GetReceiptsAsync(status, skip, take)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> GetById(long id) =>
        Ok(ApiResponse<ReceiptDto>.SuccessResponse(await _service.GetReceiptAsync(id)));

    /// <summary>Tạo phiếu nhập nháp. Thủ kho thực hiện được suy ra từ tài khoản đăng nhập.</summary>
    [HttpPost, Authorize(Roles = Roles.ThuKho)]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> Create(UpsertReceiptDto dto)
    {
        var result = await _service.CreateReceiptAsync(dto, AccountId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<ReceiptDto>.SuccessResponse(result));
    }

    [HttpPut("{id:long}"), Authorize(Roles = Roles.ThuKho)]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> Update(long id, UpsertReceiptDto dto) =>
        Ok(ApiResponse<ReceiptDto>.SuccessResponse(await _service.UpdateReceiptAsync(id, dto, AccountId())));

    /// <summary>Cộng tồn và ghi biến động kho nguyên tử; không thể xác nhận lại.</summary>
    [HttpPost("{id:long}/xac-nhan"), Authorize(Roles = Roles.ThuKho)]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> Confirm(long id) =>
        Ok(ApiResponse<ReceiptDto>.SuccessResponse(await _service.ConfirmReceiptAsync(id, AccountId())));

    [HttpPost("{id:long}/huy"), Authorize(Roles = Roles.ThuKho)]
    public async Task<ActionResult<ApiResponse<ReceiptDto>>> Cancel(long id) =>
        Ok(ApiResponse<ReceiptDto>.SuccessResponse(await _service.CancelReceiptAsync(id, AccountId())));

    private long AccountId() => long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : throw new UnauthorizedAccessException("Không xác định được tài khoản.");
}
