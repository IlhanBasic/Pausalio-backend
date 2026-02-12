using System;
using System.Text;

namespace Pausalio.Application.Helpers
{
    public static class InviteTokenHelper
    {
        private static readonly Random _random = new Random();

        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public static string GenerateToken(int length = 8)
        {
            var sb = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                var index = _random.Next(Chars.Length);
                sb.Append(Chars[index]);
            }
            return sb.ToString();
        }
    }
}
