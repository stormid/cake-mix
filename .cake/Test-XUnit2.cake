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
        var settings = new XUnit2Settings {
            XmlReportV1 = true,
            ReportName = $"{testProject.AssemblyName}.xml",
            OutputDirectory = $"{config.Artifacts.Root}/test-results",
        };
        
        XUnit2(testProject.ProjectFilePath.ToString(), settings);
    }
});

Task("CI:VSTS:NUnit:PublishTestResults")
    .WithCriteria<Configuration>((ctx, config) => BuildSystem.IsRunningOnVSTS || TFBuild.IsRunningOnTFS)
    .IsDependentOn("Test")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    Information("Publishing Test results from {0}", config.Artifacts.Root);
    var testResults = GetFiles($"{config.Artifacts.Root}/test-results/**/*.xml").Select(file => MakeAbsolute(file).ToString()).ToArray();
    TFBuild.Commands.PublishTestResults(new TFBuildPublishTestResultsData() {
        Configuration = config.Solution.BuildConfiguration,
        MergeTestResults = true,
        TestResultsFiles = testResults,
        TestRunner = TFTestRunnerType.XUnit
    });    
});