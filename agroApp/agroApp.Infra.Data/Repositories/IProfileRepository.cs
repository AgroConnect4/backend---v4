using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Domain.Repositories
{
    public interface IProfileRepository
    {
        Task<Profile> GetByIdAsync(int profileId);
        Task<Profile> GetByUserIdAsync(int userId);
        Task<Profile> AddAsync(Profile profile);
        Task<Profile> UpdateAsync(Profile profile);
        Task DeleteAsync(int profileId);
    }
}