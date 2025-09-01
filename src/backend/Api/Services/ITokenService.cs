namespace Api.Services;

public interface ITokenService
{
    string GenerateToken(string role);
    string GenerateAnonymousToken();
}