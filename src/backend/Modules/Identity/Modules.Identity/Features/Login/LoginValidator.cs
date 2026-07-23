using FluentValidation;

namespace Modules.Identity.Features.Login;

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().WithMessage("{PropertyName} is required.");

        RuleFor(x => x.Password).NotEmpty().WithMessage("{PropertyName} is required.");
    }
}
