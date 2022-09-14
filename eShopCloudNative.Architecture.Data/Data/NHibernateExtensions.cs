using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using System.Diagnostics.CodeAnalysis;

namespace eShopCloudNative.Architecture.Data;

[ExcludeFromCodeCoverage]
public static class NHibernateExtensions
{
    public static IServiceCollection AddNHibernate(this IServiceCollection services, Action<NHibernateConfigBuilder> configure)
    {
        NHibernateConfigBuilder builder = new(services);
        configure?.Invoke(builder);
        builder.Build();
        return services;
    }

    public static IServiceCollection AddSession(this IServiceCollection services)
        => services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenSession());

    public static IServiceCollection AddStatelessSession(this IServiceCollection services)
        => services.AddScoped(sp => sp.GetRequiredService<ISessionFactory>().OpenStatelessSession());

}
