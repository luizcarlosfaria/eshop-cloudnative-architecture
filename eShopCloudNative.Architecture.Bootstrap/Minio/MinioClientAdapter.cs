using Dawn;
using eShopCloudNative.Architecture.Extensions;
using Minio;
using Minio.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Minio;

public interface IMinioClientAdapter
{
    Task MakeBucketAsync(MakeBucketArgs args, CancellationToken cancellationToken = default);

    Task SetPolicyAsync(SetPolicyArgs args, CancellationToken cancellationToken = default);

    Task<ListAllMyBucketsResult> ListBucketsAsync(CancellationToken cancellationToken = default);

    Task PutObjectAsync(PutObjectArgs args, CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public class MinioClientAdapter : IMinioClientAdapter
{
    private readonly MinioClient minioClient;

    public MinioClientAdapter(System.Net.DnsEndPoint serverEndpoint, System.Net.NetworkCredential credentials, bool withSSL)
    {
        Guard.Argument(serverEndpoint, nameof(serverEndpoint)).NotNull();
        Guard.Argument(serverEndpoint.Host, nameof(serverEndpoint.Host)).NotNull().NotEmpty().NotWhiteSpace();

        Guard.Argument(credentials, nameof(credentials)).NotNull();
        Guard.Argument(credentials.UserName, nameof(credentials.UserName)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(credentials.Password, nameof(credentials.Password)).NotNull().NotEmpty().NotWhiteSpace();

        this.minioClient = new MinioClient()
            .WithEndpoint(serverEndpoint.Host, serverEndpoint.Port)
            .WithCredentials(credentials.UserName, credentials.Password)
            .IfFunction(it => withSSL, it => it.WithSSL())
            .Build();
    }

    public Task MakeBucketAsync(MakeBucketArgs args, CancellationToken cancellationToken = default)
        => this.minioClient.MakeBucketAsync(args, cancellationToken);

    public Task SetPolicyAsync(SetPolicyArgs args, CancellationToken cancellationToken = default)
        => this.minioClient.SetPolicyAsync(args, cancellationToken);

    public Task<ListAllMyBucketsResult> ListBucketsAsync(CancellationToken cancellationToken = default)
        => this.minioClient.ListBucketsAsync(cancellationToken);

    public Task PutObjectAsync(PutObjectArgs args, CancellationToken cancellationToken = default)
        => this.minioClient.PutObjectAsync(args, cancellationToken);

}