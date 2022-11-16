using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using System;

namespace WebApiObservability.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MetricController : ControllerBase
    {

        private static readonly Counter ControllerGetCount = 
                Metrics.CreateCounter("metrics_controller_get_count", "Number of GET requests.");

        private static readonly Gauge JobsInQueue =
                Metrics.CreateGauge("jobs_queued", "Number of jobs waiting for processing in the queue.");

        private static readonly Histogram OrderValueHistogram = 
            Metrics.CreateHistogram("myapp_order_value_usd", "Histogram of received order values (in USD).",
                new HistogramConfiguration
                {
                    // We divide measurements in 10 buckets of 100 each, up to 1000.
                    Buckets = Histogram.LinearBuckets(start: 100, width: 100, count: 10)
                });

        public MetricController() { }

        [HttpGet("")]
        public IActionResult Get()
        {
            ControllerGetCount.Inc();
            
            JobsInQueue.Set(DateTime.Now.Millisecond);
            
            OrderValueHistogram.Observe(323);

            return Ok("Metrics reported.");
        }
    }
}
