using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using agroApp.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration; // Add this using statement
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text; // Add this using statement


namespace agroApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private readonly IConnectionService _connectionService;
        private readonly ILogger<ConnectionController> _logger;
        private readonly IConfiguration _configuration; // Inject IConfiguration
        private readonly IHttpContextAccessor _httpContextAccessor; // Inject IHttpContextAccessor

        public ConnectionController(IConnectionService connectionService, 
                                     ILogger<ConnectionController> logger,
                                     IConfiguration configuration, // Add IConfiguration to constructor
                                     IHttpContextAccessor httpContextAccessor) //Add IHttpContextAccessor to constructor
        {
            _connectionService = connectionService;
            _logger = logger;
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

        [HttpPost("create/connection")]
        public async Task<IActionResult> CreateConnection([FromBody] CreateConnectionDto createConnectionDto)
        {
            try
            {
                Guid userId = GetUserIdFromToken();
                if (userId == Guid.Empty) 
                {
                    return Unauthorized("Invalid or missing token.");
                }

                if (createConnectionDto.ConnectedUserId == userId)
                {
                    return BadRequest("Você não pode se conectar consigo mesmo.");
                }

                await _connectionService.CreateConnectionAsync(userId, createConnectionDto.ConnectedUserId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating connection: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating connection.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpDelete("delete/connection")]
        public async Task<IActionResult> DeleteConnection([FromBody] DeleteConnectionDto deleteConnectionDto)
        {
            try
            {
                Guid userId = GetUserIdFromToken();
                await _connectionService.DeleteConnectionAsync(userId, deleteConnectionDto.ConnectedUserId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error deleting connection: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex) // Catch UnauthorizedAccessException
            {
                _logger.LogError(ex, "Unauthorized access: {Message}", ex.Message);
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting connection.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("MeConectei")]
        public async Task<IActionResult> GetConnections()
        {
            Guid userId = GetUserIdFromToken();
            if (userId == Guid.Empty) 
            {
                return Unauthorized("Invalid or missing token.");
            }
            try
            {
                var connections = await _connectionService.GetConnectionsAsync(userId);
                return Ok(connections);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connections.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet("ConectaramComigo")]
        public async Task<IActionResult> GetConnectedTo()
        {
            Guid userId = GetUserIdFromToken();
            if (userId == Guid.Empty) 
            {
                return Unauthorized("Invalid or missing token.");
            }
            
            try
            {
                var connectionsTo = await _connectionService.GetConnectedToAsync(userId);
                return Ok(connectionsTo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting connected to.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost("connection/accept")]
        public async Task<IActionResult> AcceptConnection(Guid connectedUserId)
        {
            Guid userId = GetUserIdFromToken(); // Your method to get the current user's ID from the token
            try
            {
                await _connectionService.AcceptConnectionAsync(userId, connectedUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting connection.");
                return StatusCode(500, "Error accepting connection"); //Better error handling
            }
        }

        [HttpPost("connection/reject")]
        public async Task<IActionResult> RejectConnection(Guid connectedUserId)
        {
            Guid userId = GetUserIdFromToken(); // Your method to get the current user's ID from the token
            try
            {
                await _connectionService.RejectConnectionAsync(userId, connectedUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting connection.");
                return StatusCode(500, "Error accepting connection"); //Better error handling
            }
        }
    }
}