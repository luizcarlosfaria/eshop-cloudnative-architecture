
using eShopCloudNative.Architecture.Messaging;
using eShopCloudNative.Architecture.Messaging.Consumer;
using eShopCloudNative.Architecture.Messaging.Serialization;
using FluentAssertions.Common;
using k8s.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests.Messaging;


public class AsyncQueueConsumerFlowTests
{
    public class Test1_DTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public interface ITest1_Service
    {
        Task Run(Test1_DTO data);
    }

    public class Test1_Service : ITest1_Service
    {
        public Test1_DTO Data { get; private set; }

        public Task Run(Test1_DTO data)
        {
            this.Data = data;

            return Task.CompletedTask;
        }
    }




    [Fact]
    public async Task TestAckAsync()
    {
        var activitySource = new ActivitySource("Test");

        var mockModel = new Mock<IModel>();
        mockModel.Setup(it =>
            it.BasicConsume(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>())
        ).Returns("consumerTag");

        var mockConnection = new Mock<IConnection>();
        mockConnection.Setup(it => it.CreateModel()).Returns(mockModel.Object);

        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        var service = new Test1_Service();
        services.AddSingleton<ITest1_Service>(service);
        services.AddSingleton(mockConnection.Object);
        services.AddSingleton<IAMQPSerializer>(new NewtonsoftAMQPSerializer(activitySource));
        services.AddSingleton(activitySource);

        services.MapQueue<ITest1_Service, Test1_DTO>(cfg => cfg.WithAdapter((svc, data) => svc.Run(data)).WithQueueName("a").WithPrefetchCount(1));
        
        AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task> asyncQueueConsumer = (AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task>)services.BuildServiceProvider().GetRequiredService<IHostedService>();

        CancellationTokenRegistration registration = new CancellationTokenRegistration();
        CancellationToken cancellationToken = registration.Token;

        await asyncQueueConsumer.StartAsync(cancellationToken);

        var mockBasicProperties = new Mock<IBasicProperties>();

        await asyncQueueConsumer.Receive(this, new RabbitMQ.Client.Events.BasicDeliverEventArgs()
        {
            BasicProperties = mockBasicProperties.Object,
            Body = Encoding.UTF8.GetBytes("""{"id":1, "name": "Luiz"}"""),
            ConsumerTag = "consumerTag",
            DeliveryTag = 77,
        });

        await asyncQueueConsumer.StopAsync(cancellationToken);

        Assert.Equal(1, service.Data.Id);
        Assert.Equal("Luiz", service.Data.Name);

        mockModel.Verify(model => model.BasicAck(77, false));
    }

    [Fact]
    public async Task TestNackAsync()
    {
        var activitySource = new ActivitySource("Test");

        var mockModel = new Mock<IModel>();
        mockModel.Setup(it =>
            it.BasicConsume(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>())
        ).Returns("consumerTag");

        var mockConnection = new Mock<IConnection>();
        mockConnection.Setup(it => it.CreateModel()).Returns(mockModel.Object);

        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        var service = new Test1_Service();
        services.AddSingleton<ITest1_Service>(service);
        services.AddSingleton(mockConnection.Object);
        services.AddSingleton<IAMQPSerializer>(new NewtonsoftAMQPSerializer(activitySource));
        services.AddSingleton(activitySource);

        services.MapQueue<ITest1_Service, Test1_DTO>(cfg => cfg.WithAdapter((svc, data) => throw new InvalidOperationException("falhou")).WithQueueName("a").WithPrefetchCount(1));

        AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task> asyncQueueConsumer = (AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task>)services.BuildServiceProvider().GetRequiredService<IHostedService>();

        CancellationTokenRegistration registration = new CancellationTokenRegistration();
        CancellationToken cancellationToken = registration.Token;

        await asyncQueueConsumer.StartAsync(cancellationToken);

        var mockBasicProperties = new Mock<IBasicProperties>();

        await asyncQueueConsumer.Receive(this, new RabbitMQ.Client.Events.BasicDeliverEventArgs()
        {
            BasicProperties = mockBasicProperties.Object,
            Body = Encoding.UTF8.GetBytes("""{"id":1, "name": "Luiz"}"""),
            ConsumerTag = "consumerTag",
            DeliveryTag = 88,
        });

        await asyncQueueConsumer.StopAsync(cancellationToken);

        mockModel.Verify(model => model.BasicNack(88, false, false));
    }

    [Fact]
    public async Task TestNackRequeueTrueAsync()
    {
        var activitySource = new ActivitySource("Test");

        var mockModel = new Mock<IModel>();
        mockModel.Setup(it =>
            it.BasicConsume(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>())
        ).Returns("consumerTag");

        var mockConnection = new Mock<IConnection>();
        mockConnection.Setup(it => it.CreateModel()).Returns(mockModel.Object);

        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        var service = new Test1_Service();
        services.AddSingleton<ITest1_Service>(service);
        services.AddSingleton(mockConnection.Object);
        services.AddSingleton<IAMQPSerializer>(new NewtonsoftAMQPSerializer(activitySource));
        services.AddSingleton(activitySource);

        services.MapQueue<ITest1_Service, Test1_DTO>(cfg => cfg.WithAdapter((svc, data) => throw new InvalidOperationException("falhou")).WithRequeueOnCrash().WithQueueName("a").WithPrefetchCount(1));

        AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task> asyncQueueConsumer = (AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task>)services.BuildServiceProvider().GetRequiredService<IHostedService>();

        CancellationTokenRegistration registration = new CancellationTokenRegistration();
        CancellationToken cancellationToken = registration.Token;

        await asyncQueueConsumer.StartAsync(cancellationToken);

        var mockBasicProperties = new Mock<IBasicProperties>();

        await asyncQueueConsumer.Receive(this, new RabbitMQ.Client.Events.BasicDeliverEventArgs()
        {
            BasicProperties = mockBasicProperties.Object,
            Body = Encoding.UTF8.GetBytes("""{"id":1, "name": "Luiz"}"""),
            ConsumerTag = "consumerTag",
            DeliveryTag = 99,
        });

        await asyncQueueConsumer.StopAsync(cancellationToken);

        mockModel.Verify(model => model.BasicNack(99, false, true));
    }

    [Fact]
    public async Task TestRejectAsync()
    {
        var activitySource = new ActivitySource("Test");

        var mockModel = new Mock<IModel>();
        mockModel.Setup(it =>
            it.BasicConsume(
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<IDictionary<string, object>>(),
                It.IsAny<IBasicConsumer>())
        ).Returns("consumerTag");

        var mockConnection = new Mock<IConnection>();
        mockConnection.Setup(it => it.CreateModel()).Returns(mockModel.Object);


        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        var service = new Test1_Service();
        services.AddSingleton<ITest1_Service>(service);
        services.AddSingleton(mockConnection.Object);
        services.AddSingleton<IAMQPSerializer>(new NewtonsoftAMQPSerializer(activitySource));
        services.AddSingleton(activitySource);

        services.MapQueue<ITest1_Service, Test1_DTO>(cfg => cfg.WithAdapter((svc, data) => throw new InvalidOperationException("falhou")).WithRequeueOnCrash().WithQueueName("a").WithPrefetchCount(1));

        AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task> asyncQueueConsumer = (AsyncQueueConsumer<ITest1_Service, Test1_DTO, Task>)services.BuildServiceProvider().GetRequiredService<IHostedService>();

        CancellationTokenRegistration registration = new CancellationTokenRegistration();
        CancellationToken cancellationToken = registration.Token;

        await asyncQueueConsumer.StartAsync(cancellationToken);

        var mockBasicProperties = new Mock<IBasicProperties>();

        await asyncQueueConsumer.Receive(this, new RabbitMQ.Client.Events.BasicDeliverEventArgs()
        {
            BasicProperties = mockBasicProperties.Object,
            Body = Encoding.UTF8.GetBytes("""{"id": "aaa", "name": "Luiz"}"""),
            ConsumerTag = "consumerTag",
            DeliveryTag = 99,
        });

        await asyncQueueConsumer.StopAsync(cancellationToken);

        mockModel.Verify(model => model.BasicReject(99, false));
    }
}
