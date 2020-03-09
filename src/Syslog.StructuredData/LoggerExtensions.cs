using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Syslog
{
    public static class LoggerExtensions
    {
        public static IDisposable BeginScope(this ILogger logger, string id,
            IEnumerable<KeyValuePair<string, object>> parameters)
        {
            return logger.BeginScope(new StructuredData(id, parameters));
        }
    }
}
