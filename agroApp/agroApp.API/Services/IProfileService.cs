using agroApp.Domain.Entities;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public interface IProfileService
    {
        Task CreateProfileAsync(Profile profile);
        Task<Profile> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Profile profile);
    }
}