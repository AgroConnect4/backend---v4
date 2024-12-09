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
using Microsoft.Extensions.Logging; // Adicione o namespace para ILogger

namespace agroApp.API.Services
{
    public class EventsService : IEventsService
    {
        private readonly IEventRepository _eventRepository;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<EventsService> _logger; // Adicione o Logger
        private readonly IConfiguration _configuration; // Adicione o IConfiguration

        public EventsService(IEventRepository eventRepository, 
                             IUserRepository userRepository,
                             UserManager<User> userManager, 
                             IHttpContextAccessor httpContextAccessor, // Injeta HttpContextAccessor
                             ILogger<EventsService> logger, // Injeta o Logger
                             IConfiguration configuration) // Injeta o IConfiguration
        {
            _eventRepository = eventRepository;
            _userRepository = userRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _configuration = configuration;
        }

        private int GetUserIdFromToken()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            // Logar o token para análise (opcional):
            _logger.LogInformation("Token recebido: {Token}", token);

            try
            {
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
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

        public async Task<EventDto> GetEventByIdAsync(int id)
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

        public async Task<EventDto> CreateEventAsync(CreateEventDto createEventDto)
        {
            // Obter o ID do usuário do token JWT
            var userId = GetUserIdFromToken();

            var @event = new Event
            {
                Name = createEventDto.Name,
                ImageUrl = createEventDto.ImageUrl,
                UserId = userId, // Use o ID do usuário do token
                StartDateTime = createEventDto.StartDateTime,
                EndDateTime = createEventDto.EndDateTime,
                Location = createEventDto.Location,
                Description = createEventDto.Description,
                ProductImages = createEventDto.ProductImages
            };

            var createdEvent = await _eventRepository.AddEventAsync(@event);

            return new EventDto
            {
                Id = createdEvent.Id,
                Name = createdEvent.Name,
                ImageUrl = createdEvent.ImageUrl,
                // Obtemos o organizador do evento:
                OrganizerId = createdEvent.UserId,
                StartDateTime = createdEvent.StartDateTime,
                EndDateTime = createdEvent.EndDateTime,
                Location = createdEvent.Location,
                Description = createdEvent.Description,
                ProductImages = createdEvent.ProductImages
            };
        }

        public async Task UpdateEventAsync(UpdateEventDto updateEventDto, int id)
        {
            // Obtenha o UserId do token JWT
            var userId = GetUserIdFromToken();

            var existingEvent = await _eventRepository.GetEventByIdAsync(id);

            if (existingEvent == null)
            {
                return; // Ou lance uma exceção
            }

            existingEvent.Name = updateEventDto.Name;
            existingEvent.ImageUrl = updateEventDto.ImageUrl;
            existingEvent.StartDateTime = updateEventDto.StartDateTime;
            existingEvent.EndDateTime = updateEventDto.EndDateTime;
            existingEvent.Location = updateEventDto.Location;
            existingEvent.Description = updateEventDto.Description;
            existingEvent.ProductImages = updateEventDto.ProductImages;

            // Atualize o UserId do evento com o ID do usuário autenticado
            existingEvent.UserId = userId; // Use o ID do usuário do token

            await _eventRepository.UpdateEventAsync(existingEvent);
        }

        public async Task DeleteEventAsync(int id)
        {
            await _eventRepository.DeleteEventAsync(id);
        }
    }
}