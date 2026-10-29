namespace RepairSystem.Service.Helpers;

/// <summary>
/// Băm và kiểm tra mật khẩu bằng BCrypt (khớp với hash $2a$11$... đang lưu trong cột tai_khoan.mat_khau_hash).
/// </summary>
public static class PasswordHelper
{
    private const int WorkFactor = 11;

    public static string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public static bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception)
        {
            // Hash sai định dạng (ví dụ dữ liệu cũ) -> coi như không khớp, không làm sập API.
            return false;
        }
    }
}
