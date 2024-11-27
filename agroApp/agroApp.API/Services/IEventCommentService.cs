using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public interface IEventCommentService
    {
        Task<Guid> CreateCommentAsync(Guid eventId, CreateEventCommentDto request, Guid userId);
        Task<EventComment> UpdateCommentAsync(Guid commentId, UpdateEventCommentDto request, Guid userId);
        Task DeleteCommentAsync(Guid commentId, Guid userId);
        Task<List<EventComment>> GetCommentsByEventIdAsync(Guid eventId);
        Task<EventComment> GetCommentByIdAsync(Guid commentId);
        Task<List<EventComment>> GetAllCommentsByEventIdAsync(Guid eventId);
    }
}