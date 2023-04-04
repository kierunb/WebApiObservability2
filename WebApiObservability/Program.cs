using Examples.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using System.Diagnostics.Metrics;
using System.Reflection.PortableExecutable;

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

string serviceName = "WebApiObservability";
Uri oltpUri = new Uri("http://localhost:4317");

// Build a resource configuration action to set service information.
Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: serviceName,
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
    serviceInstanceId: Environment.MachineName);

// Create a service to expose ActivitySource, and Metric Instruments
// for manual instrumentation
builder.Services.AddSingleton<Instrumentation>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(builder =>
    {
        builder
            .AddSource(serviceName)
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter()
            //.AddZipkinExporter()
            .AddOtlpExporter(opts => { opts.Endpoint = oltpUri; });    // for jeager with OLTP endpoint
    })
    .WithMetrics(builder =>
    {
        // Ensure the TracerProvider subscribes to any custom ActivitySources.
        builder
            .AddMeter(Instrumentation.MeterName)
            .SetExemplarFilter(new TraceBasedExemplarFilter())
            .AddRuntimeInstrumentation()
            .AddProcessInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();

        builder
            .AddConsoleExporter()
            .AddPrometheusExporter()
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = oltpUri;
            });
    });


// Clear default logging providers used by WebApplication host.
builder.Logging.ClearProviders();

// Configure OpenTelemetry Logging.
builder.Logging.AddOpenTelemetry(options =>
{
    // Note: See appsettings.json Logging:OpenTelemetry section for configuration.

    var resourceBuilder = ResourceBuilder.CreateDefault();
    configureResource(resourceBuilder);
    options.SetResourceBuilder(resourceBuilder);

    options.AddOtlpExporter(otlpOptions =>
    {

        otlpOptions.Endpoint = oltpUri;
    });

    options.AddConsoleExporter();

});



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

//app.UseHttpMetrics(); // prometheus-net config

app.UseAuthorization();

app.MapControllers();

app.UseOpenTelemetryPrometheusScrapingEndpoint();

// prometheus-net config for third-party metrics tools
//app.UseEndpoints(endpoints =>
//{
//    // Enable the /metrics page to export Prometheus metrics.
//    // Open http://localhost:5058/metrics to see the metrics.
//    //
//    // Metrics published in this sample:
//    // * built-in process metrics giving basic information about the .NET runtime (enabled by default)
//    // * metrics from .NET Event Counters (enabled by default)
//    // * metrics from .NET Meters (enabled by default)
//    // * metrics about requests made by registered HTTP clients used in SampleService (configured above)
//    // * metrics about requests handled by the web app (configured above)
//    // * ASP.NET health check statuses (configured above)
//    endpoints.MapMetrics(); // endpoint: '/metrics'
//});

app.Run();