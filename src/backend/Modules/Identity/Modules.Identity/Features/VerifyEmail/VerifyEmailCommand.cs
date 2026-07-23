using Shared.Kernel.Messaging;

namespace Modules.Identity.Features.VerifyEmail;

public record VerifyEmailCommand(string Token) : ICommand;
