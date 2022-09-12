using eShopCloudNative.Architecture.Bootstrap.Postgres;
using eShopCloudNative.Architecture.Bootstrap.RabbitMQ;
using eShopCloudNative.Architecture.Minio;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Reflection;

namespace eShopCloudNative.Architecture.Tests;
public class GetsAndSettersTests
{
    [Fact]
    public void CoverageTricksTests()
    {
        TestGetAndSettes<PublicPolicy>();
        TestGetAndSettes<Metadata>();
        TestGetAndSettes<VirtualHost>();
        TestGetAndSettes<User>();
        TestGetAndSettes<CreateUserRequest>();
        TestGetAndSettes<UserVhostPermission>();
        TestGetAndSettes<VhostPermission>();
        TestGetAndSettes<RabbbitMQBootstrapperService>();
        TestGetAndSettes<PostgresBootstrapperService>();
    }

    private static void TestGetAndSettes<T>()
    {
        var type = typeof(T);
        T instance = Activator.CreateInstance<T>();
        var properties = typeof(T).GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty);
        foreach (var property in properties)
        {
            TestPropertyOf(instance, property, () => Guid.NewGuid().ToString());
            TestPropertyOf(instance, property, () => new System.Net.DnsEndPoint("u", 90));
            TestPropertyOf(instance, property, () => 1D);
            TestPropertyOf(instance, property, () => 2L);
            TestPropertyOf(instance, property, () => 3);
            TestPropertyOf(instance, property, () => true);
            TestPropertyOf(instance, property, () => DateTime.Now);
            TestPropertyOf(instance, property, () => TimeSpan.FromSeconds(2));
            TestPropertyOf(instance, property, () => Mock.Of<IConfiguration>());
            TestPropertyOf(instance, property, () => Mock.Of<IConnectionFactory>());
            TestPropertyOf(instance, property, () => Mock.Of<IList<IRabbitMQCommand>>());
            TestPropertyOf<T, System.Net.NetworkCredential>(instance, property);
            TestPropertyOf<T, IList<string>>(instance, property);
            TestPropertyOf<T, Metadata>(instance, property);
        }
    }

    private static void TestPropertyOf<TParent, TProperty>(TParent instance, PropertyInfo property)
        where TProperty : class
    {
        if (property.PropertyType == typeof(TProperty))
        {
            var mockInstance = Mock.Of<TProperty>();
            property.SetValue(instance, mockInstance);
            property.GetValue(instance).Should().Be(mockInstance);
        }
    }

    private static void TestPropertyOf<TParent, TProperty>(TParent instance, PropertyInfo property, Func<TProperty> build)
    {
        if (property.PropertyType == typeof(TProperty))
        {
            var mockInstance = build();
            property.SetValue(instance, mockInstance);
            property.GetValue(instance).Should().Be(mockInstance);
        }
    }
}
