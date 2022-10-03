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
    public static List<Tag> Add(this List<Tag> tags, string key, object value) => tags.Add(key, TagType.None, value);

    public static List<Tag> AddArgument(this List<Tag> tags, string key, object value) => tags.Add(key, TagType.Argument, value);

    public static List<Tag> AddProperty(this List<Tag> tags, string key, object value) => tags.Add(key, TagType.Property, value);

    public static List<Tag> Add(this List<Tag> tags, string key, TagType tagType, object value)
    {
        Guard.Argument(tags, nameof(tags)).NotNull();
        Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();
        tags.Add(new Tag(key, tagType, value));
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


    public static void AddEnterpriseApplicationLog(this WebApplicationBuilder builder, string configurationKey, Mode mode)
    {
        if (mode == Mode.Integrated)
        {
            builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
            {
                loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.FromGlobalLogContext()
                .WriteTo.RabbitMQ(ConfigureSerilogWithRabbitMQ(configurationKey, hostBuilderContext))
                .WriteTo.Console();
            });
        }
        else if (mode == Mode.Standalone)
        {
            RabbitMQClientConfiguration rabbitMQClientConfiguration = builder.Configuration.ConfigureWith(configurationKey, new RabbitMQClientConfiguration()).Validate();
            RabbitMQSinkConfiguration rabbitMQSinkConfiguration = new RabbitMQSinkConfiguration();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.FromGlobalLogContext()
                .WriteTo.RabbitMQ(rabbitMQClientConfiguration, rabbitMQSinkConfiguration, new JsonFormatter())
                .WriteTo.Console()
                .CreateLogger();
        }
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

    public static RabbitMQClientConfiguration Validate(this RabbitMQClientConfiguration clientConfiguration)
    {
        Guard.Argument(clientConfiguration.Hostnames, nameof(clientConfiguration.Hostnames)).NotNull().NotEmpty();
        Guard.Argument(clientConfiguration.VHost, nameof(clientConfiguration.VHost)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.Username, nameof(clientConfiguration.Username)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.Password, nameof(clientConfiguration.Password)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.Exchange, nameof(clientConfiguration.Exchange)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.ExchangeType, nameof(clientConfiguration.ExchangeType)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(clientConfiguration.RouteKey, nameof(clientConfiguration.RouteKey)).NotNull();

        return clientConfiguration;
    }
}

public enum Mode
{
    /// <summary>
    /// With this mode Enterprise Application Log will integrate with asp.net, and .NET log will be send to RabbitMQ
    /// </summary>
    Integrated,

    /// <summary>
    /// With this mode Enterprise Application Log will execute in standalone mode, and .NET log will not be send to RabbitMQ.
    /// </summary>
    Standalone

}