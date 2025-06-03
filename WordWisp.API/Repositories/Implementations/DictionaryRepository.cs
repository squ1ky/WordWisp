using Microsoft.EntityFrameworkCore;
using WordWisp.API.Data;
using WordWisp.API.Entities;
using WordWisp.API.Repositories.Interfaces;

namespace WordWisp.API.Repositories.Implementations
{
    public class DictionaryRepository : IDictionaryRepository
    {
        private readonly ApplicationDbContext _context;

        public DictionaryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Dictionary>> GetByUserIdAsync(int userId)
        {
            return await _context.Dictionaries
                .Where(d => d.UserId == userId)
                .Include(d => d.Words)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Dictionary>> GetUserPublicDictionariesAsync(int userId)
        {
            return await _context.Dictionaries
                .Where(d => d.UserId == userId && d.IsPublic)
                .Include(d => d.Words)
                .Include(d => d.User)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }


        public async Task<Dictionary?> GetByIdAndUserIdAsync(int id, int userId)
        {
            return await _context.Dictionaries
                .Include(d => d.Words)
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        }

        public async Task<Dictionary?> GetPublicByIdAsync(int id)
        {
            return await _context.Dictionaries
                .Include(d => d.Words)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id && d.IsPublic);
        }

        public async Task<Dictionary> CreateAsync(Dictionary dictionary)
        {
            dictionary.CreatedAt = DateTime.UtcNow;
            dictionary.UpdatedAt = DateTime.UtcNow;

            _context.Dictionaries.Add(dictionary);
            await _context.SaveChangesAsync();

            return dictionary;
        }

        public async Task<Dictionary> UpdateAsync(Dictionary dictionary)
        {
            dictionary.UpdatedAt = DateTime.UtcNow;

            _context.Dictionaries.Update(dictionary);
            await _context.SaveChangesAsync();

            return dictionary;
        }

        public async Task DeleteAsync(int id)
        {
            var dictionary = await _context.Dictionaries.FindAsync(id);

            if (dictionary != null)
            {
                _context.Dictionaries.Remove(dictionary);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id, int userId)
        {
            return await _context.Dictionaries
                .AnyAsync(d => d.Id == id && d.UserId == userId);
        }

        public async Task<bool> ToggleVisibilityAsync(int id, int userId)
        {
            var dictionary = await _context.Dictionaries
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);

            if (dictionary == null)
                return false;

            dictionary.IsPublic = !dictionary.IsPublic;
            dictionary.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary?> GetByIdAndUserIdForUpdateAsync(int id, int userId)
        {
            return await _context.Dictionaries
                .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId);
        }


        public async Task<int> GetDictionariesCountByUserIdAsync(int userId)
        {
            return await _context.Dictionaries
                .Where(d => d.UserId == userId)
                .CountAsync();
        }

    }
}
