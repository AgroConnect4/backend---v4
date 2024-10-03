using FluentValidation;
using agroApp.Domain.Entities;

namespace agroApp.Domain.Validation
{
    public class PostValidator : AbstractValidator<Post>
    {
        public PostValidator()
        {
            RuleFor(p => p.Content)
                .NotEmpty()
                .WithMessage("Conteúdo do post é obrigatório.");
            
            RuleFor(p => p.PostType)
                .NotEmpty()
                .WithMessage("Tipo do post é obrigatório.");
            
            RuleFor(p => p.PostType)
                .Must(BeValidPostType)
                .WithMessage("Tipo de post inválido.");
        }

        private bool BeValidPostType(string postType)
        {
            // Defina os tipos de posts permitidos
            var validPostTypes = new[] { "Produto", "Serviço", "Máquina", "Outro" };
            
            return validPostTypes
            .Contains(postType, StringComparer.OrdinalIgnoreCase);
        }
    }
}