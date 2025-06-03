using WordWisp.API.Models.DTOs.Dictionaries;

namespace WordWisp.API.Services.Interfaces
{
    public interface IDictionaryService
    {
        Task<List<DictionaryDto>> GetUserDictionariesAsync(int userId);
        Task<List<DictionaryDto>> GetUserPublicDictionariesAsync(int userId);
        Task<DictionaryDetailDto?> GetDictionaryByIdAsync(int id, int userId);
        Task<DictionaryDetailDto?> GetPublicDictionaryByIdAsync(int id);
        Task<DictionaryDto> CreateDictionaryAsync(CreateDictionaryRequest request, int userId);
        Task<DictionaryDto?> UpdateDictionaryAsync(int id, UpdateDictionaryRequest request, int userId);
        Task<bool> DeleteDictionaryAsync(int id, int userId);
        Task<bool> CheckDictionaryOwnershipAsync(int dictionaryId, int userId);
        Task<bool> ToggleVisibilityAsync(int id, int userId);
    }
}
