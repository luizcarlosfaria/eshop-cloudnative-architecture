using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Bootstrap;
public class BootstrapperService : IBootstrapperService, IHostedService
{
    public event EventHandler BeforeInitialize;

    public event EventHandler AfterInitialize;

    public event EventHandler BeforeExecute;

    public event EventHandler AfterExecute;

    public List<IBootstrapperService> Services { get; set; }

    public async Task InitializeAsync()
    {
        Guard.Against.Null(this.Services, nameof(this.Services));

        this.BeforeInitialize?.Invoke(this, EventArgs.Empty);

        foreach (var service in this.Services)
        {
            Guard.Against.Null(service, nameof(service));

            await service.InitializeAsync();
        }

        this.AfterInitialize?.Invoke(this, EventArgs.Empty);
    }

    public async Task ExecuteAsync()
    {
        Guard.Against.Null(this.Services, nameof(this.Services));

        this.BeforeExecute?.Invoke(this, EventArgs.Empty);

        foreach (var service in this.Services)
        {
            Guard.Against.Null(service, nameof(service));

            await service.ExecuteAsync();
        }

        this.AfterExecute?.Invoke(this, EventArgs.Empty);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await this.InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao executar BootstrapperService.InitializeAsync()");
            throw;
        }

        try
        {
            await this.ExecuteAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erro ao executar BootstrapperService.ExecuteAsync()");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    
}
