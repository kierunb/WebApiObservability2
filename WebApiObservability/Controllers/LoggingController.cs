using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace WebApiObservability.Controllers;

[Route("[controller]")]
[ApiController]
public class LoggingController : ControllerBase
{
	private readonly ILogger<LoggingController> _logger;

	public LoggingController(ILogger<LoggingController> logger)
	{
		_logger = logger;
	}

	[HttpGet("")]
	public IActionResult Get()
	{
		// structured logging, message templates
		_logger.LogInformation("Worker {node} running at: {time}", "node 4", DateTimeOffset.UtcNow);
		
		// event ID
		_logger.LogInformation(AppLogEvents.Details, "Worker {node} running at: {time}", "node 5", DateTimeOffset.UtcNow);

		// LoggerMessage using cacheable delegates
		_logger.FailedToExecuteLoggingController(this);

		return Ok();
	}

	// Logging Exceptions

	private void Test(string id)
	{
		try
		{
			if (id is "none")
			{
				throw new Exception("Default Id detected.");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(
				AppLogEvents.Error, ex,
				"Failed to process iteration: {Id}", id);
		}
	}

}
