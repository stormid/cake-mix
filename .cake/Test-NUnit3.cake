#tool "nuget:?package=NUnit.ConsoleRunner"

#load "Configuration.cake"

Task("Test:NUnit")    
    .IsDependentOn("Build")
    .IsDependeeOf("Test")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.TestProjects.Any())
    .Does<Configuration>(config => 
{
    CreateDirectory($"{config.Artifacts.Root}/test-results");

    foreach(var testProject in config.Solution.TestProjects) {
        var assemblyName = config.Solution.GetProjectName(testProject);
        var testAssembly = $"{testProject.OutputPaths.First()}/{assemblyName}.dll";
        var settings = new NUnit3Settings() {
            NoHeader = true,
            Configuration = config.Solution.BuildConfiguration,
            Results = new[] {
                new NUnit3Result { FileName = $"{config.Artifacts.Root}/test-results/{assemblyName}.xml" }
            },
            OutputFile = $"{config.Artifacts.Root}/test-results/{assemblyName}.log",
        };

        NUnit3(testAssembly, settings);
    }
});

Task("CI:VSTS:NUnit:PublishTestResults")
    .WithCriteria<Configuration>((ctx, config) => BuildSystem.IsRunningOnVSTS || TFBuild.IsRunningOnTFS)
    .IsDependentOn("Test")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    Information("Publishing Test results from {0}", config.Artifacts.Root);
    var testResults = GetFiles($"{config.Artifacts.Root}/test-results/**/*.xml").Select(file => MakeAbsolute(file)).ToArray();
    TFBuild.Commands.PublishTestResults(new TFBuildPublishTestResultsData() {
        Configuration = config.Solution.BuildConfiguration,
        MergeTestResults = true,
        TestResultsFiles = testResults,
        TestRunner = TFTestRunnerType.NUnit
    });    

    var testLogs = GetFiles($"{config.Artifacts.Root}/test-results/**/*.log").Select(file => MakeAbsolute(file).ToString());
    foreach(var log in testLogs) 
    {
        TFBuild.Commands.UploadTaskLogFile(log);
    }
});
