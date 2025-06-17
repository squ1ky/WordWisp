namespace WordWisp.Web.Services.Interfaces
{
    public interface ITokenService
    {
        string? GetUserIdFromToken(string? token);
        string? GetUserRoleFromToken(string token);
        string? GetUsernameFromToken(string token);
        bool IsTokenValid(string token);
    }
}
