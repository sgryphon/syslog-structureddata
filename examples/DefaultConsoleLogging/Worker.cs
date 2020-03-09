using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Syslog;

namespace DefaultConsoleLogging
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var ipAddress = IPAddress.Parse("2001:db8:85a3::8a2e:370:7334");
            var customerId = 12345;
            var orderId = "PO-56789";
            var dueDate = new DateTime(2020, 1, 2);
            
            using (_logger.BeginScope(new StructuredData
            {
                Id = "origin", ["ip"] = ipAddress
            }))
            {
                using (_logger.BeginScope(new StructuredData
                {
                    ["CustomerId"] = customerId, ["OrderId"] = orderId, ["DueDate"] = dueDate 
                }))
                {
                    for (var i = 0; i < 4; i++)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken).ConfigureAwait(false);
                        Log.ProcessOrderItem(_logger, Guid.NewGuid(), null);
                    }
                }
            }
        }
    }
}
