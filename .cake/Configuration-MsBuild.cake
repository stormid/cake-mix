#load "Configuration.cake"
public partial class Configuration {

    public Configuration SetMSBuildToolVersion(MSBuildToolVersion toolVersion, bool allowArgumentOverride = true)
    {
        var argument = ParseEnum<MSBuildToolVersion>(context.Argument("MSBuildToolVersion", "Default"));

        var version = (allowArgumentOverride && argument != MSBuildToolVersion.Default) ? argument : toolVersion;

        this.TaskParameters.Add("MsBuild:Version", version);

        return this;
    }

    private static T ParseEnum<T>(string value)
    {
        return (T) Enum.Parse(typeof(T), value, true);
    }
}
