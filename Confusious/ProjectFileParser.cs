using Confusious.Models;
using NuGet.Common;
using NuGet.ProjectModel;
using System.Text.Json;

namespace Confusious
{
    public static class ProjectFileParser
    {
        public static List<Dependencies> ParseProjectFile(string projectFilePath)
        {
            List<Dependencies> dependencies = new();
            LockFile lockFile = LockFileUtilities.GetLockFile(projectFilePath, NullLogger.Instance);

            if (lockFile != null && lockFile.Libraries != null && lockFile.Libraries.Any())
            {
                foreach (var dep in lockFile.Libraries)
                {
                    Dependencies dependency = new Dependencies();
                    dependency.Name = dep.Name;
                    dependency.Version = dep.Version.ToString();
                    dependencies.Add(dependency);
                    Console.WriteLine($"Package: {dep.Name}, Version: {dep.Version}");
                }

            }

            return dependencies;
            

        }

        public static List<string> GetSourcesFromProjectFile(string projectFilePath)
        {
            var json = File.ReadAllText(projectFilePath);
            var sources = new List<string>();

            var parsedJson = JsonDocument.Parse(json);

            var root = parsedJson.RootElement;

            if (root.TryGetProperty("project", out var projectProp) &&
                projectProp.TryGetProperty("restore", out var restoreProp) &&
                restoreProp.TryGetProperty("sources", out var sourcesProp) &&
                sourcesProp.ValueKind == JsonValueKind.Object)
            {
                Console.WriteLine("Sources: ");
                foreach(var source in sourcesProp.EnumerateObject())
                {
                    sources.Add(source.Name);
                }
            }
            sources.Remove(Constants.Constants.Feed);
            sources.Insert(0, Constants.Constants.Feed);

            var sdkNuget = sources.Where(x => x.Contains(Constants.Constants.SDKNugetPath)).FirstOrDefault();

            if (!string.IsNullOrEmpty(sdkNuget))
            {
                sources.Remove(sdkNuget);
                sources.Add(sdkNuget);
            }

            foreach (var source in sources)
            {
                Console.WriteLine(source);

            }
            return sources;
        }
    }
}
