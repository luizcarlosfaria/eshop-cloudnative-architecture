using Dawn;
using eShopCloudNative.Architecture.Messaging;
using eShopCloudNative.Architecture.Messaging.Consumer.Actions;
using eShopCloudNative.Architecture.Messaging.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;

namespace eShopCloudNative.Architecture.Messaging.Consumer;


public class AsyncQueueConsumer<TRequest, TResponse> : ConsumerBase
    where TResponse : Task
    where TRequest : class
{
    protected readonly IAMQPSerializer serializer;
    protected readonly ActivitySource activitySource;
    protected readonly Func<TRequest, TResponse> dispatchFunc;

    #region Constructors 


    public AsyncQueueConsumer(ILogger logger, IConnection connection, IAMQPSerializer serializer, ActivitySource activitySource, string queueName, ushort prefetchCount, Func<TRequest, TResponse> dispatchFunc)
        : base(logger, connection, queueName, prefetchCount)
    {
        this.serializer = Guard.Argument(serializer).NotNull().Value;
        this.activitySource = Guard.Argument(activitySource).NotNull().Value;
        this.dispatchFunc = Guard.Argument(dispatchFunc).NotNull().Value;
    }

    #endregion


    protected override IBasicConsumer BuildConsumer()
    {
        Guard.Argument(this.Model).NotNull();

        var consumer = new AsyncEventingBasicConsumer(this.Model);

        consumer.Received += this.Receive;

        return consumer;
    }

    public async Task Receive(object sender, BasicDeliverEventArgs delivery)
    {
        Guard.Argument(delivery).NotNull();
        Guard.Argument(delivery.BasicProperties).NotNull();

        using Activity receiveActivity = this.activitySource.SafeStartActivity("AsyncQueueServiceWorker.Receive", ActivityKind.Server);
        receiveActivity?.SetParentId(delivery.BasicProperties.GetTraceId(), delivery.BasicProperties.GetSpanId(), ActivityTraceFlags.Recorded);
        receiveActivity?.AddTag("Queue", this.QueueName);
        receiveActivity?.AddTag("MessageId", delivery.BasicProperties.MessageId);
        receiveActivity?.AddTag("CorrelationId", delivery.BasicProperties.CorrelationId);

        IAMQPResult result = this.TryDeserialize(delivery, out TRequest request)
                            ? await this.Dispatch(delivery, receiveActivity, request)
                            : new RejectResult(false);

        result.Execute(this.Model, delivery);

        receiveActivity?.SetEndTime(DateTime.UtcNow);
    }

    private bool TryDeserialize(BasicDeliverEventArgs receivedItem, out TRequest request)
    {
        Guard.Argument(receivedItem).NotNull();

        bool returnValue  = true;

        request = default;
        try
        {
            request = this.serializer.Deserialize<TRequest>(receivedItem);
        }
        catch (Exception exception)
        {
            returnValue = false;

            this.logger.LogWarning("Message rejected during deserialization {exception}", exception);
        }

        return returnValue;
    }

    protected virtual async Task<IAMQPResult> Dispatch(BasicDeliverEventArgs receivedItem, Activity receiveActivity, TRequest request)
    {
        Guard.Argument(receivedItem).NotNull();
        Guard.Argument(receiveActivity).NotNull();
        if (request == null) return new RejectResult(false);

        IAMQPResult returnValue;

        using Activity dispatchActivity = this.activitySource.SafeStartActivity("AsyncQueueServiceWorker.Dispatch", ActivityKind.Internal, receiveActivity.Context);

        try
        {
            await this.dispatchFunc(request);
            returnValue = new AckResult();
        }
        catch (Exception exception)
        {
            this.logger.LogWarning("Exception on processing message {queueName} {exception}", this.QueueName, exception);
            returnValue = new NackResult(false);
        }

        dispatchActivity?.SetEndTime(DateTime.UtcNow);

        return returnValue;
    }
}