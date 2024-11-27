using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using agroApp.API.Services;
using agroApp.Infra.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Adicione o namespace para HttpContextAccessor
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text; // Adicione o namespace para Encoding
using Microsoft.Extensions.Logging; 
using Microsoft.Extensions.Configuration;

namespace agroApp.API.Services
{
    public class EventsService : IEventsService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IConnectionService _connectionService;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EventsService> _logger; // Adicione o Logger
        private readonly IConfiguration _configuration; // Adicione o IConfiguration
        private readonly INotificationService _notificationService;
        //private readonly ICommentService _commentService;
        //private readonly IShareService _shareService;
        private readonly IEventCommentService _eventCommentService; // Injeção de dependência
        private readonly IEventShareService _eventShareService; 
        private readonly IEventCommentRepository _eventCommentRepository; // Add this
        private readonly IEventShareRepository _eventShareRepository;

        public EventsService(IEventRepository eventRepository,
                            IConnectionService connectionService, 
                            IUserRepository userRepository,
                            UserManager<User> userManager, 
                            IHttpContextAccessor httpContextAccessor, // Injeta HttpContextAccessor
                            ILogger<EventsService> logger, // Injeta o Logger
                            IConfiguration configuration,
                            INotificationService notificationService,
                            INotificationRepository notificationRepository,
                            //ICommentService commentService, 
                            //IShareService shareService,
                            IEventCommentService eventCommentService, 
                            IEventShareService eventShareService,
                            IEventCommentRepository eventCommentRepository,
                            IEventShareRepository eventShareRepository)
        {
            _eventRepository = eventRepository;
            _connectionService = connectionService;
            _userRepository = userRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
            _notificationService = notificationService;
            _notificationRepository = notificationRepository;
            //_commentService = commentService;
            //_shareService = shareService;
            _eventCommentService = eventCommentService; 
            _eventShareService = eventShareService;
            _eventCommentRepository = eventCommentRepository;
            _eventShareRepository = eventShareRepository;
        }
        private async Task<Guid> GetUserIdFromToken()
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

        public async Task<EventDto> CreateEventAsync(CreateEventDto createEventDto)
        {
            var userId = await GetCurrentUserIdAsync(); //Corrected

            var @event = new Event
            {
                Name = createEventDto.Name,
                ImageUrl = createEventDto.ImageUrl,
                UserId = userId,
                StartDateTime = createEventDto.StartDateTime,
                EndDateTime = createEventDto.EndDateTime,
                Location = createEventDto.Location,
                Description = createEventDto.Description,
                ProductImages = createEventDto.ProductImages
            };

            var createdEvent = await _eventRepository.AddEventAsync(@event);
            //await SendEventNotifications(createdEvent);

            var acceptedConnections = await _connectionService.GetConnectionsAsync(userId); 
            foreach (var connectedUser in acceptedConnections)
            {
                await _notificationService.SendEventCreatedNotificationAsync(connectedUser.Id, createdEvent);
            }
            return MapEventToDto(createdEvent);
        }
        public async Task UpdateEventAsync(Guid id, UpdateEventDto updateEventDto, Guid userId) //Correct Signature
        {
            //var userId = await GetCurrentUserIdAsync(); //Corrected
            var existingEvent = await _eventRepository.GetEventByIdAsync(id);

            if (existingEvent == null || existingEvent.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this event.");
            }

            existingEvent.Name = updateEventDto.Name ?? existingEvent.Name;
            existingEvent.ImageUrl = updateEventDto.ImageUrl ?? existingEvent.ImageUrl;
            existingEvent.StartDateTime = updateEventDto.StartDateTime;
            existingEvent.EndDateTime = updateEventDto.EndDateTime;
            existingEvent.Location = updateEventDto.Location ?? existingEvent.Location;
            existingEvent.Description = updateEventDto.Description ?? existingEvent.Description;
            existingEvent.ProductImages = updateEventDto.ProductImages ?? existingEvent.ProductImages;

            await _eventRepository.UpdateEventAsync(existingEvent);
        }

        public async Task DeleteEventAsync(Guid id, Guid userId)
        {
            //var userId = await GetCurrentUserIdAsync(); //Corrected
            var existingEvent = await _eventRepository.GetEventByIdAsync(id);
            if (existingEvent == null || existingEvent.UserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this event.");
            }

            await _eventRepository.DeleteEventAsync(id);
        }

        private EventDto MapEventToDto(Event @event)
        {
            return new EventDto
            {
                Id = @event.Id,
                Name = @event.Name,
                ImageUrl = @event.ImageUrl,
                OrganizerId = @event.UserId,
                StartDateTime = @event.StartDateTime,
                EndDateTime = @event.EndDateTime,
                Location = @event.Location,
                Description = @event.Description,
                ProductImages = @event.ProductImages
            };
        }

        public async Task<EventDto> GetEventByIdAsync(Guid id)
        {
            var @event = await _eventRepository.GetEventByIdAsync(id);
            if (@event == null)
            {
                return null;
            }

            return new EventDto
            {
                Id = @event.Id,
                Name = @event.Name,
                ImageUrl = @event.ImageUrl,
                // Obtemos o organizador do evento:
                OrganizerId = @event.UserId, 
                EndDateTime = @event.EndDateTime,
                Location = @event.Location,
                Description = @event.Description,
                ProductImages = @event.ProductImages
            };
        }

        public async Task<List<EventDto>> GetAllEventsAsync()
        {
            var events = await _eventRepository.GetAllEventsAsync();

            // Usando Task.WhenAll para aguardar todos os eventos:
            var eventDtos = await Task.WhenAll(events.Select(async e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                ImageUrl = e.ImageUrl,
                // Obtemos o organizador do evento:
                OrganizerId = e.UserId,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                Location = e.Location,
                Description = e.Description,
                ProductImages = e.ProductImages
            }));

            return eventDtos.ToList();
        }

        public async Task ToggleEventParticipationAsync(Guid eventId, Guid userId)
        {
            var existingParticipation = await _eventRepository.GetParticipationAsync(eventId, userId);

           if (existingParticipation == null)
            {
                var @event = await _eventRepository.GetEventByIdAsync(eventId);
                if (@event != null)
                {
                    var creator = await _userRepository.GetByIdAsync(@event.UserId);
                    if (creator != null && creator.Id != userId)
                        await _notificationService.SendEventJoinedNotificationAsync(creator.Id, @event, userId);
                }
            }
        }

        public async Task HandleCommentNotificationAsync(Guid eventId, Guid userId, Guid commentId)
        {
            var @event = await _eventRepository.GetEventByIdAsync(eventId);
            if (@event != null)
            {
                var creator = await _userRepository.GetByIdAsync(@event.UserId);
                if (creator != null && creator.Id != userId)
                {
                    await _notificationService.SendEventCommentedNotificationAsync(creator.Id, @event, userId, commentId);
                }
            }
        }

        public async Task<List<Event>> GetEventsByUserIdAsync(Guid userId)
        {
             return await _eventRepository.GetEventsByUserIdAsync(userId);
        }

        public async Task<List<Event>> GetCreatedEventsByUserIdAsync(Guid userId)
        {
            return await _eventRepository.GetEventsCreatedByUserIdAsync(userId);
        }

        private async Task<Guid> GetCurrentUserIdAsync()
        {
            return await GetUserIdFromToken();
        }
        
    }
}