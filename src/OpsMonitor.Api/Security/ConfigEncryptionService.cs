using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using OpsMonitor.Api.Options;

namespace OpsMonitor.Api.Security;

public interface IConfigEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
}

public class ConfigEncryptionService : IConfigEncryptionService
{
    private readonly byte[] _keyBytes;

    public ConfigEncryptionService(IOptions<SecurityOptions> options)
    {
        var key = options.Value.ConfigEncryptionKey ?? string.Empty;
        if (key.Length < 32)
        {
            key = key.PadRight(32, '0');
        }
        _keyBytes = Encoding.UTF8.GetBytes(key[..32]);
    }

    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        var packed = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, packed, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, packed, aes.IV.Length, cipherBytes.Length);
        return Convert.ToBase64String(packed);
    }

    public string Decrypt(string ciphertext)
    {
        var packed = Convert.FromBase64String(ciphertext);
        using var aes = Aes.Create();
        aes.Key = _keyBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        var iv = new byte[16];
        Buffer.BlockCopy(packed, 0, iv, 0, 16);
        aes.IV = iv;

        var cipher = new byte[packed.Length - 16];
        Buffer.BlockCopy(packed, 16, cipher, 0, cipher.Length);
        using var decryptor = aes.CreateDecryptor();
        var plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }
}
