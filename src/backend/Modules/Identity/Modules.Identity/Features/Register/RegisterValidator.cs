using FluentValidation;

namespace Modules.Identity.Features.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .MaximumLength(25)
            .WithMessage("{PropertyName} is too long.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .MaximumLength(25)
            .WithMessage("{PropertyName} is too long.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .EmailAddress()
            .WithMessage("{PropertyName} is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .MinimumLength(8)
            .WithMessage("{PropertyName} must be at least 8 characters long.")
            .Matches("[A-Z]")
            .WithMessage("{PropertyName} must contain at least one uppercase letter.")
            .Matches("[a-z]")
            .WithMessage("{PropertyName} must contain at least one lowercase letter.")
            .Matches(@"\d")
            .WithMessage("{PropertyName} must contain at least one digit.");
    }
}
