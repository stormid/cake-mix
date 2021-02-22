#load "Configuration.cake"

public partial class Configuration {

    public Configuration RegisterBicepPath(string bicepPath)
    {
        context.Tools.RegisterFile(bicepPath);

        return this;
    }

    public Configuration WithBicepFile(params string[] bicepFilePattern)
    {
        var files = bicepFilePattern
            .ToList()
            .SelectMany(x => context.GetFiles(x))
            .ToArray();

        this.TaskParameters.Add("Bicep:Files", files);

        return this;
    }
}

Task("Bicep:Build")
    .IsDependeeOf("Build")
    .Does<Configuration>(config =>
{
    var settings = new ProcessSettings() 
    { 
        RedirectStandardOutput = false
    };

    var bicepExe = Context.Tools.Resolve("bicep.exe");

    if(bicepExe != null)
    {
        var bicepFiles = config.GetTaskParameter<FilePath[]>("Bicep:Files", new FilePath[] { "" });

        Verbose("Building {0} bicep files", bicepFiles.Count());
        foreach(var bicepFile in bicepFiles.Where(f => FileExists(f)))
        {
            Information("Building {0}...", bicepFile);
            settings.Arguments = string.Format("build {0}", bicepFile);

            using(var process = StartAndReturnProcess(bicepExe, settings))
            {
                process.WaitForExit();
                if(process.GetExitCode() != 0)
                {
                    Error("Failed");
                }
            }
        }
    }
    else 
    {
        Error("Unable to resolve bicep tool location");
    }
});