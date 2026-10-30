using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RepairSystem.Model;
using RepairSystem.Service.DTOs;
using RepairSystem.Service.Helpers;
using RepairSystem.Service.Services;
using Xunit;

namespace RepairSystem.Tests.Auth;

public sealed class AuthServiceTests
{
    private const string Password = "Test-only-password-42!";

    [Theory]
    [InlineData(null, "valid-password")]
    [InlineData("user", null)]
    [InlineData("   ", "valid-password")]
    public void Login_request_rejects_missing_credentials(string? username, string? password)
    {
        Assert.False(IsValid(new LoginRequestDto { Username = username!, Password = password! }));
    }

    [Fact]
    public void Password_change_requires_confirmation_and_minimum_length()
    {
        Assert.False(IsValid(new ChangePasswordRequestDto { OldPassword = Password, NewPassword = "short", ConfirmPassword = "short" }));
        Assert.False(IsValid(new ChangePasswordRequestDto { OldPassword = Password, NewPassword = Password, ConfirmPassword = "different" }));
        Assert.True(IsValid(new ChangePasswordRequestDto { OldPassword = Password, NewPassword = Password, ConfirmPassword = Password }));
    }

    [Fact]
    public async Task Login_expiry_matches_configured_JWT_lifetime_and_supports_bigint_account_ids()
    {
        await using var database = await AuthDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var user = await AddUserAsync(context, id: (long)int.MaxValue + 1);
        var helper = CreateTokenHelper(expirationMinutes: 45);
        var service = new AuthService(context, helper);

        var login = await service.LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = Password });

        Assert.True(login.Success);
        Assert.NotNull(login.Token);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(login.Token.AccessToken);
        Assert.Equal(parsed.ValidTo, login.Token.ExpiresIn);
        Assert.InRange(login.Token.ExpiresIn, DateTime.UtcNow.AddMinutes(44), DateTime.UtcNow.AddMinutes(46));
        var validation = await helper.ValidateTokenAsync(login.Token.AccessToken);
        Assert.True(validation.isValid);
        Assert.Equal(user.Id, validation.userId);
        Assert.Single(await context.RefreshTokens.ToListAsync());
    }

    [Fact]
    public async Task Five_failed_logins_lock_the_account_and_correct_password_cannot_bypass_the_lock()
    {
        await using var database = await AuthDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var user = await AddUserAsync(context);
        var service = new AuthService(context, CreateTokenHelper());
        for (var attempt = 0; attempt < 5; attempt++)
            Assert.False((await service.LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = "incorrect" })).Success);

        Assert.False((await service.LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = Password })).Success);
        Assert.True(user.KhoaDen > DateTime.UtcNow);
        Assert.Empty(await context.RefreshTokens.ToListAsync());
    }

    [Fact]
    public async Task Refresh_rotates_once_and_replay_revokes_the_successor()
    {
        await using var database = await AuthDatabase.CreateAsync();
        string original;
        await using (var context = database.CreateContext())
        {
            var user = await AddUserAsync(context);
            var login = await new AuthService(context, CreateTokenHelper()).LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = Password });
            original = login.Token!.RefreshToken;
        }

        string successor;
        await using (var context = database.CreateContext())
        {
            successor = (await new AuthService(context, CreateTokenHelper()).RefreshTokenAsync(original)).RefreshToken;
            Assert.NotEqual(original, successor);
            Assert.Single(await context.RefreshTokens.Where(t => t.ThuHoiLuc == null).ToListAsync());
        }

        await using (var context = database.CreateContext())
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new AuthService(context, CreateTokenHelper()).RefreshTokenAsync(original));
        await using (var context = database.CreateContext())
        {
            Assert.Empty(await context.RefreshTokens.Where(t => t.ThuHoiLuc == null).ToListAsync());
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new AuthService(context, CreateTokenHelper()).RefreshTokenAsync(successor));
        }
    }

    [Fact]
    public async Task Disabling_account_revokes_sessions_and_prevents_login_and_refresh()
    {
        await using var database = await AuthDatabase.CreateAsync();
        long userId;
        string refresh;
        await using (var context = database.CreateContext())
        {
            var user = await AddUserAsync(context);
            userId = user.Id;
            refresh = (await new AuthService(context, CreateTokenHelper()).LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = Password })).Token!.RefreshToken;
        }
        await using (var context = database.CreateContext())
            Assert.True(await new UserService(context).UpdateUserStatusAsync(userId, false));
        await using (var context = database.CreateContext())
        {
            var service = new AuthService(context, CreateTokenHelper());
            Assert.False((await service.LoginAsync(new LoginRequestDto { Username = "customer", Password = Password })).Success);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshTokenAsync(refresh));
            Assert.Empty(await context.RefreshTokens.Where(t => t.ThuHoiLuc == null).ToListAsync());
        }
    }

    [Fact]
    public async Task Password_change_persists_new_hash_and_revokes_existing_refresh_tokens()
    {
        await using var database = await AuthDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var user = await AddUserAsync(context);
        var service = new AuthService(context, CreateTokenHelper());
        var refresh = (await service.LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = Password })).Token!.RefreshToken;

        Assert.True(await service.ChangePasswordAsync(user.Id, Password, "Changed-test-password-42!"));
        Assert.False((await service.LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = Password })).Success);
        Assert.True((await service.LoginAsync(new LoginRequestDto { Username = user.TenDangNhap, Password = "Changed-test-password-42!" })).Success);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.RefreshTokenAsync(refresh));
    }

    [Fact]
    public async Task Last_active_administrator_cannot_be_disabled_through_account_management()
    {
        await using var database = await AuthDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var user = await AddUserAsync(context);
        user.VaiTro = Roles.Admin;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new UserService(context).UpdateUserStatusAsync(user.Id, false));
        Assert.True(user.HoatDong);
    }

    [Fact]
    public async Task Registration_rolls_back_account_when_customer_insert_violates_a_constraint()
    {
        await using var database = await AuthDatabase.CreateAsync();
        await using (var context = database.CreateContext())
        {
            var service = new AuthService(context, CreateTokenHelper());
            Assert.True((await service.RegisterAsync(Registration("first", "0901234567"))).Success);
        }
        await using (var context = database.CreateContext())
        {
            var service = new AuthService(context, CreateTokenHelper());
            Assert.False((await service.RegisterAsync(Registration("second", "0901234567"))).Success);
        }
        await using (var context = database.CreateContext())
        {
            Assert.Equal(1, await context.TaiKhoans.CountAsync());
            Assert.Equal(1, await context.KhachHangs.CountAsync());
            Assert.False(await context.TaiKhoans.AnyAsync(u => u.TenDangNhap == "second"));
        }
    }

    [Fact]
    public async Task Deleted_customer_profile_cannot_be_bypassed_by_reactivating_its_account()
    {
        await using var database = await AuthDatabase.CreateAsync();
        await using var context = database.CreateContext();
        var auth = new AuthService(context, CreateTokenHelper());
        Assert.True((await auth.RegisterAsync(Registration("removed_customer", "0901234567"))).Success);
        var user = await context.TaiKhoans.SingleAsync();
        user.HoatDong = false;
        (await context.KhachHangs.SingleAsync()).DeletedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => new UserService(context).UpdateUserStatusAsync(user.Id, true));
        Assert.False(user.HoatDong);
    }

    private static RegisterRequestDto Registration(string username, string phone) => new()
    {
        Username = username, Password = Password, ConfirmPassword = Password,
        HoTen = "Test customer", SoDienThoai = phone
    };

    private static bool IsValid(object value) => Validator.TryValidateObject(value, new ValidationContext(value), new List<ValidationResult>(), true);

    private static TokenHelper CreateTokenHelper(int expirationMinutes = 15) => new(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["JwtSettings:SecretKey"] = "Test-only-signing-key-at-least-thirty-two-bytes-long",
        ["JwtSettings:Issuer"] = "test-issuer", ["JwtSettings:Audience"] = "test-audience",
        ["JwtSettings:ExpirationMinutes"] = expirationMinutes.ToString()
    }).Build());

    private static async Task<TaiKhoan> AddUserAsync(AppDbContext context, long id = 1)
    {
        var user = new TaiKhoan { Id = id, TenDangNhap = "customer", MatKhauHash = PasswordHelper.HashPassword(Password), VaiTro = Roles.KhachHang, HoatDong = true };
        context.TaiKhoans.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    // These relational tests exercise transactions and ExecuteUpdate. The isolated
    // model omits SQL Server computed columns/triggers; concurrency and the full
    // production schema additionally require SQL Server integration validation.
    private sealed class AuthDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection = new("Data Source=:memory:");
        public static async Task<AuthDatabase> CreateAsync()
        {
            var database = new AuthDatabase();
            await database._connection.OpenAsync();
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync();
            return database;
        }
        public AppDbContext CreateContext() => new AuthContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }

    private sealed class AuthContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var retained = new[] { typeof(TaiKhoan), typeof(RefreshToken), typeof(KhachHang) };
            foreach (var property in typeof(AppDbContext).GetProperties())
                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                {
                    var type = property.PropertyType.GetGenericArguments()[0];
                    if (!retained.Contains(type)) builder.Ignore(type);
                }
            builder.Entity<TaiKhoan>().Ignore(u => u.NhanViens).Ignore(u => u.AuditLogs);
            builder.Entity<TaiKhoan>().HasIndex(u => u.TenDangNhap).IsUnique();
            builder.Entity<TaiKhoan>().HasMany(u => u.RefreshTokens).WithOne(t => t.TaiKhoan).HasForeignKey(t => t.TaiKhoanId);
            builder.Entity<TaiKhoan>().HasMany(u => u.KhachHangs).WithOne(k => k.TaiKhoan).HasForeignKey(k => k.TaiKhoanId);
            builder.Entity<RefreshToken>().HasIndex(t => t.TokenHash).IsUnique();
            builder.Entity<KhachHang>().Ignore(k => k.ThietBis);
            builder.Entity<KhachHang>().Property(k => k.MaKh).HasDefaultValueSql("('KH' || hex(randomblob(4)))");
            builder.Entity<KhachHang>().HasIndex(k => k.SoDienThoai).IsUnique();
        }
    }
}
