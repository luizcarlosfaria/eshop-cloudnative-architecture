using eShopCloudNative.Architecture.Bootstrap;
using eShopCloudNative.Architecture.Minio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Minio;
using Minio.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests;
public class MinioTests
{
    [Fact]
    public void BaseConstructionTests()
    {
        var staticPolicy = new StaticPolicy(){ PolicyText = "T" };
        var publicPolicy = new PublicPolicy() { BucketName = Guid.NewGuid().ToString() };
        var minioBucket = new MinioBucket(){ BucketName = "a", Policy = staticPolicy  };
        var minioBootstrapperService = new MinioBootstrapperService()
        {
            BucketsToCreate = new List<MinioBucket>(){
                minioBucket
            },
            Credentials = new NetworkCredential(),
            ServerEndpoint = new DnsEndPoint("localhost", 22),
            WithSSL = true
        };

        minioBucket.Policy.Should().NotBeNull();
        minioBucket.Policy.Should().Be(staticPolicy);

        minioBootstrapperService.Credentials.Should().NotBeNull();
        minioBootstrapperService.ServerEndpoint.Should().NotBeNull();
        minioBootstrapperService.WithSSL.Should().BeTrue();
        minioBootstrapperService.BucketsToCreate.Should().HaveCount(1);

        minioBucket.BucketName.Should().Be("a");
        minioBucket.Policy.GetJsonPolicy().Should().Be("T");
        publicPolicy.GetJsonPolicy().Should().Contain(publicPolicy.BucketName);
    }

    [Fact]
    public void PublicPolicyTests()
    {
        string key = Guid.NewGuid().ToString("D");

        PublicPolicy publicPolicy = new PublicPolicy(){ BucketName = key };

        publicPolicy.GetJsonPolicy().Should().NotBeNullOrWhiteSpace();

        publicPolicy.GetJsonPolicy().Should().Contain(key);

        Action action = () => new PublicPolicy().GetJsonPolicy();

        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void StaticPolicyTests()
    {
        string key = Guid.NewGuid().ToString("D");

        StaticPolicy publicPolicy = new StaticPolicy(){ PolicyText = key };

        Assert.Equal(key, publicPolicy.GetJsonPolicy());
    }

    [Fact]
    public async Task MinioBootstrapperServiceNoStartTestsAsync()
    {
        string login = "login";
        string password = "password";
        string host = "localhost";
        int port = 22;

        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A"
                }
            },
            Credentials = new NetworkCredential(login, password),
            ServerEndpoint = new DnsEndPoint(host, port),
        };
        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeIConfigurationSection() { 
                Key = "boostrap:minio",
                Value = "false"
            });

        await svc.InitializeAsync();
        await svc.ExecuteAsync();

        svc.IMinioClientAdapterMock.Verify(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()),Times.Never());
    }

    [Fact]
    public async Task MinioBootstrapperServiceNoOldBucketsCreateBucketWithoutPolityTestsAsync()
    {
        string login = "login";
        string password = "password";
        string host = "localhost";
        int port = 22;

        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A"
                }
            },
            Credentials = new NetworkCredential(login, password),
            ServerEndpoint = new DnsEndPoint(host, port),
        };
        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeIConfigurationSection()
            {
                Key = "boostrap:minio",
                Value = "true"
            });
        svc.IMinioClientAdapterMock
            .Setup(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new ListAllMyBucketsResult()
            {
                Buckets = new List<Bucket>()
                {

                }
            }));

        await svc.InitializeAsync();
        await svc.ExecuteAsync();

        svc.IMinioClientAdapterMock.Verify(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()), Times.Once());

        svc.IMinioClientAdapterMock.Verify(it => it.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()), Times.Once());

        svc.IMinioClientAdapterMock.Verify(it => it.SetPolicyAsync(It.IsAny<SetPolicyArgs>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task MinioBootstrapperServiceHasOldBucketsEvictCreateBucketsTestsAsync()
    {
        string login = "login";
        string password = "password";
        string host = "localhost";
        int port = 22;

        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A"
                }
            },
            Credentials = new NetworkCredential(login, password),
            ServerEndpoint = new DnsEndPoint(host, port),
        };
        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeIConfigurationSection()
            {
                Key = "boostrap:minio",
                Value = "true"
            });
        svc.IMinioClientAdapterMock
            .Setup(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new ListAllMyBucketsResult()
            {
                Buckets = new List<Bucket>()
                {
                    new Bucket(){ Name = "A" }
                }
            }));

        await svc.InitializeAsync();
        await svc.ExecuteAsync();

        svc.IMinioClientAdapterMock.Verify(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()), Times.Once());

        svc.IMinioClientAdapterMock.Verify(it => it.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()), Times.Never());

        svc.IMinioClientAdapterMock.Verify(it => it.SetPolicyAsync(It.IsAny<SetPolicyArgs>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task MinioBootstrapperServiceNoOldBucketsCreateBucketWithPolityTestsAsync()
    {
        string login = "login";
        string password = "password";
        string host = "localhost";
        int port = 22;

        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A",
                    Policy = new StaticPolicy(){ PolicyText = "T" }
                }
            },
            Credentials = new NetworkCredential(login, password),
            ServerEndpoint = new DnsEndPoint(host, port),
        };
        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeIConfigurationSection()
            {
                Key = "boostrap:minio",
                Value = "true"
            });
        svc.IMinioClientAdapterMock
            .Setup(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(new ListAllMyBucketsResult()
            {
                Buckets = new List<Bucket>()
                {

                }
            }));

        await svc.InitializeAsync();
        await svc.ExecuteAsync();

        svc.IMinioClientAdapterMock.Verify(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()), Times.Once());

        svc.IMinioClientAdapterMock.Verify(it => it.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()), Times.Once());

        svc.IMinioClientAdapterMock.Verify(it => it.SetPolicyAsync(It.IsAny<SetPolicyArgs>(), It.IsAny<CancellationToken>()), Times.Once());
    }

}


public class MinioBootstrapperServiceForTests : MinioBootstrapperService
{

    public MinioBootstrapperServiceForTests()
    {
        this.IMinioClientAdapterMock = new Mock<IMinioClientAdapter>();
        this.IConfigurationMock = new Mock<IConfiguration>();
    }

    public Mock<IMinioClientAdapter> IMinioClientAdapterMock { get; private set; }
    public Mock<IConfiguration> IConfigurationMock { get; private set; }

    protected override IMinioClientAdapter BuildMinioClient()
    {
        
        return this.IMinioClientAdapterMock.Object;
    }

    public override Task InitializeAsync()
    {
        this.Configuration = this.IConfigurationMock.Object;

        return base.InitializeAsync();
    }
}

public class FakeIConfigurationSection : IConfigurationSection
{
    public string this[string key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public string Key { get; set; }

    public string Path { get; set; }

    public string Value { get; set; }

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        throw new NotImplementedException();
    }

    public IChangeToken GetReloadToken()
    {
        throw new NotImplementedException();
    }

    public IConfigurationSection GetSection(string key)
    {
        throw new NotImplementedException();
    }
}