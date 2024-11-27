using agroApp.API.DTOs;
using agroApp.API.Services;
using agroApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using agroApp.Infra.Data.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace agroApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin")] //This authorization attribute still works with custom roles
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository; //New repository for roles
        private readonly IUserRoleRepository _userRoleRepository; //New repository for user roles
        private readonly IAdminService _adminService;

        private readonly IUserRoleService _userRoleService;

        public AdminController(
            UserManager<User> userManager,
            IUserRepository userRepository, 
            IRoleRepository roleRepository,
            IAdminService adminService,
            IUserRoleRepository userRoleRepository,
            IUserRoleService userRoleService)
        {
            _userManager = userManager;
            _adminService = adminService;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userRoleService = userRoleService;
            _userRoleRepository = userRoleRepository;
        }

        private Guid GetUserId()
        {
            string userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid User ID"); // throw instead of returning unauthorized
            }
            return userId;
        }

        [HttpPost("register/admin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] CreateAdminDto adminDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.RegisterAdminAsync(adminDto.Username, adminDto.Email, adminDto.Password);

            if (result.Succeeded)
            {
                return Ok("Admin registrado com sucesso.");
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        [HttpPut("update/password/{id}")]
        public async Task<IActionResult> UpdateAdminPassword(Guid id, [FromBody] AdminUpdatePasswordDto passwordDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            try
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, passwordDto.CurrentPassword, passwordDto.NewPassword);
                if (!changePasswordResult.Succeeded) return BadRequest(changePasswordResult.Errors);
                await _userRepository.UpdateAsync(user);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating password: {ex.Message}");
            }
        }

        [HttpPut("update/role/{id}")]
        public async Task<IActionResult> UpdateAdminRoles(Guid id, [FromBody] AdminUpdateRolesDto rolesDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return NotFound("Usuário não encontrado.");

            try
            {
                await UpdateAdminRolesAsync(user, rolesDto); // Helper function (see below)
                await _userRepository.UpdateAsync(user);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating roles: {ex.Message}");
            }
        }

        //Helper function (same as before, but moved here for better organization)
        private async Task UpdateAdminRolesAsync(User user, AdminUpdateRolesDto rolesDto)
        {
            var adminRole = await _roleRepository.GetByNameAsync("Admin");
            var userRole = await _roleRepository.GetByNameAsync("User");

            if (adminRole == null || userRole == null) throw new Exception("Admin or User role not found.");

            if (rolesDto.AddAdminRole) await _userRoleService.AddUserRoleAsync(user.Id, adminRole.Id);
            if (rolesDto.RemoveAdminRole) await _userRoleService.RemoveUserRoleAsync(user.Id, adminRole.Id);
            if (rolesDto.AddUserRole) await _userRoleService.AddUserRoleAsync(user.Id, userRole.Id);
            if (rolesDto.RemoveUserRole) await _userRoleService.RemoveUserRoleAsync(user.Id, userRole.Id);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }
    }
}