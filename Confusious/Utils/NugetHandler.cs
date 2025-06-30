using Confusious.Models;
using NuGet.Common;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System.Xml.Linq;

namespace Confusious.Utils
{
    public static class NugetHandler
    {
        public async static Task<List<Dependencies>> GetSegregatedDependenciesAsync(List<Dependencies> dependencies, List<string> sources)
        {
            List<Dependencies> dependenciesList = new List<Dependencies>();
            foreach (var source in sources)
            {
                SourceRepository _sourceRepository = Repository.Factory.GetCoreV3(source);
                var resource = await _sourceRepository.GetResourceAsync<PackageMetadataResource>();

                foreach (var dependency in dependencies)
                {
                    if (!dependency.Found)
                    {
                        var identity = new PackageIdentity(dependency.Name, new NuGetVersion(dependency.Version));
                        var package = await resource.GetMetadataAsync(identity, new SourceCacheContext(), NullLogger.Instance, CancellationToken.None);
                        if (package != null)
                        {
                            dependency.Found = true;
                            if (source == Constants.Constants.Feed)
                                dependency.IsInternal = false;
                            else
                                dependency.IsInternal = true;
                            dependenciesList.Add(dependency);
                        }
                    }
                    


                }

            }

            return dependenciesList;
        }

        public static void AddNugetFakeFeed(string configPath)
        {
            Console.WriteLine($"Adding fake feed in {configPath} ...");
            
            XDocument doc;
            if (File.Exists(configPath))
            {
                doc = XDocument.Load(configPath);
            }
            else
            {
                doc = new XDocument(new XElement("configuration"));
            }

            XElement config = doc.Element("configuration");
            XElement packageSources = config.Element("packageSources");

            if (packageSources == null)
            {
                packageSources = new XElement("packageSources");
                config.Add(packageSources);
            }

            var toRemove = packageSources.Elements("add")
                .Where(x => (string)x.Attribute("key") == Constants.Constants.FakeFeedName)
                .ToList();

            foreach (var x in toRemove)
                x.Remove();

            var existing = packageSources.Elements("add")
                .Where(x => (string)x.Attribute("key") == Constants.Constants.FakeFeedName)
                .ToList();

            foreach (var x in existing)
                x.Remove();

            var newSourceElement = new XElement("add",
                new XAttribute("key", Constants.Constants.FakeFeedName),
                new XAttribute("value", Constants.Constants.FakeFeedPath));

            var nugetOrg = packageSources.Elements("add")
                .FirstOrDefault(x => (string)x.Attribute("key") == "nuget.org");

            if (nugetOrg != null)
            {
                nugetOrg.AddBeforeSelf(newSourceElement);
            }
            else
            {
                packageSources.AddFirst(newSourceElement);
            }

            doc.Save(configPath);


        }

        public static void RemoveNugetFakeFeed(string configPath)
        {
            Console.WriteLine($"Removing fake feed from {configPath} ...");

            XDocument doc;
            if (File.Exists(configPath))
            {
                doc = XDocument.Load(configPath);
            }
            else
            {
                doc = new XDocument(new XElement("configuration"));
            }

            XElement config = doc.Element("configuration");
            XElement packageSources = config.Element("packageSources");

            if (packageSources == null)
            {
                packageSources = new XElement("packageSources");
                config.Add(packageSources);
            }

            var toRemove = packageSources.Elements("add")
                .Where(x => (string)x.Attribute("key") == Constants.Constants.FakeFeedName)
                .ToList();

            foreach (var x in toRemove)
                x.Remove();

            doc.Save(configPath);



        }

        public static void CreateDummyPackage(string packageId, string version, string outputDir)
        {
            Console.WriteLine($"Creating fake package for {packageId} {version}");
            string projectRoot = AppContext.BaseDirectory;
            string dummyDllPath = Path.Combine(projectRoot, "FakeFeedFiles", "fakefeed.dll");

            var builder = new PackageBuilder
            {
                Id = packageId,
                Version = NuGetVersion.Parse(version),
                Description = "Dummy package for dependency confusion PoC",
                Authors = { "PoC" }
            };

            builder.Files.Add(new PhysicalPackageFile
            {
                SourcePath = dummyDllPath,
                TargetPath = "lib/net8.0/fakefeed.dll"
            });

            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            string nupkgPath = Path.Combine(outputDir, $"{packageId}.{version}.nupkg");

            using var fs = File.Create(nupkgPath);
            builder.Save(fs);
        }
    }
}
