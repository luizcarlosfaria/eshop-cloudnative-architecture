using Dawn;
using eShopCloudNative.Architecture.Bootstrap.Postgres;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Refit;
using Serilog;
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
        Guard.Argument(this.Configuration, nameof(this.Configuration)).NotNull();
        Guard.Argument(this.Commands, nameof(this.Commands)).NotNull().NotEmpty();

        if (this.Commands.Any(it => it is IAmqpCommand))
        {
            Guard.Argument(this.AmqpConnectionFactory, nameof(this.AmqpConnectionFactory)).NotNull();
            Guard.Argument(this.AmqpConnectionFactory.UserName, $"{nameof(this.AmqpConnectionFactory)}.{nameof(this.AmqpConnectionFactory.UserName)}").NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(this.AmqpConnectionFactory.Password, $"{nameof(this.AmqpConnectionFactory)}.{nameof(this.AmqpConnectionFactory.Password)}").NotNull().NotEmpty().NotWhiteSpace();
        }

        if (this.Commands.Any(it => it is IAdminCommand))
        {
            Guard.Argument(this.HttpUri, nameof(this.HttpUri)).NotNull().NotEmpty();
            Guard.Argument(this.HttpApiCredentials, nameof(this.HttpApiCredentials)).NotNull();
            Guard.Argument(this.HttpApiCredentials.UserName, $"{nameof(this.HttpApiCredentials)}.{nameof(this.HttpApiCredentials.UserName)}").NotNull().NotEmpty().NotWhiteSpace();
            Guard.Argument(this.HttpApiCredentials.Password, $"{nameof(this.HttpApiCredentials)}.{nameof(this.HttpApiCredentials.Password)}").NotNull().NotEmpty().NotWhiteSpace();
        }

        return Task.CompletedTask;
    }

    public async Task ExecuteAsync()
    {
        Log.Information("{svc} Iniciando... ", nameof(RabbbitMQBootstrapperService));

        if (this.Configuration.GetValue<bool>("boostrap:rabbitmq"))
        {
            foreach (var command in this.Commands)
            {
                await this.RunAsync(command);
            }
            Log.Information("{svc} Finalizado com sucesso!!! ", nameof(RabbbitMQBootstrapperService));
        }
        else
        {
            Log.Information("{svc} Bootstrap ignorado por configuração ", nameof(RabbbitMQBootstrapperService));
        }
    }

    private IRabbitMQAdminApi BuildRabbitMQAdminApi()
    {
        IRabbitMQAdminApi api = null;

        if (!string.IsNullOrWhiteSpace(this.HttpUri))
        {
            ServiceCollection services = new ServiceCollection();

            services.AddRefitClient<IRabbitMQAdminApi>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri(this.HttpUri);
                c.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(this.HttpApiCredentials.UserName + ":" + this.HttpApiCredentials.Password))}");
            });

            api = services.BuildServiceProvider().GetRequiredService<IRabbitMQAdminApi>();
        }

        return api;
    }

    private async Task RunAsync(IRabbitMQCommand command)
    {
        Guard.Argument(command).NotNull();

        Log.Information("{svc} Executando Commando {cmdName}", nameof(RabbbitMQBootstrapperService), command.GetType().Name);

        switch (command)
        {
            case IAmqpCommand amqpCommand:
                using (IConnection connection = this.AmqpConnectionFactory.CreateConnection())
                {
                    using IModel model = connection.CreateModel();
                    amqpCommand.Prepare();
                    amqpCommand.Execute(model);
                }
                break;
            case IAdminCommand rabbitMQHTTPCommand:
                using (IRabbitMQAdminApi api = this.BuildRabbitMQAdminApi())
                {
                    await rabbitMQHTTPCommand.PrepareAsync();
                    await rabbitMQHTTPCommand.ExecuteAsync(api);
                }
                break;
            default:
                throw new NotSupportedException($"The Type {command.GetType().Name} is a valid {nameof(IRabbitMQCommand)} but RabbbitMQBootstrapperService does not know this type.");
        }

        Log.Information("{svc} Commando {cmdName} finalizado com sucesso!", nameof(RabbbitMQBootstrapperService), command.GetType().Name);
    }
}
