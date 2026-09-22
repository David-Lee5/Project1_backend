using System.Security.Cryptography;
using System.Text;

namespace RepairSystem.Service.Helpers;

public class PasswordHelper
{
    public static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));
            var hashWithSalt = new byte[hash.Length + salt.Length];

            Array.Copy(salt, 0, hashWithSalt, 0, salt.Length);
            Array.Copy(hash, 0, hashWithSalt, salt.Length, hash.Length);

            return Convert.ToBase64String(hashWithSalt);
        }
    }

    public static bool VerifyPassword(string password, string hash)
    {
        var hashBytes = Convert.FromBase64String(hash);
        var salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, salt.Length);

        using (var sha256 = SHA256.Create())
        {
            var computedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + Convert.ToBase64String(salt)));
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (hashBytes[i + salt.Length] != computedHash[i])
                {
                    return false;
                }
            }
        }

        return true;
    }
}
