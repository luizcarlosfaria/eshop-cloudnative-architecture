using eShopCloudNative.Architecture.Bootstrap;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
using eShopCloudNative.Architecture.Extensions;
using eShopCloudNative.Architecture.Minio;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static eShopCloudNative.Architecture.Extensions.SpringExtensions;

namespace eShopCloudNative.Architecture.Tests.Bootstrapp;
public class RabbbitMQBootstrapperCommandsTests
{
    private RabbbitMQBootstrapperService Build(Mock<IModel> modelMock, IRabbitMQCommand command)
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("boostrap:rabbitmq"))
            .Returns(new FakeIConfigurationSection()
            {
                Key = "boostrap:rabbitmq",
                Value = "true"
            });
        var configurationInstance = configurationMock.Object;


        var modelInstance = modelMock.Object;

        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(it => it.CreateModel()).Returns(modelInstance);
        var connectionInstance = connectionMock.Object;

        var connectionFactoryMock = new Mock<IConnectionFactory>();
        connectionFactoryMock.Setup(it => it.CreateConnection()).Returns(connectionInstance);
        var connectionFactoryInstance = connectionFactoryMock.Object;

        var commandMock = new Mock<IAdminCommand>();
        var commandInstance = commandMock.Object;

        var listInstance = new List<IRabbitMQCommand>(){ command };

        return new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance,
            AmqpConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
        };

    }

    [Fact]
    public void ExchangeBindCommandExecution()
    {
        var modelMock = new Mock<IModel>();

        var command = new ExchangeBindCommand()
        {
            Destination = "destination",
            Source = "source",
            RoutingKey = "routingKey",
            Arguments = null,
        };

        command.Prepare();
        command.Execute(modelMock.Object);

        modelMock.Verify(it => it.ExchangeBind("destination", "source", "routingKey", null), Times.Once());
    }

    [Fact]
    public void ExchangeDeclareCommandExecution()
    {
        var modelMock = new Mock<IModel>();

        var command = new ExchangeDeclareCommand()
        {
            Exchange = "Exchange",
            Type = "Type",
            Durable = true,
            AutoDelete = true,
            Arguments = null,
        };

        command.Prepare();
        command.Execute(modelMock.Object);

        modelMock.Verify(it => it.ExchangeDeclare("Exchange", "Type", true, true, null), Times.Once());
    }

    [Fact]
    public void QueueBindCommandExecution()
    {
        var modelMock = new Mock<IModel>();

        var command = new QueueBindCommand()
        {
            Queue = "Queue",
            Exchange = "Exchange",
            RoutingKey = "RoutingKey",
            Arguments = null,
        };

        command.Prepare();
        command.Execute(modelMock.Object);

        modelMock.Verify(it => it.QueueBind("Queue", "Exchange", "RoutingKey", null), Times.Once());
    }

    [Fact]
    public void QueueDecalreCommandExecution()
    {
        var modelMock = new Mock<IModel>();

        var command = new QueueDeclareCommand()
        {
            Queue = "Queue",
            Durable = true,
            Exclusive = true,
            AutoDelete = true,
            Arguments = null,
        };

        command.Prepare();
        command.Execute(modelMock.Object);

        modelMock.Verify(it => it.QueueDeclare("Queue", true, true, true, null), Times.Once());
    }

    [Fact]
    public async Task CreateUserCommandSucessAsync()
    {
        var modelMock = new Mock<IRabbitMQAdminApi>();

        var command = new CreateUserCommand()
        {
            Credential = new System.Net.NetworkCredential("u", "p"),
            Tags = "administrator",
        };

        await command.PrepareAsync();
        await command.ExecuteAsync(modelMock.Object);

        modelMock.Verify(it => it.CreateUserAsync("u", It.Is<CreateUserRequest>(it => it.Password == "p" && it.Tags == "administrator")), Times.Once());
    }

    [Fact]
    public async Task CreateVhostCommandSucessAsync()
    {
        var modelMock = new Mock<IRabbitMQAdminApi>();

        var command = new CreateVhostCommand()
        {
            Name = "a"
        };

        await command.PrepareAsync();
        await command.ExecuteAsync(modelMock.Object);

        modelMock.Verify(it => it.CreateVirtualHostAsync("a"), Times.Once());
    }

    [Fact]
    public async Task SetUserPermissionCommandSucessAsync()
    {
        var modelMock = new Mock<IRabbitMQAdminApi>();

        var command = new SetUserPermissionCommand()
        {
            Vhost = "Vhost",
            UserName = "UserName",
            ConfigurePattern = "ConfigurePattern",
            WritePattern = "WritePattern",
            ReadPattern = "ReadPattern"
        };

        await command.PrepareAsync();
        await command.ExecuteAsync(modelMock.Object);

        modelMock.Verify(it => it.SetUserVirtualHostPermissionsAsync(
            nameof (SetUserPermissionCommand.Vhost),
            nameof(SetUserPermissionCommand.UserName),
            It.Is<VhostPermission>(
                it => it.Configure == nameof(SetUserPermissionCommand.ConfigurePattern)
                && it.Write == nameof(SetUserPermissionCommand.WritePattern)
                && it.Read == nameof(SetUserPermissionCommand.ReadPattern)
            )
            ), Times.Once());
    }

    [Fact]
    public void VhostPermissionTest()
    {
        new VhostPermission().ConfitgureAll().Configure.Should().Be(".*");
        new VhostPermission().WriteAll().Write.Should().Be(".*");
        new VhostPermission().ReadAll().Read.Should().Be(".*");

        var full = new VhostPermission().FullAccessAll();
        full.Configure.Should().Be(".*");
        full.Write.Should().Be(".*");
        full.Write.Should().Be(".*");
    }

}
