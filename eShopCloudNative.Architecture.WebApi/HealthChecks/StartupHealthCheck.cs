using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.HealthChecks;
public class StartupHealthCheck : IHealthCheck
{
    private volatile bool isReady;

    public bool StartupCompleted
    {
        get => this.isReady;
        set => this.isReady = value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (this.StartupCompleted)
        {
            return Task.FromResult(HealthCheckResult.Healthy("The startup task has completed."));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("That startup task is still running."));
    }
}