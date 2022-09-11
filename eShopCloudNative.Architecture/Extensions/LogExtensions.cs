using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Configuration;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;
public static class LogExtensions
{
    public static void AddEnterpriseApplicationLog(this ConfigureHostBuilder host, string configurationKey)
    {
        host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
        {
            loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.RabbitMQ((clientConfiguration, sinkConfiguration) =>
            {
                hostBuilderContext.Configuration.ConfigureWith(configurationKey, clientConfiguration);

                Guard.Against.NullOrEmpty(clientConfiguration.Hostnames, nameof(clientConfiguration.Hostnames));
                Guard.Against.NullOrWhiteSpace(clientConfiguration.VHost, nameof(clientConfiguration.VHost));
                Guard.Against.NullOrWhiteSpace(clientConfiguration.Username, nameof(clientConfiguration.Username));
                Guard.Against.NullOrWhiteSpace(clientConfiguration.Password, nameof(clientConfiguration.Password));
                Guard.Against.NullOrWhiteSpace(clientConfiguration.Exchange, nameof(clientConfiguration.Exchange));
                Guard.Against.NullOrWhiteSpace(clientConfiguration.ExchangeType, nameof(clientConfiguration.ExchangeType));
                Guard.Against.Null(clientConfiguration.RouteKey, nameof(clientConfiguration.RouteKey));

                sinkConfiguration.TextFormatter = new JsonFormatter(closingDelimiter: null, renderMessage: true, formatProvider: null);

            })
            .WriteTo.Console();
        });
    }



}
