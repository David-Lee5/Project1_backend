using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Service.Services;

/// <summary>Invoices snapshot repair costs in SQL; clients cannot supply or amend totals.</summary>
public class BillingService
{
    private const string Unpaid = "CHUA_THANH_TOAN";
    private static readonly string[] Statuses = [Unpaid, "DA_THANH_TOAN", "HUY"];
    private static readonly string[] PaymentMethods = ["TIEN_MAT", "CHUYEN_KHOAN", "THE", "VI_DIEN_TU"];
    private readonly AppDbContext _context;

    public BillingService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<BillingInvoiceDto>> GetAllAsync(long accountId, string? status = null,
        int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        if (page < 1 || page > 1_000_000 || pageSize is < 1 or > 100)
            throw new ArgumentException("Trang phải lớn hơn 0, kích thước trang từ 1 đến 100.");
        if (status != null && !Statuses.Contains(status))
            throw new ArgumentException("Trạng thái hóa đơn không hợp lệ.");
        var query = await ReadableInvoicesAsync(accountId, cancellationToken);
        if (status != null) query = query.Where(x => x.TrangThai == status);
        return await Project(query.OrderByDescending(x => x.NgayLap).ThenByDescending(x => x.Id))
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
    }

    public async Task<BillingInvoiceDto> GetByIdAsync(long id, long accountId,
        CancellationToken cancellationToken = default)
    {
        var query = await ReadableInvoicesAsync(accountId, cancellationToken);
        return await Project(query.Where(x => x.Id == id)).SingleOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException("Không tìm thấy hóa đơn.");
    }

    public async Task<BillingInvoiceDto> CreateAsync(CreateInvoiceDto dto, long accountId,
        CancellationToken cancellationToken = default)
    {
        if (dto.PhieuId <= 0 || dto.GiamGia < 0 || dto.GiamGia > 999999999999.99m ||
            decimal.Round(dto.GiamGia, 2) != dto.GiamGia)
            throw new ArgumentException("Phiếu hoặc giảm giá không hợp lệ; số tiền có tối đa hai chữ số thập phân.");
        var actor = await ReceptionistAsync(accountId, cancellationToken);
        if (!await _context.PhieuSuaChuas.AnyAsync(x => x.Id == dto.PhieuId &&
                x.ThietBi.DeletedAt == null && x.ThietBi.KhachHang.DeletedAt == null, cancellationToken))
            throw new KeyNotFoundException("Không tìm thấy phiếu sửa chữa còn hiệu lực.");

        var invoiceId = new SqlParameter("@hoa_don_id", SqlDbType.BigInt) { Direction = ParameterDirection.Output };
        var discount = new SqlParameter("@giam_gia", SqlDbType.Decimal)
            { Precision = 14, Scale = 2, Value = dto.GiamGia };
        try
        {
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_lap_hoa_don @phieu_id, @le_tan_id, @giam_gia, @hoa_don_id OUTPUT",
                [new SqlParameter("@phieu_id", SqlDbType.BigInt) { Value = dto.PhieuId },
                 new SqlParameter("@le_tan_id", SqlDbType.BigInt) { Value = actor }, discount, invoiceId],
                cancellationToken);
        }
        catch (SqlException ex) when (ex.Number == 50030)
        {
            throw new KeyNotFoundException("Không tìm thấy phiếu sửa chữa.", ex);
        }
        catch (SqlException ex) when (ex.Number == 50032)
        {
            throw new ArgumentException("Giảm giá không được vượt quá tổng chi phí sửa chữa.", ex);
        }
        catch (SqlException ex) when (ex.Number is 50031 or 2601 or 2627)
        {
            throw new InvalidOperationException("Phiếu chưa hoàn thành hoặc đã có hóa đơn còn hiệu lực.", ex);
        }
        return await GetByIdAsync(Convert.ToInt64(invoiceId.Value), accountId, cancellationToken);
    }

    public async Task<BillingInvoiceDto> PayAsync(long id, PayInvoiceDto dto, long accountId,
        CancellationToken cancellationToken = default)
    {
        await BillingStaffAsync(accountId, cancellationToken);
        if (!PaymentMethods.Contains(dto.PhuongThucTt))
            throw new ArgumentException("Phương thức thanh toán không hợp lệ.");
        var now = DateTime.UtcNow;
        // The condition is evaluated by SQL in the UPDATE, so concurrent payment/cancel requests cannot both succeed.
        var affected = await _context.HoaDons.Where(x => x.Id == id && x.TrangThai == Unpaid)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.TrangThai, "DA_THANH_TOAN")
                .SetProperty(x => x.PhuongThucTt, dto.PhuongThucTt)
                .SetProperty(x => x.NgayThanhToan, now)
                .SetProperty(x => x.UpdatedAt, now), cancellationToken);
        await RequireTransitionAsync(id, affected, cancellationToken);
        return await GetByIdAsync(id, accountId, cancellationToken);
    }

    public async Task<BillingInvoiceDto> CancelAsync(long id, CancelInvoiceDto dto, long accountId,
        CancellationToken cancellationToken = default)
    {
        await BillingStaffAsync(accountId, cancellationToken);
        var reason = dto.LyDo?.Trim();
        if (reason == null || reason.Length is < 3 or > 2000)
            throw new ArgumentException("Lý do hủy phải có từ 3 đến 2000 ký tự.");
        var now = DateTime.UtcNow;
        var affected = await _context.HoaDons.Where(x => x.Id == id && x.TrangThai == Unpaid)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.TrangThai, "HUY")
                .SetProperty(x => x.GhiChu, reason)
                .SetProperty(x => x.UpdatedAt, now), cancellationToken);
        await RequireTransitionAsync(id, affected, cancellationToken);
        return await GetByIdAsync(id, accountId, cancellationToken);
    }

    private async Task RequireTransitionAsync(long id, int affected, CancellationToken cancellationToken)
    {
        if (affected == 1) return;
        if (!await _context.HoaDons.AnyAsync(x => x.Id == id, cancellationToken))
            throw new KeyNotFoundException("Không tìm thấy hóa đơn.");
        throw new InvalidOperationException("Chỉ được thanh toán hoặc hủy hóa đơn chưa thanh toán.");
    }

    private async Task<IQueryable<HoaDon>> ReadableInvoicesAsync(long accountId, CancellationToken cancellationToken)
    {
        var account = await ActiveAccountAsync(accountId, cancellationToken);
        var query = _context.HoaDons.AsNoTracking();
        if (account.VaiTro == Roles.KhachHang)
            return query.Where(x => x.Phieu.ThietBi.KhachHang.TaiKhoanId == accountId &&
                                    x.Phieu.ThietBi.KhachHang.DeletedAt == null);
        await BillingStaffAsync(accountId, cancellationToken);
        return query;
    }

    private async Task<TaiKhoan> ActiveAccountAsync(long accountId, CancellationToken cancellationToken) =>
        await _context.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == accountId && x.HoatDong, cancellationToken)
        ?? throw new UnauthorizedAccessException("Tài khoản không còn hoạt động.");

    private async Task<NhanVien> BillingStaffAsync(long accountId, CancellationToken cancellationToken)
    {
        var account = await ActiveAccountAsync(accountId, cancellationToken);
        if (account.VaiTro != Roles.Admin && account.VaiTro != Roles.LeTan)
            throw new UnauthorizedAccessException("Chỉ quản trị viên hoặc lễ tân được quản lý hóa đơn.");
        return await _context.NhanViens.AsNoTracking().SingleOrDefaultAsync(x => x.TaiKhoanId == accountId &&
                x.DeletedAt == null && x.ChucVu == account.VaiTro, cancellationToken)
            ?? throw new UnauthorizedAccessException("Không tìm thấy hồ sơ nhân viên đang hoạt động.");
    }

    private async Task<long> ReceptionistAsync(long accountId, CancellationToken cancellationToken)
    {
        var employee = await BillingStaffAsync(accountId, cancellationToken);
        if (employee.ChucVu != Roles.LeTan ||
            !await _context.LeTans.AnyAsync(x => x.NhanVienId == employee.Id && x.ChucVu == Roles.LeTan, cancellationToken))
            throw new UnauthorizedAccessException("Lập hóa đơn yêu cầu hồ sơ lễ tân của chính tài khoản đăng nhập.");
        return employee.Id;
    }

    private static IQueryable<BillingInvoiceDto> Project(IQueryable<HoaDon> query) => query.Select(x => new BillingInvoiceDto
    {
        Id = x.Id, MaHd = x.MaHd, PhieuId = x.PhieuId, MaPhieu = x.Phieu.MaPhieu,
        LeTanId = x.LeTanId, NgayLap = x.NgayLap, TienDichVu = x.TienDichVu,
        TienLinhKien = x.TienLinhKien, GiamGia = x.GiamGia, TongThanhToan = x.TongThanhToan,
        TrangThai = x.TrangThai, PhuongThucTt = x.PhuongThucTt, NgayThanhToan = x.NgayThanhToan,
        GhiChu = x.GhiChu, UpdatedAt = x.UpdatedAt
    });
}
