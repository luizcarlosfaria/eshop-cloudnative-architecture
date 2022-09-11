using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Extensions;
public static class ConfigurationExtensions
{
    public static T CreateInstanceAndConfigure<T>(this IConfiguration configuration, string key)
        => configuration.ConfigureWith(key, Activator.CreateInstance<T>());

    public static T ConfigureWith<T>(this IConfiguration configuration, string key, T item)
    {
        configuration.Bind(key, item);
        return item;
    }
}
