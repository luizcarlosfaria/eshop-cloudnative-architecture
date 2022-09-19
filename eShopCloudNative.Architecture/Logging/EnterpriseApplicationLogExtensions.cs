using Dawn;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System.Diagnostics.CodeAnalysis;
using eShopCloudNative.Architecture.Extensions;
using Serilog;
using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace eShopCloudNative.Architecture.Logging;

[ExcludeFromCodeCoverage]
public static class EnterpriseApplicationLogExtensions
{
    public static List<Tag> Add(this List<Tag> tags, string key, object value)
    {
        Guard.Argument(tags, nameof(tags)).NotNull();
        Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();
        tags.Add(new Tag(key, value));
        return tags;
    }

    public static List<Tag> Remove(this List<Tag> tags, string key)
    {
        Guard.Argument(tags, nameof(tags)).NotNull();
        Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();

        return tags.Remove(it => it.Key == key);
    }

    public static List<Tag> Remove(this List<Tag> tags, Func<Tag, bool> predicate)
    {
        Guard.Argument(tags, nameof(tags)).NotNull();
        Guard.Argument(predicate, nameof(predicate)).NotNull();

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
            .WriteTo.RabbitMQ(ConfigureSerilogWithRabbitMQ(configurationKey, hostBuilderContext))
            .WriteTo.Console();
        });
    }

    
    public static Action<RabbitMQClientConfiguration, RabbitMQSinkConfiguration> ConfigureSerilogWithRabbitMQ(string configurationKey, HostBuilderContext hostBuilderContext)
        => (clientConfiguration, sinkConfiguration)
            => ConfigureSerilogWithRabbitMQ(configurationKey, hostBuilderContext, clientConfiguration, sinkConfiguration);

    public static void ConfigureSerilogWithRabbitMQ(string configurationKey, HostBuilderContext hostBuilderContext, RabbitMQClientConfiguration clientConfiguration, RabbitMQSinkConfiguration sinkConfiguration)
    {
        hostBuilderContext.Configuration
                        .ConfigureWith(configurationKey, clientConfiguration)
                        .Validate();

        sinkConfiguration.TextFormatter = new JsonFormatter(closingDelimiter: null, renderMessage: true, formatProvider: null);
    }

    public static void Validate(this RabbitMQClientConfiguration clientConfiguration)
    {
        Guard.Argument(clientConfiguration.Hostnames, nameof(clientConfiguration.Hostnames)).NotNull().NotEmpty();
        Guard.Argument(clientConfiguration.VHost, nameof(clientConfiguration.VHost)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.Username, nameof(clientConfiguration.Username)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.Password, nameof(clientConfiguration.Password)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.Exchange, nameof(clientConfiguration.Exchange)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.ExchangeType, nameof(clientConfiguration.ExchangeType)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.RouteKey, nameof(clientConfiguration.RouteKey)).NotNull();
    }
}
