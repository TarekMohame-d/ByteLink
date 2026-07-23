using FluentValidation;

namespace Modules.Identity.Features.VerifyEmail;

public sealed class VerifyEmailValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("{PropertyName} is required.");
    }
}
