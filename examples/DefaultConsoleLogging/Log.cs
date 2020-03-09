using System;
using Microsoft.Extensions.Logging;

namespace Examples
{
    internal static class Log
    {
        public static readonly Action<ILogger, int, Exception?> ErrorProcessingCustomer =
            LoggerMessage.Define<int>(LogLevel.Error,
                new EventId(5000, nameof(ErrorProcessingCustomer)),
                "Unexpected error processing customer {CustomerId}.");

        public static readonly Action<ILogger, Guid, Exception?> ProcessOrderItem =
            LoggerMessage.Define<Guid>(LogLevel.Information,
                new EventId(1000, nameof(ProcessOrderItem)),
                "Processing order item {ItemId}.");
    }
}
