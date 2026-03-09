using Microsoft.Extensions.Configuration;
using Pausalio.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Services.Implementations
{
    public class XorEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public XorEncryptionService(IConfiguration config)
        {
            var keyString = config["Encryption:Key"]!;
            _key = Encoding.UTF8.GetBytes(keyString);
        }

        public string Encrypt(string plainText)
        {
            var data = Encoding.UTF8.GetBytes(plainText);
            for (int i = 0; i < data.Length; i++)
                data[i] ^= _key[i % _key.Length];
            return Convert.ToBase64String(data);
        }

        public string Decrypt(string cipherText)
        {
            var data = Convert.FromBase64String(cipherText);
            for (int i = 0; i < data.Length; i++)
                data[i] ^= _key[i % _key.Length];
            return Encoding.UTF8.GetString(data);
        }
    }
}
