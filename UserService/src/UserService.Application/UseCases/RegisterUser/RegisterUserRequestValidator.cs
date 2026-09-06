using FluentValidation;

namespace UserService.Application.UseCases.RegisterUser
{
    public class RegisterUserRequestValidator 
        : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Informe o nome.")
                .MinimumLength(3).WithMessage("O nome precisa de ao menos 3 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Informe o e-mail.")
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Informe a senha.")
                .MinimumLength(6).WithMessage("A senha precisa de ao menos 6 caracteres.");
        }
    }
}
