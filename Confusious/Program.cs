
using Confusious;
using Confusious.Constants;
using Confusious.Utils;
using System.CommandLine;

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
                Console.WriteLine($"PackageName: {package.Name}, Version: {package.Version}, Source: {package.Source}");
            }
        }

        NugetHandler.AddNugetFakeFeed(configPath);
        foreach(var package in internalPackages)
        {
            NugetHandler.CreateDummyPackage(package.Name, package.Version, Constants.FakeFeedPath);
        }
        CommandLineProcesses.RunDotnetRestore(projectFilePath.Replace("\\obj\\project.assets.json", ""));
        var fakeSource = Constants.FakeFeedPath;
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
                    Console.WriteLine($"PackageName: {package.Name}, Version: {package.Version}, Source: {package.Source}");
                }

            }

        }
       
        NugetHandler.RemoveNugetFakeFeed(configPath) ;
        CommandLineProcesses.RunDotnetRestore(projectFilePath.Replace("\\obj\\project.assets.json", ""));
        sources.Remove(fakeSource) ;
        dependencies.ForEach(x => x.Found = false);
        var packageReCheck = await NugetHandler.GetSegregatedDependenciesAsync(dependencies, sources);
        var recheckedPackages = packageReCheck.Where(x => x.IsVulnerable == true);

        if (recheckedPackages != null && recheckedPackages.Any())
        {
            Console.WriteLine("!!!Vulnerable Packages!!!");
            foreach (var package in recheckedPackages)
            {
                Console.WriteLine($"PackageName: {package.Name}, Version: {package.Version}, Source: {package.Source}");
            }

        }
    }

}
else
{
    Console.Write("No file found! Scared?");
}
    
