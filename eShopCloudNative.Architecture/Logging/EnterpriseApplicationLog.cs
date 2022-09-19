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

        var tags = new List<Tag>();

        tagBuilder?.Invoke(tags);

        tags.Add("Application", applicationIdentity);
        tags.Add("Hostname", Environment.MachineName);
        tags.Add("UserName", Environment.UserName);
        tags.Add("Platform", Environment.OSVersion.Platform);
        tags.Add("OSVersion", Environment.OSVersion.ToString());
        tags.Add("ServiceStartup", new { Utc = DateTime.UtcNow, Local = DateTime.Now });

        foreach (var tag in tags)
            GlobalLogContext.PushProperty(tag.Key, tag.Value, true);
    }
}
