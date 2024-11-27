using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace agroApp.API.Services
{
public class NewsService : INewsService
    {
        private readonly INewsRepository _newsRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository; //Added UserManager to get the user details.

        public NewsService(INewsRepository newsRepository,
         UserManager<User> userManager,
         IUserRepository userRepository)
        {
            _newsRepository = newsRepository;
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<List<News>> GetAllNewsAsync() => await _newsRepository.GetAllNewsAsync();
        public async Task<News> GetNewsByIdAsync(Guid id) => await _newsRepository.GetNewsByIdAsync(id);

        public async Task<News> CreateNewsAsync(News news, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("User not found");
            news.Author = user;
            await _newsRepository.AddNewsAsync(news);
            return news;
        }

        public async Task UpdateNewsAsync(News news) => await _newsRepository.UpdateNewsAsync(news);
        public async Task DeleteNewsAsync(Guid id) => await _newsRepository.DeleteNewsAsync(id);
    }
}