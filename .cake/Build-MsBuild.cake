#load "Configuration.cake"

Task("Build:MsBuild")
    .IsDependentOn("Restore")
    .IsDependeeOf("Build")
    .Does<Configuration>(config =>
{
    Information("MS Build Tool Version: " + config.MsBuildToolVersion.ToString());

    MSBuild(config.Solution.Path.ToString(), c => c
        .SetConfiguration(config.Solution.BuildConfiguration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(config.MsBuildToolVersion)
        .WithWarningsAsError()
        .WithTarget("Build")
    );
});