using agroApp.API.DTOs;
using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using agroApp.Domain.Entities;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using agroApp.Infra.Data.Repositories;
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
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventsService _eventsService;
        private readonly IEventCommentService _eventCommentService; // Inject EventCommentService
        private readonly ILogger<EventsController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public EventsController(
            ILogger<EventsController> logger,
            IEventsService eventsService,
            IHttpContextAccessor httpContextAccessor,
            IEventCommentService eventCommentService,
            IConfiguration configuration) 
        {
            _logger = logger;
            _eventsService = eventsService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _eventCommentService = eventCommentService;
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

        [HttpPost("event")]
        public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createEventDto)
        {
            try
            {
                var eventDto = await _eventsService.CreateEventAsync(createEventDto);
                return Ok(eventDto); // Corrected: Return eventDto
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event.");
                return StatusCode(500, "An unexpected error occurred while creating the event.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventDto updateEventDto)
        {
            try
            {
                Guid userId = GetUserIdFromToken(); // Get the user ID
                await _eventsService.UpdateEventAsync(id, updateEventDto, userId); // Pass userId to UpdateEventAsync
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access to update event.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event.");
                return StatusCode(500, "An unexpected error occurred while updating the event.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            try
            {
                Guid userId = GetUserIdFromToken(); // Get the user ID
                await _eventsService.DeleteEventAsync(id, userId); // Pass userId to DeleteEventAsync
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access to delete event.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting event.");
                return StatusCode(500, "An unexpected error occurred while deleting the event.");
            }
        }

        [HttpPost("{eventId}/comments")]
        public async Task<IActionResult> CreateComment(Guid eventId, [FromBody] CreateEventCommentDto createCommentDto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var commentId = await _eventCommentService.CreateCommentAsync(eventId, createCommentDto, userId);
                return Ok(commentId); // Return the ID of the newly created comment.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment.");
                return StatusCode(500, "An unexpected error occurred while creating the comment.");
            }
}

        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateEventCommentDto request)
        {
            try
            {
                var userId = GetUserIdFromToken(); //GetUserIdFromToken is in EventsService.
                var updatedComment = await _eventCommentService.UpdateCommentAsync(commentId, request, userId);
                if (updatedComment == null)
                {
                    return NotFound(); // Or Unauthorized(), depending on your error handling
                }
                return Ok(updatedComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment.");
                return StatusCode(500, "An unexpected error occurred while updating the comment.");
            }
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            try
            {
                var userId = GetUserIdFromToken(); //GetUserIdFromToken is in EventsService.
                await _eventCommentService.DeleteCommentAsync(commentId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment.");
                return StatusCode(500, "An unexpected error occurred while deleting the comment.");
            }
        }

        [HttpGet("comments/{id}")]
        public async Task<IActionResult> GetCommentById(Guid id)
        {
            try
            {
                var comment = await _eventCommentService.GetCommentByIdAsync(id);
                if (comment == null) return NotFound();
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comment by ID {id}", id);
                return StatusCode(500, "An unexpected error occurred while retrieving the comment.");
            }
        }

        [HttpGet("{eventId}/comments")]
        public async Task<IActionResult> GetAllCommentsByEventId(Guid eventId)
        {
            try
            {
                var comments = await _eventCommentService.GetAllCommentsByEventIdAsync(eventId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for event ID {eventId}", eventId);
                return StatusCode(500, "An unexpected error occurred while retrieving comments.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<EventDto>>> GetAllEvents()
        {
            var events = await _eventsService.GetAllEventsAsync();
            return Ok(events);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEventById(Guid id)
        {
            var eventDto = await _eventsService.GetEventByIdAsync(id);
            if (eventDto == null)
            {
                return NotFound();
            }
            return Ok(eventDto);
        }

        [HttpPost("{eventId}/participate/event")] // Endpoint único
        public async Task<IActionResult> ToggleEventParticipation(Guid eventId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _eventsService.ToggleEventParticipationAsync(eventId, userId);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar participação no evento.");
                return StatusCode(500, "Erro inesperado ao alterar participação no evento.");
            }
        }

        [HttpGet("myevents/participation")]
        public async Task<ActionResult<List<Event>>> GetMyEvents()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var events = await _eventsService.GetEventsByUserIdAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter eventos do usuário.");
                return StatusCode(500, "Erro inesperado ao obter eventos do usuário.");
            }
        }

        [HttpGet("{userId}/anyusers/participation/events")]
        public async Task<ActionResult<List<Event>>> GetEventsByUserId(Guid userId)
        {
            try
            {
                var events = await _eventsService.GetEventsByUserIdAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events for user ID {userId}", userId);
                return StatusCode(500, "An unexpected error occurred while retrieving events.");
            }
        }

        [HttpGet("myevents/created")] // Endpoint for events created by the current user
        public async Task<ActionResult<List<Event>>> GetMyCreatedEvents()
        {
            try
            {
                var userId = GetUserIdFromToken();
                var events = await _eventsService.GetCreatedEventsByUserIdAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter eventos criados pelo usuário.");
                return StatusCode(500, "Erro inesperado ao obter eventos criados pelo usuário.");
            }
        }

        [HttpGet("{userId}/events/created")] // Endpoint for events created by a specific user
        public async Task<ActionResult<List<Event>>> GetEventsCreatedByUserId(Guid userId)
        {
            try
            {
                var events = await _eventsService.GetCreatedEventsByUserIdAsync(userId);
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting events created by user ID {userId}", userId);
                return StatusCode(500, "An unexpected error occurred while retrieving created events.");
            }
        }
    } 
}