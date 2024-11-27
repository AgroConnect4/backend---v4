using agroApp.Domain.Entities;
using agroApp.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
public class NewsRepository : INewsRepository
    {
        private readonly AppDbContext _context;
        public NewsRepository(AppDbContext context) => _context = context;

        public async Task<List<News>> GetAllNewsAsync() => await _context.News.ToListAsync();
        public async Task<News> GetNewsByIdAsync(Guid id) => await _context.News.FindAsync(id);
        public async Task AddNewsAsync(News news) { await _context.News.AddAsync(news); await _context.SaveChangesAsync(); }
        public async Task UpdateNewsAsync(News news) { _context.News.Update(news); await _context.SaveChangesAsync(); }
        public async Task DeleteNewsAsync(Guid id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
            }
        }
    }
}