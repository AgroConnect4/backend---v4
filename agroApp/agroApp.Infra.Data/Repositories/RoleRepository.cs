using agroApp.Domain.Entities;
using agroApp.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role> GetByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task AddOrUpdateAsync(Role role)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role.Name);
            if (existingRole != null)
            {
                //Do nothing - Role already exists, no need to update (assuming Name is unique)
            }
            else
            {
                await _context.Roles.AddAsync(role);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<Role> AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }
    }
}