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

        this.className = className;
        this.methodName = methodName;
        this.startAt = DateTime.Now.Ticks;
    }

    public void SetException(Exception ex)
    {
        Guard.Argument(ex, nameof(ex)).NotNull();

        ExceptionTag tag = (ExceptionTag)this.tags.SingleOrDefault(it => it.Type ==  TagType.Exception);

        if (tag != null)
        { 
            tag.UpdateException(ex);
        }
        else
        {
            tag = new ExceptionTag(ex);
            this.tags.Add(tag);
        }
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
        var elapsed = TimeSpan.FromTicks(endAt - startAt);

        var list = this.tags.Select(it => new PropertyEnricher(it.Key, it.Value, true)).ToList();
        list.Add(new PropertyEnricher("elapsed", elapsed, true));

        using (IDisposable serilogLogContext = LogContext.Push(list.ToArray()))
        {
            Log.Information($"{this.BuildSignature()} | Telemetry | {{Elapsed}}", elapsed);
        }
    }
}
