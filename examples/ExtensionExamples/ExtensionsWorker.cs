using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Syslog;

namespace ExtensionsLogging
{
    public class ExtensionsWorker : BackgroundService
    {
        private readonly ILogger<ExtensionsWorker> _logger;

        public ExtensionsWorker(ILogger<ExtensionsWorker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var userId = 12345;
            var eventId = "XXXAAA";

            using (_logger.BeginScope("userevent@-",
                new Dictionary<string, object> {["UserId"] = userId, ["EventId"] = eventId}))
            {
                LogDebug.ExampleDebug(_logger, Guid.NewGuid(), null);
                await Task.Delay(TimeSpan.FromMilliseconds(1000), stoppingToken);
                Log.ExampleWarning(_logger, Guid.NewGuid(), null);
            }
        }
    }
}
