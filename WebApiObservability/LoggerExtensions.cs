using WebApiObservability.Controllers;

internal static class LoggerExtensions
{
    // High-performance logging
    // https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging

    private static readonly Action<ILogger, Exception> failedToExecuteLoggingController =
        LoggerMessage.Define(
            LogLevel.Critical,
            new EventId(13, nameof(LoggingController)),
            "Epic failure processing item!");

    public static void FailedToExecuteLoggingController(this ILogger logger, LoggingController loggingController) => failedToExecuteLoggingController(logger, default!);
}