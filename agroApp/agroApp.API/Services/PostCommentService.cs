using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using Microsoft.AspNetCore.Http; 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens; 
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging;

namespace agroApp.API.Services
{
    public class PostCommentService : IPostCommentService
    {
        private readonly IPostCommentRepository _postCommentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<User> _userManager;

        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PostCommentService> _logger; 
        private readonly JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
        private readonly IPostRepository _postRepository;
        private readonly INotificationService _notificationService;

        public PostCommentService(IPostCommentRepository postCommentRepository, 
            IHttpContextAccessor httpContextAccessor, 
            UserManager<User> userManager,
            IUserRepository userRepository,
            IConfiguration configuration,
            ILogger<PostCommentService> logger,
            INotificationService notificationService,
            IPostRepository postRepository)
        {
            _postCommentRepository = postCommentRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
            _postRepository = postRepository;
        }

        private Guid GetUserIdFromToken()
        {
            if (_httpContextAccessor == null || _httpContextAccessor.HttpContext == null)
            {
                _logger.LogError("HttpContextAccessor is null or HttpContext is null.");
                throw new InvalidOperationException("Cannot access HTTP context.");
            }

            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
            if (!authorizationHeader.Any())
            {
                _logger.LogError("Authorization header not found.");
                throw new UnauthorizedAccessException("Authorization header is missing.");
            }

            var token = authorizationHeader.FirstOrDefault().Split(" ").LastOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("JWT token not found in Authorization header.");
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
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier); // Usar NameIdentifier
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId)) 
                {
                    _logger.LogError("Claim de ID de usuário inválida ou ausente no token JWT.");
                    throw new UnauthorizedAccessException("ID de usuário inválida no token.");
                }

                return userId; // Agora retorna Guid
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Erro ao validar o token JWT.");
                throw new UnauthorizedAccessException("Token JWT inválido.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao recuperar o ID do usuário do token JWT.");
                throw;
            }
        }

        public async Task<PostComment> CreateCommentAsync(Guid postId,CreatePostCommentDto request, Guid userId)
        {
            var post = await _postRepository.GetByIdAsync(postId); 
            if (post == null)
            {
                throw new ArgumentException($"Post with ID {postId} not found.");
            }

            var comment = new PostComment
            {
                PostId = postId,
                //PostId = request.PostId,
                UserId = userId,
                Content = request.Content,
                CreatedAt = DateTime.UtcNow
            };

            await _postCommentRepository.AddAsync(comment);
            await SendCommentNotificationAsync(post, comment);
            return comment; // Return the created comment
        }

        public async Task SendCommentNotificationAsync(Post post, PostComment comment)
        {
            var user = await _userRepository.GetByIdAsync(post.UserId);
            if (user != null && user.Id != comment.UserId) // Avoid notifying the commenter themself
            {
                var commenter = await _userRepository.GetByIdAsync(comment.UserId);
                // Call SendPostCommentedNotificationAsync with the correct parameters
                await _notificationService.SendPostCommentedNotificationAsync(post.UserId, post, comment.UserId, comment.Id); 
            }
        }

        public async Task<PostComment> UpdateCommentAsync(Guid commentId, UpdatePostCommentDto request, Guid userId)
        {
            var comment = await _postCommentRepository.GetByIdAsync(commentId);
            if(comment == null)
            {
                return null; //Or throw exception
            }
            if(comment.UserId != userId) //Authorization Check
            {
                return null; //Or throw exception
            }
            comment.Content = request.Content;
            comment.UpdatedAt = DateTime.UtcNow;
            await _postCommentRepository.UpdateAsync(comment);
            return comment;
        }

        public async Task DeleteCommentAsync(Guid commentId, Guid userId)
        {
            var comment = await _postCommentRepository.GetByIdAsync(commentId);
            if(comment == null)
            {
                return; //Or throw exception
            }
            if(comment.UserId != userId) //Authorization Check
            {
                return; //Or throw exception
            }
            await _postCommentRepository.DeleteAsync(commentId);
        }
        
        public async Task<List<PostComment>> GetCommentsByPostIdAsync(Guid postId)
        {
            return await _postCommentRepository.GetCommentsByPostIdAsync(postId);
        }

        public async Task<List<PostComment>> GetAllCommentsByPostIdAsync(Guid postId)
        {
            return await _postCommentRepository.GetAllByPostIdAsync(postId); // This assumes your repository has this method
        }

        

        

        public async Task<PostComment> GetCommentByIdAsync(Guid commentId)
        {
            return await _postCommentRepository.GetByIdAsync(commentId);
        }
    }
}