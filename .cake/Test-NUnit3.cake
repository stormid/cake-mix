#tool "nuget:?package=NUnit.ConsoleRunner"

#load "Configuration.cake"

Task("Test:NUnit")    
    .IsDependentOn("Build")
    .IsDependeeOf("Test")
    .WithCriteria<Configuration>((ctx, config) => config.Solution.TestProjects.Any())
    .Does<Configuration>(config => 
{
    CreateDirectory($"{config.Artifacts.Root}/test-results");

    var shouldFail = false;
    foreach(var testProject in config.Solution.TestProjects) {
        var assemblyName = config.Solution.GetProjectName(testProject);
        var testResults = $"{config.Artifacts.Root}/test-results";
        var testResultsXml = $"{testResults}/{assemblyName}.xml";
        try 
        {
            var settings = new NUnit3Settings() {
                NoHeader = true,
                Configuration = config.Solution.BuildConfiguration,
                Results = new[] {
                    new NUnit3Result { FileName = testResultsXml }
                },
                OutputFile = $"{testResults}/{assemblyName}.log",
            };

            NUnit3(testAssembly, settings);
        } 
        catch
        {
            shouldFail = true;
        }
    }

    Information("Publishing Test results from {0}", config.Artifacts.Root);
    var testResults = GetFiles($"{config.Artifacts.Root}/test-results/**/*.xml").Select(file => MakeAbsolute(file)).ToArray();
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
