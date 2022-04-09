using System.Security.Cryptography;
using System.Text;

namespace DevBin.Utils
{
    public class Utils
    {
        public static string ToIECFormat(int d)
        {
            var output = (float)d;

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

        public static string GetUserSessionID(HttpContext httpContext, string suffix = "")
        {
            var builder = new StringBuilder();
            builder.Append("SessionID:");
            builder.Append(httpContext.Connection.RemoteIpAddress);
            builder.Append(':');
            builder.Append(httpContext.Request.Headers.UserAgent);
            builder.Append(':');
            builder.Append(suffix);

            return Hash(builder.ToString());
        }

        public static string Hash(string input)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
        }

        public static string GenerateRandomSecureToken(int length = 32)
        {
            byte[] numbers = RandomNumberGenerator.GetBytes(length);
            return Convert.ToHexString(numbers);
        }
    }
}
