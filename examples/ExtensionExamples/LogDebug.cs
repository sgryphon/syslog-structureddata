using System;
using Microsoft.Extensions.Logging;

namespace ExtensionsLogging
{
    internal static class LogDebug
    {
        public static readonly Action<ILogger, Guid, Exception?> ExampleDebug =
            LoggerMessage.Define<Guid>(LogLevel.Debug,
                new EventId(6500, nameof(ExampleDebug)),
                "Example debug message {ItemId}.");
    }
}
