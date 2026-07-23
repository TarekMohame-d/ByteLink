using FluentValidation;

namespace Modules.Identity.Features.ResetPassword;

public sealed class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("{PropertyName} is required.");

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
