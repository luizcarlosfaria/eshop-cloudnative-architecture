﻿using Dawn;
using RabbitMQ.Client;

namespace eShopCloudNative.Architecture.Messaging.Consumer;

public class ConsumerBaseParameters
{
    public string QueueName { get; private set; }
    public ConsumerBaseParameters WithQueueName(string queueName)
    {
        this.QueueName = queueName;
        return this;
    }

    public ushort PrefetchCount { get; private set; }
    public ConsumerBaseParameters WithPrefetchCount(ushort prefetchCount)
    {
        this.PrefetchCount = prefetchCount;
        return this;
    }

    public Func<IConnection> ConnectionFactoryFunc { get; private set; }
    public ConsumerBaseParameters WithConnectionFactoryFunc(Func<IConnection> connectionFactoryFunc)
    {
        this.ConnectionFactoryFunc = connectionFactoryFunc;
        return this;
    }

    public int TestQueueRetryCount { get; private set; }
    public ConsumerBaseParameters WithTestQueueRetryCount(int testQueueRetryCount)
    {
        this.TestQueueRetryCount = testQueueRetryCount;
        return this;
    }

    public TimeSpan DisplayLoopInConsoleEvery { get; private set; }
    public ConsumerBaseParameters WithDisplayLoopInConsoleEvery(TimeSpan timeToDisplay)
    {
        this.DisplayLoopInConsoleEvery = timeToDisplay;
        return this;
    }

    public virtual void Validate()
    {
        Guard.Argument(this.QueueName).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(this.PrefetchCount).NotZero().NotNegative();
        Guard.Argument(this.TestQueueRetryCount).NotNegative();
        Guard.Argument(this.ConnectionFactoryFunc).NotNull();
    }
}
