using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using System;
using System.Diagnostics;

namespace WebApiObservability.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TracingController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly ActivitySource source = new("WebApiObservability", "1.0.0");
        public TracingController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("")]
        public async Task<IActionResult> Get()
        {

            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.GetStringAsync("https://example.com/");

            using (Activity? activity = source.StartActivity("Custom Avctivity"))
            {
                activity?.SetTag("parameter 1", "my custom data");
                await Task.Delay(10);
            }

            return Ok($"Response size: {response.Length}");
        }

    }
}
