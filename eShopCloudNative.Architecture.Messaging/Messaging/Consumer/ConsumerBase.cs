using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Messaging.Consumer;

public abstract class ConsumerBase : BackgroundService
{
    protected readonly ILogger logger;
    protected readonly IConnection connection;
    protected IBasicConsumer consumer;
    private string consumerTag;

    protected IModel Model { get; private set; }

    public ushort PrefetchCount { get; }

    public string QueueName { get; }

    #region Constructors 

    protected ConsumerBase(ILogger logger, IConnection connection, string queueName, ushort prefetchCount)
    {
        this.logger = logger;
        this.connection = connection;
        this.QueueName = queueName;
        this.PrefetchCount = prefetchCount;
    }


    #endregion

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.Model = this.connection.CreateModel();

        this.Model.BasicQos(0, this.PrefetchCount, false);

        this.consumer = this.BuildConsumer();

        await this.WaitQueueCreationAsync();

        DateTimeOffset startTime = DateTimeOffset.UtcNow;

        this.logger.LogInformation($"Consuming Queue {this.QueueName} since: {startTime}");

        this.consumerTag = this.Model.BasicConsume(
                         queue: this.QueueName,
                         autoAck: false,
                         consumer: consumer);

        while (!stoppingToken.IsCancellationRequested)
        {
            this.logger.LogInformation($"Consuming Queue {this.QueueName} since: {startTime} uptime: {DateTimeOffset.Now - startTime}");
            await Task.Delay(1000, stoppingToken);
        }
    }

    protected virtual async Task WaitQueueCreationAsync()
    {
        await Policy
        .Handle<OperationInterruptedException>()
            .WaitAndRetryAsync(5, retryAttempt =>
            {
                var timeToWait = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                this.logger.LogWarning("Queue {queueName} not found... We will try in {tempo}.", this.QueueName, timeToWait);
                return timeToWait;
            })
            .ExecuteAsync(() =>
            {
                using IModel testModel = this.connection.CreateModel();
                testModel.QueueDeclarePassive(this.QueueName);
                return Task.CompletedTask;
            });
    }

    protected abstract IBasicConsumer BuildConsumer();

    public override void Dispose()
    {
        if (this.Model != null && !string.IsNullOrWhiteSpace(this.consumerTag))
        {
            this.Model.BasicCancelNoWait(this.consumerTag);
        }
        if (this.Model != null)
        {
            this.Model.Dispose();
            this.Model = null;
        }

        base.Dispose();
    }

}
