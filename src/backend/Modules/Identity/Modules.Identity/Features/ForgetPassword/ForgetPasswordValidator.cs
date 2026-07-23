using FluentValidation;
using Modules.Identity.Domain;

namespace Modules.Identity.Features.ForgetPassword;

public sealed class ForgetPasswordValidator : AbstractValidator<ForgetPasswordToken>
{
    public ForgetPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .EmailAddress()
            .WithMessage("{PropertyName} is invalid.");
    }
}
