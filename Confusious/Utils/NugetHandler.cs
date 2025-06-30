using Confusious.Models;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;

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
    }
}
