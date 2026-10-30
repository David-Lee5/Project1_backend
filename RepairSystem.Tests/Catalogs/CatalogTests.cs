using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RepairSystem.Controllers;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.IServices;
using RepairSystem.Service.Services;

namespace RepairSystem.Tests.Catalogs;

public class CatalogTests
{
    private static AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options);

    [Theory]
    [InlineData(null, false)]
    [InlineData(Roles.KhachHang, false)]
    [InlineData(Roles.LeTan, false)]
    [InlineData(Roles.Admin, true)]
    public async Task DeletedServicesAreVisibleOnlyToAdmin(string? role, bool expected)
    {
        var service = new RecordingDichVuService();
        var controller = new DichVuController(service) { ControllerContext = ControllerContext(role) };
        await controller.GetAll(includeDeleted: true);
        Assert.Equal(expected, service.IncludeDeleted);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(Roles.KhachHang, false)]
    [InlineData("UNKNOWN_ROLE", false)]
    [InlineData(Roles.Admin, true)]
    [InlineData(Roles.LeTan, true)]
    [InlineData(Roles.ThuKho, true)]
    [InlineData(Roles.KyThuatVien, true)]
    public async Task OnlyRecognizedStaffReceivePartStock(string? role, bool expected)
    {
        var service = new RecordingLinhKienService();
        var controller = new LinhKienController(service) { ControllerContext = ControllerContext(role) };
        await controller.GetAll();
        Assert.Equal(expected, service.IncludeStock);
    }

    [Fact]
    public async Task CatalogServicesRejectInvalidInputsBeforePersisting()
    {
        await using var db = Context();
        await Assert.ThrowsAsync<ArgumentException>(() => new DichVuService(db).CreateAsync(new()
        {
            TenDichVu = "   ", GiaNiemYet = 100
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => new DichVuService(db).CreateAsync(new()
        {
            TenDichVu = "Sửa máy", GiaNiemYet = 10.001m
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => new LinhKienService(db).CreateAsync(new()
        {
            TenLinhKien = "Pin", DonGiaBan = -1, ThoiHanBaoHanhThang = 0
        }));
        await Assert.ThrowsAsync<ArgumentException>(() => new NhaCungCapService(db).CreateAsync(new()
        {
            TenNcc = "Nhà cung cấp", Email = "not-an-email"
        }));
        Assert.Empty(db.ChangeTracker.Entries());
    }

    [Fact]
    public async Task PartsWithStockCannotBeSoftDeleted()
    {
        await using var db = Context();
        db.LinhKiens.Add(new LinhKien { Id = 1, MaLk = "LK1", TenLinhKien = "Pin", DonViTinh = "cái" });
        db.Khos.Add(new Kho { Id = 1, MaKho = "K1", TenKho = "Kho chính" });
        db.TonKhos.Add(new TonKho { KhoId = 1, LinhKienId = 1, SoLuongTon = 2 });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new LinhKienService(db).DeleteAsync(1));
        Assert.Null((await db.LinhKiens.FindAsync(1L))!.DeletedAt);
        Assert.Equal(2, (await db.TonKhos.FindAsync(1L, 1L))!.SoLuongTon);
    }

    [Fact]
    public async Task StaffStockTotalsUseLongAcrossWarehousesAndAreHiddenFromPublic()
    {
        await using var db = Context();
        db.LinhKiens.Add(new LinhKien { Id = 1, MaLk = "LK1", TenLinhKien = "Pin", DonViTinh = "cái" });
        db.TonKhos.AddRange(
            new TonKho { KhoId = 1, LinhKienId = 1, SoLuongTon = 2_000_000_000 },
            new TonKho { KhoId = 2, LinhKienId = 1, SoLuongTon = 2_000_000_000 });
        await db.SaveChangesAsync();
        var service = new LinhKienService(db);
        Assert.Equal(4_000_000_000L, (await service.GetByIdAsync(1, includeStock: true))!.TongTon);
        Assert.Null((await service.GetByIdAsync(1))!.TongTon);
    }

    [Fact]
    public async Task LastActiveAdministratorCannotBeDeleted()
    {
        await using var db = new EmployeeSqliteContext();
        await db.Database.OpenConnectionAsync();
        await db.Database.EnsureCreatedAsync();
        var account = new TaiKhoan
        {
            Id = 1, TenDangNhap = "admin", MatKhauHash = "hash", VaiTro = Roles.Admin, HoatDong = true
        };
        db.TaiKhoans.Add(account);
        db.NhanViens.Add(new NhanVien
        {
            Id = 1, MaNv = "NV1", HoTen = "Admin", SoDienThoai = "0900000000", ChucVu = Roles.Admin,
            TaiKhoanId = account.Id, TaiKhoan = account
        });
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new NhanVienService(db).DeleteAsync(1));
        Assert.True(account.HoatDong);
        Assert.Null((await db.NhanViens.FindAsync(1L))!.DeletedAt);
    }

    private static ControllerContext ControllerContext(string? role) => new()
    {
        HttpContext = new DefaultHttpContext
        {
            User = role == null ? new ClaimsPrincipal(new ClaimsIdentity()) :
                new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"))
        }
    };

    // SQL Server is tested separately. This relational fixture exercises the deletion
    // transaction and account/employee relationship without copying SQL Server computed columns.
    private sealed class EmployeeSqliteContext : AppDbContext
    {
        public EmployeeSqliteContext() : base(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:").Options) { }

        protected override void OnModelCreating(ModelBuilder model)
        {
            foreach (var property in typeof(AppDbContext).GetProperties())
            {
                if (!property.PropertyType.IsGenericType ||
                    property.PropertyType.GetGenericTypeDefinition() != typeof(DbSet<>)) continue;
                var entity = property.PropertyType.GetGenericArguments()[0];
                if (entity != typeof(TaiKhoan) && entity != typeof(NhanVien)) model.Ignore(entity);
            }
            model.Entity<TaiKhoan>().HasKey(t => t.Id);
            model.Entity<TaiKhoan>().Ignore(t => t.AuditLogs).Ignore(t => t.KhachHangs).Ignore(t => t.RefreshTokens);
            model.Entity<NhanVien>().HasKey(n => n.Id);
            model.Entity<NhanVien>().Ignore(n => n.BienDongKhos).Ignore(n => n.KyThuatViens)
                .Ignore(n => n.LeTans).Ignore(n => n.ThuKhos).Ignore(n => n.LichSuTrangThaiPhieus);
            model.Entity<NhanVien>().HasOne(n => n.TaiKhoan).WithMany(t => t.NhanViens)
                .HasForeignKey(n => n.TaiKhoanId);
        }
    }

    private sealed class RecordingDichVuService : IDichVuService
    {
        public bool IncludeDeleted { get; private set; }
        public Task<IEnumerable<DichVuDto>> GetAllAsync(bool includeDeleted = false)
        {
            IncludeDeleted = includeDeleted;
            return Task.FromResult(Enumerable.Empty<DichVuDto>());
        }
        public Task<DichVuDto?> GetByIdAsync(long id) => throw new NotImplementedException();
        public Task<DichVuDto> CreateAsync(UpsertDichVuDto dto) => throw new NotImplementedException();
        public Task<DichVuDto> UpdateAsync(long id, UpsertDichVuDto dto) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(long id) => throw new NotImplementedException();
    }

    private sealed class RecordingLinhKienService : ILinhKienService
    {
        public bool IncludeStock { get; private set; }
        public Task<IEnumerable<LinhKienDto>> GetAllAsync(string? keyword = null, bool includeStock = false, bool includeDeleted = false)
        {
            IncludeStock = includeStock;
            return Task.FromResult(Enumerable.Empty<LinhKienDto>());
        }
        public Task<LinhKienDto?> GetByIdAsync(long id, bool includeStock = false) => throw new NotImplementedException();
        public Task<LinhKienDto> CreateAsync(UpsertLinhKienDto dto) => throw new NotImplementedException();
        public Task<LinhKienDto> UpdateAsync(long id, UpsertLinhKienDto dto) => throw new NotImplementedException();
        public Task<bool> DeleteAsync(long id) => throw new NotImplementedException();
        public Task<IReadOnlyList<AnhLinhKienDto>> GetImagesAsync(long partId) => throw new NotImplementedException();
        public Task<AnhLinhKienDto> AddImageAsync(long partId, CreateAnhLinhKienDto dto) => throw new NotImplementedException();
        public Task<AnhLinhKienDto> SetPrimaryImageAsync(long partId, long imageId) => throw new NotImplementedException();
        public Task<bool> DeleteImageAsync(long partId, long imageId) => throw new NotImplementedException();
    }
}
