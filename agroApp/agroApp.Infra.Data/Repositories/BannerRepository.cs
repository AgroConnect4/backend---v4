using agroApp.Domain.Entities;
using agroApp.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public class BannerRepository : IBannerRepository
        {
            private readonly AppDbContext _context;
            public BannerRepository(AppDbContext context) => _context = context;

            public async Task<List<Banner>> GetAllBannersAsync() => await _context.Banners.ToListAsync();
            public async Task<Banner> GetBannerByIdAsync(Guid id) => await _context.Banners.FindAsync(id);
            public async Task AddBannerAsync(Banner banner) { await _context.Banners.AddAsync(banner); await _context.SaveChangesAsync(); }
            public async Task UpdateBannerAsync(Banner banner) { _context.Banners.Update(banner); await _context.SaveChangesAsync(); }
            public async Task DeleteBannerAsync(Guid id)
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner != null)
                {
                    _context.Banners.Remove(banner);
                    await _context.SaveChangesAsync();
                }
            }
        }
}