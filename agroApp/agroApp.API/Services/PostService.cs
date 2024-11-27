using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agroApp.API.DTOs;
using Microsoft.AspNetCore.Http;
using agroApp.API.Services;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity; // Adicione o namespace para UserManager
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace agroApp.API.Services
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IPostRepository _postRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService; 
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager; // Injete UserManager
        private readonly IConnectionService _connectionService;
        private readonly INotificationRepository _notificationRepository;
        //private readonly ICommentRepository _commentRepository;
        //private readonly IShareRepository _shareRepository;
        private readonly IPostCommentRepository _postCommentRepository;
        private readonly IPostShareRepository _postShareRepository;   
        
        public PostService(IPostRepository postRepository,
        IConfiguration configuration,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PostService> logger,
        UserManager<User> userManager,
        IConnectionService connectionService,
        INotificationService notificationService,
        INotificationRepository notificationRepository,
        //ICommentRepository commentRepository,
        //IShareRepository shareRepository
        IPostCommentRepository postCommentRepository,
        IPostShareRepository postShareRepository
        )
        {
            _logger = logger;
            _configuration = configuration;
            _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _userManager = userManager; 
            _connectionService = connectionService;
            _notificationRepository = notificationRepository;
            //_commentRepository = commentRepository;
            //_shareRepository = shareRepository;
            _notificationService = notificationService;
            _postCommentRepository = postCommentRepository; // Assign the repository
            _postShareRepository = postShareRepository;
        }


        private Guid GetUserIdFromToken()
{
    var httpContextAccessor = _httpContextAccessor; //Assuming you have this injected.
    var configuration = _configuration; // Assuming you have this injected.
    var logger = _logger; // Assuming you have this injected.

    if (httpContextAccessor == null || httpContextAccessor.HttpContext == null)
    {
        logger.LogError("HttpContextAccessor is null or HttpContext is null.");
        throw new InvalidOperationException("Cannot access HTTP context.");
    }

    var authorizationHeader = httpContextAccessor.HttpContext.Request.Headers["Authorization"];
    if (!authorizationHeader.Any())
    {
        logger.LogError("Authorization header not found.");
        throw new UnauthorizedAccessException("Authorization header is missing.");
    }

    var token = authorizationHeader.FirstOrDefault().Split(" ").LastOrDefault();
    if (string.IsNullOrEmpty(token))
    {
        logger.LogError("JWT token not found in Authorization header.");
        throw new UnauthorizedAccessException("JWT token is missing.");
    }

    try
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"])),
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            _logger.LogError("User ID claim not found in JWT token.");
            throw new UnauthorizedAccessException("User ID claim is missing.");
        }

        // CORRECTED: Use Guid.TryParse with a Guid out parameter
        if (Guid.TryParse(userIdClaim.Value, out Guid userId)) 
        {
            return userId;
        }
        else
        {
            _logger.LogError("Invalid User ID format in JWT token. Value: {userIdClaimValue}", userIdClaim.Value);
            throw new UnauthorizedAccessException("Invalid User ID format in JWT token.");
        }
    }
    catch (SecurityTokenException ex)
    {
        logger.LogError(ex, "Error validating JWT token: {Message}", ex.Message);
        throw new UnauthorizedAccessException("Invalid JWT token.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Unexpected error retrieving User ID from JWT token.");
        throw;
    }
}

        public async Task<Post> CreatePostAsync(CreatePostDto request, Guid userId)
        {

                var post = new Post
                {
                    UserId = userId,
                    Content = request.Content,
                    Title = request.Title,
                    ImageUrl = request.ImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    Categories = request.Categories,
                    IsActive = true
                };

                var createdPost = await _postRepository.AddAsync(post);
                
                //Notifição
                try
                {
                    await SendPostCreatedNotifications(createdPost);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending post created notifications.");
                }

                return post;
        }

        public async Task SendPostCreatedNotifications(Post createdPost)
        {
            var userId = createdPost.UserId;
            var acceptedConnections = await _connectionService.GetConnectionsAsync(userId);

            foreach (var connectedUser in acceptedConnections)
            {
                try
                {
                    await _notificationService.SendPostCreatedNotificationAsync(connectedUser.Id, createdPost);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending notification to user {connectedUser.Id}");
                }
            }
        }

        public async Task<Post> UpdatePostAsync(Guid postId, UpdatePostDto request, Guid userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.UserId != userId)
            {
                return null;
            } 

            post.Content = request.Content ?? post.Content; // Null-coalescing for optional updates
            post.Title = request.Title ?? post.Title;
            post.ImageUrl = request.ImageUrl ?? post.ImageUrl;
            post.EditedAt = DateTime.UtcNow;
            post.Categories = request.Categories ?? post.Categories;

            return await _postRepository.UpdateAsync(post);
        }

        public async Task DeletePostAsync(Guid postId, Guid userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.UserId != userId) //Authorization Check
            {
                return; // Or throw exception
            }
            await _postRepository.DeleteAsync(postId);
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            return await _postRepository.GetByIdAsync(postId);
        }

         public async Task<List<Post>> GetAllPostsByUserIdAsync(Guid userId)
        {
            return await _postRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<List<Post>> GetAllPostsByCategoryNameAsync(string categoryName)
        {
            try
            {
                // Client-side filtering because EF Core can't translate Contains on a List<string>
                return (await _postRepository.GetAllAsync()).Where(p => p.Categories.Contains(categoryName, StringComparer.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts by category name.");
                throw; // Re-throw the exception to be handled higher up
            }
        }

        public async Task<Post> ToggleReactionAsync(Guid postId, Guid userId, ReactionType reactionType)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || !post.IsActive) return null;

            var existingReaction = post.Reactions.FirstOrDefault(r => r.UserId == userId && r.ReactionType == reactionType);
            if (existingReaction == null)
            {
                // Add reaction
                post.Reactions.Add(new PostReaction { PostId = postId, UserId = userId, ReactionType = reactionType, ReactedAt = DateTime.UtcNow });
            }
            else
            {
                // Remove reaction
                post.Reactions.Remove(existingReaction);
            }
            await _postRepository.UpdateAsync(post);
            return post;
        }

        public async Task<Post> CreateReactionAsync(Guid postId, Guid userId, ReactionType reactionType)
        {
            try
            {
                // Verifica se o post existe e se está ativo
                var post = await _postRepository.GetByIdAsync(postId);
                if (post == null || !post.IsActive)
                {
                    throw new ArgumentException("Post não encontrado ou inativo.");
                }

                // Verifica se a reação já existe para este usuário e tipo
                var existingReaction = post.Reactions.FirstOrDefault(r => r.UserId == userId && r.ReactionType == reactionType);

                if (existingReaction != null)
                {
                    // Reação já existe, remove-a
                    post.Reactions.Remove(existingReaction);
                }
                else
                {
                    // Verifica se o usuário já possui outra reação para este post
                    var existingReactionOfAnotherType = post.Reactions.FirstOrDefault(r => r.UserId == userId);

                    if (existingReactionOfAnotherType != null)
                    {
                        // Usuário já possui uma reação diferente, atualiza para a nova
                        existingReactionOfAnotherType.ReactionType = reactionType;
                        existingReactionOfAnotherType.ReactedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Adiciona uma nova reação
                        post.Reactions.Add(new PostReaction
                        {
                            PostId = postId,
                            UserId = userId,
                            ReactionType = reactionType,
                            ReactedAt = DateTime.UtcNow
                        });
                    }
                }

                await _postRepository.UpdateAsync(post);
                await SendReactionNotificationAsync(post, userId, reactionType);
                return post;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reaction.");
                throw; // Re-throw para ser tratado no controlador
            }
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _postRepository.GetAllAsync();
        }

        public async Task SendReactionNotificationAsync(Post post, Guid userId, ReactionType reactionType)
        {
            if (post != null && post.UserId != userId) // Only send if post exists and user is not the post creator
            {
                var user = await _userRepository.GetByIdAsync(userId);
                string reactionName = reactionType.ToString(); // Get the name of the reaction type
                string message = $"{user?.UserName ?? "An unknown user"} reacted to your post with {reactionName}: \"{post.Title}\"";

                var notification = new Notification
                {
                    UserId = post.UserId,
                    Message = message,
                    CreatedAt = DateTime.UtcNow,
                    Type = NotificationType.PostReacted // Use a more general notification type
                };
                await _notificationService.CreateNotificationAsync(notification);
            }
        }

        public async Task<Post> ReportPostAsync(Guid postId, Guid userId, string reason)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return null; // Or throw a more specific exception
            }

            var newReport = new PostReport
            {
                PostId = postId,
                UserId = userId,
                ReportedAt = DateTime.UtcNow,
                Reason = reason
            };
            post.Reports.Add(newReport);
            await _postRepository.UpdateAsync(post); //Save changes to DB.
            return post; // Return the updated post
        }

        public async Task TogglePostActivationAsync(Guid postId, Guid userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                throw new ArgumentException("Post não encontrado.");
            }

            post.IsActive = !post.IsActive; // Alterna o status IsActive
            await _postRepository.UpdateAsync(post);
        }

        public async Task<bool> HasBeenReportedAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            return post?.Reports.Any() ?? false;
        }

        public async Task<List<PostReport>> GetPostReportsAsync(Guid postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            return post?.Reports.ToList() ?? new List<PostReport>();
        }
    }
}
