using FluentValidation;

namespace IkerFinance.Application.Features.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Valid email address is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}