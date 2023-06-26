using Serilog.Core.Enrichers;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Context;
using Dawn;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace eShopCloudNative.Architecture.Logging;

public class EnterpriseApplicationLog
{

    public static void SetGlobalContext(string applicationIdentity, Action<List<Tag>> tagBuilder = null)
    {
        Guard.Argument(applicationIdentity, nameof(applicationIdentity)).NotNull().NotEmpty().NotWhiteSpace();

        var tags = new List<Tag>
        {
            new Tag("Application", TagType.Property, applicationIdentity),
            new Tag("Hostname", TagType.Property, Environment.MachineName) ,
            new Tag("UserName", TagType.Property, Environment.UserName) ,
            new Tag("Platform", TagType.Property, Environment.OSVersion.Platform) ,
            new Tag("OSVersion", TagType.Property, Environment.OSVersion.ToString()) ,
            new Tag("ServiceStartup", TagType.Property, new { Utc = DateTime.UtcNow, Local = DateTime.Now })
        };

        tagBuilder?.Invoke(tags);

        foreach (var tag in tags)
        {
            GlobalLogContext.PushProperty(tag.Key, tag.Value, true);
        }
    }

    public static void ExecuteWithLog(Action<EnterpriseApplicationLogContext> configure, Action actionToExecute)
    {
        using (var logContext = new EnterpriseApplicationLogContext())
        {
            configure?.Invoke(logContext);

            logContext.ExecuteWithLog(actionToExecute);
        }
    }

    public static async Task ExecuteWithLogAsync(Action<EnterpriseApplicationLogContext> configure, Func<Task> actionToExecute)
    {
        using (var logContext = new EnterpriseApplicationLogContext())
        {
            configure?.Invoke(logContext);

            await logContext.ExecuteWithLogAsync(actionToExecute);
        }
    }


    public static TOutput ExecuteWithLogAndReturn<TOutput>(Action<EnterpriseApplicationLogContext> configure, Func<TOutput> actionToExecute)
    {
        using (var logContext = new EnterpriseApplicationLogContext())
        {
            configure?.Invoke(logContext);

            return logContext.ExecuteWithLogAndReturn(actionToExecute);
        }
    }
    public static async Task<TOutput> ExecuteWithLogAndReturnAsync<TOutput>(Action<EnterpriseApplicationLogContext> configure, Func<Task<TOutput>> actionToExecute)
    {
        using (var logContext = new EnterpriseApplicationLogContext())
        {
            configure?.Invoke(logContext);

            return await logContext.ExecuteWithLogAndReturnAsync(actionToExecute);
        }
    }

}
