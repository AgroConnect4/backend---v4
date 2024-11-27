using agroApp.API.DTOs;
using System.Threading.Tasks;
using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;

namespace agroApp.API.Services
{
public interface INewsService
    {
        Task<List<News>> GetAllNewsAsync();
        Task<News> GetNewsByIdAsync(Guid id);
        Task<News> CreateNewsAsync(News news, Guid userId);
        Task UpdateNewsAsync(News news);
        Task DeleteNewsAsync(Guid id);
    }
}