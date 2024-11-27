using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using System;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task CreateProfileAsync(Profile profile)
        {
            await _profileRepository.AddAsync(profile);
        }

        public async Task<Profile> GetProfileAsync(Guid userId)
        {
            return await _profileRepository.GetByUserIdAsync(userId); //You'll need this in IProfileRepository
        }

        public async Task UpdateProfileAsync(Profile profile)
        {
            await _profileRepository.UpdateAsync(profile);
        }
    }
}