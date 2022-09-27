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
    private readonly Action<List<Tag>> action;
    private readonly string className;
    private readonly string methodName;

    private IDisposable serilogLogContext;
    private List<Tag> tags;
    public long startAt;
    private long endAt;

    public EnterpriseApplicationLogContext(string className, string methodName, Action<List<Tag>> action)
    {
        this.tags = new List<Tag>()
            .Add("Class", className)
            .Add("Method", methodName);

        this.action?.Invoke(tags);

        this.serilogLogContext = LogContext.Push(tags.Select(it => new PropertyEnricher(it.Key, it.Value, true)).ToArray());
        this.className = className;
        this.methodName = methodName;
        this.startAt = DateTime.Now.Ticks;
    }

    public void Dispose()
    {
        this.endAt = DateTime.Now.Ticks;

        Log.Information($"{className}.{methodName}() | Telemetry {{StartAt}} | {{EndAt}} | {{Elapsed}}", startAt, endAt, TimeSpan.FromTicks(endAt - startAt));

        serilogLogContext.Dispose();
    }
}
