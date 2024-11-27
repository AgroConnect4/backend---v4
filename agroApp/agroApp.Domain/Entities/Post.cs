using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization; 

namespace agroApp.Domain.Entities
{
    public class Post
    {   
        public Guid Id { get; set; }

        // Chave estrangeira para User
        public Guid UserId { get; set; }

        public string Content { get; set; }

        public string Title { get; set; }

        public string ImageUrl { get; set; }
        
        public DateTime CreatedAt { get; set; }

        public DateTime? EditedAt { get; set; }
        
        [JsonIgnore] 
        public virtual User User { get; set; }

        //public int LikesCount { get; set; }

        public bool IsActive { get; set; } = true;

        //public bool IsReported { get; set; } = false; 

        [JsonIgnore] 
        public List<PostComment> Comments { get; set; } = new List<PostComment>();

        [JsonIgnore]
        public List<PostReaction> Reactions { get; set; } = new List<PostReaction>();
        
        [JsonIgnore]
        public List<PostReport> Reports { get; set; } = new List<PostReport>();
        
        [JsonIgnore]
        public List<PostShare> Shares { get; set; } = new List<PostShare>();

        public List<string> Categories { get; set; } = new List<string>();
    }
}
