namespace WordWisp.API.Services.Interfaces
{
    public interface IUserContextService
    {
        int? GetCurrentUserId();
        bool IsOwner(int userId);
        bool IsAuthenticated();
    }
}

