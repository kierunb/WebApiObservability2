using Examples.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace WebApiObservability.Controllers;

// Example of using logs, traces and metrics via OpenTelemtry libraries

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private static readonly HttpClient HttpClient = new();

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly Instrumentation? _instrumentation;
    private readonly ActivitySource _activitySource;
    private readonly Counter<long> _freezingDaysCounter;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, Instrumentation instrumentation)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ArgumentNullException.ThrowIfNull(instrumentation);
        _activitySource = instrumentation.ActivitySource;
        _freezingDaysCounter = instrumentation.FreezingDaysCounter;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        using var scope = _logger.BeginScope("{Id}", Guid.NewGuid().ToString("N"));

        var res = HttpClient.GetStringAsync("http://example.com").Result;

        using var activity = _activitySource.StartActivity("calculate forecast");

        var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateTime.Now.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        _freezingDaysCounter.Add(forecast.Count(f => f.TemperatureC < 0));

        _logger.LogInformation(
            "WeatherForecasts generated {count}: {forecasts}",
            forecast.Length,
            forecast);

        _logger.LogWarning("WeatherForecasts warning");

        return forecast;
    }
}