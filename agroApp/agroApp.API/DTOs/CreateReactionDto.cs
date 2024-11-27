using System;
using System.ComponentModel.DataAnnotations;
using agroApp.Domain.Entities;
using System.Text.Json.Serialization; 

namespace agroApp.API.DTOs
{
    public class CreateReactionDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ReactionType ReactionType { get; set; }
    }
}