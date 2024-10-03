using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agroApp.Domain.Entities
{
    public class Connection
    {
        public int Id { get; set; }
        private int _userId;
        public int UserId
        {
            get { return _userId; }
            set
            {
                ValidateUserId(value);
                _userId = value;
            }
        }
        
        private int _connectedUserId;
        public int ConnectedUserId
        {
            get { return _connectedUserId; }
            set
            {
                ValidateConnectedUserId(value);
                _connectedUserId = value;
            }
        }
        
        public User User { get; set; }
        public User ConnectedUser { get; set; }

         private void ValidateUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("UserId deve ser maior que zero.");
            }
        }

        // Validação para ConnectedUserId
        private void ValidateConnectedUserId(int connectedUserId)
        {
            if (connectedUserId <= 0)
            {
                throw new ArgumentException("ConnectedUserId deve ser maior que zero.");
            }
        }
    }
}