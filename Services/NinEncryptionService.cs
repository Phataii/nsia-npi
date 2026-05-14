using System.Security.Cryptography;
using System.Text;

namespace nsia.Services
{
    public interface INinEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class NinEncryptionService : INinEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public NinEncryptionService(IConfiguration config)
        {
            var secret = config["NinEncryption:Key"]
                ?? throw new InvalidOperationException("NinEncryption:Key not configured.");

            // Derive a 256-bit key and 128-bit IV from the secret
            using var sha = SHA256.Create();
            _key = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
            _iv = _key[..16];
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText)) return "";

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText)) return "";

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            var decryptor = aes.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(cipherText);
            var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}