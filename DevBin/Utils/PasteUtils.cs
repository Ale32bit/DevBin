using System.Security.Cryptography;

namespace DevBin.Utils;

public class PasteUtils
{
    public static string GetShortContent(string content, int length)
    {
        var shortened = content[..Math.Min(content.Length, length)];
        if (content.Length > length)
        {
            shortened += "...";
        }

        return shortened;
    }

    public static string GenerateRandomCode(int length = 8)
    {
        byte[] numbers = RandomNumberGenerator.GetBytes(length);
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        int i = 0;
        return new string(Enumerable.Repeat(chars, length)
            .Select(s =>
            {
                int j = numbers[i] % chars.Length;
                i++;
                return s[j];
            }).ToArray());
    }
}