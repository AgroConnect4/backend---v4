using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace agroApp.API.Services
{
    public class TipService : ITipService
    {
        private readonly ITipRepository _tipRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public TipService(ITipRepository tipRepository, 
        UserManager<User> userManager,
        IUserRepository userRepository)
        {
            _tipRepository = tipRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<List<Tip>> GetAllTipsAsync() => await _tipRepository.GetAllTipsAsync();
        public async Task<Tip> GetTipByIdAsync(Guid id) => await _tipRepository.GetTipByIdAsync(id);

        public async Task<Tip> CreateTipAsync(Tip tip, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            tip.Author = user;
            await _tipRepository.AddTipAsync(tip);
            return tip;
        }

        public async Task UpdateTipAsync(Tip tip) => await _tipRepository.UpdateTipAsync(tip);
        public async Task DeleteTipAsync(Guid id) => await _tipRepository.DeleteTipAsync(id);
    }
}