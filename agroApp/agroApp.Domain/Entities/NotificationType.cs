using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace agroApp.Domain.Entities
{
    public enum NotificationType
    {
        ConnectionRequest,
        ConnectionAccepted,
        ConnectionRejected,
        PostCreated,
        PostReacted,
        PostCommented,
        EventCreated,
        EventJoined
    }
}