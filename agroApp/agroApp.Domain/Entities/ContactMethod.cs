using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace agroApp.Domain.Entities
{
    public class ContactMethod
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string UrlOrNumber { get; set; }
        public Guid ProfileId { get; set; }
        [JsonIgnore]
        public virtual Profile Profile { get; set; }
    }
}