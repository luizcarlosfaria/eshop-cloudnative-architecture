using Ardalis.GuardClauses;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;
public static class ConfigurationExtensions
{
    public static T CreateInstanceAndConfigureWith<T>(this IConfiguration configuration, string key)
    {
        Guard.Against.Null(configuration, nameof(configuration));
        Guard.Against.NullOrWhiteSpace(key, nameof(key));
        return configuration.ConfigureWith(key, Activator.CreateInstance<T>());
    }

    public static T ConfigureWith<T>(this IConfiguration configuration, string key, T item)
    {
        Guard.Against.Null(configuration, nameof(configuration));
        Guard.Against.NullOrWhiteSpace(key, nameof(key));
        configuration.Bind(key, item);
        return item;
    }
}
