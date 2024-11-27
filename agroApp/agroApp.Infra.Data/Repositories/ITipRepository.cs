using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
public interface ITipRepository
    {
        Task<List<Tip>> GetAllTipsAsync();
        Task<Tip> GetTipByIdAsync(Guid id);
        Task AddTipAsync(Tip tip);
        Task UpdateTipAsync(Tip tip);
        Task DeleteTipAsync(Guid id);
    }
}