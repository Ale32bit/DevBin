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
}