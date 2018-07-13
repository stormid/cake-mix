#load "Configuration.cake"

Task("Publish:MsBuild")
    .IsDependentOn("Build")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    foreach(var webProject in config.Solution.WebProjects) {
        var projectArtifactDirectory = $"{config.Artifacts.GetRootFor(ArtifactTypeOption.Zip)}/{webProject.AssemblyName}";
        var artifactZipName = $"{webProject.AssemblyName}.zip";
        var artifactZipFullPath = $"{projectArtifactDirectory}/{artifactZipName}";

        MSBuild(webProject.ProjectFilePath, c => c
            .SetConfiguration(config.Solution.BuildConfiguration)
            .SetVerbosity(Verbosity.Quiet)
            .UseToolVersion(MSBuildToolVersion.VS2017)
            .WithWarningsAsError()
            .WithTarget("Package")
            .WithProperty("DeployTarget", "PipelinePreDeployCopyAllFilesToOneFolder")
            .WithProperty("SkipInvalidConfigurations", "false")
            .WithProperty("AutoParameterizationWebConfigConnectionStrings", "false")
            .WithProperty("PackageTempRootDir", projectArtifactDirectory)
        );

        Zip($"{projectArtifactDirectory}/PackageTmp", artifactZipFullPath);

        DeleteDirectory($"{projectArtifactDirectory}/PackageTmp", new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });

        config.Artifacts.Add(ArtifactTypeOption.Zip, webProject.AssemblyName, $"{artifactZipFullPath}");
    }
});