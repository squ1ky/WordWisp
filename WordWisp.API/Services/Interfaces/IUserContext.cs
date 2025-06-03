namespace WordWisp.API.Services.Interfaces
{
    public interface IUserContext
    {
        int GetCurrentUserId();
        bool IsOwner(int userId);
        bool IsAdmin();
    }
}
