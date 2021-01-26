#load ".cake/Configuration.cake"

/**********************************************************/
Setup<Configuration>(ctx => 
    Configuration
        .Create(ctx)
        .RunNpmScript("ci", "./src/frontend")
        .WithBicepFile("./deploy/*.bicep")
        .IncludeArtifactCopyTarget("./deploy")
        .IncludeAsEfDbContext(p => p.AssemblyName.EndsWith(".Web"))
);
/**********************************************************/

#load ".cake/CI.cake"

// -- DotNetCore
#load ".cake/Npm-RunScript.cake"
#load ".cake/Build-Bicep.cake"
#load ".cake/Restore-DotNetCore.cake"
#load ".cake/Build-DotNetCore.cake"
#load ".cake/Test-DotNetCore.cake"
#load ".cake/Publish-Zip-DotNetCore.cake"
#load ".cake/Publish-Pack-DotNetCore.cake"
#load ".cake/Artifacts-DotNetCore-Ef.cake"
#load ".cake/Artifacts-Copy.cake"
// -------------

RunTarget(Argument("target", Argument("Target", "Default")));