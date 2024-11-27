using agroApp.Domain.Entities;
using agroApp.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly AppDbContext _context;

        public EventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Event> GetByIdAsync(Guid eventId)
        {
            return await _context.Events.FindAsync(eventId);
        }

        public async Task<List<Event>> GetAllAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<Event> AddAsync(Event eventItem)
        {
            _context.Events.Add(eventItem);
            await _context.SaveChangesAsync();
            return eventItem;
        }

        public async Task<Event> UpdateAsync(Event eventItem)
        {
            _context.Entry(eventItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return eventItem;
        }

        public async Task DeleteAsync(Guid eventId)
        {
            var eventItem = await _context.Events.FindAsync(eventId);
            if (eventItem != null)
            {
                _context.Events.Remove(eventItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<EventComment>> GetCommentsByEventIdAsync(Guid eventId)
        {
            return await _context.EventComments.Where(c => c.EventId == eventId).ToListAsync();
        }

        public async Task<List<EventShare>> GetSharesByEventIdAsync(Guid eventId)
        {
            return await _context.EventShares.Where(s => s.EventId == eventId).ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(Guid eventId)
        {
            return await _context.Events.FindAsync(eventId); 
        }

        public async Task<Event> AddEventAsync(Event @event)
        {
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task<Event> UpdateEventAsync(Event @event)
        {
            _context.Entry(@event).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task DeleteEventAsync(Guid eventId)
        {
            var @event = await _context.Events.FindAsync(eventId);
            if (@event != null)
            {
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task AddParticipationAsync(EventParticipant participation)
        {
            _context.EventParticipants.Add(participation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteParticipationAsync(Guid participationId)
        {
            var participation = await _context.EventParticipants.FindAsync(participationId);
            if (participation != null)
            {
                _context.EventParticipants.Remove(participation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<EventParticipant> GetParticipationAsync(Guid eventId, Guid userId)
        {
            return await _context.EventParticipants.FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId);
        }

        public async Task<List<Event>> GetEventsByUserIdAsync(Guid userId)
        {
            return await _context.Events
                .Include(e => e.Participants) // Inclui os participantes
                .Where(e => e.Participants.Any(p => p.UserId == userId))
                .ToListAsync();
        }

        public async Task<List<Event>> GetEventsCreatedByUserIdAsync(Guid userId)
        {
            return await _context.Events.Where(e => e.UserId == userId).ToListAsync();
        }
    }
}