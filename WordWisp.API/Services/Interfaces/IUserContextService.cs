namespace WordWisp.API.Services.Interfaces
{
    public interface IUserContextService
    {
        int? GetCurrentUserId();
        string? GetCurrentUserRole();
        bool IsOwner(int userId);
        bool IsAuthenticated();
        bool IsTeacher();
        bool IsStudent();
    }
}

