#load "Configuration.cake"

Task("Publish:MsBuild")
    .IsDependentOn("Build")
    .IsDependeeOf("Publish")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.ConsoleProjects.Any() || config.Solution.WebProjects.Any())
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

    foreach(var project in config.Solution.ConsoleProjects) {
        var projectArtifactDirectory = config.Artifacts.GetRootFor(ArtifactTypeOption.Zip);
        var publishDirectory = $"{projectArtifactDirectory}/build/{project.AssemblyName}/";

        EnsureDirectoryExists(publishDirectory);
        
        var counter = 1; // protecting multiple output paths until i can test with a project containing mulitple output paths
        foreach(var outputPath in project.OutputPaths) 
        {
            var artifactZipName = $"{project.AssemblyName}-{counter++}.zip";
            var artifactZipFullPath = $"{projectArtifactDirectory}/{artifactZipName}";

            Information("Zipping {0} to {1}", outputPath, artifactZipFullPath);
            Zip(outputPath, artifactZipFullPath);
            config.Artifacts.Add(ArtifactTypeOption.Zip, artifactZipName, artifactZipFullPath);
        }
    }    
});