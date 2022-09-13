using eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AdminCommands;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ.AmqpCommands;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace eShopCloudNative.Architecture.Tests.Bootstrapp;
public class RabbbitMQBootstrapperServiceTests
{

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
            AmqpConnectionFactory = connectionFactoryInstance,
            Commands = new List<IRabbitMQCommand>(){ new Mock<IRabbitMQCommand>().Object },
        };
        await src0.InitializeAsync();

        var src1 = new RabbbitMQBootstrapperService()
        {
            Configuration = null ,
            AmqpConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
        };
        await Assert.ThrowsAsync<ArgumentNullException>(() => src1.InitializeAsync());

        var src2 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            AmqpConnectionFactory = null,
            Commands = listInstance,
        };
        await Assert.ThrowsAsync<ArgumentException>(() => src2.InitializeAsync());

        var src3 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            AmqpConnectionFactory = connectionFactoryInstance,
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
            AmqpConnectionFactory = connectionFactoryInstance,
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
        connectionFactoryMock.Setup(it => it.UserName).Returns("u");
        connectionFactoryMock.Setup(it => it.Password).Returns("p");
        var connectionFactoryInstance = connectionFactoryMock.Object;

        var IAmqpCommandMock = new Mock<IAmqpCommand>();
        var IAmqpCommandInstance = IAmqpCommandMock.Object;

        var IAdminCommandMock = new Mock<IAdminCommand>();
        var IAdminCommandInstance = IAdminCommandMock.Object;


        var listInstance = new List<IRabbitMQCommand>(){
            IAmqpCommandInstance,
            IAdminCommandInstance
        };

        var src1 = new RabbbitMQBootstrapperService()
        {
            Configuration = configurationInstance ,
            AmqpConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
            HttpUri = "http://localhost:15672",
            HttpApiCredentials = new System.Net.NetworkCredential("u","p")
        };
        await src1.InitializeAsync();
        await src1.ExecuteAsync();

        IAmqpCommandMock.Verify(it => it.Prepare(), Times.Once());
        IAmqpCommandMock.Verify(it => it.Execute(modelInstance), Times.Once());

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
            AmqpConnectionFactory = connectionFactoryInstance,
            Commands = listInstance,
            HttpUri = "http://localhost:15672",
            HttpApiCredentials= new System.Net.NetworkCredential("u","p")
        };
        await src1.ExecuteAsync();

        commandMock.Verify(it => it.PrepareAsync(), Times.Once());
        commandMock.Verify(it => it.ExecuteAsync(It.IsNotNull<IRabbitMQAdminApi>()), Times.Once());

    }

}

