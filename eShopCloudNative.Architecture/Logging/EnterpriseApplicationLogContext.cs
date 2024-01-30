using Dawn;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Serilog;
using Serilog.Context;
using Serilog.Core.Enrichers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Logging;
public partial class EnterpriseApplicationLogContext : IDisposable
{

    private string className;
    private string methodName;
    public long startAt;
    private long endAt;
    internal List<Tag> Tags { get; private set; }
    public Exception Exception { get; internal set; }

    public EnterpriseApplicationLogContext()
    {
        this.Tags = new List<Tag>();
        this.startAt = DateTime.Now.Ticks;
    }

    public EnterpriseApplicationLogContext SetIdentity<T>([CallerMemberName] string methodName = "")
    {
        this.className = typeof(T).Name;
        this.methodName = methodName;
        return this;
    }

    public EnterpriseApplicationLogContext SetIdentity(string className, string methodName)
    {
        this.className = className;
        this.methodName = methodName;
        return this;
    }


    public EnterpriseApplicationLogContext SetException(Exception ex)
    {
        this.Exception = ex;
        return this;
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
        list.Add(new PropertyEnricher("Class", className, true));
        list.Add(new PropertyEnricher("Method", methodName, true));

        using (IDisposable serilogLogContext = LogContext.Push(list.ToArray()))
        {
            if (this.Exception != null)
                Log.Error(this.Exception, $"{this.BuildSignature()} | Error | {{elapsed}}", elapsed);
            else
                Log.Information($"{this.BuildSignature()} | Telemetry | {{elapsed}}", elapsed);
        }
    }
}
