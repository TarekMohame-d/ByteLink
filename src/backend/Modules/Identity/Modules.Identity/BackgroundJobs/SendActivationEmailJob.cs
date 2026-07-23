// using Hangfire;
// using Modules.Identity.Interfaces;

// namespace Modules.Identity.BackgroundJobs;

// public sealed class SendActivationEmailJob(IKeycloakAdminClientService keycloakAdminClient)
// {
//     [AutomaticRetry(Attempts = 3, DelaysInSeconds = [5, 10, 15])]
//     public async Task ExecuteAsync(string keycloakId)
//     {
//         await keycloakAdminClient.SendPasswordResetEmailAsync(keycloakId);
//     }
// }
