using System.Diagnostics;

namespace Confusious.Utils
{
    public static class CommandLineProcesses
    {
        public static bool RunDotnetRestore(string projectPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"restore \"{projectPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            Console.WriteLine(output);
            if (!string.IsNullOrWhiteSpace(error))
                Console.Error.WriteLine(error);

            return process.ExitCode == 0;
        }
    }
}
