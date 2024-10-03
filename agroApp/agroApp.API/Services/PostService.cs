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


namespace agroApp.Application.Services
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IPostRepository _postRepository;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostService(IPostRepository postRepository,IConfiguration configuration, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor, ILogger<PostService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

private int GetUserIdFromToken()
{
    var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

    // **Logar o token para análise:**
    _logger.LogInformation("Token recebido: {Token}", token);

    try
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey   
 = true,
        ValidIssuer = _configuration["Jwt:Issuer"],
        ValidAudience = _configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, parameters, out SecurityToken validatedToken);

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new SecurityTokenException("ID do usuário não encontrado no token.");
        }

        return int.Parse(userIdClaim.Value);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao validar o token JWT");
        throw; // Ou trate a exceção de acordo com sua aplicação
    }
}

        public async Task<int> CreatePostAsync(CreatePostDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Os dados de criação de post não podem ser nulos.");
            }

            // Obter o usuário logado
            var userId = GetUserIdFromToken();

            // Validar se o usuário existe
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Usuário não encontrado.");
            }

            // Criar o post
            var post = new Post
            {
                UserId = userId,
                Content = request.Content,
                PostType = request.PostType,
                CreatedAt = DateTime.UtcNow
            };

            // Salvar o post
            await _postRepository.AddAsync(post);

            return post.Id;
        }

        public async Task<Post> GetPostByIdAsync(int postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("Post não encontrado.");
            }
            return post;
        }

        public async Task<List<Post>> GetAllPostsByUserIdAsync(int userId)
        {
            return await _postRepository.GetAllByUserIdAsync(userId);
        }

        public async Task<List<Post>> GetAllPostsByPostTypeAsync(string postType)
        {
            return await _postRepository.GetAllByPostTypeAsync(postType);
        }

        public async Task<Post> UpdatePostAsync(int postId, UpdatePostDto request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Os dados de atualização não podem ser nulos.");
            }

            // Obter o post
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("Post não encontrado.");
            }

            // Atualizar o post
            post.Content = request.Content;
            post.PostType = request.PostType;
            post.CreatedAt = DateTime.UtcNow;

            // Salvar as alterações
            return await _postRepository.UpdateAsync(post);
        }

        public async Task DeletePostAsync(int postId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                throw new KeyNotFoundException("Post não encontrado.");
            }
            await _postRepository.DeleteAsync(postId);
        }
    }
}
