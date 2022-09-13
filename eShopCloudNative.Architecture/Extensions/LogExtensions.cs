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
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;
public static class LogExtensions
{
    [ExcludeFromCodeCoverage]
    public static void AddEnterpriseApplicationLog(this ConfigureHostBuilder host, string configurationKey)
    {
        host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
        {
            loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.FromGlobalLogContext()
            .WriteTo.RabbitMQ(ConfigureRabbitMQ(configurationKey, hostBuilderContext))
            .WriteTo.Console();
        });
    }

    [ExcludeFromCodeCoverage]
    public static Action<RabbitMQClientConfiguration, RabbitMQSinkConfiguration> ConfigureRabbitMQ(string configurationKey, HostBuilderContext hostBuilderContext)
        => (clientConfiguration, sinkConfiguration) 
            => ConfigureRabbitMQ(configurationKey, hostBuilderContext, clientConfiguration, sinkConfiguration);
    
    [ExcludeFromCodeCoverage]
    public static void ConfigureRabbitMQ(string configurationKey, HostBuilderContext hostBuilderContext, RabbitMQClientConfiguration clientConfiguration, RabbitMQSinkConfiguration sinkConfiguration)
    {
        hostBuilderContext.Configuration
                        .ConfigureWith(configurationKey, clientConfiguration)
                        .Validate();

        sinkConfiguration.TextFormatter = new JsonFormatter(closingDelimiter: null, renderMessage: true, formatProvider: null);
    }

    public static void Validate(this RabbitMQClientConfiguration clientConfiguration)
    {
        Guard.Against.NullOrEmpty(clientConfiguration.Hostnames, nameof(clientConfiguration.Hostnames));
        Guard.Against.NullOrWhiteSpace(clientConfiguration.VHost, nameof(clientConfiguration.VHost));
        Guard.Against.NullOrWhiteSpace(clientConfiguration.Username, nameof(clientConfiguration.Username));
        Guard.Against.NullOrWhiteSpace(clientConfiguration.Password, nameof(clientConfiguration.Password));
        Guard.Against.NullOrWhiteSpace(clientConfiguration.Exchange, nameof(clientConfiguration.Exchange));
        Guard.Against.NullOrWhiteSpace(clientConfiguration.ExchangeType, nameof(clientConfiguration.ExchangeType));
        Guard.Against.Null(clientConfiguration.RouteKey, nameof(clientConfiguration.RouteKey));
    }
}
