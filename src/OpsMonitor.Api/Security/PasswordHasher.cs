using System.Security.Cryptography;

namespace OpsMonitor.Api.Security;

public interface IPasswordHasher
{
    (string Hash, string Salt, int Iterations) HashPassword(string password);
    bool Verify(string password, string hashBase64, string saltBase64, int iterations);
}

public class PasswordHasher : IPasswordHasher
{
    private const int KeySize = 32;
    private const int SaltSize = 16;
    private const int Iterations = 100_000;

    public (string Hash, string Salt, int Iterations) HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt), Iterations);
    }

    public bool Verify(string password, string hashBase64, string saltBase64, int iterations)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var hash = Convert.FromBase64String(hashBase64);
        var candidate = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, hash.Length);
        return CryptographicOperations.FixedTimeEquals(candidate, hash);
    }
}
