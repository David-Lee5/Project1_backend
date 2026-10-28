namespace RepairSystem.Service.Helpers;

/// <summary>
/// Các giá trị của cột tai_khoan.vai_tro trong database.
/// Dùng với [Authorize(Roles = Roles.Admin)] hoặc [Authorize(Roles = Roles.Admin + "," + Roles.LeTan)].
/// </summary>
public static class Roles
{
    public const string Admin = "ADMIN";
    public const string LeTan = "LE_TAN";
    public const string ThuKho = "THU_KHO";
    public const string KyThuatVien = "KY_THUAT_VIEN";
    public const string KhachHang = "KHACH_HANG";

    /// <summary>Tất cả vai trò nhân viên (không gồm khách hàng).</summary>
    public const string NhanVien = Admin + "," + LeTan + "," + ThuKho + "," + KyThuatVien;
}
