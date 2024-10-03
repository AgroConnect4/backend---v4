using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agroApp.Domain.Entities
{
    public class BlogPost
    {
        public int Id { get; set; }
        private string _title;
        public string Title
        {
            get { return _title; }
            set 
            {
                ValidateTitle(value);
                _title = value; 
            }
        }

        private string _content;
        public string Content
        {
            get { return _content; }
            set 
            {
                ValidateContent(value);
                _content = value; 
            }
        }
        public DateTime DateCreated { get; set; }

        public int UserId { get; set; } // Autor da postagem
        public User User { get; set; }  // Referência ao autor

        private void ValidateTitle(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException("Title não pode ser nulo ou vazio.");
            }
        }

        // Validação para Content
        private void ValidateContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("Content não pode ser nulo ou vazio.");
            }
        }
    }

}
