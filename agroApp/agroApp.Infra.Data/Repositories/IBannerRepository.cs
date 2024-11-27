using agroApp.Domain.Entities;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public interface IBannerRepository
    {
        Task<List<Banner>> GetAllBannersAsync();
        Task<Banner> GetBannerByIdAsync(Guid id);
        Task AddBannerAsync(Banner banner);
        Task UpdateBannerAsync(Banner banner);
        Task DeleteBannerAsync(Guid id);
    }
}