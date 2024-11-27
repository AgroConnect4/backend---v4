using agroApp.API.DTOs;
using System.Threading.Tasks;
using agroApp.Domain.Entities;
using System;
using System.Collections.Generic;

namespace agroApp.API.Services
{
    public interface IEventsService
    {
        Task<EventDto> CreateEventAsync(CreateEventDto createEventDto);
        Task UpdateEventAsync(Guid id, UpdateEventDto updateEventDto, Guid userId);
        Task DeleteEventAsync(Guid id, Guid userId);
        Task<EventDto> GetEventByIdAsync(Guid id);
        Task<List<EventDto>> GetAllEventsAsync();
        Task ToggleEventParticipationAsync(Guid eventId, Guid userId);
        Task<List<Event>> GetEventsByUserIdAsync(Guid userId);
        //Task SendEventNotifications(Event @event);
        Task<List<Event>> GetCreatedEventsByUserIdAsync(Guid userId);
    }
}