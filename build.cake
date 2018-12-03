

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");


// Define directories.
var solutions = GetFiles("./**/*.sln");
var solutionPaths = solutions.Select(solution => solution.GetDirectory());


//////////////////////////////////////////////////////////////////////
// CLEANNING
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

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

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

//////////////////////////////////////////////////////////////////////
// TEST RUNNER
//////////////////////////////////////////////////////////////////////

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
// DLL COPYING
//////////////////////////////////////////////////////////////////////

Task("Dll-Сopying")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() => 
{
    if(DirectoryExists("./nuget"))
    {
        CleanDirectories("./nuget");
    }
    else
    {
        CreateDirectory("./nuget");
    }

    CreateDirectory("./nuget/tools");
    CreateDirectory("./nuget/analyzers/dotnet/cs");
    CopyFile("./Vp.Roslyn.Analyzers.All.nuspec", "./nuget/Vp.Roslyn.Analyzers.All.nuspec");

    foreach(var path in solutionPaths)
    {
        Information("Search DLL {0}", path);
        var files = GetFiles(path + "/**/bin/" + configuration + "/**/*Analyzer.dll");
        if(!files.Any()) Warning("Files not found");
        foreach(var file in files) 
        {
            Information("Сopying DLL {0}", file);
        }
        CopyFiles(files, "./nuget/analyzers/dotnet/cs");

        var scripts = GetFiles(path + "/**/bin/" + configuration + "/**/tools/*.ps1");
        CopyFiles(scripts, "./nuget/tools");
    }
});

//////////////////////////////////////////////////////////////////////
// PACK NUGET
//////////////////////////////////////////////////////////////////////

Task("Pack-Nuget")
    .IsDependentOn("Dll-Сopying")
    .Does(() =>
{
    var nugetSettings = new NuGetPackSettings
    {
      OutputDirectory = "./nuget"
    };

    NuGetPack("./nuget/Vp.Roslyn.Analyzers.All.nuspec", nugetSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Pack-Nuget");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
