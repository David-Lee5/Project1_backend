using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;

namespace RepairSystem.Tests.Repairs;

public class RepairServiceTests
{
    [Theory]
    [InlineData(int.MaxValue, 100)]
    [InlineData(0, 20)]
    [InlineData(1, 101)]
    public async Task InvalidOrOverflowingPaginationIsRejected(int page, int pageSize)
    {
        await using var fixture = await Fixture.CreateAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.GetAllAsync(2, page: page, pageSize: pageSize));
    }

    [Theory]
    [InlineData("0.001")]
    [InlineData("12500.123")]
    [InlineData("1000000000000")]
    [InlineData("-1")]
    public async Task InvalidMoneyIsRejectedWithoutAddingServiceLine(string value)
    {
        await using var fixture = await Fixture.CreateAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.UpsertServiceAsync(1, 10, 1,
            new UpsertRepairServiceDto { SoLuong = 1, DonGiaThucTe = decimal.Parse(value, CultureInfo.InvariantCulture) }));
        Assert.Empty(await fixture.Context.ChiTietDichVus.ToListAsync());
    }

    [Fact]
    public async Task ExactDecimal14MoneyUpperBoundIsAcceptedAndOverflowingLineTotalIsRejected()
    {
        await using var fixture = await Fixture.CreateAsync();
        var dto = new UpsertRepairServiceDto { SoLuong = 1, DonGiaThucTe = 999999999999.99m };
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);
        var result = await fixture.Service.UpsertServiceAsync(1, 10, 1, dto);
        Assert.Equal(999999999999.99m, Assert.Single(result.DichVu).ThanhTien);
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.UpsertServiceAsync(1, 10, 1,
            new UpsertRepairServiceDto { SoLuong = 2, DonGiaThucTe = 999999999999.99m }));
        Assert.Equal(1, (await fixture.Context.ChiTietDichVus.SingleAsync()).SoLuong);
    }

    [Fact]
    public async Task CustomerCannotReadOtherCustomersRepairOrListIt()
    {
        await using var fixture = await Fixture.CreateAsync();
        Assert.Empty((await fixture.Service.GetAllAsync(5)).Items);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => fixture.Service.GetByIdAsync(5, 10));
        var own = await fixture.Service.GetByIdAsync(4, 10);
        Assert.Equal("Khách hàng A", own.Phieu.HoTenKhachHang);
    }

    [Fact]
    public async Task TechnicianCannotMutateUnassignedJob()
    {
        await using var fixture = await Fixture.CreateAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.ChangeStatusAsync(6, 10,
            new ChangeRepairStatusDto { TrangThai = "DANG_SUA" }));
        Assert.Equal("TIEP_NHAN", (await fixture.Context.PhieuSuaChuas.SingleAsync()).TrangThai);
    }

    [Fact]
    public async Task ReceptionAndTechnicianCompletePaidRepairWithHistory()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.Service.AssignAsync(2, 10, new AssignRepairDto { KyThuatVienId = 3 });
        await fixture.Service.ChangeStatusAsync(3, 10, new ChangeRepairStatusDto { TrangThai = "DANG_SUA" });
        await fixture.Service.UpsertServiceAsync(3, 10, 1, new UpsertRepairServiceDto { SoLuong = 2 });
        var completed = await fixture.Service.ChangeStatusAsync(3, 10,
            new ChangeRepairStatusDto { TrangThai = "HOAN_THANH", KetQuaSuaChua = "Đã sửa xong" });
        Assert.Equal(200000m, completed.TienDichVu);
        Assert.NotNull(completed.Phieu.NgayHoanThanh);
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.ChangeStatusAsync(2, 10,
            new ChangeRepairStatusDto { TrangThai = "DA_TRA" }));
        fixture.Context.HoaDons.Add(new HoaDon
        {
            Id = 1, MaHd = "HD1", PhieuId = 10, LeTanId = 2, NgayLap = DateTime.UtcNow,
            TienDichVu = 200000, TongThanhToan = 200000, TrangThai = "DA_THANH_TOAN",
            PhuongThucTt = "TIEN_MAT", NgayThanhToan = DateTime.UtcNow
        });
        await fixture.Context.SaveChangesAsync();
        var returned = await fixture.Service.ChangeStatusAsync(2, 10, new ChangeRepairStatusDto { TrangThai = "DA_TRA" });
        Assert.NotNull(returned.Phieu.NgayTra);
        Assert.Equal(new[] { "TIEP_NHAN", "DANG_SUA", "HOAN_THANH", "DA_TRA" }, returned.LichSu.Select(h => h.TrangThaiMoi));
        Assert.Equal(new long?[] { 2, 3, 3, 2 }, returned.LichSu.Select(h => h.NguoiThucHienId));
    }

    [Fact]
    public async Task CompletedRepairCostsCannotBeChanged()
    {
        await using var fixture = await Fixture.CreateAsync();
        var repair = await fixture.Context.PhieuSuaChuas.SingleAsync();
        repair.TrangThai = "HOAN_THANH";
        await fixture.Context.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.UpsertServiceAsync(1, 10, 1,
            new UpsertRepairServiceDto { SoLuong = 1 }));
        Assert.Empty(await fixture.Context.ChiTietDichVus.ToListAsync());
    }

    [Fact]
    public async Task TechnicianCannotOverrideCatalogPrices()
    {
        await using var fixture = await Fixture.CreateAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.UpsertServiceAsync(3, 10, 1,
            new UpsertRepairServiceDto { SoLuong = 1, DonGiaThucTe = 1 }));
        Assert.Empty(await fixture.Context.ChiTietDichVus.ToListAsync());
    }

    [Fact]
    public async Task LinePriceRemainsSnapshotAfterCatalogChanges()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.Service.UpsertServiceAsync(3, 10, 1, new UpsertRepairServiceDto { SoLuong = 1 });
        var catalog = await fixture.Context.DichVus.SingleAsync();
        catalog.GiaNiemYet = 250000;
        await fixture.Context.SaveChangesAsync();
        var result = await fixture.Service.UpsertServiceAsync(3, 10, 1, new UpsertRepairServiceDto { SoLuong = 2 });
        Assert.Equal(100000m, Assert.Single(result.DichVu).DonGiaThucTe);
        Assert.Equal(200000m, result.TongChiPhi);
    }

    [Fact]
    public async Task CannotReceiveDeviceThatAlreadyHasOpenRepair()
    {
        await using var fixture = await Fixture.CreateAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => fixture.Service.CreateAsync(2,
            new CreateRepairDto { ThietBiId = 1, TinhTrangLoi = "Không lên nguồn" }));
        Assert.Single(await fixture.Context.PhieuSuaChuas.ToListAsync());
    }

    [Fact]
    public async Task ReceivesNewDeviceAndPersistsInitialHistoryAtomically()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Context.ThietBis.Add(new ThietBi
        { Id = 2, MaThietBi = "TB2", KhachHangId = 1, TenThietBi = "Máy tính B", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await fixture.Context.SaveChangesAsync();
        var result = await fixture.Service.CreateAsync(2,
            new CreateRepairDto { ThietBiId = 2, TinhTrangLoi = "Không nhận ổ cứng", NgayHenTra = DateTime.UtcNow.AddDays(2) });
        Assert.Equal("TIEP_NHAN", result.Phieu.TrangThai);
        Assert.Equal(2, result.Phieu.LeTanId);
        Assert.Equal(2, Assert.Single(result.LichSu).NguoiThucHienId);
    }

    [Fact]
    public async Task CannotRecordJavascriptPhotoUrl()
    {
        await using var fixture = await Fixture.CreateAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.AddPhotoAsync(2, 10,
            new AddRepairPhotoDto { Loai = "KHAC", DuongDan = "javascript:alert(1)" }));
        Assert.Empty(await fixture.Context.AnhPhieuSuaChuas.ToListAsync());
    }

    [Fact]
    public async Task DeletedEmployeeCannotReadRepairs()
    {
        await using var fixture = await Fixture.CreateAsync();
        var employee = await fixture.Context.NhanViens.SingleAsync(n => n.Id == 2);
        employee.DeletedAt = DateTime.UtcNow;
        await fixture.Context.SaveChangesAsync();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.GetAllAsync(2));
    }

    private sealed class SqliteContext : AppDbContext
    {
        public SqliteContext(DbContextOptions<AppDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            foreach (var type in builder.Model.GetEntityTypes().ToList())
            {
                if (type.FindPrimaryKey() == null) { builder.Ignore(type.ClrType); continue; }
                foreach (var property in type.GetProperties())
                {
                    if (property.GetDefaultValueSql() != null) property.SetDefaultValueSql(null);
                    if (property.GetComputedColumnSql() != null)
                    {
                        property.SetComputedColumnSql(null);
                        property.ValueGenerated = ValueGenerated.Never;
                    }
                }
            }
            // SQL Server supplies the business code; SQLite uses a harmless test default.
            builder.Entity<PhieuSuaChua>().Property(p => p.MaPhieu).HasDefaultValue("").ValueGeneratedOnAdd();
        }
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public SqliteContext Context { get; }
        public RepairService Service { get; }
        private Fixture(SqliteConnection connection, SqliteContext context)
        { _connection = connection; Context = context; Service = new RepairService(context); }
        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var context = new SqliteContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
            await context.Database.EnsureCreatedAsync();
            var roles = new[] { Roles.Admin, Roles.LeTan, Roles.KyThuatVien, Roles.KhachHang, Roles.KhachHang, Roles.KyThuatVien };
            for (var i = 1; i <= roles.Length; i++)
                context.TaiKhoans.Add(new TaiKhoan
                { Id = i, TenDangNhap = "user" + i, MatKhauHash = "unused", VaiTro = roles[i - 1], HoatDong = true });
            foreach (var id in new[] { 1, 2, 3, 6 })
                context.NhanViens.Add(new NhanVien
                { Id = id, MaNv = "NV" + id, TaiKhoanId = id, ChucVu = roles[id - 1], HoTen = "Nhân viên " + id, SoDienThoai = "090000000" + id });
            context.LeTans.Add(new LeTan { NhanVienId = 2, ChucVu = Roles.LeTan });
            context.KyThuatViens.AddRange(new KyThuatVien { NhanVienId = 3, ChucVu = Roles.KyThuatVien }, new KyThuatVien { NhanVienId = 6, ChucVu = Roles.KyThuatVien });
            context.KhachHangs.AddRange(
                new KhachHang { Id = 1, MaKh = "KH1", TaiKhoanId = 4, VaiTroTk = Roles.KhachHang, HoTen = "Khách hàng A", SoDienThoai = "0900000004" },
                new KhachHang { Id = 2, MaKh = "KH2", TaiKhoanId = 5, VaiTroTk = Roles.KhachHang, HoTen = "Khách hàng B", SoDienThoai = "0900000005" });
            context.ThietBis.Add(new ThietBi { Id = 1, MaThietBi = "TB1", KhachHangId = 1, TenThietBi = "Máy tính A" });
            context.DichVus.Add(new DichVu { Id = 1, MaDv = "DV1", TenDichVu = "Sửa nguồn", GiaNiemYet = 100000 });
            context.PhieuSuaChuas.Add(new PhieuSuaChua
            {
                Id = 10, MaPhieu = "SC10", ThietBiId = 1, LeTanId = 2, KyThuatVienId = 3,
                TrangThai = "TIEP_NHAN", TinhTrangLoi = "Không lên nguồn", NgayNhan = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
            context.LichSuTrangThaiPhieus.Add(new LichSuTrangThaiPhieu
            { Id = 1, PhieuId = 10, TrangThaiMoi = "TIEP_NHAN", NguoiThucHienId = 2, ThoiGian = DateTime.UtcNow.AddMinutes(-1) });
            await context.SaveChangesAsync();
            return new Fixture(connection, context);
        }
        public async ValueTask DisposeAsync() { await Context.DisposeAsync(); await _connection.DisposeAsync(); }
    }
}
