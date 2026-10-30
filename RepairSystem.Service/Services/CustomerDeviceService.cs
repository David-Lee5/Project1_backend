using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Service.Services;

/// <summary>Thông tin phạm vi truy cập được controller lấy từ JWT đã xác thực.</summary>
public sealed record CustomerDeviceActor(long AccountId, bool IsStaff, bool CanManageCustomers);

public class CustomerDeviceService
{
    private readonly AppDbContext _db;

    public CustomerDeviceService(AppDbContext db) => _db = db;

    public async Task<CustomerDevicePage<KhachHangDto>> GetCustomersAsync(
        CustomerDeviceActor actor, string? search = null, int page = 1, int pageSize = 20)
    {
        ValidatePage(page, pageSize);
        var query = Customers(actor).AsNoTracking();
        search = Clean(search);
        if (search is not null)
            query = query.Where(x => x.HoTen.Contains(search) || x.MaKh.Contains(search)
                || x.SoDienThoai.Contains(search) || (x.Email != null && x.Email.Contains(search)));
        return new CustomerDevicePage<KhachHangDto>
        {
            Page = page, PageSize = pageSize, TotalCount = await query.CountAsync(),
            Items = await query.OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => CustomerDto(x)).ToListAsync()
        };
    }

    public async Task<KhachHangDto> GetCustomerAsync(long id, CustomerDeviceActor actor)
        => CustomerDto(await FindCustomer(id, actor, tracking: false));

    public async Task<KhachHangDto> CreateCustomerAsync(UpsertKhachHangDto input, CustomerDeviceActor actor)
    {
        RequireManager(actor);
        Validate(input);
        await CheckEmail(input.Email);
        var now = DateTime.UtcNow;
        var customer = new KhachHang
        {
            HoTen = input.HoTen.Trim(), SoDienThoai = input.SoDienThoai.Trim(),
            Email = Clean(input.Email), DiaChi = Clean(input.DiaChi),
            VaiTroTk = Roles.KhachHang, CreatedAt = now, UpdatedAt = now
        };
        _db.KhachHangs.Add(customer);
        await _db.SaveChangesAsync();
        return CustomerDto(customer);
    }

    public async Task<KhachHangDto> UpdateCustomerAsync(long id, UpsertKhachHangDto input, CustomerDeviceActor actor)
    {
        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        var customer = await FindCustomer(id, actor);
        RequireCustomerWrite(actor, customer);
        Validate(input);
        await CheckEmail(input.Email, id);
        customer.HoTen = input.HoTen.Trim();
        customer.SoDienThoai = input.SoDienThoai.Trim();
        customer.Email = Clean(input.Email);
        customer.DiaChi = Clean(input.DiaChi);
        customer.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return CustomerDto(customer);
    }

    public async Task DeleteCustomerAsync(long id, CustomerDeviceActor actor)
    {
        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        RequireManager(actor);
        var customer = await FindCustomer(id, actor);
        if (await _db.PhieuSuaChuas.AnyAsync(x => x.ThietBi.KhachHangId == id
                && x.TrangThai != "HUY" && x.TrangThai != "DA_TRA"))
            throw new InvalidOperationException("Khách hàng vẫn có phiếu sửa chữa chưa kết thúc.");
        customer.DeletedAt = customer.UpdatedAt = DateTime.UtcNow;
        if (customer.TaiKhoanId is long accountId)
        {
            var account = await _db.TaiKhoans.FindAsync(accountId);
            if (account is not null)
            {
                account.HoatDong = false;
                account.UpdatedAt = DateTime.UtcNow;
            }
            var refreshTokens = await _db.RefreshTokens.Where(x => x.TaiKhoanId == accountId && x.ThuHoiLuc == null).ToListAsync();
            foreach (var token in refreshTokens) token.ThuHoiLuc = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
    }

    public async Task<CustomerDevicePage<ThietBiDto>> GetDevicesAsync(CustomerDeviceActor actor,
        long? khachHangId = null, string? search = null, int page = 1, int pageSize = 20)
    {
        ValidatePage(page, pageSize);
        var query = Devices(actor).AsNoTracking();
        if (khachHangId.HasValue) query = query.Where(x => x.KhachHangId == khachHangId.Value);
        search = Clean(search);
        if (search is not null)
            query = query.Where(x => x.TenThietBi.Contains(search) || x.MaThietBi.Contains(search)
                || (x.SoSerial != null && x.SoSerial.Contains(search)));
        return new CustomerDevicePage<ThietBiDto>
        {
            Page = page, PageSize = pageSize, TotalCount = await query.CountAsync(),
            Items = await query.Include(x => x.KhachHang).OrderBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => DeviceDto(x)).ToListAsync()
        };
    }

    public async Task<ThietBiDto> GetDeviceAsync(long id, CustomerDeviceActor actor)
        => DeviceDto(await FindDevice(id, actor, tracking: false));

    public async Task<ThietBiDto> CreateDeviceAsync(UpsertThietBiDto input, CustomerDeviceActor actor)
    {
        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        Validate(input);
        var customer = await FindCustomer(input.KhachHangId, actor);
        RequireCustomerWrite(actor, customer);
        await CheckSerial(input.SoSerial);
        var now = DateTime.UtcNow;
        var device = new ThietBi
        {
            KhachHangId = customer.Id, KhachHang = customer, TenThietBi = input.TenThietBi.Trim(),
            LoaiMay = Clean(input.LoaiMay), SoSerial = Clean(input.SoSerial), NgayMua = input.NgayMua,
            GhiChu = Clean(input.GhiChu), CreatedAt = now, UpdatedAt = now
        };
        _db.ThietBis.Add(device);
        await _db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return DeviceDto(device);
    }

    public async Task<ThietBiDto> UpdateDeviceAsync(long id, UpsertThietBiDto input, CustomerDeviceActor actor)
    {
        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        var device = await FindDevice(id, actor);
        RequireCustomerWrite(actor, device.KhachHang);
        Validate(input);
        var customer = await FindCustomer(input.KhachHangId, actor);
        // Ownership is part of the repair/invoice history and must not be reassigned by a profile edit.
        if (device.KhachHangId != customer.Id)
            throw new InvalidOperationException("Không thể chuyển thiết bị sang khách hàng khác qua thao tác cập nhật.");
        await CheckSerial(input.SoSerial, id);
        device.TenThietBi = input.TenThietBi.Trim();
        device.LoaiMay = Clean(input.LoaiMay);
        device.SoSerial = Clean(input.SoSerial);
        device.NgayMua = input.NgayMua;
        device.GhiChu = Clean(input.GhiChu);
        device.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return DeviceDto(device);
    }

    public async Task DeleteDeviceAsync(long id, CustomerDeviceActor actor)
    {
        await using var transaction = _db.Database.IsRelational()
            ? await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable) : null;
        var device = await FindDevice(id, actor);
        RequireCustomerWrite(actor, device.KhachHang);
        if (await _db.PhieuSuaChuas.AnyAsync(x => x.ThietBiId == id && x.TrangThai != "HUY" && x.TrangThai != "DA_TRA"))
            throw new InvalidOperationException("Thiết bị vẫn có phiếu sửa chữa chưa kết thúc.");
        device.DeletedAt = device.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
    }

    private IQueryable<KhachHang> Customers(CustomerDeviceActor actor)
    {
        RequireActor(actor);
        var query = _db.KhachHangs.Where(x => x.DeletedAt == null);
        return actor.IsStaff ? query : query.Where(x => x.TaiKhoanId == actor.AccountId);
    }

    private IQueryable<ThietBi> Devices(CustomerDeviceActor actor)
    {
        RequireActor(actor);
        var query = _db.ThietBis.Where(x => x.DeletedAt == null && x.KhachHang.DeletedAt == null);
        return actor.IsStaff ? query : query.Where(x => x.KhachHang.TaiKhoanId == actor.AccountId);
    }

    private async Task<KhachHang> FindCustomer(long id, CustomerDeviceActor actor, bool tracking = true)
    {
        var query = Customers(actor);
        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy khách hàng.");
    }

    private async Task<ThietBi> FindDevice(long id, CustomerDeviceActor actor, bool tracking = true)
    {
        var query = Devices(actor).Include(x => x.KhachHang).AsQueryable();
        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(x => x.Id == id)
            ?? throw new KeyNotFoundException("Không tìm thấy thiết bị.");
    }

    private async Task CheckEmail(string? email, long exceptId = 0)
    {
        email = Clean(email);
        if (email is not null && await _db.KhachHangs.AnyAsync(x => x.Id != exceptId
                && x.DeletedAt == null && x.Email != null && x.Email.ToLower() == email.ToLower()))
            throw new InvalidOperationException("Email đã được một khách hàng khác sử dụng.");
    }

    private async Task CheckSerial(string? serial, long exceptId = 0)
    {
        serial = Clean(serial);
        if (serial is not null && await _db.ThietBis.AnyAsync(x => x.Id != exceptId
                && x.DeletedAt == null && x.SoSerial != null && x.SoSerial.ToLower() == serial.ToLower()))
            throw new InvalidOperationException("Số serial đã được một thiết bị khác sử dụng.");
    }

    private static void RequireActor(CustomerDeviceActor actor)
    {
        if (actor.AccountId <= 0) throw new UnauthorizedAccessException("Không xác định được tài khoản.");
    }

    private static void RequireManager(CustomerDeviceActor actor)
    {
        RequireActor(actor);
        if (!actor.CanManageCustomers) throw new UnauthorizedAccessException("Bạn không có quyền quản lý khách hàng.");
    }

    private static void RequireCustomerWrite(CustomerDeviceActor actor, KhachHang customer)
    {
        if (!actor.CanManageCustomers && customer.TaiKhoanId != actor.AccountId)
            throw new UnauthorizedAccessException("Chỉ quản trị viên, lễ tân hoặc chính khách hàng được cập nhật hồ sơ và thiết bị.");
    }

    private static void ValidatePage(int page, int pageSize)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100 || page > int.MaxValue / pageSize)
            throw new ArgumentException("Trang phải từ 1 và kích thước trang từ 1 đến 100.");
    }

    private static void Validate(object input)
    {
        switch (input)
        {
            case UpsertKhachHangDto customer:
                customer.HoTen = customer.HoTen?.Trim()!;
                customer.SoDienThoai = customer.SoDienThoai?.Trim()!;
                customer.Email = Clean(customer.Email);
                customer.DiaChi = Clean(customer.DiaChi);
                break;
            case UpsertThietBiDto device:
                device.TenThietBi = device.TenThietBi?.Trim()!;
                device.LoaiMay = Clean(device.LoaiMay);
                device.SoSerial = Clean(device.SoSerial);
                device.GhiChu = Clean(device.GhiChu);
                break;
        }
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), errors, true))
            throw new ArgumentException(string.Join(" ", errors.Select(x => x.ErrorMessage)));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static KhachHangDto CustomerDto(KhachHang x) => new()
    {
        Id = x.Id, MaKh = x.MaKh, TaiKhoanId = x.TaiKhoanId, HoTen = x.HoTen,
        SoDienThoai = x.SoDienThoai, Email = x.Email, DiaChi = x.DiaChi,
        CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };

    private static ThietBiDto DeviceDto(ThietBi x) => new()
    {
        Id = x.Id, MaThietBi = x.MaThietBi, KhachHangId = x.KhachHangId, MaKh = x.KhachHang.MaKh,
        TenThietBi = x.TenThietBi, LoaiMay = x.LoaiMay, SoSerial = x.SoSerial, NgayMua = x.NgayMua,
        GhiChu = x.GhiChu, CreatedAt = x.CreatedAt, UpdatedAt = x.UpdatedAt
    };
}
