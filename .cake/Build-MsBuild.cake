#load "Configuration.cake"
#load "Configuration-MsBuild.cake"

Task("Build:MsBuild")
    .IsDependentOn("Restore")
    .IsDependeeOf("Build")
    .Does<Configuration>(config =>
{
    var toolVersion = config.GetTaskParameter<MSBuildToolVersion>("MsBuild:Version", MSBuildToolVersion.Default);

    Information("MS Build Tool Version: " + toolVersion.ToString());

    MSBuild(config.Solution.Path.ToString(), c => c
        .SetConfiguration(config.Solution.BuildConfiguration)
        .SetVerbosity(Verbosity.Minimal)
        .UseToolVersion(toolVersion)
        .WithWarningsAsError()
        .WithTarget("Build")
    );
});