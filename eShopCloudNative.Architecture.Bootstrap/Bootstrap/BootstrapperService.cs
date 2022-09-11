using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap;
public class BootstrapperService : IBootstrapperService, IHostedService
{
    public List<IBootstrapperService> Services { get; set; }

    public async Task InitializeAsync()
    {
        Guard.Against.Null(this.Services, nameof(this.Services));

        foreach (var service in this.Services)
        {
            Guard.Against.Null(service, nameof(service));

            await service.InitializeAsync();
        }
    }

    public async Task ExecuteAsync()
    {
        Guard.Against.Null(this.Services, nameof(this.Services));

        foreach (var service in this.Services)
        {
            Guard.Against.Null(service, nameof(service));

            await service.ExecuteAsync();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await this.InitializeAsync();
        await this.ExecuteAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
}
