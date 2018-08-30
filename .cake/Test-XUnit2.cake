#tool "xunit.runner.console"
#load "Configuration.cake"

Task("Test:XUnit2")    
    .IsDependentOn("Build")
    .IsDependeeOf("Test")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.TestProjects.Any())
    .Does<Configuration>(config => 
{
    CreateDirectory($"{config.Artifacts.Root}/test-results");

    foreach(var testProject in config.Solution.TestProjects) {
        var testAssembly = $"{testProject.OutputPaths.First()}/{testProject.AssemblyName}.dll";
        var settings = new XUnit2Settings {
            XmlReport = true,
            ReportName = testProject.AssemblyName,
            OutputDirectory = $"{config.Artifacts.Root}/test-results",
        };
        
        XUnit2(testAssembly, settings);
    }
});

Task("CI:VSTS:XUnit:PublishTestResults")
    .WithCriteria<Configuration>((ctx, config) => BuildSystem.IsRunningOnVSTS || TFBuild.IsRunningOnTFS)
    .IsDependentOn("Test")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    Information("Publishing Test results from {0}", config.Artifacts.Root);
    var testResults = GetFiles($"{config.Artifacts.Root}/test-results/**/*.xml").Select(file => MakeAbsolute(file).ToString()).ToArray();
    if(testResults.Any()) 
    {
        TFBuild.Commands.PublishTestResults(new TFBuildPublishTestResultsData() {
            Configuration = config.Solution.BuildConfiguration,
            MergeTestResults = true,
            TestResultsFiles = testResults,
            TestRunner = TFTestRunnerType.XUnit
        });    
    }
    else
    {
        Warning("No test results to publish");
    }
});
