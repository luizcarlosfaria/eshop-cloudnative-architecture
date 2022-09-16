using Ardalis.GuardClauses;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.HealthChecks;
public static class HealthChecksExtensions
{
    public static void AddCloudNativeHealthChecks(this IServiceCollection services, Action<IHealthChecksBuilder> configure = null)
    {
        services.AddSingleton<StartupHealthCheck>();

        services
          .AddHealthChecksUI(setupSettings: setup =>
          {
              setup.AddHealthCheckEndpoint($"Self:{Environment.MachineName}", "http://localhost:80/healthz");
          })
          .AddInMemoryStorage();

        IHealthChecksBuilder builder = services.AddHealthChecks()
            .AddCheck<StartupHealthCheck>(name: "self", failureStatus: HealthStatus.Unhealthy, tags: new[] { "self" });
            //.AddCheck("self", () => startupProbeFunction() ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy(), tags: new[] { "self" })
            //.AddProcessAllocatedMemoryHealthCheck(maximumMegabytesAllocated: 200, tags: new[] { "self" });

        if (configure != null)
        {
            configure(builder);
        }
    }

    public static void UseCloudNativeHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks("/liveness-probe", new HealthCheckOptions
        {
            Predicate = r => r.Name == "self"
        });

        app.UseHealthChecks("/readiness-probe", new HealthCheckOptions
        {
            Predicate = r => r.Name.Contains("self") && r.Tags.Contains("services")
        });

        app.MapHealthChecks("/healthz", new HealthCheckOptions
        {
            AllowCachingResponses = false,
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecksUI(healthChecksUIOptions =>
        {
            healthChecksUIOptions.UIPath = "/status";
        });
    }

}
