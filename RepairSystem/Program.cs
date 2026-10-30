using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using RepairSystem.Middleware;
using RepairSystem.Service.DTOs;
using Microsoft.AspNetCore.Mvc;
using RepairSystem.Model;
using RepairSystem.Service.IServices;
using RepairSystem.Service.Services;
using RepairSystem.Service.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
// (Role/Permission đã bỏ: vai trò lưu ở cột tai_khoan.vai_tro, phân quyền bằng [Authorize(Roles = ...)])

builder.Services.AddScoped<ITokenService, TokenHelper>();

// Register Catalog Services (Phase 0)
builder.Services.AddScoped<INhanVienService, NhanVienService>();
builder.Services.AddScoped<IDichVuService, DichVuService>();
builder.Services.AddScoped<INhaCungCapService, NhaCungCapService>();
builder.Services.AddScoped<ILinhKienService, LinhKienService>();
builder.Services.AddScoped<CustomerDeviceService>();
builder.Services.AddScoped<RepairService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<BillingService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSecret = jwtSettings["SecretKey"];
    if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
        throw new InvalidOperationException("Configure JwtSettings__SecretKey with at least 32 bytes via environment variables or user secrets.");
    if (string.IsNullOrWhiteSpace(jwtSettings["Issuer"]) || string.IsNullOrWhiteSpace(jwtSettings["Audience"]) ||
        !int.TryParse(jwtSettings["ExpirationMinutes"], out var expirationMinutes) || expirationMinutes < 1)
        throw new InvalidOperationException("Configure a JWT issuer, audience, and positive expiration time.");
    var secretKey = Encoding.UTF8.GetBytes(jwtSecret);

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var principal = context.Principal!;
            var idValue = principal.FindFirst("UserId")?.Value ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!long.TryParse(idValue, out var accountId) || accountId <= 0)
            {
                context.Fail("Invalid account identity.");
                return;
            }
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var account = await db.TaiKhoans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == accountId, context.HttpContext.RequestAborted);
            if (account is null || !account.HoatDong ||
                account.VaiTro != principal.FindFirst(ClaimTypes.Role)?.Value ||
                account.TenDangNhap != principal.Identity?.Name ||
                (account.VaiTro == Roles.KhachHang && !await db.KhachHangs.AnyAsync(k => k.TaiKhoanId == accountId && k.DeletedAt == null, context.HttpContext.RequestAborted)) ||
                (account.VaiTro != Roles.KhachHang && !await db.NhanViens.AnyAsync(n => n.TaiKhoanId == accountId && n.DeletedAt == null, context.HttpContext.RequestAborted)))
                context.Fail("Account is inactive or its authorization has changed.");
        },
        OnChallenge = async context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.Headers.WWWAuthenticate = "Bearer";
            await context.Response.WriteAsJsonAsync(ApiResponse<object>.ErrorResponse("Vui lòng đăng nhập để tiếp tục."));
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.Response.WriteAsJsonAsync(ApiResponse<object>.ErrorResponse("Bạn không có quyền thực hiện thao tác này."));
        }
    };
});

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme).ValidateOnStart();
builder.Services.AddAuthorization();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context => new BadRequestObjectResult(
        ApiResponse<object>.ErrorResponse("Dữ liệu không hợp lệ.", context.ModelState.Values
            .SelectMany(x => x.Errors).Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? "Giá trị không hợp lệ." : x.ErrorMessage).ToList()));
});
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
    if (allowedOrigins.Length > 0) policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
}));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Initialize database with seed data (wrapped in try-catch)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedData.InitializeAsync(context, builder.Configuration["Bootstrap:AdminPassword"]);
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning("Database bootstrap did not complete ({ExceptionType}). Check database readiness.", ex.GetType().Name);
}

app.UseMiddleware<ApiExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseCors();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" })).AllowAnonymous();
app.MapGet("/health/ready", async (AppDbContext db, CancellationToken token) =>
{
    try
    {
        await db.TaiKhoans.AsNoTracking().Select(x => x.Id).Take(1).ToListAsync(token);
        return Results.Ok(new { status = "ready" });
    }
    catch (Exception)
    {
        return Results.Json(new { status = "not_ready" }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }
}).AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }


