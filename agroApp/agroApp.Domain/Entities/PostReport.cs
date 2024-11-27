using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace agroApp.Domain.Entities
{
    public class PostReport
    {
        public Guid Id { get; set; }
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public DateTime ReportedAt { get; set; }
        public string Reason { get; set; }
        public virtual Post Post { get; set; }
        public virtual User User { get; set; }
    }
}