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
            using (var context = new EnterpriseApplicationLogContext("A", "B"))
            {
                Assert.Throws<InvalidOperationException>(() =>
                {
                    context.ExecuteAndCatch(() => throw new InvalidOperationException());
                });
                Assert.NotNull(context.Exception);
            }

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
            using (var context = new EnterpriseApplicationLogContext("A", "B"))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await context.ExecuteAndCatchAsync(() =>
                    {
                        throw new InvalidOperationException();
                    });
                });

                Assert.NotNull(context.Exception);
            }

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
            using (var context = new EnterpriseApplicationLogContext(nameof(ContextTests)))
            {
                context.ExecuteAndCatch(() => { });

                var tags = context.GetTags();
                Assert.Contains(tags, it => it.Key == "Class" && it.Value as string == nameof(ContextTests));
                Assert.Contains(tags, it => it.Key == "Method" && it.Value as string == nameof(Success_ExecuteAndCatch));
            }

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
            using (var context = new EnterpriseApplicationLogContext(nameof(ContextTests)))
            {
                await context.ExecuteAndCatchAsync(() => Task.CompletedTask);

                var tags = context.GetTags();
                Assert.Contains(tags, it => it.Key == "Class" && it.Value as string == nameof(ContextTests));
                Assert.Contains(tags, it => it.Key == "Method" && it.Value as string == nameof(Success_ExecuteAndCatchAsync));
            }
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
            using (var context = new EnterpriseApplicationLogContext(nameof(ContextTests)))
            {
                int result = context.GetAndCatch(() => 7);

                var tags = context.GetTags();
                Assert.Contains(tags, it => it.Key == "Class" && it.Value as string == nameof(ContextTests));
                Assert.Contains(tags, it => it.Key == "Method" && it.Value as string == nameof(Success_GetAndCatch));
                Assert.Equal(7, result);
            }
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
            using (var context = new EnterpriseApplicationLogContext(nameof(ContextTests)))
            {
                int result = await context.GetAndCatchAsync(() => Task.FromResult(7));

                var tags = context.GetTags();
                Assert.Contains(tags, it => it.Key == "Class" && it.Value as string == nameof(ContextTests));
                Assert.Contains(tags, it => it.Key == "Method" && it.Value as string == nameof(Success_GetAndCatchAsync));
                Assert.Equal(7, result);
            }
            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should().ContainSingle()
                .Which.MessageTemplate.Text
            .Should().Be($"{nameof(ContextTests)}.{nameof(Success_GetAndCatchAsync)}() | Telemetry | {{elapsed}}");
        }
    }
}
