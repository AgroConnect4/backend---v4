using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace agroApp.Domain.Entities
{
    public class Post
    {
        public int Id { get; set; }

        // Chave estrangeira para User
        public int UserId { get; set; }

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

        private string _postType;
        public string PostType
        {
            get { return _postType; }
            set
            {
                ValidatePostType(value);
                _postType = value;
            }
        }
        
        public DateTime CreatedAt { get; set; }

        [ForeignKey("UserId")]
        // Propriedade de navegação para User
        public User User { get; set; }

        // Adicionando o campo Username
        //public string Username { get; set; }

       private void ValidateContent(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentException("Content não pode ser nulo ou vazio.");
            }
        }

        // Método de validação para PostType
        private void ValidatePostType(string postType)
        {
            if (string.IsNullOrEmpty(postType))
            {
                throw new ArgumentException("PostType não pode ser nulo ou vazio.");
            }

            // Chame o método IsValidPostType apenas uma vez para realizar a validação
            if (!IsValidPostType(postType))
            {
                throw new ArgumentException("PostType inválido.");
            }
        }

        private bool IsValidPostType(string postType)
        {
            // Adicione sua lógica de validação para PostType aqui
            // Por exemplo, você pode verificar se o tipo está dentro de uma lista de tipos válidos
            if (string.IsNullOrEmpty(postType))
            {
                return false; 
            }

            // Adicione mais regras de validação aqui
            // Exemplo: verificar se o tipo está dentro de uma lista de tipos permitidos
            string[] validPostTypes = { "Máquinas", "Serviços", "Produtos" }; 
            if (!validPostTypes.Contains(postType, StringComparer.OrdinalIgnoreCase)) 
            {
                return false;
            }

            // Se o tipo passou em todas as validações, retornar true
            return true;
        }
    }
}
