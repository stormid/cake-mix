#load "Configuration.cake"
#load "Configuration-MsBuild.cake"

Task("Build:MsBuild")
    .IsDependentOn("Restore")
    .IsDependeeOf("Build")
    .Does<Configuration>(config =>
{
    Information("MS Build Tool Version: " + config.MSBuildToolVersion.ToString());

    MSBuild(config.Solution.Path.ToString(), c => c
        .SetConfiguration(config.Solution.BuildConfiguration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(config.MSBuildToolVersion)
        .WithWarningsAsError()
        .WithTarget("Build")
    );
});