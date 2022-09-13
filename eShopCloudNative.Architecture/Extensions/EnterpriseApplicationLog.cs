using Serilog.Core.Enrichers;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Context;

namespace eShopCloudNative.Architecture.Extensions;
public class EnterpriseApplicationLog
{
    public static void SetGlobalContext(string applicationIdentity)
    {
        GlobalLogContext.PushProperty("Application", applicationIdentity);
        GlobalLogContext.PushProperty("Hostname", Environment.MachineName);
        GlobalLogContext.PushProperty("UserName", Environment.UserName);
        GlobalLogContext.PushProperty("Platform", Environment.OSVersion.Platform);
        GlobalLogContext.PushProperty("OSVersion", Environment.OSVersion.ToString());
    }
}
