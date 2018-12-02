//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

// Define directories.
var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());
//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    // Clean solution directories.
    foreach(var path in solutionPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }
});

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
{
     // Restore all NuGet packages.
    foreach(var solution in solutions)
    {
        Information("Restoring {0}...", solution);
        NuGetRestore(solution);
    }
});

Task("Build")
    .ContinueOnError()
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    // Build all solutions.
    Information("Solution Count {0}", solutions.Count());
    foreach(var solution in solutions)
    {
        try
        {
            Information("Building {0}", solution);
            MSBuild(solution, settings => settings.SetConfiguration(configuration));
        }
        catch (System.Exception)
        {
            
        }
    }
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .Does(() =>
{
    foreach(var path in solutionPaths)
    {
        Information("Run Tests {0}", path);
        MSTest(path + "/**/bin/" + configuration + "/*.Tests.dll");
    }
});

//////////////////////////////////////////////////////////////////////
// PACK NUPKG
//////////////////////////////////////////////////////////////////////

Task("Pack-Nupkg")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => 
{
    if(DirectoryExists("./build"))
    {
        CleanDirectories("./build");
    }
    else
    {
        CreateDirectory("./build");
    }

    CreateDirectory("./build/tools");
    CreateDirectory("./build/analyzers/dotnet/cs");
    CopyFile("./Vp.Roslyn.Analyzers.All.nuspec", "./build/Vp.Roslyn.Analyzers.All.nuspec");

    foreach(var path in solutionPaths)
    {
        Information("Search DLL {0}", path);
        var files = GetFiles(path + "/**/bin/" + configuration + "/netstandard1.3/*.dll");
        if(!files.Any()) Information("Files not found");
        foreach(var file in files) 
        {
            Information("Coping DLL {0}", file);
        }
        CopyFiles(files, "./build/analyzers/dotnet/cs");

        var scripts = GetFiles(path + "/**/bin/" + configuration + "/netstandard1.3/tools/*.ps1");
        CopyFiles(scripts, "./build/tools");
    }

    var nuGetPackSettings = new NuGetPackSettings
	{
		IncludeReferencedProjects = false,
		Properties = new Dictionary<string, string>
		{
			{ "Configuration", "Release" }
		}
	};

    NuGetPack("./build/Vp.Roslyn.Analyzers.All.nuspec", nuGetPackSettings);

});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack-Nupkg");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
