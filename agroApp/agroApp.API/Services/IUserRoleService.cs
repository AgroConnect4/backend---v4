using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agroApp.API.DTOs;

namespace agroApp.API.Services
{
    public interface IUserRoleService
    {
        Task AddUserRoleAsync(Guid userId, Guid roleId);
        Task RemoveUserRoleAsync(Guid userId, Guid roleId);
        // Other methods as needed.
    }
}