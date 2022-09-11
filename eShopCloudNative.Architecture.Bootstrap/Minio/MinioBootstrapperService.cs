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
using System.Diagnostics.CodeAnalysis;

namespace eShopCloudNative.Architecture.Minio;
public class MinioBootstrapperService : IBootstrapperService
{
    public System.Net.NetworkCredential Credentials { get; set; }

    public System.Net.DnsEndPoint ServerEndpoint { get; set; }

    public bool WithSSL { get; set; }

    public List<MinioBucket> BucketsToCreate { get; set; }

    public IConfiguration Configuration { get; set; }

    private IMinioClientAdapter minio;

    public virtual Task InitializeAsync()
    {
        if (this.Configuration.GetValue<bool>("boostrap:minio"))
        {
            this.minio = this.BuildMinioClient();
        }
        else
        {
            //TODO: Logar dizendo que está ignorando
        }

        return Task.CompletedTask;
    }

    [ExcludeFromCodeCoverage]
    protected virtual IMinioClientAdapter BuildMinioClient() => new MinioClientAdapter(this.ServerEndpoint, this.Credentials, this.WithSSL);

    public virtual async Task ExecuteAsync()
    {
        if (this.Configuration.GetValue<bool>("boostrap:minio"))
        {
            List<Bucket> oldBuckets  = (await this.minio.ListBucketsAsync()).Buckets;

            foreach (var bucket in this.BucketsToCreate)
            {
                if (oldBuckets.Any(it => it.Name == bucket.BucketName) == false)
                {
                    await this.minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket.BucketName));

                    if (bucket.Policy != null)
                        await this.minio.SetPolicyAsync(new SetPolicyArgs().WithBucket(bucket.BucketName).WithPolicy(bucket.Policy.GetJsonPolicy()));

                }
            }
        }
        else
        {
            //TODO: Logar dizendo que está ignorando
        }
    }

}
