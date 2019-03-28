#load "Configuration.cake"

Task("Test:DotNetCore")
    .IsDependentOn("Build")
    .IsDependeeOf("Test")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.TestProjects.Any(p => p.IsDotNetCliTestProject()))
    .Does<Configuration>(config => 
{
    CreateDirectory($"{config.Artifacts.Root}/test-results");

    var shouldFail = false;
    foreach(var testProject in config.Solution.TestProjects.Where(p => p.IsDotNetCliTestProject())) {
        var assemblyName = config.Solution.GetProjectName(testProject);
        var testResultsRoot = $"{config.Artifacts.Root}/test-results";
        var testResultsXml = $"{testResultsRoot}/{assemblyName}.xml";
        try 
        {
            var settings = new DotNetCoreTestSettings() {
                Configuration = config.Solution.BuildConfiguration,
                Logger = $"trx;LogFileName={testResultsXml}",
                NoBuild = true,
                NoRestore = true
            };

            DotNetCoreTest(testProject.ProjectFilePath.ToString(), settings);
        } 
        catch
        {
            shouldFail = true;
        }
    }

    Information("Publishing Test results from {0}", config.Artifacts.Root);
    var testResults = GetFiles($"{config.Artifacts.Root}/test-results/**/*.xml").ToArray();
    if(testResults.Any()) 
    {
        if((BuildSystem.IsRunningOnVSTS || TFBuild.IsRunningOnTFS)) 
        {
            TFBuild.Commands.PublishTestResults(new TFBuildPublishTestResultsData() {
                Configuration = config.Solution.BuildConfiguration,
                MergeTestResults = true,
                TestResultsFiles = testResults,
                TestRunner = TFTestRunnerType.VSTest
            });    
        }
    }

    if(shouldFail)
    {
        throw new Exception("Tests have failed");
    }
});
