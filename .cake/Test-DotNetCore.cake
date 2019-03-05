#load "Configuration.cake"

Task("Test:DotNetCore")
    .IsDependentOn("Build")
    .IsDependeeOf("Test")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.TestProjects.Any())
    .Does<Configuration>(config => 
{        
    foreach(var testProject in config.Solution.TestProjects) {
        var assemblyName = config.Solution.GetProjectName(testProject);
        var settings = new DotNetCoreTestSettings() {
            Configuration = config.Solution.BuildConfiguration,
            Logger = $"trx;LogFileName={config.Artifacts.Root}/test-results/{assemblyName}.xml",
            NoBuild = true,
            NoRestore = true
        };

        DotNetCoreTest(testProject.ProjectFilePath.ToString(), settings);
    }
});

Task("CI:VSTS:VSTest:PublishTestResults")
    .WithCriteria<Configuration>((ctx, config) => BuildSystem.IsRunningOnVSTS || TFBuild.IsRunningOnTFS)
    .IsDependentOn("Test")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    Information("Publishing Test results from {0}", config.Artifacts.Root);
    var testResults = GetFiles($"{config.Artifacts.Root}/test-results/**/*.xml").Select(file => MakeAbsolute(file)).ToArray();
    if(testResults.Any()) 
    {
        TFBuild.Commands.PublishTestResults(new TFBuildPublishTestResultsData() {
            Configuration = config.Solution.BuildConfiguration,
            MergeTestResults = true,
            TestResultsFiles = testResults,
            TestRunner = TFTestRunnerType.VSTest
        });    
    }
    else
    {
        Warning("No test results to publish");
    }
});
