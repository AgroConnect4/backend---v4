using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization; 

namespace agroApp.Domain.Entities
{
public class Banner
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ImageUrl { get; set; }
    public string Title { get; set; }
    public string LinkUrl { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? UpdatedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Foreign key to the User table
    public Guid UserId { get; set; } 
    [JsonIgnore]
    public virtual User User { get; set; }
}
}