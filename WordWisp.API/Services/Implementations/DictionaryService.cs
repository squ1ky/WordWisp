using WordWisp.API.Entities;
using WordWisp.API.Models.DTOs.Dictionaries;
using WordWisp.API.Models.DTOs.Words;
using WordWisp.API.Repositories.Interfaces;
using WordWisp.API.Services.Interfaces;

namespace WordWisp.API.Services.Implementations
{
    public class DictionaryService : IDictionaryService
    {
        private readonly IDictionaryRepository _dictionaryRepository;

        public DictionaryService(IDictionaryRepository dictionaryRepository)
        {
            _dictionaryRepository = dictionaryRepository;
        }

        public async Task<List<DictionaryDto>> GetUserDictionariesAsync(int userId)
        {
            var dictionaries = await _dictionaryRepository.GetByUserIdAsync(userId);

            return dictionaries.Select(d => new DictionaryDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                WordsCount = d.Words.Count,
                CreatedAt = d.CreatedAt,
                IsPublic = d.IsPublic
            }).ToList();
        }

        public async Task<List<DictionaryDto>> GetUserPublicDictionariesAsync(int userId)
        {
            var dictionaries = await _dictionaryRepository.GetUserPublicDictionariesAsync(userId);

            return dictionaries.Select(d => new DictionaryDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                WordsCount = d.Words.Count,
                CreatedAt = d.CreatedAt,
                IsPublic = d.IsPublic,
                AuthorName = $"{d.User.Name} {d.User.Surname}"
            }).ToList();
        }

        public async Task<DictionaryDetailDto?> GetDictionaryByIdAsync(int id, int userId)
        {
            var dictionary = await _dictionaryRepository.GetByIdAndUserIdAsync(id, userId);

            if (dictionary == null) return null;

            return new DictionaryDetailDto
            {
                Id = dictionary.Id,
                Name = dictionary.Name,
                Description = dictionary.Description,
                CreatedAt = dictionary.CreatedAt,
                IsPublic = dictionary.IsPublic,
                Words = dictionary.Words.Select(w => new WordDto
                {
                    Id = w.Id,
                    Term = w.Term,
                    Definition = w.Definition,
                    Transcription = w.Transcription,
                    Example = w.Example
                }).ToList()
            };
        }

        public async Task<DictionaryDetailDto?> GetPublicDictionaryByIdAsync(int id)
        {
            var dictionary = await _dictionaryRepository.GetPublicByIdAsync(id);

            if (dictionary == null) return null;

            return new DictionaryDetailDto
            {
                Id = dictionary.Id,
                Name = dictionary.Name,
                Description = dictionary.Description,
                CreatedAt = dictionary.CreatedAt,
                IsPublic = dictionary.IsPublic,
                Words = dictionary.Words.Select(w => new WordDto
                {
                    Id = w.Id,
                    Term = w.Term,
                    Definition = w.Definition,
                    Transcription = w.Transcription,
                    Example = w.Example
                }).ToList()
            };
        }

        public async Task<DictionaryDto> CreateDictionaryAsync(CreateDictionaryRequest request, int userId)
        {
            var dictionary = new Dictionary
            {
                Name = request.Name,
                Description = request.Description,
                IsPublic = request.IsPublic,
                UserId = userId
            };

            var createdDictionary = await _dictionaryRepository.CreateAsync(dictionary);

            return new DictionaryDto
            {
                Id = createdDictionary.Id,
                Name = createdDictionary.Name,
                Description = createdDictionary.Description,
                WordsCount = 0,
                CreatedAt = createdDictionary.CreatedAt,
                IsPublic = createdDictionary.IsPublic
            };
        }

        public async Task<DictionaryDto?> UpdateDictionaryAsync(int id, UpdateDictionaryRequest request, int userId)
        {
            var dictionary = await _dictionaryRepository.GetByIdAndUserIdAsync(id, userId);

            if (dictionary == null) return null;

            dictionary.Name = request.Name;
            dictionary.Description = request.Description;
            dictionary.IsPublic = request.IsPublic;

            var updatedDictionary = await _dictionaryRepository.UpdateAsync(dictionary);

            return new DictionaryDto
            {
                Id = updatedDictionary.Id,
                Name = updatedDictionary.Name,
                Description = updatedDictionary.Description,
                WordsCount = updatedDictionary.Words.Count,
                CreatedAt = updatedDictionary.CreatedAt,
                IsPublic = updatedDictionary.IsPublic
            };
        }

        public async Task<bool> DeleteDictionaryAsync(int id, int userId)
        {
            var exists = await _dictionaryRepository.ExistsAsync(id, userId);

            if (!exists) return false;

            await _dictionaryRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> CheckDictionaryOwnershipAsync(int dictionaryId, int userId)
        {
            return await _dictionaryRepository.ExistsAsync(dictionaryId, userId);
        }

        public async Task<bool> ToggleVisibilityAsync(int id, int userId)
        {
            return await _dictionaryRepository.ToggleVisibilityAsync(id, userId);
        }
    }
}
