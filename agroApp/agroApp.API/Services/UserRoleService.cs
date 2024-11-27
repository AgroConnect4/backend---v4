using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public class UserRoleService : IUserRoleService
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRoleService(IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public async Task AddUserRoleAsync(Guid userId, Guid roleId)
        {
            await _userRoleRepository.AddAsync(new UserRole { UserId = userId, RoleId = roleId });
        }

        public async Task RemoveUserRoleAsync(Guid userId, Guid roleId)
        {
            await _userRoleRepository.RemoveUserRoleAsync(userId, roleId);
        }
        // Add other methods for UserRole management as needed. For example:
        // public async Task<List<Role>> GetUserRolesAsync(Guid userId); etc.
    }
}