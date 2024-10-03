using FluentValidation;
using agroApp.Domain.Entities;

namespace agroApp.Domain.Validation
{
    public class ConnectionValidator : AbstractValidator<Connection>
    {
        public ConnectionValidator()
        {
            RuleFor(c => c.UserId)
                .NotEmpty()
                .WithMessage("ID do usuário é obrigatório.");
            
            RuleFor(c => c.ConnectedUserId)
                .NotEmpty()
                .WithMessage("ID do usuário conectado é obrigatório.");
        }
    }
}