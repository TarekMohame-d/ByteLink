using System.Security.Cryptography;
using Modules.Identity.Interfaces;

namespace Modules.Identity.Infrastructure.Services;

public class SecureGenerator : ISecureGenerator
{
    public string GenerateToken(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);

        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
