#load "Configuration.cake"

public partial class Configuration {
    public MSBuildToolVersion MSBuildToolVersion { get; private set; } = MSBuildToolVersion.Default;

    public Configuration SetMSBuildToolVersion(MSBuildToolVersion toolVersion, bool allowArgumentOverride = true){

        var argument = ParseEnum<MSBuildToolVersion>(context.Argument("MSBuildToolVersion", "Default"));
        if(allowArgumentOverride && argument != MSBuildToolVersion.Default){
            MSBuildToolVersion = argument;
            return this;
        }

        MSBuildToolVersion = toolVersion;
        return this;
    }

    private static T ParseEnum<T>(string value)
    {
        return (T) Enum.Parse(typeof(T), value, true);
    }
}