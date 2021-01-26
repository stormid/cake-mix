#addin "nuget:?package=Cake.Npm&version=0.17.0"

public partial class Configuration 
{
    public Configuration RunNpmScript(string scriptName, string workingDirectory = "./")
    {
        if(Npm == null) 
        {
            Npm = new NpmConfiguration(context, scriptName, workingDirectory);
        }
        else
        {
            Npm.AddScript(scriptName, workingDirectory);
        }
        return this;
    }

    public NpmConfiguration Npm { get; private set; }
}

public class NpmConfiguration
{
    private readonly IList<NpmScriptEntry> _scriptEntries = new List<NpmScriptEntry>();
    public IEnumerable<NpmScriptEntry> ScriptEntries => _scriptEntries.ToList();

    private readonly ICakeContext cakeContext;

    public NpmConfiguration(ICakeContext cakeContext, string scriptName, string workingDirectory)
    {
        this.cakeContext = cakeContext;
        _scriptEntries.Add(new NpmScriptEntry(cakeContext, scriptName, workingDirectory));
    }

    public void AddScript(string scriptName, string workingDirectory) 
    {
        _scriptEntries.Add(new NpmScriptEntry(cakeContext, scriptName, workingDirectory));
    }

    public bool CanExecuteNpm {
        get 
        {
            return ScriptEntries.Any(s => s.CanExecuteNpm);
        }
    }

    public class NpmScriptEntry 
    {
        public string ScriptName { get; }
        public string WorkingDirectory { get; } 

        private readonly ICakeContext cakeContext;

        public NpmScriptEntry(ICakeContext cakeContext, string scriptName, string workingDirectory)
        {
            this.cakeContext = cakeContext;
            ScriptName = scriptName;
            WorkingDirectory = workingDirectory ?? "./";
        }

        public bool CanExecuteNpm {
            get 
            {
                return cakeContext.FileExists($"{WorkingDirectory.TrimEnd('/')}/package.json");
            }
        }
    }
}

Task("Npm").IsDependeeOf("Restore").IsDependentOn("Npm:Install").IsDependentOn("Npm:Build");

Task("Npm:Install")
    .WithCriteria<Configuration>((ctx, config) => config.Npm.CanExecuteNpm)
    .Does<Configuration>(config => 
{
    foreach(var script in config.Npm.ScriptEntries) 
    {
        if(script.CanExecuteNpm)
        {
            Information($"Running Install from {script.WorkingDirectory}");
            var settings = new NpmInstallSettings();
            settings.WorkingDirectory = script.WorkingDirectory;
            settings.LogLevel = NpmLogLevel.Silent;
            settings.RedirectStandardError = false;
            settings.RedirectStandardOutput = false;
            NpmInstall(settings);
        }
    }
});

Task("Npm:Install")
    .WithCriteria<Configuration>((ctx, config) => config.Npm.CanExecuteNpm)
    .Does<Configuration>(config => 
{
    foreach(var script in config.Npm.ScriptEntries)
    {    
        if(script.CanExecuteNpm)
        {
            Information($"Running CI from {script.WorkingDirectory}");
            var settings = new NpmCiSettings();
            settings.WorkingDirectory = script.WorkingDirectory;
            settings.LogLevel = NpmLogLevel.Silent;
            settings.RedirectStandardError = false;
            settings.RedirectStandardOutput = false;
            NpmCi(settings);
        }
    }
});

Task("Npm:Build")
    .IsDependentOn("Npm:Install")
    .WithCriteria<Configuration>((ctx, config) => config.Npm.CanExecuteNpm)
    .Does<Configuration>(config => 
{
    foreach(var script in config.Npm.ScriptEntries) 
    {   
        if(script.CanExecuteNpm)
        {
            Information($"Running script {script.ScriptName} from {script.WorkingDirectory}");
            var settings = new NpmRunScriptSettings();
            settings.WorkingDirectory = script.WorkingDirectory;
            settings.LogLevel = NpmLogLevel.Silent;
            settings.RedirectStandardError = false;
            settings.RedirectStandardOutput = false;
            settings.ScriptName = script.ScriptName;
            NpmRunScript(settings);
        }
    }
});