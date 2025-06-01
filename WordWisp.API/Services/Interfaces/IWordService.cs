using WordWisp.API.Models.DTOs.Words;

namespace WordWisp.API.Services.Interfaces
{
    public interface IWordService
    {
        Task<List<WordDto>> GetWordsByDictionaryIdAsync(int dictionaryId, int userId);
        Task<WordDto?> GetWordByIdAsync(int id, int userId);
        Task<WordDto> CreateWordAsync(CreateWordRequest request, int dictionaryId, int userId);
        Task<WordDto?> UpdateWordAsync(int id, UpdateWordRequest request, int userId);
        Task<bool> DeleteWordAsync(int id, int userId);
        Task<List<WordDto>> SearchWordsAsync(int dictionaryId, string searchTerm, int userId);
    }
}
