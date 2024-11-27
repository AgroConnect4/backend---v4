using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agroApp.API.DTOs;

namespace agroApp.API.Services
{
public interface ITipService
    {
        Task<List<Tip>> GetAllTipsAsync();
        Task<Tip> GetTipByIdAsync(Guid id);
        Task<Tip> CreateTipAsync(Tip tip, Guid userId);
        Task UpdateTipAsync(Tip tip);
        Task DeleteTipAsync(Guid id);
    }
}