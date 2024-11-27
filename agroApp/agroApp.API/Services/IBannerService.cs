using agroApp.API.DTOs;
using System.Threading.Tasks;
using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;

namespace agroApp.API.Services
{
    public interface IBannerService
        {
            Task<List<Banner>> GetAllBannersAsync();
            Task<Banner> GetBannerByIdAsync(Guid id);
            Task<Banner> CreateBannerAsync(Banner banner, Guid userId);
            Task UpdateBannerAsync(Banner banner);
            Task DeleteBannerAsync(Guid id);
        }
}
