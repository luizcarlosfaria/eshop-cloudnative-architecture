using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System.Diagnostics.CodeAnalysis;
using eShopCloudNative.Architecture.Extensions;
using Serilog;
using System;

namespace eShopCloudNative.Architecture.Logging;

[ExcludeFromCodeCoverage]
public static class EnterpriseApplicationLogExtensions
{
    public static List<Tag> Add(this List<Tag> tags, string key, object value)
    {
        Guard.Against.Null(tags, nameof(tags));
        Guard.Against.NullOrWhiteSpace(key, nameof(key));
        tags.Add(new Tag(key, value));
        return tags; ;
    }

    public static List<Tag> Remove(this List<Tag> tags, string key)
    {
        Guard.Against.Null(tags, nameof(tags));
        Guard.Against.NullOrWhiteSpace(key, nameof(key));

        return tags.Remove(it => it.Key == key);
    }

    public static List<Tag> Remove(this List<Tag> tags, Func<Tag, bool> predicate)
    {
        Guard.Against.Null(tags, nameof(tags));
        Guard.Against.Null(predicate, nameof(predicate));

        var itensToDelete = tags.Where(predicate).ToArray();
        foreach (var itemToDelete in itensToDelete)
            tags.Remove(itemToDelete);
        return tags;
    }

   
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

    
    public static Action<RabbitMQClientConfiguration, RabbitMQSinkConfiguration> ConfigureRabbitMQ(string configurationKey, HostBuilderContext hostBuilderContext)
        => (clientConfiguration, sinkConfiguration)
            => ConfigureRabbitMQ(configurationKey, hostBuilderContext, clientConfiguration, sinkConfiguration);

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
