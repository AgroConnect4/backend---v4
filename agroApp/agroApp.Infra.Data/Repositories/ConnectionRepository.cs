using agroApp.Domain.Entities;
using agroApp.Infra.Data.Context; // Assuma que você tem um contexto de banco de dados
using Microsoft.EntityFrameworkCore;
using System;

using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public class ConnectionRepository : IConnectionRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ConnectionRepository> _logger; 

        public ConnectionRepository(AppDbContext context, ILogger<ConnectionRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        
        public async Task<Connection> GetConnectionAsync(Guid userId, Guid connectedUserId)
        {
            return await _context.Connections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ConnectedUserId == connectedUserId);
        }
        public async Task<Connection> AddAsync(Connection connection)
        {
            _context.Connections.Add(connection);
            await _context.SaveChangesAsync();
            return connection;
        }

        public async Task<Connection> UpdateAsync(Connection connection)
        {
            _context.Connections.Update(connection);
            await _context.SaveChangesAsync();
            return connection;
        }

        public async Task DeleteAsync(Connection connection)
        {
            _context.Connections.Remove(connection);
            await _context.SaveChangesAsync();
        }

        public async Task<Connection> GetByIdAsync(Guid userId, Guid connectedUserId)
        {
            return await _context.Connections
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ConnectedUserId == connectedUserId);
        }

        public async Task<List<User>> GetConnectionsAsync(Guid userId)
        {
             return await _context.Connections
                .Where(c => c.UserId == userId && c.Status == ConnectionStatus.Accepted) // Only accepted connections
                .Include(c => c.ConnectedUser)
                .Select(c => c.ConnectedUser)
                .ToListAsync();
        }

        public async Task<Connection> GetConnectedUserIdAsync(Guid connectionId, Guid userId) // Added userId parameter
        {
            return await _context.Connections
                .FindAsync(new object[] { userId, connectionId });
        }

        public async Task<List<User>> GetPendingConnectionsAsync(Guid userId)
        {
            return await _context.Connections
                .Where(c => c.ConnectedUserId == userId && c.Status == ConnectionStatus.Pending) // Only pending connections for userId
                .Include(c => c.User)
                .Select(c => c.User)
                .ToListAsync();
        }

        public async Task<List<User>> GetConnectedToAsync(Guid userId)
        {
             return await _context.Connections
                .Where(c => c.ConnectedUserId == userId && c.Status == ConnectionStatus.Accepted) // Only accepted connections
                .Include(c => c.User)
                .Select(c => c.User)
                .ToListAsync();
        }
    }
}