using Microsoft.EntityFrameworkCore;
using RepairSystem.Model;
using RepairSystem.Service.Services;

namespace RepairSystem.Tests.Catalogs;

public class PartImageTests
{
    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://username:password@example.com/image.jpg")]
    public async Task ImagesRejectNonHttpOrCredentialBearingUrls(string url)
    {
        await using var db = new ImageSqliteContext();
        await Assert.ThrowsAsync<ArgumentException>(() => new LinhKienService(db)
            .AddImageAsync(1, new() { DuongDan = url }));
    }

    [Fact]
    public async Task ImagesPromoteExactlyOnePrimaryAndDeletingItPromotesTheNext()
    {
        await using var db = await ImageSqliteContext.CreateAsync();
        var service = new LinhKienService(db);
        var first = await service.AddImageAsync(1, new() { DuongDan = "https://example.com/first.jpg", ThuTu = 1 });
        var second = await service.AddImageAsync(1, new() { DuongDan = "https://example.com/second.jpg", LaAnhChinh = true });

        Assert.True(first.LaAnhChinh);
        Assert.True(second.LaAnhChinh);
        Assert.Equal(second.Id, (await service.GetImagesAsync(1)).Single(a => a.LaAnhChinh).Id);
        await service.SetPrimaryImageAsync(1, first.Id);
        Assert.Equal(first.Id, (await service.GetImagesAsync(1)).Single(a => a.LaAnhChinh).Id);

        Assert.True(await service.DeleteImageAsync(1, first.Id));
        var remaining = Assert.Single(await service.GetImagesAsync(1));
        Assert.Equal(second.Id, remaining.Id);
        Assert.True(remaining.LaAnhChinh);
    }

    [Fact]
    public async Task ImageIdsCannotBeUsedToModifyAnotherPart()
    {
        await using var db = await ImageSqliteContext.CreateAsync();
        var service = new LinhKienService(db);
        var image = await service.AddImageAsync(1, new() { DuongDan = "https://example.com/image.jpg" });

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.SetPrimaryImageAsync(2, image.Id));
        Assert.False(await service.DeleteImageAsync(2, image.Id));
        Assert.Single(await service.GetImagesAsync(1));
    }

    [Fact]
    public async Task MainModelRetainsMultipleImagesPerPart()
    {
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var part = new LinhKien { Id = 1, MaLk = "LK1", TenLinhKien = "Pin", DonViTinh = "cái" };
        db.LinhKiens.Add(part);
        db.AnhLinhKiens.AddRange(
            new AnhLinhKien { Id = 1, LinhKienId = 1, DuongDan = "https://example.com/one.jpg" },
            new AnhLinhKien { Id = 2, LinhKienId = 1, DuongDan = "https://example.com/two.jpg" });
        await db.SaveChangesAsync();
        Assert.Equal(2, part.AnhLinhKiens.Count);
        Assert.Equal(2, await db.AnhLinhKiens.CountAsync(a => a.LinhKienId == 1));
    }

    private sealed class ImageSqliteContext : AppDbContext
    {
        public ImageSqliteContext() : base(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:").Options) { }

        public static async Task<ImageSqliteContext> CreateAsync()
        {
            var context = new ImageSqliteContext();
            await context.Database.OpenConnectionAsync();
            await context.Database.EnsureCreatedAsync();
            context.LinhKiens.AddRange(
                new LinhKien { Id = 1, MaLk = "LK1", TenLinhKien = "Pin", DonViTinh = "cái" },
                new LinhKien { Id = 2, MaLk = "LK2", TenLinhKien = "Màn hình", DonViTinh = "cái" });
            await context.SaveChangesAsync();
            return context;
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            foreach (var property in typeof(AppDbContext).GetProperties())
            {
                if (!property.PropertyType.IsGenericType ||
                    property.PropertyType.GetGenericTypeDefinition() != typeof(DbSet<>)) continue;
                var entity = property.PropertyType.GetGenericArguments()[0];
                if (entity != typeof(LinhKien) && entity != typeof(AnhLinhKien)) model.Ignore(entity);
            }
            model.Entity<LinhKien>().HasKey(l => l.Id);
            model.Entity<LinhKien>().Ignore(l => l.ChiTietNhapKhos).Ignore(l => l.TonKhos);
            model.Entity<AnhLinhKien>().HasKey(a => a.Id);
            model.Entity<AnhLinhKien>().HasOne(a => a.LinhKien).WithMany(l => l.AnhLinhKiens)
                .HasForeignKey(a => a.LinhKienId);
            model.Entity<AnhLinhKien>().HasIndex(a => a.LinhKienId).IsUnique().HasFilter("[LaAnhChinh] = 1");
        }
    }
}
