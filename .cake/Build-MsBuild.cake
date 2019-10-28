#load "Configuration.cake"

Task("Build:MsBuild")
    .IsDependentOn("Restore")
    .IsDependeeOf("Build")
    .Does<Configuration>(config =>
{
    MSBuild(config.Solution.Path.ToString(), c => c
        .SetConfiguration(config.Solution.BuildConfiguration)
        .SetVerbosity(Verbosity.Minimal)
        .WithWarningsAsError()
        .WithTarget("Build")
    );
});