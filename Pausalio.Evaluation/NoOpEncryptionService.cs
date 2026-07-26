using Pausalio.Application.Services.Interfaces;

namespace Pausalio.Evaluation
{
    public class NoOpEncryptionService : IEncryptionService
    {
        public string Encrypt(string plainText) => plainText;
        public string Decrypt(string cipherText) => cipherText;
    }
}
