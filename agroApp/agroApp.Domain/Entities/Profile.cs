using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agroApp.Domain.Entities
{
    public class Profile
    {
        public int Id { get; set; }
        private string _bio;
        public string Bio
        {
            get { return _bio; }
            set
            {
                ValidateBio(value);
                _bio = value;
            }
        }

        private string _profilePicture;
        public string ProfilePicture
        {
            get { return _profilePicture; }
            set
            {
                ValidateProfilePicture(value);
                _profilePicture = value;
            }
        }

        private string _coverPicture;
        public string CoverPicture
        {
            get { return _coverPicture; }
            set
            {
                ValidateCoverPicture(value);
                _coverPicture = value;
            }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set
            {
                ValidateDescription(value);
                _description = value;
            }
        }

        // Chave estrangeira para User
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } // Propriedade de navegação para User
    
         private void ValidateBio(string bio)
        {
            if (string.IsNullOrEmpty(bio))
            {
                throw new ArgumentNullException("Bio não pode ser nulo ou vazio.");
            }
        }

        private void ValidateProfilePicture(string profilePicture)
        {
            if (string.IsNullOrEmpty(profilePicture))
            {
                throw new ArgumentException("ProfilePicture não pode ser nulo ou vazio.");
            }

            // Validar se o valor é uma URL válida usando Uri.TryCreate
            if (!Uri.TryCreate(profilePicture, UriKind.Absolute, out var uriResult) || uriResult == null)
            {
                throw new FormatException("ProfilePicture deve ser uma URL válida.");
            }
        }

        private void ValidateCoverPicture(string coverPicture)
        {
            if (string.IsNullOrEmpty(coverPicture))
            {
                throw new ArgumentException("CoverPicture não pode ser nulo ou vazio.");
            }
            // Adicionar validação de URL aqui se necessário
        }

        private void ValidateDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                throw new ArgumentException("Description não pode ser nulo ou vazio.");
            }
        }
    }

}
