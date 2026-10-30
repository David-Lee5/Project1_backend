using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController]
[Route("api/hoa-don")]
[Authorize(Roles = Roles.Admin + "," + Roles.LeTan + "," + Roles.KhachHang)]
public class HoaDonController : ControllerBase
{
    private const string WriteRoles = Roles.Admin + "," + Roles.LeTan;
    private readonly BillingService _service;
    public HoaDonController(BillingService service) => _service = service;

    private long AccountId => long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new UnauthorizedAccessException("Không xác định được tài khoản.");

    /// <summary>Khách hàng chỉ được xem hóa đơn của mình; lễ tân và quản trị viên xem tất cả.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BillingInvoiceDto>>>> GetAll(
        [FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<IReadOnlyList<BillingInvoiceDto>>.SuccessResponse(
            await _service.GetAllAsync(AccountId, status, page, pageSize, cancellationToken)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<BillingInvoiceDto>>> GetById(long id, CancellationToken cancellationToken) =>
        Ok(ApiResponse<BillingInvoiceDto>.SuccessResponse(await _service.GetByIdAsync(id, AccountId, cancellationToken)));

    /// <summary>Lễ tân lập hóa đơn từ phiếu đã hoàn thành. Số tiền do database chốt từ chi tiết sửa chữa.</summary>
    [HttpPost]
    [Authorize(Roles = Roles.LeTan)]
    public async Task<ActionResult<ApiResponse<BillingInvoiceDto>>> Create(CreateInvoiceDto dto, CancellationToken cancellationToken)
    {
        var invoice = await _service.CreateAsync(dto, AccountId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = invoice.Id },
            ApiResponse<BillingInvoiceDto>.SuccessResponse(invoice, "Lập hóa đơn thành công."));
    }

    [HttpPost("{id:long}/thanh-toan")]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<BillingInvoiceDto>>> Pay(long id, PayInvoiceDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<BillingInvoiceDto>.SuccessResponse(await _service.PayAsync(id, dto, AccountId, cancellationToken), "Thanh toán thành công."));

    [HttpPost("{id:long}/huy")]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<BillingInvoiceDto>>> Cancel(long id, CancelInvoiceDto dto, CancellationToken cancellationToken) =>
        Ok(ApiResponse<BillingInvoiceDto>.SuccessResponse(await _service.CancelAsync(id, dto, AccountId, cancellationToken), "Đã hủy hóa đơn."));
}
