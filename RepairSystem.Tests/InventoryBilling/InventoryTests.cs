using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Services;

namespace RepairSystem.Tests.InventoryBilling;

public class InventoryTests
{
    private static AppDbContext CreateContext() => new(new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    [Fact]
    public async Task DuplicatePartLinesAreRejectedBeforeCreatingReceipt()
    {
        await using var context = CreateContext();
        var service = new InventoryService(context);
        var input = new UpsertReceiptDto { KhoId = 1, NhaCungCapId = 1,
            ChiTiet = [new() { LinhKienId = 5, SoLuongNhap = 1, DonGiaNhap = 100 }, new() { LinhKienId = 5, SoLuongNhap = 2, DonGiaNhap = 200 }] };

        await Assert.ThrowsAsync<ArgumentException>(() => service.CreateReceiptAsync(input, 99));
        Assert.Empty(await context.PhieuNhaps.ToListAsync());
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(0, 100)]
    [InlineData(1, -10)]
    [InlineData(1, 0.001)]
    [InlineData(2, 999999999999.99)]
    public async Task InvalidReceiptQuantitiesAndMoneyAreRejected(int quantity, decimal price)
    {
        await using var context = CreateContext();
        var input = new UpsertReceiptDto { KhoId = 1, NhaCungCapId = 1,
            ChiTiet = [new() { LinhKienId = 5, SoLuongNhap = quantity, DonGiaNhap = price }] };

        await Assert.ThrowsAsync<ArgumentException>(() => new InventoryService(context).CreateReceiptAsync(input, 99));
        Assert.Empty(context.ChangeTracker.Entries<PhieuNhap>());
    }

    [Fact]
    public async Task DispatchRequiresPositiveQuantityBeforeAnyStockMutation()
    {
        await using var context = CreateContext();
        var input = new DispatchRequestDto { PhieuId = 1, KhoId = 1, LinhKienId = 1, SoLuong = 0 };
        await Assert.ThrowsAsync<ArgumentException>(() => new InventoryService(context).DispatchAsync(input, 99));
        Assert.Empty(await context.ChiTietXuatKhos.ToListAsync());
    }

    [Fact]
    public async Task MissingKeeperProfileCannotChangeStockThreshold()
    {
        await using var context = CreateContext();
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new InventoryService(context).SetStockThresholdAsync(1, 1, 5, 99));
        Assert.Empty(await context.TonKhos.ToListAsync());
    }

    [Fact]
    public async Task LowStockListHidesDeletedCatalogEntriesAndKeepsThresholdBoundary()
    {
        await using var context = CreateContext();
        var active = new Kho { Id = 1, MaKho = "KHO001", TenKho = "Active" };
        var deleted = new Kho { Id = 2, MaKho = "KHO002", TenKho = "Deleted", DeletedAt = DateTime.UtcNow };
        var part = new LinhKien { Id = 1, MaLk = "LK000001", TenLinhKien = "Active part", DonViTinh = "cai" };
        var inactivePart = new LinhKien { Id = 2, MaLk = "LK000002", TenLinhKien = "Deleted part", DonViTinh = "cai", DeletedAt = DateTime.UtcNow };
        context.AddRange(active, deleted, part, inactivePart);
        context.TonKhos.AddRange(new TonKho { Kho = active, LinhKien = part, SoLuongTon = 3, MucTonToiThieu = 3 },
            new TonKho { Kho = active, LinhKien = inactivePart, SoLuongTon = 0, MucTonToiThieu = 3 },
            new TonKho { Kho = deleted, LinhKien = part, SoLuongTon = 0, MucTonToiThieu = 3 });
        await context.SaveChangesAsync();

        var result = await new InventoryService(context).GetStockAsync(lowOnly: true);

        var only = Assert.Single(result);
        Assert.Equal(1, only.KhoId);
        Assert.Equal(1, only.LinhKienId);
        Assert.Equal(3, only.SoLuongTon);
    }

    [Fact]
    public async Task ReceiptListRejectsUnboundedPagesAndUnknownState()
    {
        await using var context = CreateContext();
        var service = new InventoryService(context);
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetReceiptsAsync(take: 201));
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetReceiptsAsync(status: "UNKNOWN"));
    }
}
