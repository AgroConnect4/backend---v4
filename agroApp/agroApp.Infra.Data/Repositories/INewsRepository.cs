using agroApp.Domain.Entities;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public interface INewsRepository
    {
        Task<List<News>> GetAllNewsAsync();
        Task<News> GetNewsByIdAsync(Guid id);
        Task AddNewsAsync(News news);
        Task UpdateNewsAsync(News news);
        Task DeleteNewsAsync(Guid id);
    }
}