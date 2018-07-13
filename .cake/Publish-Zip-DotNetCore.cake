#load "Configuration.cake"

Task("Publish:Zip:DotNetCore")
    .IsDependeeOf("Publish")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.WebProjects.Any())
    .Does<Configuration>(config => 
{
    foreach(var webProject in config.Solution.WebProjects) {
        var projectArtifactDirectory = config.Artifacts.GetRootFor(ArtifactTypeOption.Zip);
        var publishDirectory = $"{projectArtifactDirectory}/build/{webProject.AssemblyName}/";
        var artifactZipName = $"{webProject.AssemblyName}.zip";
        var artifactZipFullPath = $"{projectArtifactDirectory}/{artifactZipName}";

        EnsureDirectoryExists(publishDirectory);

        var settings = new DotNetCorePublishSettings
        {
            NoRestore = true,
            Configuration = config.Solution.BuildConfiguration,
            OutputDirectory = publishDirectory,
            Verbosity = DotNetCoreVerbosity.Minimal,
        };

        settings.MSBuildSettings = new DotNetCoreMSBuildSettings();
        settings.MSBuildSettings.NoLogo = true;

        DotNetCorePublish(webProject.ProjectFilePath.ToString(), settings);
        Zip(publishDirectory, artifactZipFullPath);
        config.Artifacts.Add(ArtifactTypeOption.Zip, artifactZipName, artifactZipFullPath);
    }
});