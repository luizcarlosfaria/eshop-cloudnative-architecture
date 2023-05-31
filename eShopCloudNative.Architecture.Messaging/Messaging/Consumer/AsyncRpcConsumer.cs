using Dawn;
using eShopCloudNative.Architecture.Messaging;
using eShopCloudNative.Architecture.Messaging.Consumer.Actions;
using eShopCloudNative.Architecture.Messaging.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;

namespace eShopCloudNative.Architecture.Messaging.Consumer;


public class AsyncRpcConsumer<TRequest, TResponse> : AsyncQueueConsumer<TRequest, Task<TResponse>>
    where TResponse : class
    where TRequest : class
{
    public AsyncRpcConsumer(ILogger logger, IConnection connection, IAMQPSerializer serializer, ActivitySource activitySource, string queueName, ushort prefetchCount, Func<TRequest, Task<TResponse>> dispatchFunc) : base(logger, connection, serializer, activitySource, queueName, prefetchCount, dispatchFunc)
    {
    }

    protected override async Task<IAMQPResult> Dispatch(BasicDeliverEventArgs receivedItem, Activity receiveActivity, TRequest request)
    {
        Guard.Argument(receivedItem).NotNull();
        Guard.Argument(receiveActivity).NotNull();
        Guard.Argument(request).NotNull();

        using Activity dispatchActivity = this.activitySource.SafeStartActivity($"{nameof(AsyncRpcConsumer<TRequest, TResponse>)}.Dispatch", ActivityKind.Internal, receiveActivity.Context);

        if (receivedItem.BasicProperties.ReplyTo == null)
        {
            this.logger.LogWarning("Message cannot be processed in RPC Flow because original message didn't have a ReplyTo.");

            return new RejectResult(false);
        }

        TResponse responsePayload = default;

        try
        {
            responsePayload = await this.dispatchFunc(request);
        }
        catch (Exception ex)
        {
            this.SendReply(receivedItem, receiveActivity, ex);

            return new NackResult(false);
        }

        dispatchActivity?.SetEndTime(DateTime.UtcNow);

        this.SendReply(receivedItem, receiveActivity, responsePayload);

        return new AckResult();
    }

    private void SendReply(BasicDeliverEventArgs receivedItem, Activity receiveActivity, TResponse responsePayload)
    {
        if (receivedItem is null) throw new ArgumentNullException(nameof(receivedItem));
        if (responsePayload is null) throw new ArgumentNullException(nameof(responsePayload));

        using Activity replyActivity = this.activitySource.SafeStartActivity($"{nameof(AsyncRpcConsumer<TRequest, TResponse>)}.Reply", ActivityKind.Client, receiveActivity.Context);

        IBasicProperties responseProperties = this.Model.CreateBasicProperties()
                                                        .SetMessageId()
                                                        .SetTelemetry(replyActivity)
                                                        .SetCorrelationId(receivedItem.BasicProperties);

        replyActivity?.AddTag("Queue", receivedItem.BasicProperties.ReplyTo);
        replyActivity?.AddTag("MessageId", responseProperties.MessageId);
        replyActivity?.AddTag("CorrelationId", responseProperties.CorrelationId);

        this.Model.BasicPublish(string.Empty, receivedItem.BasicProperties.ReplyTo, responseProperties, this.serializer.Serialize(responseProperties, responsePayload));

        replyActivity?.SetEndTime(DateTime.UtcNow);
    }

    private void SendReply(BasicDeliverEventArgs receivedItem, Activity receiveActivity, Exception exception)
    {
        if (receivedItem is null) throw new ArgumentNullException(nameof(receivedItem));
        if (exception is null) throw new ArgumentNullException(nameof(exception));

        using Activity replyActivity = this.activitySource.SafeStartActivity($"{nameof(AsyncRpcConsumer<TRequest, TResponse>)}.Reply", ActivityKind.Client, receiveActivity.Context);

        replyActivity?.AddTag("Queue", receivedItem.BasicProperties.ReplyTo);

        IBasicProperties responseProperties = this.Model.CreateBasicProperties()
                                                        .SetMessageId()
                                                        .SetException(exception)
                                                        .SetTelemetry(replyActivity)
                                                        .SetCorrelationId(receivedItem.BasicProperties);

        replyActivity?.AddTag("MessageId", responseProperties.MessageId);

        replyActivity?.AddTag("CorrelationId", responseProperties.CorrelationId);

        this.Model.BasicPublish(string.Empty, receivedItem.BasicProperties.ReplyTo, responseProperties, Array.Empty<byte>());

        replyActivity?.SetEndTime(DateTime.UtcNow);
    }

}
