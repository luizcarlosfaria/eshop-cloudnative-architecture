using Ardalis.GuardClauses;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
public class RabbbitMQBootstrapperService : IBootstrapperService
{

    public IConfiguration Configuration { get; set; }

    public IConnectionFactory AmqpConnectionFactory { get; set; }

    public System.Net.NetworkCredential HttpApiCredentials { get; set; }

    public string HttpUri { get; set; }

    public IList<IRabbitMQCommand> Commands { get; set; }

    public Task InitializeAsync()
    {
        Guard.Against.Null(this.Configuration, nameof(this.Configuration));
        Guard.Against.NullOrEmpty(this.Commands, nameof(this.Commands));

        if (this.Commands.Any(it => it is IAmqpCommand))
        {
            Guard.Against.Null(this.AmqpConnectionFactory, nameof(this.AmqpConnectionFactory));
            Guard.Against.NullOrEmpty(this.AmqpConnectionFactory.UserName, $"{nameof(this.AmqpConnectionFactory)}.{nameof(this.AmqpConnectionFactory.UserName)}");
            Guard.Against.NullOrEmpty(this.AmqpConnectionFactory.Password, $"{nameof(this.AmqpConnectionFactory)}.{nameof(this.AmqpConnectionFactory.Password)}");
        }

        if (this.Commands.Any(it => it is IAdminCommand))
        {
            Guard.Against.NullOrEmpty(this.HttpUri, nameof(this.HttpUri));
            Guard.Against.Null(this.HttpApiCredentials, nameof(this.HttpApiCredentials));
            Guard.Against.NullOrEmpty(this.HttpApiCredentials.UserName, $"{nameof(this.HttpApiCredentials)}.{nameof(this.HttpApiCredentials.UserName)}");
            Guard.Against.NullOrEmpty(this.HttpApiCredentials.Password, $"{nameof(this.HttpApiCredentials)}.{nameof(this.HttpApiCredentials.Password)}");
        }

        return Task.CompletedTask;
    }

    protected Func<Task<string>> AuthorizationHeaderValueGetter()
        => () => Task.FromResult($"Basic {Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(this.HttpApiCredentials.UserName + ":" + this.HttpApiCredentials.Password))}");

    public async Task ExecuteAsync()
    {
        if (this.Configuration.GetValue<bool>("boostrap:rabbitmq"))
        {
            using IConnection connection = this.AmqpConnectionFactory.CreateConnection();
            using IModel model = connection.CreateModel();
            using IRabbitMQAdminApi api = this.BuildRabbitMQAdminApi();

            foreach (var command in this.Commands)
            {
                await this.RunAsync(model, api, command);
            }
        }
    }

    private IRabbitMQAdminApi BuildRabbitMQAdminApi()
    {
        IRabbitMQAdminApi api = null;
        if (!string.IsNullOrWhiteSpace(this.HttpUri))
        {
            api = RestService.For<IRabbitMQAdminApi>(this.HttpUri, new RefitSettings()
            {
                AuthorizationHeaderValueGetter = this.AuthorizationHeaderValueGetter()
            });
        }
        return api;
    }

    private async Task RunAsync(IModel model, IRabbitMQAdminApi api, IRabbitMQCommand command)
    {
        Guard.Against.Null(command);

        switch (command)
        {
            case IAmqpCommand amqpCommand:
                amqpCommand.Prepare();
                amqpCommand.Execute(model);
                break;
            case IAdminCommand rabbitMQHTTPCommand:
                await rabbitMQHTTPCommand.PrepareAsync();
                await rabbitMQHTTPCommand.ExecuteAsync(api);
                break;
            default:
                throw new NotSupportedException($"The Type {command.GetType().Name} is a valid {nameof(IRabbitMQCommand)} but RabbbitMQBootstrapperService does not know this type.");
        }
    }
}
