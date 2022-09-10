using eShopCloudNative.Architecture.Bootstrap;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eShopCloudNative.Architecture.Extensions;

namespace eShopCloudNative.Architecture.Bootstrap.Minio;
public class MinioBootstrapperService : IBootstrapperService
{
    public System.Net.NetworkCredential Credentials { get; set; }
    
    public System.Net.DnsEndPoint ServerEndpoint { get; set; }

    public bool WithSSL { get; set; }

    public List<MinioBucket> BucketsToCreate { get; set; }

    private MinioClient minio;
    private List<Bucket> oldBuckets;
    private IConfiguration configuration;

    public async Task InitializeAsync(IConfiguration configuration)
    {
        this.configuration = configuration;

        if (configuration.GetValue<bool>("boostrap:minio"))
        {
            this.minio = this.BuildMinioClient();

            this.oldBuckets = (await this.minio.ListBucketsAsync()).Buckets;
        }
        else
        {
            //TODO: Logar dizendo que está ignorando
        }
    }

    private MinioClient BuildMinioClient() => new MinioClient()
                        .WithEndpoint(this.ServerEndpoint.Host, this.ServerEndpoint.Port)
                        .WithCredentials(this.Credentials.UserName, this.Credentials.Password)
                        .If(it => this.WithSSL, it => it.WithSSL())
                        .Build();

    public async Task ExecuteAsync()
    {
        if (this.configuration.GetValue<bool>("boostrap:minio"))
        {
            foreach (var bucket in this.BucketsToCreate)
            {
                if (oldBuckets.Any(it => it.Name == bucket.BucketName) == false)
                {
                    await this.minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket.BucketName));

                    if (bucket.Policy != null)
                    {
                        await this.minio.SetPolicyAsync(new SetPolicyArgs().WithBucket(bucket.BucketName).WithPolicy(bucket.Policy.GetJsonPolicy()));
                    }

                }
            }
        }
        else
        {
            //TODO: Logar dizendo que está ignorando
        }
    }

}
