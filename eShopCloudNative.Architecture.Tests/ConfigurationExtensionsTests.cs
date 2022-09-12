using eShopCloudNative.Architecture.Extensions;
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
            .Returns(new FakeIConfigurationSection()
            {
                Key = "Teste",
                //Value = "true"
                FakeChildren = new List<IConfigurationSection>()
                {
                    new FakeIConfigurationSection()
                    {
                        Key = "Nome",
                        Value = "Gago",
                        FakeChildren = new List<IConfigurationSection>()
                        {

                        }
                    }
                }
            });
        var configurationInstance = configurationMock.Object;


        var result = configurationInstance.CreateInstanceAndConfigureWith<Teste>("Teste");

        result.Should().NotBeNull();
        result.Nome.Should().Be("Gago");
    }
}

public class Teste
{
    public string Nome { get; set; }
}
