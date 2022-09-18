using eShopCloudNative.Architecture.Bootstrap;
using eShopCloudNative.Architecture.Minio;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests.Bootstrapp;
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
        };

        minioBucket.Policy.Should().NotBeNull();
        minioBucket.Policy.Should().Be(staticPolicy);

        minioBootstrapperService.BucketsToCreate.Should().HaveCount(1);

        minioBucket.BucketName.Should().Be("a");
        minioBucket.Policy.GetJsonPolicy().Should().Be("T");
        publicPolicy.GetJsonPolicy().Should().Contain(publicPolicy.BucketName);
    }

    [Fact]
    public void PublicPolicyTests()
    {
        string key = Guid.NewGuid().ToString("D");

        var publicPolicy = new PublicPolicy(){ BucketName = key };

        publicPolicy.GetJsonPolicy().Should().NotBeNullOrWhiteSpace();

        publicPolicy.GetJsonPolicy().Should().Contain(key);

        Action action = () => new PublicPolicy().GetJsonPolicy();

        action.Should().Throw<InvalidOperationException>();

    }

    [Fact]
    public void StaticPolicyTests()
    {
        string key = Guid.NewGuid().ToString("D");

        var publicPolicy = new StaticPolicy(){ PolicyText = key };

        Assert.Equal(key, publicPolicy.PolicyText);
        Assert.Equal(key, publicPolicy.GetJsonPolicy());
    }

    [Fact]
    public async Task MinioBootstrapperServiceNoStartTestsAsync()
    {

        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A"
                }
            },
        };
        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeConfigurationSection().SetKeyValue("boostrap:minio", "false"));

        await svc.InitializeAsync();
        await svc.ExecuteAsync();

        svc.IMinioClientAdapterMock.Verify(it => it.ListBucketsAsync(It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task MinioBootstrapperServiceNoOldBucketsCreateBucketWithoutPolicyTestsAsync()
    {
        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A"
                }
            },
        };
        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeConfigurationSection().SetKeyValue("boostrap:minio", "true"));
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
        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A"
                }
            },
        };

        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeConfigurationSection().SetKeyValue("boostrap:minio", "true"));

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
    public async Task MinioBootstrapperServiceNoOldBucketsCreateBucketWithPolicyTestsAsync()
    {
        var svc = new MinioBootstrapperServiceForTests()
        {
            BucketsToCreate = new List<MinioBucket>(){
                new MinioBucket(){
                    BucketName = "A",
                    Policy = new StaticPolicy(){ PolicyText = "T" }
                }
            },
        };

        svc.IConfigurationMock
            .Setup(it => it.GetSection("boostrap:minio"))
            .Returns(new FakeConfigurationSection().SetKeyValue("boostrap:minio", "true"));

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

    public override Task InitializeAsync()
    {
        this.Minio = this.IMinioClientAdapterMock.Object;
        this.Configuration = this.IConfigurationMock.Object;

        return base.InitializeAsync();
    }
}
