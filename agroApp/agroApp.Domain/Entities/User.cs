using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization; 
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace agroApp.Domain.Entities;

    public class User : IdentityUser
    {
        public Guid Id { get; set; }
        
        public string Email {get; set;}

        public virtual Profile Profile { get; set; }

        [NotMapped]
        public string Cpf { get; set; }

        [NotMapped]
        public string Cnpj { get; set; }

        [JsonIgnore]
        public List<Post> Posts { get; set; } = new List<Post>();
        [JsonIgnore] 
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
        [JsonIgnore] 
        public List<Event> OrganizedEvents { get; set; } = new List<Event>();
        [JsonIgnore] 
        public List<Connection> Connections { get; set; } = new List<Connection>();
        [JsonIgnore] 
        public List<Connection> ConnectedTo { get; set; } = new List<Connection>(); // Conexões que outros usuários fizeram com este usuário
        [JsonIgnore] 
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        [JsonIgnore]
        public List<News> News { get; set; } = new List<News>();
        [JsonIgnore]
        public List<Tip> Tips { get; set; } = new List<Tip>();
        [JsonIgnore]
        public List<EventComment> EventComments { get; set; } = new List<EventComment>();
        [JsonIgnore]
        public List<Banner> Banners { get; set; } = new List<Banner>();
        public List<EventShare> EventShares { get; set; } = new List<EventShare>();
        [JsonIgnore]
        public List<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
        
}

