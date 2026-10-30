using RepairSystem.Model;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Service.Helpers;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context, string? initialAdminPassword = null)
    {
        // Seed chỉ chạy khi DB đã có schema (dùng SQL script thủ công, không MigrateAsync)
        // Kiểm tra xem đã có tài khoản admin chưa
        bool hasAdmin = false;
        try
        {
            hasAdmin = await context.TaiKhoans.AnyAsync(tk => tk.TenDangNhap == "admin");
        }
        catch
        {
            // DB chưa sẵn sàng — bỏ qua seed
            return;
        }

        if (hasAdmin) return;

        // Chỉ tạo tài khoản quản trị khi người vận hành cung cấp mật khẩu bootstrap.
        // Không sử dụng mật khẩu mặc định dùng chung hoặc ghi mật khẩu ra log.
        if (string.IsNullOrWhiteSpace(initialAdminPassword)) return;
        if (initialAdminPassword.Length < 12 || initialAdminPassword.Length > 100)
            throw new InvalidOperationException("Bootstrap:AdminPassword phải dài 12–100 ký tự.");

        await using var tx = await context.Database.BeginTransactionAsync();
        var adminTk = new TaiKhoan
        {
            TenDangNhap = "admin",
            MatKhauHash = PasswordHelper.HashPassword(initialAdminPassword),
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

        Console.WriteLine("Seed: tài khoản quản trị bootstrap đã được tạo.");
    }
}
