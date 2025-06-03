using WordWisp.API.Entities;
using WordWisp.API.Models.DTOs.Words;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class WordService : IWordService
    {
        private readonly IWordRepository _wordRepository;
        private readonly IDictionaryRepository _dictionaryRepository;

        public WordService(IWordRepository wordRepository, IDictionaryRepository dictionaryRepository)
        {
            _wordRepository = wordRepository;
            _dictionaryRepository = dictionaryRepository;
        }

        public async Task<List<WordDto>> GetWordsByDictionaryIdAsync(int dictionaryId, int userId)
        {
            var canAccess = await _dictionaryRepository.ExistsAsync(dictionaryId, userId);

            if (!canAccess) return new List<WordDto>();

            var words = await _wordRepository.GetByDictionaryIdAsync(dictionaryId);

            return words.Select(w => new WordDto
            {
                Id = w.Id,
                Term = w.Term,
                Definition = w.Definition,
                Transcription = w.Transcription,
                Example = w.Example
            }).ToList();
        }

        public async Task<WordDto?> GetWordByIdAsync(int id, int userId)
        {
            var word = await _wordRepository.GetByIdAsync(id);

            if (word == null || word.Dictionary.UserId != userId)
                return null;

            return new WordDto
            {
                Id = word.Id,
                Term = word.Term,
                Definition = word.Definition,
                Transcription = word.Transcription,
                Example = word.Example
            };
        }

        public async Task<WordDto> CreateWordAsync(CreateWordRequest request, int dictionaryId, int userId)
        {
            var canAccess = await _dictionaryRepository.ExistsAsync(dictionaryId, userId);

            if (!canAccess)
                throw new UnauthorizedAccessException("Нет доступа к словарю");

            var word = new Word
            {
                Term = request.Term,
                Definition = request.Definition,
                Transcription = request.Transcription,
                Example = request.Example,
                DictionaryId = dictionaryId
            };

            var createdWord = await _wordRepository.CreateAsync(word);

            return new WordDto
            {
                Id = createdWord.Id,
                Term = createdWord.Term,
                Definition = createdWord.Definition,
                Transcription = createdWord.Transcription,
                Example = createdWord.Example
            };
        }

        public async Task<WordDto?> UpdateWordAsync(int id, UpdateWordRequest request, int userId)
        {
            var word = await _wordRepository.GetByIdAsync(id);

            if (word == null || word.Dictionary.UserId != userId)
                return null;

            word.Term = request.Term;
            word.Definition = request.Definition;
            word.Transcription = request.Transcription;
            word.Example = request.Example;

            var updatedWord = await _wordRepository.UpdateAsync(word);

            return new WordDto
            {
                Id = updatedWord.Id,
                Term = updatedWord.Term,
                Definition = updatedWord.Definition,
                Transcription = updatedWord.Transcription,
                Example = updatedWord.Example
            };
        }

        public async Task<bool> DeleteWordAsync(int id, int userId)
        {
            var word = await _wordRepository.GetByIdAsync(id);

            if (word == null || word.Dictionary.UserId != userId)
                return false;

            await _wordRepository.DeleteAsync(id);

            return true;
        }

        public async Task<List<WordDto>> SearchWordsAsync(int dictionaryId, string searchTerm, int userId)
        {
            var canAccess = await _dictionaryRepository.ExistsAsync(dictionaryId, userId);

            if (!canAccess) return new List<WordDto>();

            var words = await _wordRepository.SearchInDictionaryAsync(dictionaryId, searchTerm);

            return words.Select(w => new WordDto
            {
                Id = w.Id,
                Term = w.Term,
                Definition = w.Definition,
                Transcription = w.Transcription,
                Example = w.Example
            }).ToList();
        }
    }
}
