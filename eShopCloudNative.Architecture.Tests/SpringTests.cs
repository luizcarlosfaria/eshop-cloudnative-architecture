using eShopCloudNative.Architecture.Bootstrap;
using eShopCloudNative.Architecture.Bootstrap.Minio;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static eShopCloudNative.Architecture.Bootstrap.Extensions.SpringExtensions;

namespace eShopCloudNative.Architecture.Tests;
public class SpringTests
{
    [Fact]
    public void ObjectContainerTests()
    {
        ObjectContainer.Define("a").Should().Be("a");

        var item = new Object();

        ObjectContainer.Define(item).Should().Be(item);
    }

    [Fact]
    public void RegisterInstanceTests()
    {
        new CodeConfigApplicationContext()
            .RegisterInstance("texto", "a")
            .GetObject<string>("texto")
            .Should()
            .Be("a");
    }

    [Fact]
    public void CreateChildContextTests()
    {
        var policy = new PublicPolicy() { BucketName = "teste" };

        new CodeConfigApplicationContext()
            .RegisterInstance("policy", policy)
            .CreateChildContext("./bootstrapper.xml")
            .GetObject<MinioBucket>("bucket")
            .Policy
            .Should()
            .Be(policy);
    }
}
