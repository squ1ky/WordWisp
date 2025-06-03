using WordWisp.API.Entities;

namespace WordWisp.API.Repositories.Interfaces
{
    public interface IWordRepository
    {
        Task<List<Word>> GetByDictionaryIdAsync(int dictionaryId);
        Task<Word?> GetByIdAsync(int id);
        Task<Word> CreateAsync(Word word);
        Task<Word> UpdateAsync(Word word);
        Task DeleteAsync(int id);
        Task<List<Word>> SearchInDictionaryAsync(int dictionaryId, string searchTerm);
    }
}
