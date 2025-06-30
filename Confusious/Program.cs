
using Confusious;
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
    var sources = ProjectFileParser.GetSourcesFromProjectFile(projectFilePath);

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
    }

}
else
{
    Console.Write("No file found! Scared?");
}
    
