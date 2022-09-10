using eShopCloudNative.Architecture.Bootstrap;
using eShopCloudNative.Architecture.Bootstrap.Minio;
using System;
using System.Collections.Generic;
using System.Linq;
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
            }
        };

        minioBucket.Policy.Should().NotBeNull();
        minioBucket.Policy.Should().Be(staticPolicy);

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


}
