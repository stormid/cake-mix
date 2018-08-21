#load ".cake/Configuration.cake"

Setup<Configuration>(ctx => Configuration.Create(ctx, "./src/*.sln", c => c.Solution.IncludeAsTestProject(p => p.AssemblyName.Contains("Tests"))));

RunTarget(Argument("target", Argument("Target", "Default")));