using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public interface IPostCommentService
    {
        Task<PostComment> CreateCommentAsync(Guid postId, CreatePostCommentDto request, Guid userId);
        Task<PostComment> UpdateCommentAsync(Guid commentId, UpdatePostCommentDto request, Guid userId); //add userId to service
        Task DeleteCommentAsync(Guid commentId, Guid userId);
        Task SendCommentNotificationAsync(Post post, PostComment comment);
        Task<List<PostComment>> GetCommentsByPostIdAsync(Guid postId);
        Task<PostComment> GetCommentByIdAsync(Guid commentId);
        Task<List<PostComment>> GetAllCommentsByPostIdAsync(Guid postId); 
    }
}