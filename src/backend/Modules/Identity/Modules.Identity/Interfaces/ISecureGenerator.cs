namespace Modules.Identity.Interfaces;

public interface ISecureGenerator
{
    string GenerateToken(int byteLength = 32);
}
