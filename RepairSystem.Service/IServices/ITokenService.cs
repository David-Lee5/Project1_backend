namespace RepairSystem.Service.IServices;

public interface ITokenService
{
    string GenerateAccessToken(long userId, string username, string? roleName, List<string>? permissions = null);
    string GenerateRefreshToken();
    Task<(bool isValid, long? userId)> ValidateTokenAsync(string token);
}
