using agroApp.API.DTOs;
using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using agroApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;

namespace agroApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Autenticação obrigatória para todos os métodos
    public class PostCommentsController : ControllerBase
    {
        private readonly IPostCommentService _postCommentService;
        private readonly UserManager<User> _userManager; // Adicione UserManagerz
        private readonly ILogger<PostCommentsController> _logger;
        private readonly IPostService _postService;

        private readonly IHttpContextAccessor _httpContextAccessor; // Inject IHttpContextAccessor
        private readonly IConfiguration _configuration;

        public PostCommentsController(
            IPostCommentService postCommentService,
            UserManager<User> userManager, 
            ILogger<PostCommentsController> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration, 
            IPostService postService) 
        {
            _postCommentService = postCommentService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
            _postService = postService;
        }

        private Guid GetUserIdFromToken()
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token JWT não encontrado no cabeçalho de autorização.");
                throw new UnauthorizedAccessException("Token JWT ausente.");
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
                    ClockSkew = TimeSpan.Zero
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
    }
}