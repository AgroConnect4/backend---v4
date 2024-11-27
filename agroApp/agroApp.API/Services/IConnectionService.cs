using agroApp.Domain.Entities;
using System.Threading.Tasks;

namespace agroApp.API.Services
{
    public interface IConnectionService
    {
        Task<Connection> CreateConnectionAsync(Guid userId, Guid connectedUserId);
        Task DeleteConnectionAsync(Guid userId, Guid connectedUserId); 
        Task<List<Guid>> GetConnectedUserIdsAsync(Guid userId);
        Task<List<User>> GetConnectionsAsync(Guid userId);
        Task<List<User>> GetConnectedToAsync(Guid userId);
        Task AcceptConnectionAsync(Guid userId, Guid connectionId);
        Task RejectConnectionAsync(Guid userId, Guid connectionId);
        //Task SendConnectionAcceptedNotification(Guid userId, User connectedUser);
        //Task<List<User>> GetMyConnectionsAsync(Guid userId);
        //Task<List<int>> GetConnectedUserIdsAsync(Guid userId); 
    }
}