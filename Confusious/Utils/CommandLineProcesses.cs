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

        public static void CleanupFromAssetsPath(string assetsJsonPath, string fakeFeedDir)
    {
        string objDir = Path.GetDirectoryName(assetsJsonPath);
        if (objDir == null) throw new InvalidOperationException("Invalid assets.json path");

        string projectDir = Directory.GetParent(objDir)?.FullName;
        if (projectDir == null) throw new InvalidOperationException("Unable to resolve project directory");

        string binDir = Path.Combine(projectDir, "bin");

        string[] foldersToDelete = { binDir, objDir, fakeFeedDir };

        foreach (var folder in foldersToDelete)
        {
            if (Directory.Exists(folder))
            {
                try
                {
                    Directory.Delete(folder, recursive: true);
                    Console.WriteLine($"Deleted: {folder}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete {folder}: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"Not found (skipped): {folder}");
            }
        }
    }

}
}
