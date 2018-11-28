#load ".cake/Configuration.cake"

Setup<Configuration>(Configuration.Create);

#load ".cake/CI.cake"
#load ".cake/Restore-NuGet.cake"
#load ".cake/Build-MsBuild.cake"
#load ".cake/Publish-MsBuild-WebDeploy.cake"

RunTarget(Argument("target", Argument("Target", "Default")));