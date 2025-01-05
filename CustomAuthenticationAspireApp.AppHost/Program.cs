var builder = DistributedApplication.CreateBuilder(args);

var apiService = builder.AddProject<Projects.CustomAuthenticationAspireApp_ApiService>("apiservice");

builder.AddProject<Projects.CustomAuthenticationAspireApp_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
