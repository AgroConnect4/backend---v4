using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public interface IProfileRepository
    {
        Task<Profile> GetProfileByIdAsync(int profileId);
        Task<Profile> GetByUserIdAsync(int userId);
        Task<Profile> UpdateAsync(Profile profile);
        Task DeleteAsync(int profileId);
    }
}