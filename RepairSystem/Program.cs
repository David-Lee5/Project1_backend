using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RepairSystem.Model;
using RepairSystem.Model.ScaffoldV3.Entities;
using RepairSystem.Service.IServices;
using RepairSystem.Service.Services;
using RepairSystem.Service.Helpers;
using RepairSystem.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var appointmentStart = builder.Configuration.GetValue("Appointments:StartHour", 9);
var appointmentEnd = builder.Configuration.GetValue("Appointments:EndHourExclusive", 18);
var appointmentCapacity = builder.Configuration.GetValue("Appointments:CapacityPerSlot", 3);
if (appointmentStart is < 0 or > 23 || appointmentEnd <= appointmentStart || appointmentEnd > 24
    || appointmentCapacity < 1)
    throw new InvalidOperationException("Cấu hình Appointments không hợp lệ: cần giờ bắt đầu, giờ kết thúc và sức chứa dương.");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "Thiếu ConnectionStrings:DefaultConnection. Đặt biến môi trường ConnectionStrings__DefaultConnection.");

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDbContext<RepairDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDataProtection();

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

// Register Warehouse Services (Phase 1)
builder.Services.AddScoped<IKhoService, KhoService>();
builder.Services.AddScoped<IPhieuNhapService, PhieuNhapService>();
builder.Services.AddScoped<ILinhKienSerialService, LinhKienSerialService>();
builder.Services.AddScoped<IXuatKhoService, XuatKhoService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var configuredSecret = jwtSettings["SecretKey"];
if (string.IsNullOrWhiteSpace(configuredSecret) || Encoding.UTF8.GetByteCount(configuredSecret) < 32)
    throw new InvalidOperationException(
        "Thiếu JWT SecretKey hợp lệ (ít nhất 32 byte). Đặt biến môi trường JwtSettings__SecretKey.");
var secretKey = Encoding.UTF8.GetBytes(configuredSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
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

// Không phục vụ API trên schema cũ: lỗi sẽ được báo ngay lúc khởi động, kèm CSDL và migration cần chạy.
await V3SchemaValidator.ValidateAsync(connectionString);

// Initialize database with seed data (wrapped in try-catch)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await SeedData.InitializeAsync(context, builder.Configuration, app.Environment.IsDevelopment());
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database initialization: {ex.Message}");
    // Continue anyway - database might not be accessible but API can still run in read-only mode
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


