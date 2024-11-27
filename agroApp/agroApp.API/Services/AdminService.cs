// agroApp.API.Services/AdminService.cs
using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using agroApp.API.Services; // Make sure to add this

namespace agroApp.API.Services
{
    public class AdminService : IAdminService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleService _userRoleService; // Use IUserRoleService here

        public AdminService(UserManager<User> userManager, IRoleRepository roleRepository, IUserRoleService userRoleService) //Inject IUserRoleService
        {
            _userManager = userManager;
            _roleRepository = roleRepository;
            _userRoleService = userRoleService;
        }

        public async Task<IdentityResult> RegisterAdminAsync(string username, string email, string password)
        {
            var user = new User { UserName = username, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var adminRole = await _roleRepository.GetByNameAsync("Admin") ?? new Role { Name = "Admin" };
                await _roleRepository.AddOrUpdateAsync(adminRole);
                await _userRoleService.AddUserRoleAsync(user.Id, adminRole.Id); // Use the service
                return IdentityResult.Success;
            }
            else
            {
                return result;
            }
        }
    }
    // ... rest of AdminService ...
}