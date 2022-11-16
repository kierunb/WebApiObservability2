using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHealthChecksUI()
    .AddInMemoryStorage();

var app = builder.Build();

app.MapGet("/", () => "WatchDog Portal. Visit: /healthchecks-ui");

app.UseRouting()
   .UseEndpoints(config => config.MapHealthChecksUI()); // /healthchecks-ui

app.Run();
