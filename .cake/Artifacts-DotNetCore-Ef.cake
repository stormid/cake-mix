#load "Configuration.cake"
#addin nuget:?package=Newtonsoft.Json
using Newtonsoft.Json;

public class EfMigration 
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string SafeName { get; set; }
}

public class EfContext
{
    public string FullName { get; set; }
    public string SafeName { get; set; }
    public string Name { get; set; }
    public string AssemblyQualifiedName { get; set; }
}

IEnumerable<EfContext> GetAllDbContexts(DirectoryPath workingDirectory, string configuration) 
{
    var settings = new ProcessSettings() 
    { 
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true
    };

    settings.Arguments = string.Format("ef dbcontext list --configuration {0} --json", configuration);
    var list = Enumerable.Empty<EfContext>();
    
    using(var process = StartAndReturnProcess("dotnet", settings))
    {
        process.WaitForExit();
        if(process.GetExitCode() == 0)
        {
            try 
            {
                var outputAsJson = string.Join(Environment.NewLine, process.GetStandardOutput());
                list = JsonConvert.DeserializeObject<List<EfContext>>(outputAsJson);
                Verbose("Found {0} Db contexts", list.Count());
            }
            catch(Exception exception)             
            {
                Error("Unable to determine db context's for {0} : {1}", workingDirectory, exception.Message);
            }
        }
    }
    return list.ToList();
}

IEnumerable<EfMigration> GetMigrationsForContext(string dbContext, DirectoryPath workingDirectory, string configuration) 
{
    var settings = new ProcessSettings() 
    { 
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true
    };

    settings.Arguments = string.Format("ef migrations list --configuration {0} --context {1} --json", configuration, dbContext);

    var list = Enumerable.Empty<EfMigration>();
    using(var process = StartAndReturnProcess("dotnet", settings))
    {
        process.WaitForExit();
        if(process.GetExitCode() == 0)
        {
            try
            {
                var outputAsJson = string.Join(Environment.NewLine, process.GetStandardOutput());
                list = JsonConvert.DeserializeObject<List<EfMigration>>(outputAsJson);
            }
            catch(Exception exception)             
            {
                Error("Unable to determine db migration list for {0} : {1}", dbContext, exception.Message);
            }            
        }
    }
    return list;
}

Task("Artifacts:DotNetCore:Ef:Migration-Script")
    .IsDependentOn("Build")
    .IsDependeeOf("Publish")
    .Does<Configuration>(config => 
{
    var efProjects = config.Solution.Projects.ToList();

    Information("Generating scripts for {0} projects", efProjects.Count());
    foreach(var project in efProjects) {
        var assemblyName = config.Solution.GetProjectName(project);
        var workingDirectory = project.ProjectFilePath.GetDirectory();
        var availableDbContexts = GetAllDbContexts(workingDirectory, config.Solution.BuildConfiguration).ToList();

        Information("Generating scripts for {0} containing {1} contexts", assemblyName, availableDbContexts.Count);
        foreach(var dbContext in availableDbContexts) 
        {
            Information("Generating Sql Script for {0}", dbContext.SafeName);
            var migrations = GetMigrationsForContext(dbContext.SafeName, workingDirectory, config.Solution.BuildConfiguration);
            
            var sqlScript = MakeAbsolute(File($"{config.Artifacts.Root}/sql/{dbContext.SafeName}.sql"));
            if(FileExists(sqlScript)) {
                DeleteFile(sqlScript);
            }

            var settings = new ProcessSettings() 
            { 
                WorkingDirectory = workingDirectory
            };

            settings.Arguments = string.Format("ef migrations script -i -o {0} --configuration {1} --context {2}", sqlScript, config.Solution.BuildConfiguration, dbContext.SafeName);

            using(var process = StartAndReturnProcess("dotnet", settings))
            {
                process.WaitForExit();
                Verbose("Exit code: {0}", process.GetExitCode());
            }

            config.Artifacts.Add(ArtifactTypeOption.Other, sqlScript.GetFilename().ToString(), sqlScript);
        }
    }

});