using agroApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public interface IAdminService
    {
        Task<IdentityResult> RegisterAdminAsync(string username, string email, string password);
    }
}
