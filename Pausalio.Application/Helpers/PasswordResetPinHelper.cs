using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Pausalio.Application.Helpers
{
    public static class PasswordResetPinHelper
    {
        public static string GeneratePin(int length = 6)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            var pin = "";

            foreach (var b in bytes)
                pin += (b % 10).ToString();

            return pin;
        }
    }
}
