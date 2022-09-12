using RabbitMQ.Client;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;

public interface IAdminCommand : IRabbitMQCommand
{
    Task PrepareAsync();
    Task ExecuteAsync(IModel model);
}
