using System.Collections.Concurrent;
using System.Diagnostics;

using N2.Core.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using N2.Core.Commands;
using N2.Core.UnitTests.Models;

namespace N2.Core.UnitTests.Commands;
/// <summary>
/// The dispatcher accept messages that arew distributed throughout the application.
/// It provides insight in worker queues, delays and dead letter queues. The dispatcher uses
/// containers to find handlers for messages.
/// </summary>
[TestClass]
public class ConductorTests
{
    private readonly IServiceCollection Sc = new ServiceCollection();
#pragma warning disable CS8618 
    private IServiceProvider Sp;
#pragma warning restore CS8618 

    [TestInitialize]
    public void TestInitialize()
    {
        Sc.AddScoped<IConductor, Conductor>();
        Sc.AddCommandHandlers(typeof(ConductorTests));
        Sc.AddLogging(opt => opt.AddConsole());
        Sc.AddMemoryCache();

        Sp = Sc.BuildServiceProvider();
    }

    [TestMethod]
    public void DispatcherCanInitialize()
    {
        IConductor? sut = Sp.GetService<IConductor>();
        Assert.IsNotNull(sut);
    }

    [TestMethod]
    public void DispatcherIsActive()
    {
        IConductor sut = Sp.GetService<IConductor>()!;
        Assert.IsTrue(sut.IsConfigured);
    }

    [TestMethod]
    public void DispatcherCanFindRegisteredHandler()
    {
        IConductor sut = Sp.GetService<IConductor>()!;
        Assert.IsTrue(sut.IsCommandAvailable<TestCommand>());
    }

    [TestMethod]
    public void DispatcherCanExecuteRegisteredHandler()
    {
        IConductor sut = Sp.GetService<IConductor>()!;
        TestCommand action = new();
        sut.Invoke(action);
    }

    [TestMethod]
    public void DispatcherCanRegisterCallbackOnHandle()
    {
        IConductor sut = Sp.GetService<IConductor>()!;

        TestResponse? result = null;

        using (ICallback callback = sut.RegisterCallback<TestResponse>((c) => result = (TestResponse)c))
        {
            Assert.AreEqual(1, sut.CallbackHandlerCount);
            TestCommand action = new(callback.Handle!.Value);
            sut.Invoke(action);

            Stopwatch timer = new();
            timer.Start();
#pragma warning disable CA1508 // Avoid dead conditional code
            while (result == null)
            {
                Thread.Sleep(100);
                if (timer.ElapsedMilliseconds > 2000)
                {
                    throw new TimeoutException();
                }
            }
            ;
#pragma warning restore CA1508 // Avoid dead conditional code
        }
        Assert.IsNotNull(result);
        Assert.AreEqual(0, sut.CallbackHandlerCount);
    }

    [TestMethod]
    public void DispatcherWillInvokeAllHandlers()
    {
        IConductor sut = Sp.GetService<IConductor>()!;
        ConcurrentQueue<TestResponse> result = new();

        using (ICallback callback = sut.RegisterCallback<TestResponse>((c) => result.Enqueue((TestResponse)c)))
        {
            TestCommandToo action = new(callback.Handle!.Value);
            sut.Invoke(action);

            Stopwatch timer = new();
            timer.Start();

            while (result.Count < 2)
            {
                Thread.Sleep(100);
                if (timer.ElapsedMilliseconds > 2000)
                {
                    throw new TimeoutException();
                }
            }
            ;
        }
        Assert.HasCount(2, result);
        Assert.AreEqual(0, sut.CallbackHandlerCount);
    }
}