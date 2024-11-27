using agroApp.Domain.Entities;
using agroApp.Infra.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using agroApp.Infra.Data.Context;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace agroApp.API.Services
{
    public class ConnectionService : IConnectionService
    {
        private readonly IConnectionRepository _connectionRepository;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly AppDbContext _context;
        private readonly INotificationRepository _notificationRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ConnectionService> _logger; // Inject ILogger
        private readonly IConfiguration _configuration; // Inject IConfiguration
        private readonly IHttpContextAccessor _httpContextAccessor; // Inject IHttpContextAccessor

        public ConnectionService(AppDbContext context,
                                INotificationService notificationService,
                                IUserRepository userRepository,
                                INotificationRepository notificationRepository,
                                IConnectionRepository connectionRepository, 
                                UserManager<User> userManager,
                                ILogger<ConnectionService> logger, // Add ILogger to constructor
                                IConfiguration configuration, // Add IConfiguration to constructor
                                IHttpContextAccessor httpContextAccessor) // Add IHttpContextAccessor to constructor
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); 
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _notificationService = notificationService;
            _connectionRepository = connectionRepository ?? throw new ArgumentNullException(nameof(connectionRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger; // Assign logger
            _configuration = configuration; // Assign configuration
            _httpContextAccessor = httpContextAccessor; // Assign httpContextAccessor
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

        public async Task<Connection> CreateConnectionAsync(Guid userId, Guid connectedUserId)
            {
                //Retrieve users. Throw exception if not found.
                var userInitiatingConnection = await _userRepository.GetByIdAsync(userId) ?? throw new ArgumentException($"User with ID {userId} not found.");
                var connectedUser = await _userRepository.GetByIdAsync(connectedUserId) ?? throw new ArgumentException($"User with ID {connectedUserId} not found.");

                //Check for self-connection. Throw exception.
                if (userInitiatingConnection.Id == connectedUser.Id)
                {
                    throw new ArgumentException("You cannot connect with yourself.");
                }

                //Check for existing connection (handle all statuses).
                var existingConnection = await _connectionRepository.GetConnectionAsync(userId, connectedUserId);
                if (existingConnection != null)
                {
                    switch (existingConnection.Status)
                    {
                        case ConnectionStatus.Pending:
                            return existingConnection; // Already pending
                        case ConnectionStatus.Accepted:
                            return existingConnection; // Already accepted
                        case ConnectionStatus.Rejected:
                            await _connectionRepository.DeleteAsync(existingConnection); // Delete old rejected connection before creating new request.
                            break; // Proceed to create new pending connection.
                        default:
                            throw new ArgumentException($"Unexpected connection status: {existingConnection.Status}");
                    }
                }

                var newConnection = new Connection { UserId = userId, ConnectedUserId = connectedUserId, Status = ConnectionStatus.Pending };
                await _connectionRepository.AddAsync(newConnection);
                await _notificationService.SendConnectionRequestNotificationAsync(connectedUserId, userInitiatingConnection);

                return newConnection;
            }

        public async Task DeleteConnectionAsync(Guid userId, Guid connectedUserId)
        {
            try
            {
                // Remove somente a conexão do usuário atual
                var connection = await _connectionRepository.GetConnectionAsync(userId, connectedUserId);
                if (connection != null)
                {
                    await _connectionRepository.DeleteAsync(connection);
                    // Atualiza IsMutual na outra conexão, se existir
                    var reverseConnection = await _connectionRepository.GetConnectionAsync(connectedUserId, userId);
                    if (reverseConnection != null)
                    {
                        reverseConnection.IsMutual = false;
                        await _connectionRepository.UpdateAsync(reverseConnection);
                    }
                }
                else
                {
                    throw new ArgumentException("Conexão não encontrada.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting connection.");
                throw;
            }
        }

        public async Task AcceptConnectionAsync(Guid userId, Guid connectedUserId)
        {
            var connection = await _connectionRepository.GetConnectionAsync(connectedUserId, userId); // Correct order!
            if (connection == null || connection.Status != ConnectionStatus.Pending)
            {
                throw new InvalidOperationException("Invalid connection or status. Cannot accept.");
            }

            connection.Status = ConnectionStatus.Accepted;
            connection.IsMutual = true;
            await _connectionRepository.UpdateAsync(connection);

            // Send notifications (to BOTH users):
            try
            {
                var userWhoSentRequest = await _userRepository.GetByIdAsync(connectedUserId);
                var userWhoAccepted = await _userRepository.GetByIdAsync(userId);

                if (userWhoSentRequest != null)
                {
                    await _notificationService.SendConnectionAcceptedNotificationAsync(connectedUserId, userWhoAccepted, true); // To initiator
                }
                if (userWhoAccepted != null)
                {
                    await _notificationService.SendConnectionAcceptedNotificationAsync(userId, userWhoSentRequest, false); // To acceptor
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting connection.");
                throw new Exception("An error occurred while accepting the connection.", ex);
            }
        }


        public async Task RejectConnectionAsync(Guid userId, Guid connectedUserId)
        {
            // Attempt to find the connection regardless of the status.
            var connection = await _connectionRepository.GetConnectionAsync(connectedUserId, userId);

            if (connection == null)
            {
                _logger.LogWarning($"Connection not found for rejection. User ID: {userId}, Connected User ID: {connectedUserId}");
                return; // Or throw an exception, depending on error handling strategy.
            }

            //If connection already rejected, don't change status.  Log it.
            if (connection.Status == ConnectionStatus.Rejected)
            {
                _logger.LogInformation($"Connection already rejected. User ID: {userId}, Connected User ID: {connectedUserId}");
                return;
            }

            connection.Status = ConnectionStatus.Rejected;
            await _connectionRepository.UpdateAsync(connection);

            try
            {
                var userWhoSentRequest = await _userRepository.GetByIdAsync(connectedUserId);
                var userWhoRejected = await _userRepository.GetByIdAsync(userId); // Get the user who rejected the request

                if (userWhoSentRequest != null)
                {
                    await _notificationService.SendConnectionRejectedNotificationAsync(connectedUserId, userWhoRejected); //Correctly notifies the request sender
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting connection.");
                throw new Exception("An error occurred while rejecting the connection.", ex);
            }
        }

        public async Task<List<Guid>> GetConnectedUserIdsAsync(Guid userId)
        {
            return await _context.Connections
                .Where(c => c.UserId == userId || c.ConnectedUserId == userId)
                .Select(c => c.UserId == userId ? c.ConnectedUserId : c.UserId) // Seleciona o outro usuário da conexão
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<User>> GetConnectionsAsync(Guid userId)
        {
            return await _context.Connections
                .Where(c => c.UserId == userId && c.Status == ConnectionStatus.Accepted) // Only accepted connections
                .Include(c => c.ConnectedUser)
                .Select(c => c.ConnectedUser)
                .ToListAsync();
        }

        public async Task<List<User>> GetConnectedToAsync(Guid userId)
        {
            var connectedUserIds = await _context.Connections
                .Where(c => c.ConnectedUserId == userId)
                .Select(c => c.UserId)
                .Distinct()
                .ToListAsync();

            // Carrega os usuários correspondentes aos IDs obtidos.
            return await _context.Users
                .Where(u => connectedUserIds.Contains(u.Id))
                .ToListAsync();
        }
    }
}