#load "Configuration.cake"

Task("CI:VSTS:UploadArtifacts")
    .WithCriteria<Configuration>((ctx, config) => BuildSystem.IsRunningOnAzurePipelinesHosted || AzurePipelines.IsRunningOnAzurePipelines)
    .IsDependentOn("Publish")
    .IsDependeeOf("CI:UploadArtifacts")
    .Does<Configuration>(config => 
{
    Information("Uploading artifacts from {0}", config.Artifacts.Root);
    AzurePipelines.Commands.UploadArtifact("artifacts", config.Artifacts.Root.ToString(), "artifacts");
});

Task("CI:VSTS:UpdateBuildNumber")
    .IsDependeeOf("CI:UpdateBuildNumber")
    .WithCriteria<Configuration>((ctx, config) => BuildSystem.IsRunningOnAzurePipelinesHosted || AzurePipelines.IsRunningOnAzurePipelines)
    .Does<Configuration>(config =>
{
    Information(
        @"Repository:
        Branch: {0}
        SourceVersion: {1}
        Shelveset: {2}",
        BuildSystem.AzurePipelines.Environment.Repository.SourceBranchName,
        BuildSystem.AzurePipelines.Environment.Repository.SourceVersion,
        BuildSystem.AzurePipelines.Environment.Repository.Shelveset
        );    

    AzurePipelines.Commands.UpdateBuildNumber(config.Version.FullSemVersion);
    AzurePipelines.Commands.SetVariable("GitVersion.Version", config.Version.Version);
    AzurePipelines.Commands.SetVariable("GitVersion.SemVer", config.Version.SemVersion);
    AzurePipelines.Commands.SetVariable("GitVersion.InformationalVersion", config.Version.InformationalVersion);
    AzurePipelines.Commands.SetVariable("GitVersion.FullSemVer", config.Version.FullSemVersion);
    AzurePipelines.Commands.SetVariable("Cake.Version", config.Version.CakeVersion);
});