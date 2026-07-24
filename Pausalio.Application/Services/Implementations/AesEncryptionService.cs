using Microsoft.Extensions.Configuration;
using Pausalio.Application.Services.Interfaces;
using System.Text;
using System.Security.Cryptography;

namespace Pausalio.Application.Services.Implementations
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly string _key;

        public AesEncryptionService(IConfiguration configuration)
        {
            _key = configuration["Encryption:Key"]
                ?? throw new Exception("Encryption key missing.");
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                return string.Empty;

            using var aes = Aes.Create();

            aes.Key = Convert.FromBase64String(_key);
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var encryptedBytes = encryptor.TransformFinalBlock(
                plainBytes,
                0,
                plainBytes.Length);

            var result = new byte[aes.IV.Length + encryptedBytes.Length];

            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrWhiteSpace(cipherText))
                return string.Empty;

            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();

            aes.Key = Convert.FromBase64String(_key);

            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
            Buffer.BlockCopy(fullCipher, 16, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();

            var decryptedBytes = decryptor.TransformFinalBlock(
                cipher,
                0,
                cipher.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
