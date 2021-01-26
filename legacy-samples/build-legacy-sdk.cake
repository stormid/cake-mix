#load ".cake/Configuration.cake"

Setup<Configuration>(ctx => Configuration.Create(ctx, "./src/*.sln", c => c.Solution.IncludeAsTestProject(p => p.AssemblyName.Contains("Tests"))));

#load ".cake/CI.cake"
#load ".cake/Restore-NuGet.cake"
#load ".cake/Build-MsBuild.cake"
#load ".cake/Test-NUnit2.cake"
#load ".cake/Publish-MsBuild-WebDeploy.cake"

RunTarget(Argument("target", Argument("Target", "Default")));