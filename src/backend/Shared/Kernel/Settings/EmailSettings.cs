namespace Shared.Kernel.Settings;

public sealed class EmailSettings
{
    public string SmtpServer { get; init; } = default!;
    public string SmtpPort { get; init; } = default!;
    public string SenderName { get; init; } = default!;
    public string SenderEmail { get; init; } = default!;
    public string BackendBaseUrl { get; init; } = default!;
    public string AppPassword { get; init; } = default!;
}
