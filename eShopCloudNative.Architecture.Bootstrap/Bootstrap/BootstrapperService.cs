using Dawn;
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
        Guard.Argument(this.Services, nameof(this.Services)).NotNull();

        this.BeforeInitialize?.Invoke(this, EventArgs.Empty);

        foreach (var service in this.Services)
        {
            Guard.Argument(service, nameof(service)).NotNull();

            await service.InitializeAsync();
        }

        this.AfterInitialize?.Invoke(this, EventArgs.Empty);
    }

    public async Task ExecuteAsync()
    {
        Guard.Argument(this.Services, nameof(this.Services)).NotNull();

        this.BeforeExecute?.Invoke(this, EventArgs.Empty);

        foreach (var service in this.Services)
        {
            Guard.Argument(service, nameof(service)).NotNull();

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
