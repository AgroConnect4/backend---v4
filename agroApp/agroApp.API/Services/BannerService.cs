using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace agroApp.API.Services
{
public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public BannerService(IBannerRepository bannerRepository, UserManager<User> userManager, IUserRepository userRepository)
        {
            _bannerRepository = bannerRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<List<Banner>> GetAllBannersAsync() => await _bannerRepository.GetAllBannersAsync();
        public async Task<Banner> GetBannerByIdAsync(Guid id) => await _bannerRepository.GetBannerByIdAsync(id);

        public async Task<Banner> CreateBannerAsync(Banner banner, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            banner.User = user;
            await _bannerRepository.AddBannerAsync(banner);
            return banner;
        }

        public async Task UpdateBannerAsync(Banner banner) => await _bannerRepository.UpdateBannerAsync(banner);
        public async Task DeleteBannerAsync(Guid id) => await _bannerRepository.DeleteBannerAsync(id);
    }
}