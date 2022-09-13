using eShopCloudNative.Architecture.Extensions;
using eShopCloudNative.Architecture.Tests.Bootstrapp;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace eShopCloudNative.Architecture.Tests;
public class LogExtensions
{
    [Fact]
    public void CreateInstanceAndConfigureWithTest()
    {
        RabbitMQClientConfiguration rabbitMQClientConfiguration = new RabbitMQClientConfiguration();

        Assert.Throws<ArgumentException>("Hostnames", () => rabbitMQClientConfiguration.Validate());

        rabbitMQClientConfiguration.Hostnames.Add("google.com");

        Assert.Throws<ArgumentException>("VHost", () => rabbitMQClientConfiguration.Validate());

        rabbitMQClientConfiguration.VHost = "teste";

        Assert.Throws<ArgumentException>("Username", () => rabbitMQClientConfiguration.Validate());

        rabbitMQClientConfiguration.Username = "u";

        Assert.Throws<ArgumentException>("Password", () => rabbitMQClientConfiguration.Validate());

        rabbitMQClientConfiguration.Password = "p";

        Assert.Throws<ArgumentException>("Exchange", () => rabbitMQClientConfiguration.Validate());

        rabbitMQClientConfiguration.Exchange = "u";

        Assert.Throws<ArgumentException>("ExchangeType", () => rabbitMQClientConfiguration.Validate());

        rabbitMQClientConfiguration.ExchangeType = "u";

        rabbitMQClientConfiguration.RouteKey = null;

        Assert.Throws<ArgumentNullException>("RouteKey", () => rabbitMQClientConfiguration.Validate());

        rabbitMQClientConfiguration.RouteKey = "a";

        rabbitMQClientConfiguration.Validate();
    }
}
