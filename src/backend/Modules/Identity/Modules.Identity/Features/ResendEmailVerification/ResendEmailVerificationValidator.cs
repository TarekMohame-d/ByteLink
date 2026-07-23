using FluentValidation;

namespace Modules.Identity.Features.ResendEmailVerification;

public sealed class ResendEmailVerificationValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .EmailAddress()
            .WithMessage("{PropertyName} is invalid.");
    }
}
