namespace Modules.Identity.Interfaces;

public interface ISecureHasher
{
    string HashPassword(string value);
    bool VerifyPassword(string value, string hash);
    string HashToken(string token);
    bool VerifyToken(string token, string storedHash);
}
