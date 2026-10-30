using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;
using Xunit;

namespace RepairSystem.Tests.Customers;

public class CustomerDeviceServiceTests
{
    private static CustomerDeviceActor Customer(long id = 101) => new(id, false, false);
    private static readonly CustomerDeviceActor Receptionist = new(1, true, true);

    private static AppDbContext CreateDb() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString(), options => options.EnableNullChecks(false)).Options);

    private static KhachHang Profile(long id, long? account = null, string? email = null) => new()
    {
        Id = id, TaiKhoanId = account, MaKh = $"KH{id:000000}", HoTen = $"Khách {id}",
        SoDienThoai = "0900000000", Email = email, VaiTroTk = Roles.KhachHang
    };

    private static ThietBi Device(long id, KhachHang customer, string? serial = null) => new()
    {
        Id = id, KhachHangId = customer.Id, KhachHang = customer,
        MaThietBi = $"TB{id:000000}", TenThietBi = "Laptop", SoSerial = serial
    };

    [Fact]
    public async Task Customer_lists_only_own_profile_and_devices()
    {
        await using var db = CreateDb();
        var own = Profile(1, 101);
        var other = Profile(2, 102);
        db.AddRange(own, other, Device(1, own), Device(2, other));
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);

        var customers = await service.GetCustomersAsync(Customer());
        var devices = await service.GetDevicesAsync(Customer());

        Assert.Equal(1, customers.TotalCount);
        Assert.Equal(1, Assert.Single(customers.Items).Id);
        Assert.Equal(1, devices.TotalCount);
        Assert.Equal(1, Assert.Single(devices.Items).Id);
        Assert.Empty((await service.GetDevicesAsync(Customer(), khachHangId: 2)).Items);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetCustomerAsync(2, Customer()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetDeviceAsync(2, Customer()));
    }

    [Fact]
    public async Task Customer_cannot_modify_another_profile_or_create_device_for_it()
    {
        await using var db = CreateDb();
        db.AddRange(Profile(1, 101), Profile(2, 102));
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateCustomerAsync(2,
            new UpsertKhachHangDto { HoTen = "Changed", SoDienThoai = "0900000000" }, Customer()));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.CreateDeviceAsync(
            new UpsertThietBiDto { KhachHangId = 2, TenThietBi = "Device" }, Customer()));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateCustomerAsync(
            new UpsertKhachHangDto { HoTen = "New", SoDienThoai = "0900000000" }, Customer()));
        Assert.Empty(db.ThietBis);
        Assert.Equal("Khách 2", (await db.KhachHangs.FindAsync(2L))!.HoTen);
    }

    [Theory]
    [InlineData(Roles.KyThuatVien)]
    [InlineData(Roles.ThuKho)]
    public async Task Nonmanager_staff_can_read_but_cannot_edit_customers_or_devices(string role)
    {
        await using var db = CreateDb();
        var customer = Profile(1, 101);
        db.AddRange(customer, Device(1, customer));
        await db.SaveChangesAsync();
        var staff = new CustomerDeviceActor(role == Roles.KyThuatVien ? 201 : 202, true, false);
        var service = new CustomerDeviceService(db);
        var deviceInput = new UpsertThietBiDto { KhachHangId = 1, TenThietBi = "Changed" };

        Assert.Single((await service.GetCustomersAsync(staff)).Items);
        Assert.Single((await service.GetDevicesAsync(staff)).Items);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateCustomerAsync(1,
            new UpsertKhachHangDto { HoTen = "Changed", SoDienThoai = "0900000000" }, staff));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateDeviceAsync(deviceInput, staff));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.UpdateDeviceAsync(1, deviceInput, staff));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteDeviceAsync(1, staff));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.DeleteCustomerAsync(1, staff));
        Assert.Equal("Khách 1", customer.HoTen);
        Assert.Equal("Laptop", (await db.ThietBis.SingleAsync()).TenThietBi);
        Assert.Null(customer.DeletedAt);
    }

    [Fact]
    public async Task Customer_can_update_own_profile_and_device()
    {
        await using var db = CreateDb();
        var customer = Profile(1, 101);
        db.AddRange(customer, Device(1, customer));
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);

        var profile = await service.UpdateCustomerAsync(1,
            new UpsertKhachHangDto { HoTen = "Updated owner", SoDienThoai = "0911111111" }, Customer());
        var device = await service.UpdateDeviceAsync(1,
            new UpsertThietBiDto { KhachHangId = 1, TenThietBi = "Updated device", SoSerial = "NEW-SERIAL" }, Customer());

        Assert.Equal("Updated owner", profile.HoTen);
        Assert.Equal("Updated device", device.TenThietBi);
        Assert.Equal("NEW-SERIAL", device.SoSerial);
        Assert.Equal(customer.Id, device.KhachHangId);
    }

    [Fact]
    public async Task New_customer_is_unlinked_and_has_fixed_customer_role()
    {
        await using var db = CreateDb();
        var service = new CustomerDeviceService(db);
        var result = await service.CreateCustomerAsync(new UpsertKhachHangDto
        {
            HoTen = "  Nguyễn Văn A  ", SoDienThoai = "0900000000", Email = "a@example.com", DiaChi = "  Hà Nội  "
        }, Receptionist);

        var stored = await db.KhachHangs.SingleAsync();
        Assert.Equal("Nguyễn Văn A", result.HoTen);
        Assert.Equal("Hà Nội", result.DiaChi);
        Assert.Null(stored.TaiKhoanId);
        Assert.Equal(Roles.KhachHang, stored.VaiTroTk);
        Assert.True(stored.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task Duplicate_active_email_is_rejected_without_mutating_profile()
    {
        await using var db = CreateDb();
        db.AddRange(Profile(1, 101, "first@example.com"), Profile(2, 102, "other@example.com"));
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateCustomerAsync(1,
            new UpsertKhachHangDto { HoTen = "Changed", SoDienThoai = "0900000000", Email = "OTHER@example.com" }, Customer()));

        Assert.Equal("first@example.com", (await db.KhachHangs.FindAsync(1L))!.Email);
        Assert.Equal("Khách 1", (await db.KhachHangs.FindAsync(1L))!.HoTen);
    }

    [Fact]
    public async Task Duplicate_serial_is_rejected_but_serial_of_deleted_device_can_be_reused()
    {
        await using var db = CreateDb();
        var own = Profile(1, 101);
        var old = Device(1, own, "SERIAL-1");
        db.AddRange(own, old);
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);
        var request = new UpsertThietBiDto { KhachHangId = 1, TenThietBi = "Replacement", SoSerial = "serial-1" };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateDeviceAsync(request, Customer()));
        old.DeletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var created = await service.CreateDeviceAsync(request, Customer());

        Assert.NotEqual(old.Id, created.Id);
        Assert.Equal("serial-1", created.SoSerial);
    }

    [Fact]
    public async Task Device_owner_cannot_be_reassigned_by_profile_edit()
    {
        await using var db = CreateDb();
        var own = Profile(1, 101);
        var other = Profile(2, 102);
        db.AddRange(own, other, Device(1, own));
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateDeviceAsync(1,
            new UpsertThietBiDto { KhachHangId = 2, TenThietBi = "Changed" }, Receptionist));

        Assert.Equal(1, (await db.ThietBis.FindAsync(1L))!.KhachHangId);
    }

    [Theory]
    [InlineData("TIEP_NHAN")]
    [InlineData("DANG_SUA")]
    [InlineData("CHO_LINH_KIEN")]
    [InlineData("HOAN_THANH")]
    public async Task Unfinished_repair_prevents_customer_and_device_deletion(string status)
    {
        await using var db = CreateDb();
        var own = Profile(1, 101);
        var device = Device(1, own);
        db.AddRange(own, device, new PhieuSuaChua
        {
            Id = 1, MaPhieu = "PSC1", ThietBi = device, ThietBiId = device.Id,
            TrangThai = status, TinhTrangLoi = "Lỗi"
        });
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteCustomerAsync(1, Receptionist));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteDeviceAsync(1, Customer()));
        Assert.Null(own.DeletedAt);
        Assert.Null(device.DeletedAt);
    }

    [Fact]
    public async Task Customer_deletion_disables_login_revokes_refresh_and_hides_devices()
    {
        await using var db = CreateDb();
        var account = new TaiKhoan { Id = 101, TenDangNhap = "customer", MatKhauHash = "hash", VaiTro = Roles.KhachHang, HoatDong = true };
        var own = Profile(1, account.Id);
        var token = new RefreshToken { TaiKhoanId = account.Id, TokenHash = "token-hash", HetHan = DateTime.UtcNow.AddDays(1) };
        db.AddRange(account, own, Device(1, own), token);
        await db.SaveChangesAsync();
        var service = new CustomerDeviceService(db);

        await service.DeleteCustomerAsync(1, Receptionist);

        Assert.NotNull(own.DeletedAt);
        Assert.False(account.HoatDong);
        Assert.NotNull(token.ThuHoiLuc);
        Assert.Empty((await service.GetCustomersAsync(Receptionist)).Items);
        Assert.Empty((await service.GetDevicesAsync(Receptionist)).Items);
    }

    [Fact]
    public async Task Staff_search_is_paginated_and_reports_total_matching_count()
    {
        await using var db = CreateDb();
        db.AddRange(Profile(1), Profile(2), Profile(3));
        await db.SaveChangesAsync();

        var page = await new CustomerDeviceService(db).GetCustomersAsync(Receptionist, "Khách", 2, 2);

        Assert.Equal(3, page.TotalCount);
        Assert.Equal(2, page.Page);
        Assert.Equal(3, Assert.Single(page.Items).Id);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 101)]
    [InlineData(int.MaxValue, 100)]
    public async Task Invalid_page_bounds_are_rejected(int page, int pageSize)
    {
        await using var db = CreateDb();
        await Assert.ThrowsAsync<ArgumentException>(() => new CustomerDeviceService(db)
            .GetCustomersAsync(Receptionist, page: page, pageSize: pageSize));
    }
}
