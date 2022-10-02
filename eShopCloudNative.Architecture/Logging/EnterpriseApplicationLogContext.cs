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
    private IDisposable serilogLogContext;
    private string className;
    private string methodName;
    public long startAt;
    private long endAt;
    private List<Tag> tags;

    public EnterpriseApplicationLogContext(string className, string methodName, Action<List<Tag>> action)
    {
        Guard.Argument(className, nameof(className)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(methodName, nameof(methodName)).NotNull().NotEmpty().NotWhiteSpace();
        Guard.Argument(action, nameof(action)).NotNull();

        this.tags = new List<Tag>()
            .Add("Class", className)
            .Add("Method", methodName);

        action?.Invoke(tags);

        this.serilogLogContext = LogContext.Push(tags.Select(it => new PropertyEnricher(it.Key, it.Value, true)).ToArray());
        this.className = className;
        this.methodName = methodName;
        this.startAt = DateTime.Now.Ticks;
    }

    private string BuildSignature()
    {
        var arguments = this.tags
            .Where(it => it.Type == TagType.Argument)
            .Select(it => $"{it.Key}: {it.Value}")
            .ToArray();

        var returnValue = $"{className}.{methodName}({string.Join(",", arguments)})";

        return returnValue;
    }


    public void Dispose()
    {
        this.endAt = DateTime.Now.Ticks;

        Log.Information($"{this.BuildSignature()} | Telemetry | {{Elapsed}}", TimeSpan.FromTicks(endAt - startAt));

        serilogLogContext.Dispose();
    }
}
