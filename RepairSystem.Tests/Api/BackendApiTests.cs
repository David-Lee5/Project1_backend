using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RepairSystem.Model;
using RepairSystem.Service.Helpers;

namespace RepairSystem.Tests.Api;

public sealed class BackendApiTests : IClassFixture<BackendFactory>
{
    private readonly BackendFactory factory;
    public BackendApiTests(BackendFactory factory) => this.factory = factory;

    [Fact]
    public async Task OpenApi_exposes_all_business_modules_and_health_is_separate()
    {
        using var client = factory.CreateClient();
        using var spec = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
        var paths = spec.RootElement.GetProperty("paths");
        foreach (var module in new[] { "/api/khach-hang", "/api/thiet-bi", "/api/phieu-sua-chua", "/api/kho", "/api/ton-kho", "/api/phieu-nhap", "/api/xuat-kho", "/api/hoa-don" })
            Assert.True(paths.TryGetProperty(module, out _), module);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
    }

    [Fact]
    public async Task Anonymous_private_routes_return_safe_JSON_401()
    {
        using var client = factory.CreateClient();
        foreach (var path in new[] { "/api/auth/me", "/api/khach-hang", "/api/thiet-bi", "/api/phieu-sua-chua", "/api/hoa-don", "/api/ton-kho" })
        {
            using var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.False(body.GetProperty("success").GetBoolean());
        }
    }

    [Fact]
    public async Task Invalid_auth_request_returns_API_envelope_instead_of_server_error()
    {
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/api/auth/login", new { });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(body.GetProperty("success").GetBoolean());
        Assert.NotEmpty(body.GetProperty("errors").EnumerateArray());
    }

    [Fact]
    public async Task Disabled_account_existing_JWT_is_rejected_immediately()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var account = new TaiKhoan { TenDangNhap = "disabled_api_check", MatKhauHash = "not-a-credential", VaiTro = Roles.KhachHang, HoatDong = false };
        db.TaiKhoans.Add(account);
        await db.SaveChangesAsync();
        var jwt = new TokenHelper(scope.ServiceProvider.GetRequiredService<IConfiguration>())
            .GenerateAccessToken(account.Id, account.TenDangNhap, account.VaiTro);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task CORS_allows_only_configured_frontend_origin()
    {
        using var client = factory.CreateClient();
        foreach (var origin in new[] { "http://localhost:5173", "https://untrusted.example" })
        {
            using var request = new HttpRequestMessage(HttpMethod.Options, "/api/dich-vu");
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", "GET");
            using var response = await client.SendAsync(request);
            Assert.Equal(origin == "http://localhost:5173", response.Headers.Contains("Access-Control-Allow-Origin"));
        }
    }
}

public sealed class BackendFactory : WebApplicationFactory<Program>
{
    private readonly string database = Guid.NewGuid().ToString();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = "test-signing-key-only-not-for-deployment-0123456789",
            ["JwtSettings:Issuer"] = "backend-tests",
            ["JwtSettings:Audience"] = "backend-tests",
            ["JwtSettings:ExpirationMinutes"] = "15",
            ["Cors:AllowedOrigins:0"] = "http://localhost:5173"
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(database, config => config.EnableNullChecks(false)));
        });
    }
}
