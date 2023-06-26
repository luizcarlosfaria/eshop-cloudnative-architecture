using Dawn;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Serilog;
using Serilog.Context;
using Serilog.Core.Enrichers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Logging;
public class EnterpriseApplicationLogContext : IDisposable
{

    private string className;
    private string methodName;
    public long startAt;
    private long endAt;
    internal List<Tag> Tags { get; private set; }
    public Exception Exception { get; internal set; }

    public EnterpriseApplicationLogContext(string className, [System.Runtime.CompilerServices.CallerMemberName] string methodName = "")
    {
        Guard.Argument(className, nameof(className)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(methodName, nameof(methodName)).NotNull().NotEmpty().NotWhiteSpace();

        this.Tags = new List<Tag>();
        this.Add("Class", className);
        this.Add("Method", methodName);

        this.className = className;
        this.methodName = methodName;
        this.startAt = DateTime.Now.Ticks;
    }



    private string BuildSignature()
    {
        var arguments = this.Tags
            .Where(it => it.Type == TagType.Argument)
            .Select(it => $"{it.Key}: {it.Value}")
            .ToArray();

        var returnValue = $"{this.className}.{this.methodName}({string.Join(",", arguments)})";

        return returnValue;
    }


    public void Dispose()
    {
        this.endAt = DateTime.Now.Ticks;
        var elapsed = TimeSpan.FromTicks(this.endAt - this.startAt);

        var list = this.Tags.Select(it => new PropertyEnricher(it.Key, it.Value, true)).ToList();
        list.Add(new PropertyEnricher("elapsed", elapsed, true));

        using (IDisposable serilogLogContext = LogContext.Push(list.ToArray()))
        {
            if (this.Exception != null)
                Log.Error(this.Exception, $"{this.BuildSignature()} | Telemetry | {{elapsed}}", elapsed);
            else
                Log.Information($"{this.BuildSignature()} | Telemetry | {{elapsed}}", elapsed);
        }
    }
}
