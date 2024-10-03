using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public interface IPostRepository
    {
        Task<Post> GetByIdAsync(int postId);
        Task<List<Post>> GetAllByUserIdAsync(int userId);
        Task<List<Post>> GetAllByPostTypeAsync(string postType);
        Task<Post> AddAsync(Post post);
        Task<Post> UpdateAsync(Post post);
        Task DeleteAsync(int postId);
    }
}