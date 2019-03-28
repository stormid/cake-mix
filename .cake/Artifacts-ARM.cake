public partial class Configuration 
{
    public Configuration ConfigureARM(DirectoryPath sourceFolder)
    {
        ARM = new ARMConfiguration(context, sourceFolder);
        return this;
    }

    public ARMConfiguration ARM { get; private set; }
}

public class ARMConfiguration
{
    public DirectoryPath SourceFolder { get; } = "./src/arm";

    private readonly ICakeContext cakeContext;

    public ARMConfiguration(ICakeContext cakeContext, DirectoryPath sourceFolder)
    {
        this.cakeContext = cakeContext;
        SourceFolder = sourceFolder;
    }
}

Task("Artifacts:Copy:ARM")
    .WithCriteria<Configuration>((ctx, config) => ctx.DirectoryExists(config.ARM.SourceFolder))
    .IsDependentOn("Build")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    var artifacts = $"{config.Artifacts.Root}/arm";
    EnsureDirectoryExists(artifacts);
    foreach(var directory in GetSubDirectories(config.SourceFolder)) 
    {
        if(DirectoryExists(directory)) {
            var copyFrom = directory;
            var copyTo = $"{artifacts}/{directory.GetDirectoryName()}";
            Information("{0} -> {1}", copyFrom, copyTo);
            EnsureDirectoryExists(copyTo);
            CopyDirectory(directory, copyTo);
            config.Artifacts.Add(ArtifactTypeOption.Other, directory.GetDirectoryName(), directory.FullPath);
        }
    }
});