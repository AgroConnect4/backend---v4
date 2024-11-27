using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agroApp.Domain.Entities
{
    public class Connection
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        
        public Guid ConnectedUserId { get; set; }

        public ConnectionStatus Status { get; set; } = ConnectionStatus.Pending;

        public bool IsMutual { get; set; } = false;
        
        public virtual User User { get; set; }
        public virtual User ConnectedUser { get; set; }
        
    }
}