using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;
using Xunit;

namespace RepairSystem.Tests.InventoryBilling;

public class BillingTests
{
    private static AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task Customer_cannot_list_or_fetch_another_customers_invoice()
    {
        await using var db = Context();
        db.HoaDons.AddRange(Invoice(1, 11), Invoice(2, 22));
        await db.SaveChangesAsync();
        var service = new BillingService(db);

        var own = await service.GetAllAsync(11);

        Assert.Equal(1, Assert.Single(own).Id);
        Assert.Equal("HD1", (await service.GetByIdAsync(1, 11)).MaHd);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.GetByIdAsync(2, 11));
    }

    [Fact]
    public async Task Disabled_account_and_deleted_staff_profile_cannot_read_billing_data()
    {
        await using var db = Context();
        var disabled = Account(1, Roles.KhachHang);
        disabled.HoatDong = false;
        db.TaiKhoans.Add(disabled);
        var staff = Staff(2, Roles.LeTan);
        staff.DeletedAt = DateTime.UtcNow;
        db.NhanViens.Add(staff);
        await db.SaveChangesAsync();
        var service = new BillingService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync(1));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.GetAllAsync(2));
    }

    [Fact]
    public async Task Admin_cannot_impersonate_receptionist_when_creating_invoice()
    {
        await using var db = Context();
        db.NhanViens.Add(Staff(1, Roles.Admin));
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new BillingService(db)
            .CreateAsync(new CreateInvoiceDto { PhieuId = 1 }, 1));
        Assert.Empty(await db.HoaDons.ToListAsync());
    }

    [Theory]
    [InlineData("0.001")]
    [InlineData("-1")]
    [InlineData("1000000000000")]
    public async Task Invalid_discount_is_rejected_before_invoice_creation(string amount)
    {
        await using var db = Context();
        var discount = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);
        await Assert.ThrowsAsync<ArgumentException>(() => new BillingService(db)
            .CreateAsync(new CreateInvoiceDto { PhieuId = 1, GiamGia = discount }, 1));
        Assert.Empty(await db.HoaDons.ToListAsync());
    }

    [Fact]
    public async Task Customer_cannot_pay_or_cancel_even_when_calling_service_directly()
    {
        await using var db = Context();
        db.HoaDons.Add(Invoice(1, 11));
        await db.SaveChangesAsync();
        var service = new BillingService(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.PayAsync(1,
            new PayInvoiceDto { PhuongThucTt = "TIEN_MAT" }, 11));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CancelAsync(1,
            new CancelInvoiceDto { LyDo = "Customer cancellation" }, 11));
        Assert.Equal("CHUA_THANH_TOAN", (await db.HoaDons.SingleAsync()).TrangThai);
    }

    private static TaiKhoan Account(long id, string role) => new()
    {
        Id = id, TenDangNhap = "user" + id, MatKhauHash = "unused-test-hash", VaiTro = role, HoatDong = true
    };

    private static NhanVien Staff(long id, string role) => new()
    {
        Id = id, MaNv = "NV" + id, TaiKhoanId = id, TaiKhoan = Account(id, role),
        ChucVu = role, HoTen = "Test staff", SoDienThoai = "0900000000"
    };

    private static HoaDon Invoice(long id, long accountId)
    {
        var customer = new KhachHang
        {
            Id = accountId, MaKh = "KH" + accountId, HoTen = "Test customer", SoDienThoai = "0900000000",
            VaiTroTk = Roles.KhachHang, TaiKhoanId = accountId, TaiKhoan = Account(accountId, Roles.KhachHang)
        };
        var device = new ThietBi
        {
            Id = id, MaThietBi = "TB" + id, TenThietBi = "Test device", KhachHangId = customer.Id, KhachHang = customer
        };
        var repair = new PhieuSuaChua
        {
            Id = id, MaPhieu = "SC" + id, ThietBiId = device.Id, ThietBi = device,
            TinhTrangLoi = "Test fault", TrangThai = "HOAN_THANH"
        };
        return new HoaDon
        {
            Id = id, MaHd = "HD" + id, PhieuId = repair.Id, Phieu = repair, TrangThai = "CHUA_THANH_TOAN",
            NgayLap = DateTime.UtcNow, TienDichVu = 100m, TongThanhToan = 100m
        };
    }
}
