using eShopCloudNative.Architecture.Extensions;
using eShopCloudNative.Architecture.Tests.Bootstrapp;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests;
public class ConfigurationExtensionsTests
{
    [Fact]
    public void CreateInstanceAndConfigureWithTest()
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("Teste"))
            .Returns(
                new FakeConfigurationSection()
                    .SetKey("Teste")
                    .AddChild(c1 => c1.SetKeyValue(nameof(Teste.Host), "rabbitmq"))
                    .AddChild(c1 => c1.SetKeyValue(nameof(Teste.Port), "5672"))
            );
        var configurationInstance = configurationMock.Object;


        var result = configurationInstance.CreateAndConfigureWith<Teste>("Teste");

        result.Should().NotBeNull();
        result.Host.Should().Be("rabbitmq");
        result.Port.Should().Be(5672);
    }


    [Fact]
    public void GetFlagTrueTest()
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("Teste"))
            .Returns(new FakeConfigurationSection().SetKeyValue("Teste", "true"));
        var configurationInstance = configurationMock.Object;

        var result = configurationInstance.GetFlag("Teste");

        result.Should().BeTrue();
    }

    [Fact]
    public void GetFlagFalseTest()
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("Teste"))
            .Returns(new FakeConfigurationSection().SetKeyValue("Teste", "false"));
        var configurationInstance = configurationMock.Object;

        var result = configurationInstance.GetFlag("Teste");

        result.Should().BeFalse();
    }


    [Fact]
    public void GetFlagNotFoundTest()
    {
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(it => it.GetSection("Teste"))
            .Returns(new FakeConfigurationSection().SetKeyValue("Teste", null));
        var configurationInstance = configurationMock.Object;

        var result = configurationInstance.GetFlag("Teste");

        result.Should().BeFalse();
    }
}

public class Teste
{
    public string Host { get; set; }
    public int Port { get; set; }
}
