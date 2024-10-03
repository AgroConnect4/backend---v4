using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace agroApp.Domain.Entities;

    public class User : IdentityUser
    {
        public int Id { get; set; }
        // Não redefina UserName se você não precisa de uma lógica específica
        // public string UserName { get; set; } // Remova esta linha, pois já está na classe base
        private string _email;
        public string Email
        {
            get { return _email; }
            set 
            {
                ValidateEmail(value); // Chame o método de validação
                _email = value; 
            }
        }

        public Profile Profile { get; set; }
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    
        private void ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("O email não pode ser vazio.");
            }

            if (!IsValidEmail(email))
            {
                throw new FormatException("Formato de email inválido.");
            }
        }

        private bool IsValidEmail(string email)
        {
                if (string.IsNullOrEmpty(email))
        {
            // Lançar ArgumentException
            throw new ArgumentException("O email não pode ser vazio.");
        }

        // 2. Verificar se o email é um formato válido usando Regex
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
        {
            // Lançar FormatException
            throw new FormatException("Formato de email inválido.");
        }

        // 3. Se o email passou em todas as validações, retornar true
        return true; 
        }
}

