#load "Configuration.cake"

Task("Test:DotNetCore")
    .IsDependentOn("Build")
    .IsDependeeOf("Test")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.TestProjects.Any(p => p.IsDotNetCliTestProject()))
    .Does<Configuration>(config => 
{
    CreateDirectory($"{config.Artifacts.Root}/test-results");
    var testResultsRoot = $"{config.Artifacts.Root}/test-results";

    var shouldFail = false;
    foreach(var testProject in config.Solution.TestProjects.Where(p => p.IsDotNetCliTestProject())) {
        var assemblyName = config.Solution.GetProjectName(testProject);
        var testResultsXml = $"{testResultsRoot}/{assemblyName}.xml";
        try 
        {
            var settings = new DotNetCoreTestSettings() {
                Configuration = config.Solution.BuildConfiguration,
                Loggers = new [] { $"trx;LogFileName={testResultsXml}" },
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

    Information("Publishing Test results from {0}", testResultsRoot);
    var testResults = GetFiles($"{testResultsRoot}/**/*.xml").ToArray();
    if(testResults.Any()) 
    {
        if(BuildSystem.IsRunningOnAzurePipelines || AzurePipelines.IsRunningOnAzurePipelines) 
        {
            AzurePipelines.Commands.PublishTestResults(new AzurePipelinesPublishTestResultsData() {
                Configuration = config.Solution.BuildConfiguration,
                MergeTestResults = true,
                TestResultsFiles = testResults,
                TestRunner = AzurePipelinesTestRunnerType.VSTest
            });    
        }
    }

    if(shouldFail)
    {
        throw new Exception("Tests have failed");
    }
});
