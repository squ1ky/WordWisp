namespace WordWisp.Web.Services.Interfaces
{
    public interface ITokenService
    {
        string? GetUserIdFromToken(string? token);
    }
}
