using RepairSystem.Model;
using Microsoft.EntityFrameworkCore;

namespace RepairSystem.Service.Helpers;

public static class SeedData
{
    public static async Task InitializeAsync(AppDbContext context)
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

        // Tạo tài khoản admin mặc định
        var adminTk = new TaiKhoan
        {
            TenDangNhap = "admin",
            MatKhauHash = PasswordHelper.HashPassword("Admin@123"),
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

        Console.WriteLine("✅ Seed: tài khoản admin/Admin@123 đã được tạo.");
    }
}
