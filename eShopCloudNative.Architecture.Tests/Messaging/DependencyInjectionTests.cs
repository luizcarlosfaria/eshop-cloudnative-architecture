using eShopCloudNative.Architecture.Messaging;
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


public interface IXpto
{

}

public class Class1 : IXpto
{
    private readonly string name;

    public Class1(string name)
    {
        this.name = name;
    }

    public override string ToString() => this.name;
}

public class Class2 : IXpto
{
    private readonly string name;

    public Class2(string name)
    {
        this.name = name;
    }

    public override string ToString() => this.name;


}


public class DependencyInjectionTests
{

    [Fact]
    public void TestMultipleCreation()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddSingleton<IXpto>(new Class1("a"));
        services.AddSingleton<IXpto>(new Class1("b"));
        services.AddSingleton<IXpto>(new Class2("c"));

        var sp = services.BuildServiceProvider();
        var instances = sp.GetServices<IXpto>().ToArray();

        Assert.Equal(3, instances.Length);

    }

    public class Test1_DTO { }
    public class Test1_Service
    {
        public Task Run(Test1_DTO data)
        {
            return Task.CompletedTask;
        }
    }


    [Fact]
    public void TestQuantityOfInstances()
    {
        var mockConnection = new Mock<IConnection>();
        var mockSerializer= new Mock<IAMQPSerializer>();

        ServiceCollection services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(mockConnection.Object);
        services.AddSingleton(mockSerializer.Object);
        services.AddSingleton(new ActivitySource("Teste"));

        services.MapQueue<Test1_Service, Test1_DTO>(cfg => cfg
            .WithAdapter((svc, data) => svc.Run(data))            
            .WithQueueName("fila1")
            .WithPrefetchCount(10)            
        );
        services.MapQueue<Test1_Service, Test1_DTO>(cfg => cfg
            .WithAdapter((svc, data) => svc.Run(data))
            .WithQueueName("fila2")
            .WithPrefetchCount(10)
        );
        
        var sp = services.BuildServiceProvider();
        Assert.Equal(2, sp.GetServices<IHostedService>().ToArray().Length);
        Assert.Equal(2, sp.GetServices<IHostedService>().ToArray().Length);

    }

    [Fact]
    public void TestGracefulShutdown()
    {
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
        ).Returns("aaa");


        var mockConnection = new Mock<IConnection>();
        mockConnection.Setup(it => it.CreateModel()).Returns(mockModel.Object);

        var mockSerializer= new Mock<IAMQPSerializer>();

        var builder = WebApplication.CreateBuilder(new string[]{ });

        builder.Services.AddSingleton(mockConnection.Object);
        builder.Services.AddSingleton(mockSerializer.Object);
        builder.Services.AddSingleton(new ActivitySource("Test"));

        builder.Services.MapQueue<Test1_Service, Test1_DTO>(cfg => cfg
            .WithAdapter((svc, data) => svc.Run(data))
            .WithQueueName("fila1")
            .WithPrefetchCount(10)
        );
        builder.Services.MapQueue<Test1_Service, Test1_DTO>(cfg => cfg
            .WithAdapter((svc, data) => svc.Run(data))
            .WithQueueName("fila2")
            .WithPrefetchCount(11)
        );
        var app = builder.Build();

        CancellationTokenSource source = new CancellationTokenSource();
        CancellationToken token = source.Token;

        var webserverTask = app.RunAsync(token);

        var delayTask = Task.Run(async () =>
        {
            await Task.Delay(5_000);

            source.Cancel();
        });

        Task.WaitAll(webserverTask, delayTask);

        Assert.Equal(2, mockModel.Invocations.Count(it => it.Method.Name == nameof(IModel.BasicConsume)));

        Assert.Equal(2, mockModel.Invocations.Count(it => it.Method.Name == nameof(IModel.BasicCancelNoWait)));
        
        Assert.Equal(4, mockConnection.Invocations.Count(it => it.Method.Name == nameof(IConnection.CreateModel)));

    }
}
