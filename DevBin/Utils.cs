using DevBin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DevBin
{
    public static class Utils
    {
        private static RNGCryptoServiceProvider _random = new();
        public static string RandomAlphaString(int length = 8)
        {
            byte[] numbers = new byte[length];
            _random.GetBytes(numbers);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            int i = 0;
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => {
                    int j = numbers[i] % chars.Length;
                    i++;
                    return s[j];
                }).ToArray());
        }

        public static string RandomString(int length = 16)
        {
            byte[] numbers = new byte[length];
            _random.GetBytes(numbers);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.";
            int i = 0;
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => {
                    int j = numbers[i] % chars.Length;
                    i++;
                    return s[j];
                }).ToArray());
        }

        public static bool ValidatePassword(User user, string password)
        {
            var userPassword = Encoding.ASCII.GetString(user.Password);
            return BCrypt.Net.BCrypt.EnhancedVerify(password, userPassword);
        }

        public static string GetTemplate(string name, Dictionary<string, object> items)
        {
            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "EmailTemplates", name + ".html")))
            {
                var content = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "EmailTemplates", name + ".html"));

                foreach (var key in items.Keys)
                {
                    var value = items[key].ToString();

                    content = content.Replace("{" + key + "}", value);
                }

                return content;
            }

            return string.Empty;
        }

        public static string FriendlySize(int bytes)
        {
            var output = (float)bytes;

            var prefixes = new string[]
            {
                "Bytes",
                "KiB",
                "MiB",
                "GiB",
            };
            int i;
            for (i = 0; i < prefixes.Length; i++)
            {
                if (output < 1024)
                    break;

                output /= 1024;
            }

            return string.Format("{0:0.##} {1}", output, prefixes[i]);
        }
    }
}
