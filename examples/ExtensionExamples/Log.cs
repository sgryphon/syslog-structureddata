using System;
using Microsoft.Extensions.Logging;

namespace ExtensionsLogging
{
    internal static class Log
    {
        public static readonly Action<ILogger, Guid, Exception?> ExampleWarning =
            LoggerMessage.Define<Guid>(LogLevel.Warning,
                new EventId(3000, nameof(ExampleWarning)),
                "Example warning message {ItemId}.");
    }
}
