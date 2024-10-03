using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agroApp.API.DTOs;

namespace agroApp.Application.Services
{
    public interface IPostService
    {
        Task<int> CreatePostAsync(CreatePostDto request);
        Task<Post> GetPostByIdAsync(int postId);
        Task<List<Post>> GetAllPostsByUserIdAsync(int userId);
        Task<List<Post>> GetAllPostsByPostTypeAsync(string postType); // Novo método para buscar por tipo de post
        Task<Post> UpdatePostAsync(int postId, UpdatePostDto request); // DTO para atualização de post
        Task DeletePostAsync(int postId);
    }
}