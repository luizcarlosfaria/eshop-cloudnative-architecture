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
    #region Tag Facilities

    public static EnterpriseApplicationLogContext Add(this EnterpriseApplicationLogContext context, string key, object value) => context.Add(key, TagType.None, value);

    public static EnterpriseApplicationLogContext AddArgument(this EnterpriseApplicationLogContext context, string key, object value) => context.Add(key, TagType.Argument, value);

    public static EnterpriseApplicationLogContext AddProperty(this EnterpriseApplicationLogContext context, string key, object value) => context.Add(key, TagType.Property, value);

    public static EnterpriseApplicationLogContext Add(this EnterpriseApplicationLogContext context, string key, TagType tagType, object value)
    {
        Guard.Argument(context, nameof(context)).NotNull();
        Guard.Argument(context.Tags, nameof(context.Tags)).NotNull();
        Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();
        context.Tags.Add(new Tag(key, tagType, value));
        return context;
    }

    public static EnterpriseApplicationLogContext Remove(this EnterpriseApplicationLogContext context, string key)
    {
        Guard.Argument(context, nameof(context)).NotNull();
        Guard.Argument(context.Tags, nameof(context.Tags)).NotNull();
        Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();

        context.Remove(it => it.Key == key);

        return context;
    }

    public static EnterpriseApplicationLogContext Remove(this EnterpriseApplicationLogContext context, Func<Tag, bool> predicate)
    {
        Guard.Argument(context, nameof(context)).NotNull();
        Guard.Argument(context.Tags, nameof(context.Tags)).NotNull();
        Guard.Argument(predicate, nameof(predicate)).NotNull();

        var itensToDelete = context.Tags.Where(predicate).ToArray();

        foreach (var itemToDelete in itensToDelete)
            context.Tags.Remove(itemToDelete);

        return context;
    }

    #endregion

    #region Exception Management


    public static void ExecuteWithLog(this EnterpriseApplicationLogContext context, Action actionToExecute)
    {
        Guard.Argument(context, nameof(context)).NotNull();
        try
        {
            actionToExecute();
        }
        catch (Exception exception)
        {
            context.Exception = exception;
            throw;
        }
    }

    public static async Task ExecuteWithLogAsync(this EnterpriseApplicationLogContext context, Func<Task> actionToExecute)
    {
        Guard.Argument(context, nameof(context)).NotNull();
        try
        {
            await actionToExecute();
        }
        catch (Exception exception)
        {
            context.Exception = exception;
            throw;
        }
    }

    public static T ExecuteWithLogAndReturn<T>(this EnterpriseApplicationLogContext context, Func<T> functionToExecute)
    {
        Guard.Argument(context, nameof(context)).NotNull();
        try
        {
            return functionToExecute();
        }
        catch (Exception exception)
        {
            context.Exception = exception;
            throw;
        }
    }

    public static async Task<T> ExecuteWithLogAndReturnAsync<T>(this EnterpriseApplicationLogContext context, Func<Task<T>> functionToExecute)        
    {
        Guard.Argument(context, nameof(context)).NotNull();
        try
        {
            return await functionToExecute();
        }
        catch (Exception exception)
        {
            context.Exception = exception;
            throw;
        }
    }


    public static IEnumerable<Tag> GetTags(this EnterpriseApplicationLogContext context)
    {
        Guard.Argument(context, nameof(context)).NotNull();
        return context.Tags.ToArray();
    }
    #endregion


    #region Configuration

    public static void AddEnterpriseApplicationLog(this WebApplicationBuilder builder, string configurationKey, Mode mode)
    {
        RabbitMQClientConfiguration rabbitMQClientConfiguration = builder.Configuration.ConfigureWith(configurationKey, new RabbitMQClientConfiguration()).Validate();

        RabbitMQSinkConfiguration rabbitMQSinkConfiguration = new RabbitMQSinkConfiguration();

        if (mode == Mode.Integrated)
        {
            builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) =>
            {
                loggerConfiguration.ConfigureSerilog(rabbitMQClientConfiguration, rabbitMQSinkConfiguration);
            });
        }
        else if (mode == Mode.Standalone)
        {
            Log.Logger = new LoggerConfiguration()
                .ConfigureSerilog(rabbitMQClientConfiguration, rabbitMQSinkConfiguration)
                .CreateLogger();
        }
    }

    private static LoggerConfiguration ConfigureSerilog(this LoggerConfiguration loggerConfiguration, RabbitMQClientConfiguration rabbitMQClientConfiguration, RabbitMQSinkConfiguration rabbitMQSinkConfiguration)
    {
        var formatter = new JsonFormatter(closingDelimiter: null, renderMessage: true, formatProvider: null);

        return loggerConfiguration
                        .Enrich.FromLogContext()
                        .Enrich.FromGlobalLogContext()
                        .WriteTo.RabbitMQ(rabbitMQClientConfiguration, rabbitMQSinkConfiguration, formatter)
                        .WriteTo.Console();
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

    #endregion
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