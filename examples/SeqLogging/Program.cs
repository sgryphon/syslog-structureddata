﻿using System;
using System.Threading.Tasks;
using DefaultConsoleLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SeqLogging
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder()
                .UseConsoleLifetime()
                .ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    configurationBuilder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory);
                })
                .ConfigureLogging((hostContext, loggingBuilder) =>
                {
                    //loggingBuilder.;
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
}
