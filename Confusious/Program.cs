
using Confusious;
using Confusious.Constants;
using Confusious.Utils;
using System.CommandLine;
using System.Reflection.Metadata;

var projectFileOption = new Option<string>("--path")
{
    Description = "Path to your project's project.assets.json file."
};

RootCommand command =  new RootCommand("Confusious - Will your project survive dependency confusion?");

command.Add(projectFileOption);

var parser = command.Parse(args);

var projectFilePath = parser.GetValue<string>(projectFileOption);

if (!string.IsNullOrEmpty(projectFilePath))
{
    Console.WriteLine($"Reading {projectFilePath} ....");
    var dependencies = ProjectFileParser.ParseProjectFile(projectFilePath);
    (var sources, var configPath) = ProjectFileParser.GetSourcesFromProjectFile(projectFilePath);

    Console.WriteLine($"Config Path: {configPath}");

    var segregation = await NugetHandler.GetSegregatedDependenciesAsync(dependencies, sources);

    if(segregation != null)
    {
        var publicPackages = segregation.Where(x => x.IsInternal == false);
        if(publicPackages != null && publicPackages.Any())
        {
            Console.WriteLine("Public packages: ");
            foreach(var package in publicPackages)
            {
                Console.WriteLine($"PackageName: {package.Name}, Version: {package.Version}");
            }
        }

        var internalPackages = segregation.Where(x => x.IsInternal == true);
        if (internalPackages != null && internalPackages.Any())
        {
            Console.WriteLine("Internal packages: ");
            foreach (var package in internalPackages)
            {
                Console.WriteLine($"PackageName: {package.Name}, Version: {package.Version}");
            }
        }

        NugetHandler.AddNugetFakeFeed(configPath);
        foreach(var package in internalPackages)
        {
            NugetHandler.CreateDummyPackage(package.Name, package.Version, Constants.FakeFeedPath);
        }
        CommandLineProcesses.RunDotnetRestore(projectFilePath.Replace("\\obj\\project.assets.json", ""));
        var fakeSource = sources.Where(x => x.Contains(Constants.FakeFeedPath)).FirstOrDefault();
        if (!string.IsNullOrEmpty(fakeSource)) 
        {
            sources.Remove(fakeSource);
            sources.Insert(0, fakeSource);
            dependencies.ForEach(x => x.Found = false);
            var packageCheck = await NugetHandler.GetSegregatedDependenciesAsync(dependencies, sources);
            var vulnerablePackages = packageCheck.Where(x => x.IsVulnerable == true);

            if(vulnerablePackages  != null && vulnerablePackages.Any())
            {
                Console.WriteLine("!!!Vulnerable Packages!!!");
                foreach (var package in vulnerablePackages) 
                {
                    Console.WriteLine($"PackageName: {package.Name}, Version: {package.Version}");
                }

            }

        }
       
        NugetHandler.RemoveNugetFakeFeed(configPath) ;
    }

}
else
{
    Console.Write("No file found! Scared?");
}
    
