using agroApp.Domain.Entities;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Repositories
{
    public interface IConnectionRepository
    {
        Task<Connection> GetConnectionAsync(Guid userId, Guid connectedUserId);
        Task<Connection> AddAsync(Connection connection);
        Task<List<User>> GetPendingConnectionsAsync(Guid userId);
        Task DeleteAsync(Connection connection);
        Task<Connection> UpdateAsync(Connection connection);
        Task<List<User>> GetConnectionsAsync(Guid userId);
        //sao diferentes olhe antes de apagar
        Task<List<User>> GetConnectedToAsync(Guid userId);
        Task<Connection> GetByIdAsync(Guid userId, Guid connectedUserId);
        Task<Connection> GetConnectedUserIdAsync(Guid connectionId, Guid userId);
        //Task<List<int>> GetConnectedUserIdsAsync(Guid userId);
        
        //Task<List<int>> GetConnectedUserIdsAsync(Guid userId);
    }
}