
using eShopCloudNative.Architecture.Messaging.Consumer;
using eShopCloudNative.Architecture.Messaging.Serialization;
using FluentAssertions.Common;
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

        Test1_Service service = new Test1_Service();

        var asyncQueueConsumer = new AsyncQueueConsumer<Test1_DTO, Task>(new Mock<ILogger>().Object, mockConnection.Object, new NewtonsoftAMQPSerializer(activitySource), activitySource, "", 1, (request) => service.Run(request));

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

        mockModel.Verify(it => it.BasicAck(77, false));
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

        Test1_Service service = new Test1_Service();

        var asyncQueueConsumer = new AsyncQueueConsumer<Test1_DTO, Task>(new Mock<ILogger>().Object, mockConnection.Object, new NewtonsoftAMQPSerializer(activitySource), activitySource, "", 1, (request) =>
        {
            throw new InvalidOperationException("falhou");
        });

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

        mockModel.Verify(it => it.BasicNack(88, false, false));
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

        Test1_Service service = new Test1_Service();

        var asyncQueueConsumer = new AsyncQueueConsumer<Test1_DTO, Task>(new Mock<ILogger>().Object, mockConnection.Object, new NewtonsoftAMQPSerializer(activitySource), activitySource, "", 1, (request) => service.Run(request));

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

        mockModel.Verify(it => it.BasicReject(99, false));
    }
}
