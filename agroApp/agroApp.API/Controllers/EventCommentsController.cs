using agroApp.API.DTOs;
using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using agroApp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class EventCommentsController : ControllerBase
    {
        private readonly IEventCommentService _commentService;
        private readonly UserManager<User> _userManager; // Adicione UserManager
        private readonly IHttpContextAccessor _httpContextAccessor; // Inject IHttpContextAccessor
        private readonly IConfiguration _configuration; // Inject IConfiguration
        private readonly ILogger<EventCommentsController> _logger;

        public EventCommentsController(
            IEventCommentService commentService, 
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<EventCommentsController> logger)
        {
            _commentService = commentService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
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

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
        {
            _logger.LogError("User ID claim not found or invalid format in JWT token. Value: {userIdClaimValue}", userIdClaim?.Value ?? "null");
            throw new UnauthorizedAccessException("Invalid User ID in token.");
        }

        // CORRECTED: Parse as Guid
        if (Guid.TryParse(userIdClaim.Value, out Guid userId))
        {
            return userId;
        }
        else
        {
            _logger.LogError("Invalid User ID format in JWT token. Value: {userIdClaimValue}", userIdClaim?.Value ?? "null");
            throw new UnauthorizedAccessException("Invalid User ID format in JWT token.");
        }
    }
    catch (SecurityTokenException ex)
    {
        _logger.LogError(ex, "Invalid JWT token.");
        throw new UnauthorizedAccessException("Invalid JWT token.", ex);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error validating JWT token.");
        throw;
    }
}

    }
}