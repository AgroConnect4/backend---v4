using agroApp.API.DTOs; // Se você precisar de um DTO
using agroApp.API.Services;
using agroApp.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations;

namespace agroApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public NotificationController(
            INotificationService notificationService,
            ILogger<NotificationController> logger,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration)
        {
            _logger = logger;
            _notificationService = notificationService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetNotificationsByUserId([Required] Guid userId)
        {
            try
            {
                var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
                if (notifications == null || notifications.Count == 0)
                {
                    return NotFound(new { message = "No notifications found for this user." }); // Return JSON object
                }
                return Ok(notifications);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = $"Invalid userId: {ex.Message}" }); //Return JSON object
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting notifications for userId {userId}");
                // Always return a structured JSON response, even on server errors
                return StatusCode(500, new { message = "An unexpected error occurred." }); 
            }
        }

        [HttpGet("notification")] // Endpoint para obter todas as notificações
        public async Task<IActionResult> GetNotifications()
        {
            // Obter ID do usuário logado.  Use seu mecanismo de autenticação (JWT, etc.)
            Guid userId = GetUserIdFromToken();

            try
            {
                var notifications = await _notificationService.GetNotificationsByUserIdAsync(userId);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar notificações: {ex.Message}");
            }
        }

        [HttpGet("connection/notification")]
        public async Task<IActionResult> GetLatestConnectionNotification()
        {
            Guid userId = GetUserIdFromToken();

            try
            {
                var notification = await _notificationService.GetLatestConnectionNotificationAsync(userId);
                return Ok(notification); // or return NotFound() if null.
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting notification: {ex.Message}");
            }
        }
    }
}