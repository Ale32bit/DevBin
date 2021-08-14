using DevBin.Models;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace DevBin
{
    public static class Utils
    {
        public static string RandomAlphaString(int length = 8)
        {
            Random rng = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rng.Next(s.Length)]).ToArray());
        }

        public static string RandomString(int length = 16)
        {
            Random rng = new();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rng.Next(s.Length)]).ToArray());
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
    }
}
