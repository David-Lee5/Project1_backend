using RepairSystem.Model;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Service.Helpers;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context,
        Microsoft.Extensions.Configuration.IConfiguration configuration, bool isDevelopment)
    {
        // Seed chỉ chạy khi DB đã có schema (dùng SQL script thủ công, không MigrateAsync)
        // Kiểm tra xem đã có tài khoản admin chưa
        bool hasAdmin = false;
        try
        {
            hasAdmin = await context.TaiKhoans.AnyAsync(tk => tk.VaiTro == Roles.Admin);
        }
        catch
        {
            // DB chưa sẵn sàng — bỏ qua seed
            return;
        }

        if (hasAdmin) return;

        var password = configuration["BootstrapAdmin:Password"];
        var sharedDevelopmentPassword = false;
        if (string.IsNullOrWhiteSpace(password) && isDevelopment)
        {
            password = configuration["BootstrapAdmin:DevelopmentPassword"];
            sharedDevelopmentPassword = !string.IsNullOrWhiteSpace(password);
        }
        if (string.IsNullOrWhiteSpace(password))
        {
            Console.WriteLine("Chưa có admin. Đặt biến môi trường BootstrapAdmin__Password để tạo tài khoản admin ban đầu.");
            return;
        }
        if (!sharedDevelopmentPassword && password.Length < 12)
            throw new InvalidOperationException("Mật khẩu admin riêng phải có ít nhất 12 ký tự.");
        var username = configuration["BootstrapAdmin:Username"]?.Trim();
        if (string.IsNullOrWhiteSpace(username)) username = "admin";
        if (await context.TaiKhoans.AnyAsync(tk => tk.TenDangNhap == username))
            throw new InvalidOperationException("Tên tài khoản admin ban đầu đã tồn tại nhưng chưa được tạo đúng vai trò.");

        await using var tx = await context.Database.BeginTransactionAsync();

        // Tạo tài khoản admin mặc định
        var adminTk = new TaiKhoan
        {
            TenDangNhap = username,
            MatKhauHash = PasswordHelper.HashPassword(password),
            VaiTro = Roles.Admin,
            HoatDong = true
        };

        context.TaiKhoans.Add(adminTk);
        await context.SaveChangesAsync();

        // Tạo nhân viên admin gắn với tài khoản
        var adminNv = new NhanVien
        {
            HoTen = "Quản trị viên",
            SoDienThoai = "0000000000",
            Email = "admin@repairsystem.com",
            ChucVu = Roles.Admin,
            TaiKhoanId = adminTk.Id
        };

        context.NhanViens.Add(adminNv);
        await context.SaveChangesAsync();

        await tx.CommitAsync();
        Console.WriteLine($"Đã tạo tài khoản admin ban đầu: {username}."
            + (sharedDevelopmentPassword ? " Đây là mật khẩu dùng chung cho môi trường Development."
                : " Hãy xóa biến môi trường BootstrapAdmin__Password sau khi khởi tạo."));
    }
}
