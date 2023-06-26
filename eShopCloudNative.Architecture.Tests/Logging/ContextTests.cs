using eShopCloudNative.Architecture.Logging;
using Humanizer;
using Serilog;
using Serilog.Sinks.RabbitMQ.Sinks.RabbitMQ;
using Serilog.Sinks.TestCorrelator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace eShopCloudNative.Architecture.Tests.Logging;
public class ContextTests
{

    [Fact]
    public void ExceptionPropagation_ExecuteAndCatch()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

        using (TestCorrelator.CreateContext())
        {

            Assert.Throws<InvalidOperationException>(() =>
            {
                EnterpriseApplicationLog.ExecuteWithLog(context => context.SetIdentity<ContextTests>().AddProperty("Tests", 1), () =>
                {
                    throw new InvalidOperationException();
                });
            });

            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should().ContainSingle()
                .Which.Exception
                .Should().BeAssignableTo<InvalidOperationException>();
        }

    }

    [Fact]
    public async Task ExceptionPropagation_ExecuteAndCatchAsync()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();
        using (TestCorrelator.CreateContext())
        {

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await EnterpriseApplicationLog.ExecuteWithLogAsync(context => context.SetIdentity<ContextTests>().AddProperty("Tests", 1), () =>
                {
                    throw new InvalidOperationException();
                });
            });

            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should().ContainSingle()
                .Which.Exception
                .Should().BeAssignableTo<InvalidOperationException>();
        }
    }

    [Fact]
    public void Success_ExecuteAndCatch()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

        using (TestCorrelator.CreateContext())
        {
            EnterpriseApplicationLog.ExecuteWithLog(context => context.SetIdentity<ContextTests>().AddProperty("Tests", 1), () => { });

            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should().ContainSingle()
                .Which.MessageTemplate.Text
            .Should().Be("ContextTests.Success_ExecuteAndCatch() | Telemetry | {elapsed}");

        }
    }

    [Fact]
    public async Task Success_ExecuteAndCatchAsync()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

        using (TestCorrelator.CreateContext())
        {
            await EnterpriseApplicationLog.ExecuteWithLogAsync(context => context.SetIdentity<ContextTests>().AddProperty("Tests", 1), () => Task.CompletedTask);

            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should().ContainSingle()
                .Which.MessageTemplate.Text
            .Should().Be($"{nameof(ContextTests)}.{nameof(Success_ExecuteAndCatchAsync)}() | Telemetry | {{elapsed}}");
        }
    }

    [Fact]
    public void Success_GetAndCatch()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

        using (TestCorrelator.CreateContext())
        {
            int result = EnterpriseApplicationLog.ExecuteWithLogAndReturn(context => context.SetIdentity<ContextTests>().AddProperty("Tests", 1), () => 7);

            Assert.Equal(7, result);

            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should().ContainSingle()
                .Which.MessageTemplate.Text
            .Should().Be($"{nameof(ContextTests)}.{nameof(Success_GetAndCatch)}() | Telemetry | {{elapsed}}");
        }
    }

    [Fact]
    public async Task Success_GetAndCatchAsync()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.TestCorrelator().CreateLogger();

        using (TestCorrelator.CreateContext())
        {
            int result = await EnterpriseApplicationLog.ExecuteWithLogAndReturnAsync<int>(context => context.SetIdentity<ContextTests>().AddProperty("Tests", 1), () => Task.FromResult(7));

            Assert.Equal(7, result);

            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should().ContainSingle()
                .Which.MessageTemplate.Text
            .Should().Be($"{nameof(ContextTests)}.{nameof(Success_GetAndCatchAsync)}() | Telemetry | {{elapsed}}");
        }
    }
}
