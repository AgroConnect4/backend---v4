using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public interface IProfileRepository
    {
        Task AddAsync(Profile profile);

        Task UpdateAsync(Profile profile);

        Task<Profile> GetByUserIdAsync(Guid userId);
    }
}