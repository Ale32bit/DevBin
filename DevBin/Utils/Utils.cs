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
    }
}
