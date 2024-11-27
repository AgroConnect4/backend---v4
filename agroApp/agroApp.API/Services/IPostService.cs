using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agroApp.API.DTOs;

namespace agroApp.API.Services
{
    public interface IPostService
    {

        Task<Post> CreatePostAsync(CreatePostDto request, Guid userId);
        Task SendPostCreatedNotifications(Post post);
        Task<Post> UpdatePostAsync(Guid postId, UpdatePostDto request, Guid userId); 
        Task DeletePostAsync(Guid postId, Guid userId);
        Task<Post> GetPostByIdAsync(Guid postId);
        Task<List<Post>> GetAllPostsAsync();
        Task<List<Post>> GetAllPostsByUserIdAsync(Guid userId);
        Task<List<Post>> GetAllPostsByCategoryNameAsync(string categoryName);
        Task<Post> ReportPostAsync(Guid postId, Guid userId, string reason);
        //Task<Post> ToggleLikeAsync(Guid postId, Guid userId);
        Task<Post> ToggleReactionAsync(Guid postId, Guid userId, ReactionType reactionType);
        Task TogglePostActivationAsync(Guid postId, Guid userId);
        Task<bool> HasBeenReportedAsync(Guid postId);
        Task<Post> CreateReactionAsync(Guid postId, Guid userId, ReactionType reactionType);
        Task<List<PostReport>> GetPostReportsAsync(Guid postId);
        Task SendReactionNotificationAsync(Post post, Guid userId, ReactionType reactionType);
        //Task<PostComment> CreateCommentAsync(CreatePostCommentDto request, Guid userId);
    }
}