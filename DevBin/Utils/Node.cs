using System.Diagnostics;

namespace DevBin.Utils;
public class Node
{
    public static async Task<string> RunScript(string scriptName, IDictionary<string, string> env)
    {
        var process = new Process();
        process.StartInfo.FileName = "node";
        process.StartInfo.Arguments = scriptName;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.WorkingDirectory = Path.Combine(Environment.CurrentDirectory, "Scripts");

        foreach (var (k, v) in env)
        {
            process.StartInfo.Environment[k] = v;
        }

        process.Start();
        await process.WaitForExitAsync();

        if(process.ExitCode != 0)
        {
            throw new Exception(process.StandardError.ReadToEnd());
        }

        return process.StandardOutput.ReadToEnd();
    }
}
