using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Controllers;

[ApiController, Route("api/ton-kho")]
[Authorize(Roles = Roles.NhanVien)]
public class TonKhoController : ControllerBase
{
    private readonly InventoryService _service;
    public TonKhoController(InventoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<StockDto>>>> GetAll(long? khoId = null, bool lowOnly = false, int skip = 0, int take = 100) =>
        Ok(ApiResponse<List<StockDto>>.SuccessResponse(await _service.GetStockAsync(khoId, lowOnly, skip, take)));

    [HttpGet("bien-dong")]
    public async Task<ActionResult<ApiResponse<List<StockMovementDto>>>> GetMovements(long? khoId = null, long? linhKienId = null, int skip = 0, int take = 100) =>
        Ok(ApiResponse<List<StockMovementDto>>.SuccessResponse(await _service.GetMovementsAsync(khoId, linhKienId, skip, take)));

    [HttpPut("{khoId:long}/{linhKienId:long}/muc-ton-toi-thieu"), Authorize(Roles = Roles.ThuKho)]
    public async Task<ActionResult<ApiResponse<bool>>> SetThreshold(long khoId, long linhKienId, UpdateStockThresholdDto dto)
    {
        await _service.SetStockThresholdAsync(khoId, linhKienId, dto.MucTonToiThieu, AccountId());
        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    private long AccountId() => long.TryParse(User.FindFirstValue("UserId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
        ? id : throw new UnauthorizedAccessException("Không xác định được tài khoản.");
}
