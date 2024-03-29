﻿using eShopCloudNative.Architecture.Bootstrap;
using eShopCloudNative.Architecture.Extensions;
using eShopCloudNative.Architecture.Minio;
using Spring.Context.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using static eShopCloudNative.Architecture.Extensions.SpringExtensions;

namespace eShopCloudNative.Architecture.Tests.Bootstrapp;
public class SpringTests
{
    [Fact]
    public void ObjectContainerTests()
    {
        ObjectContainer.Define("a").Should().Be("a");

        var item = new object();

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
