using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Logging via SEQ

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddSeq();
});

// Health Checks

builder.Services.AddHealthChecks()
    .AddCheck<SampleHealthCheck>(name: "Sample", tags: new[] { "sample" });
builder.Services.AddHealthChecks().AddUrlGroup(new Uri("https://www.example.com"), "Rest API 1");

// OpenTelemetry

var serviceName = builder.Environment.ApplicationName;
var serviceVersion = "1.0.0";

builder.Services.AddOpenTelemetryTracing(
    (builder) => builder
        .AddSource(serviceName)
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(serviceName: serviceName, serviceVersion: serviceVersion))
        .AddAspNetCoreInstrumentation()    
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        //.AddZipkinExporter()
        // Working exporter setting for Jeager
        .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); })
        );

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapHealthChecks("/healthz", new HealthCheckOptions { ResponseWriter = HealtzResponseFormatter.WriteResponse });

app.UseRouting();

app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();

app.UseEndpoints(endpoints =>
{
    // Enable the /metrics page to export Prometheus metrics.
    // Open http://localhost:5058/metrics to see the metrics.
    //
    // Metrics published in this sample:
    // * built-in process metrics giving basic information about the .NET runtime (enabled by default)
    // * metrics from .NET Event Counters (enabled by default)
    // * metrics from .NET Meters (enabled by default)
    // * metrics about requests made by registered HTTP clients used in SampleService (configured above)
    // * metrics about requests handled by the web app (configured above)
    // * ASP.NET health check statuses (configured above)
    endpoints.MapMetrics(); // '/metrics'
});



app.Run();
