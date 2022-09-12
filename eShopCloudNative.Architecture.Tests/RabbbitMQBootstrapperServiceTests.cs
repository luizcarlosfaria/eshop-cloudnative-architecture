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

namespace eShopCloudNative.Architecture.Tests;
public class RabbbitMQBootstrapperServiceTests
{

    [Fact]
    public void RabbbitMQBootstrapperServiceBossalities()
    {
        var configurationMock = new Mock<IConfiguration>();
        var configurationInstance = configurationMock.Object;

        var connectionFactoryMock = new Mock<IConnectionFactory>();
        var connectionFactoryInstance = connectionFactoryMock.Object;

        var listMock = new Mock<IList<IRabbitMQCommand>>();
        var listInstance = listMock.Object;


        var src = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            ConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
        };
        src.Configuration.Should().Be(configurationInstance);
        src.ConnectionFactory.Should().Be(connectionFactoryInstance);
        src.Commands.Should().BeSameAs(listInstance);
    }

    [Fact]
    public async Task RabbbitMQBootstrapperServiceInitializeValidationsAsync()
    {
        var configurationMock = new Mock<IConfiguration>();
        var configurationInstance = configurationMock.Object;

        var connectionFactoryMock = new Mock<IConnectionFactory>();
        var connectionFactoryInstance = connectionFactoryMock.Object;

        var listMock = new Mock<IList<IRabbitMQCommand>>();
        var listInstance = listMock.Object;

        var src0 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            ConnectionFactory = connectionFactoryInstance,
            Commands = new List<IRabbitMQCommand>(){ new Mock<IRabbitMQCommand>().Object },
        };
        await src0.InitializeAsync();

        var src1 = new RabbbitMQBootstrapperService()
        {
            Configuration = null ,
            ConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
        };
        await Assert.ThrowsAsync<ArgumentNullException>(() => src1.InitializeAsync());

        var src2 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            ConnectionFactory = null,
            Commands = listInstance,
        };
        await Assert.ThrowsAsync<ArgumentNullException>(() => src2.InitializeAsync());

        var src3 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            ConnectionFactory = connectionFactoryInstance,
            Commands = null,
        };
        await Assert.ThrowsAsync<ArgumentNullException>(() => src3.InitializeAsync());

    }


    [Fact]
    public async Task RabbbitMQBootstrapperServiceExecuteAsyncValidationsAsync()
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

        var modelMock = new Mock<IModel>();
        var modelInstance = modelMock.Object;

        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(it => it.CreateModel()).Returns(modelInstance);
        var connectionInstance = connectionMock.Object;

        var connectionFactoryMock = new Mock<IConnectionFactory>();
        connectionFactoryMock.Setup(it => it.CreateConnection()).Returns(connectionInstance);
        var connectionFactoryInstance = connectionFactoryMock.Object;

        var rabbitMQCommandMock = new Mock<IRabbitMQCommand>();
        var rabbitMQCommandInstance = rabbitMQCommandMock.Object;

        var listInstance = new List<IRabbitMQCommand>(){ rabbitMQCommandInstance };

        var src1 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            ConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
        };
        await Assert.ThrowsAsync<NotSupportedException>(() => src1.ExecuteAsync());
    }

    [Fact]
    public async Task RabbbitMQBootstrapperServiceExecuteAsyncIAmqpCommandExecutionAsync()
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

        var modelMock = new Mock<IModel>();
        var modelInstance = modelMock.Object;

        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(it => it.CreateModel()).Returns(modelInstance);
        var connectionInstance = connectionMock.Object;

        var connectionFactoryMock = new Mock<IConnectionFactory>();
        connectionFactoryMock.Setup(it => it.CreateConnection()).Returns(connectionInstance);
        var connectionFactoryInstance = connectionFactoryMock.Object;

        var commandMock = new Mock<IAmqpCommand>();
        var commandInstance = commandMock.Object;

        var listInstance = new List<IRabbitMQCommand>(){ commandInstance };

        var src1 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            ConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
        };
        await src1.ExecuteAsync();

        commandMock.Verify(it => it.Prepare(), Times.Once());
        commandMock.Verify(it => it.Execute(modelInstance), Times.Once());

    }


    [Fact]
    public async Task RabbbitMQBootstrapperServiceExecuteAsyncIAdminCommandExecutionAsync()
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

        var modelMock = new Mock<IModel>();
        var modelInstance = modelMock.Object;

        var connectionMock = new Mock<IConnection>();
        connectionMock.Setup(it => it.CreateModel()).Returns(modelInstance);
        var connectionInstance = connectionMock.Object;

        var connectionFactoryMock = new Mock<IConnectionFactory>();
        connectionFactoryMock.Setup(it => it.CreateConnection()).Returns(connectionInstance);
        var connectionFactoryInstance = connectionFactoryMock.Object;

        var commandMock = new Mock<IAdminCommand>();
        var commandInstance = commandMock.Object;

        var listInstance = new List<IRabbitMQCommand>(){ commandInstance };

        var src1 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            ConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
        };
        await src1.ExecuteAsync();

        commandMock.Verify(it => it.PrepareAsync(), Times.Once());
        commandMock.Verify(it => it.ExecuteAsync(modelInstance), Times.Once());

    }

}
