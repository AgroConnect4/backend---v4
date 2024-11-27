using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agroApp.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        
        public string Name {get; set;}

        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
        /*private void ValidateName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name não pode ser nulo ou vazio.");
            }
        }*/
    }
}