using WordWisp.API.Models.DTOs.Users;
using WordWisp.API.Models.Entities;
using WordWisp.API.Models.Requests.Users;

namespace WordWisp.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<UserDto> UpdateUserAsync(int id, UpdateUserRequest request);
        Task<bool> ChangePasswordAsync(int id, ChangePasswordRequest request);
        Task<bool> ChangeEmailAsync(int id, string newEmail);
    }
}
