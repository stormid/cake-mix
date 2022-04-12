#tool nuget:?package=NuGet.CommandLine&version=6.1.0

#load "Configuration.cake"

Task("Restore:NuGet")
    .IsDependeeOf("Restore")
    .Does<Configuration>(config => 
{
    NuGetRestore(config.Solution.Path.ToString(), new NuGetRestoreSettings());
});