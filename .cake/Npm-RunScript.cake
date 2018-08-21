#addin "Cake.Npm"
#addin "Cake.Gulp"

Task("Npm").IsDependentOn("Npm:Install").IsDependentOn("Npm:Build");

Task("Npm:Install")
    .IsDependeeOf("Restore")
    .Does<Configuration>(config => 
{
    var npmWorkingDirectory = config.Items["NpmWorkingDirectory"];

    var settings = new NpmInstallSettings();
    settings.WorkingDirectory = npmWorkingDirectory;
    settings.LogLevel = NpmLogLevel.Silent;
    settings.RedirectStandardError = false;
    settings.RedirectStandardOutput = false;
    NpmInstall(settings);
});

Task("Npm:Build")
    .IsDependentOn("Npm:Install")
    .Does<Configuration>(config => 
{
    var npmWorkingDirectory = config.Items["NpmWorkingDirectory"];
    var settings = new NpmScriptSettings();
    settings.WorkingDirectory = npmWorkingDirectory;
    settings.LogLevel = NpmLogLevel.Silent;
    settings.RedirectStandardError = false;
    settings.RedirectStandardOutput = false;
    settings.ScriptName = NpmRunScriptName;
    NpmRunScript(settings);
});