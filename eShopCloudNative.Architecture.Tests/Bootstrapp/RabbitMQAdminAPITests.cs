﻿using eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests.Bootstrapp;


public class RabbitMQAdminAPITests
{
    private static IRabbitMQAdminAPI BuildApi()
    {
        var services = new ServiceCollection();
        services
            .AddRefitClient<IRabbitMQAdminAPI>()
            .ConfigureHttpClient(c =>
            {
                c.BaseAddress = new Uri("http://localhost:15672/");
                c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes("guest:guest")));
            });

        return services.BuildServiceProvider().GetRequiredService<IRabbitMQAdminAPI>();
    }

    /*
     var authHeader = "Basic " + Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
    var baseAddress = "https://my.test.net";
     */

    //[Fact]
    [Fact(Skip = "Integration")]
    public async Task GetVirtualHostAsync()
    {
        IRabbitMQAdminAPI api = BuildApi();

        var vhosts = await api.GetVirtualHostsAsync();
    }

    //[Fact]
    [Fact(Skip = "Integration")]
    public async Task CreateVirtualHostAsync()
    {
        IRabbitMQAdminAPI api = BuildApi();

        await api.CreateVirtualHostAsync("vhost1");

        await api.CreateUserAsync("userA", new CreateUserRequest() { Password = "userA", Tags = "administrator" });

        await api.SetUserVirtualHostPermissionsAsync("vhost1", "userA", new VhostPermission().FullAccessAll());

    }
}