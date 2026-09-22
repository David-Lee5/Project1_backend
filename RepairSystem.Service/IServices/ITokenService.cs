namespace RepairSystem.Service.IServices;

public interface ITokenService
{
    string GenerateAccessToken(int userId, string username, string? roleName, List<string>? permissions = null);
    string GenerateRefreshToken();
    Task<(bool isValid, int? userId)> ValidateTokenAsync(string token);
}
