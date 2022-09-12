using Ardalis.GuardClauses;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
public class RabbbitMQBootstrapperService : IBootstrapperService
{

    public IConfiguration Configuration { get; set; }

    public IConnectionFactory ConnectionFactory { get; set; }

    public IList<IRabbitMQCommand> Commands { get; set; }

    public Task InitializeAsync()
    {
        Guard.Against.Null(this.Configuration, nameof(this.Configuration));
        Guard.Against.Null(this.ConnectionFactory, nameof(this.ConnectionFactory));
        Guard.Against.NullOrEmpty(this.Commands, nameof(this.Commands));

        return Task.CompletedTask;
    }
    public async Task ExecuteAsync()
    {
        if (this.Configuration.GetValue<bool>("boostrap:rabbitmq"))
        {
            using var connection = this.ConnectionFactory.CreateConnection();
            using var model = connection.CreateModel();

            foreach (var command in this.Commands)
            {
                await this.RunAsync(model, command);
            }
        }
    }

    private async Task RunAsync(IModel model, IRabbitMQCommand command)
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
                await rabbitMQHTTPCommand.ExecuteAsync(model);
                break;
            default:
                throw new NotSupportedException($"The Type {command.GetType().Name} is a valid {nameof(IRabbitMQCommand)} but RabbbitMQBootstrapperService does not know this type.");
        }
    }
}
