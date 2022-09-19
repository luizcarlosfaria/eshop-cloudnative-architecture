using Dawn;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;
public static class ConfigurationExtensions
{
    public static T CreateAndConfigureWith<T>(this IConfiguration configuration, string key)
    {
        Guard.Argument(configuration, nameof(configuration)).NotNull();
        Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();
        return configuration.ConfigureWith(key, Activator.CreateInstance<T>());
    }

    public static T ConfigureWith<T>(this IConfiguration configuration, string key, T item)
    {
        Guard.Argument(configuration, nameof(configuration)).NotNull();
        Guard.Argument(key, nameof(key)).NotNull().NotEmpty().NotWhiteSpace();
        configuration.Bind(key, item);
        return item;
    }

    public static bool GetFlag(this IConfiguration configuration, params string[] keys)
    {
        Guard.Argument(configuration, nameof(configuration)).NotNull();
        Guard.Argument(keys, nameof(keys)).NotNull().NotEmpty();
        string key = string.Join(":", keys);
        return configuration.GetValue<bool>(key);
    }
}
