using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using agroApp.Infra.Data.Context;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
public class TipRepository : ITipRepository
    {
        private readonly AppDbContext _context;
        public TipRepository(AppDbContext context) => _context = context;

        public async Task<List<Tip>> GetAllTipsAsync() => await _context.Tips.ToListAsync();
        public async Task<Tip> GetTipByIdAsync(Guid id) => await _context.Tips.FindAsync(id);
        public async Task AddTipAsync(Tip tip) { await _context.Tips.AddAsync(tip); await _context.SaveChangesAsync(); }
        public async Task UpdateTipAsync(Tip tip) { _context.Tips.Update(tip); await _context.SaveChangesAsync(); }
        public async Task DeleteTipAsync(Guid id)
        {
            var tip = await _context.Tips.FindAsync(id);
            if (tip != null)
            {
                _context.Tips.Remove(tip);
                await _context.SaveChangesAsync();
            }
        }
    }
}