using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Syslog.StructuredData;

namespace DefaultConsoleLogging
{
    public class Program
    {
        public static Random Random = new Random();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder()
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });

        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }
    }
    
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(() =>
            {
                using (_logger.BeginScope(new StructuredData("example@32473",
                    new Dictionary<string, object> {["id"] = 5, ["name"] = "cafe", ["x"] = DateTimeOffset.Now })))
                {
                    using (_logger.BeginScope(new StructuredData(new Dictionary<string, object>
                    {
                        ["hex"] = new byte[] {0x1a, 0x2b}, ["day"] = new DateTime(2020, 1, 2)
                    })))
                    {
                        Log.WorkerExecuted(_logger, Guid.NewGuid(), null);
                    }
                }
            }, stoppingToken).ConfigureAwait(true);
        }
    }
    
    internal static class Log
    {
        public static readonly Action<ILogger, Guid, Exception?> WorkerExecuted =
            LoggerMessage.Define<Guid>(LogLevel.Information,
                new EventId(1000, nameof(WorkerExecuted)),
                "Worked executed {WorkerId}");
    }
}
