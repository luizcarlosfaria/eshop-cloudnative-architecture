using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace eShopCloudNative.Architecture.Messaging.Serialization;

public interface IAMQPSerializer
{
    TResponse Deserialize<TResponse>(BasicDeliverEventArgs eventArgs);

    byte[] Serialize<T>(IBasicProperties basicProperties, T objectToSerialize);
}
