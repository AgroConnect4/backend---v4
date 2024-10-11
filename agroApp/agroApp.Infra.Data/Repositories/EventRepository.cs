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

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _context.Events.ToListAsync();
        }

        public async Task<Event> GetEventByIdAsync(int id)
        {
            return await _context.Events.FindAsync(id);
        }

        public async Task<Event> AddEventAsync(Event @event)
        {
            _context.Events.Add(@event);
            await _context.SaveChangesAsync();
            return @event;
        }

        public async Task UpdateEventAsync(Event @event)
        {
            // 1. Verificar se o User existe
            var userExists = await _context.Users.AnyAsync(u => u.Id == @event.UserId);

            if (!userExists)
            {
                // 2. Lançar uma exceção se o User não existir
                throw new ArgumentException($"Usuário com ID { @event.UserId } não encontrado."); 
            }

            _context.Events.Update(@event);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEventAsync(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }
        }
    }
}