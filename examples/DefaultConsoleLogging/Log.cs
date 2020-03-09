using System;
using Microsoft.Extensions.Logging;

namespace DefaultConsoleLogging
{
    internal static class Log
    {
        public static readonly Action<ILogger, Guid, Exception?> ProcessOrderItem =
            LoggerMessage.Define<Guid>(LogLevel.Information,
                new EventId(1000, nameof(ProcessOrderItem)),
                "Processing order item {ItemId}");
    }
}
