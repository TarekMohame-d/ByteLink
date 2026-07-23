using System.Security.Cryptography;
using System.Text;
using Modules.Identity.Interfaces;

namespace Modules.Identity.Infrastructure.Services;

public class SecureHasher : ISecureHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 500_000;

    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    // --- OTP / PASSWORD METHODS (PBKDF2) ---
    public string HashPassword(string value)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(value, salt, Iterations, Algorithm, HashSize);

        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool VerifyPassword(string value, string hash)
    {
        var parts = hash.Split('-');
        if (parts.Length != 2)
            return false;
        try
        {
            var hashPart = Convert.FromHexString(parts[0]);
            var salt = Convert.FromHexString(parts[1]);

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(value, salt, Iterations, Algorithm, HashSize);

            return CryptographicOperations.FixedTimeEquals(hashPart, inputHash);
        }
        catch
        {
            return false;
        }
    }

    // --- TOKEN METHODS (SALTED SHA-256) ---
    public string HashToken(string token)
    {
        var tokenBytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(tokenBytes);

        return Convert.ToHexString(hash);
    }

    public bool VerifyToken(string token, string storedHash)
    {
        try
        {
            var inputHash = HashToken(token);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(inputHash),
                Encoding.UTF8.GetBytes(storedHash)
            );
        }
        catch
        {
            return false;
        }
    }
}
