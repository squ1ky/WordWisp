using Microsoft.EntityFrameworkCore;
using WordWisp.API.Data;
using WordWisp.API.Entities;
using WordWisp.API.Repositories.Interfaces;

namespace WordWisp.API.Repositories.Implementations
{
    public class WordRepository : IWordRepository
    {
        private readonly ApplicationDbContext _context;

        public WordRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Word>> GetByDictionaryIdAsync(int dictionaryId)
        {
            return await _context.Words
                .Where(w => w.DictionaryId == dictionaryId)
                .OrderByDescending(w => w.Id)
                .ToListAsync();
        }

        public async Task<Word?> GetByIdAsync(int id)
        {
            return await _context.Words
                .Include(w => w.Dictionary)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<Word> CreateAsync(Word word)
        {
            _context.Words.Add(word);
            await _context.SaveChangesAsync();
            return word;
        }

        public async Task<Word> UpdateAsync(Word word)
        {
            _context.Words.Update(word);
            await _context.SaveChangesAsync();
            return word;
        }

        public async Task DeleteAsync(int id)
        {
            var word = await _context.Words.FindAsync(id);
            if (word != null)
            {
                _context.Words.Remove(word);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Word>> SearchInDictionaryAsync(int dictionaryId, string searchTerm)
        {
            return await _context.Words
                .Where(w => w.DictionaryId == dictionaryId &&
                           (w.Term.Contains(searchTerm) || w.Definition.Contains(searchTerm)))
                .OrderByDescending(w => w.Id)
                .ToListAsync();
        }
    }
}
