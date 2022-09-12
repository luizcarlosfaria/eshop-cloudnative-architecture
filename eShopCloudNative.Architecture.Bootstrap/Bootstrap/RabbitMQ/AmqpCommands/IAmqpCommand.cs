using RabbitMQ.Client;

namespace eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;

public interface IAmqpCommand : IRabbitMQCommand
{
    void Prepare();
    void Execute(IModel model);
}
