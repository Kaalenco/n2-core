namespace N2.Core.UnitTests;

[TestClass]
public class WithBackgroundWorker
{
    [TestMethod]
    public void CanInitializeWithAutoStartFalse()
    {
        using BackgroundWorker sut = new(100, false, null, _ => Task.CompletedTask);
        Assert.IsNotNull(sut);
    }

    [TestMethod]
    public void IsNotRunningWhenCreatedWithAutoStartFalse()
    {
        using BackgroundWorker sut = new(100, false, null, _ => Task.CompletedTask);
        Assert.IsFalse(sut.IsRunning());
    }

    [TestMethod]
    public void IsNotWaitingWhenCreatedWithAutoStartFalse()
    {
        using BackgroundWorker sut = new(100, false, null, _ => Task.CompletedTask);
        Assert.IsFalse(sut.IsWaiting());
    }

    [TestMethod]
    public void CurrentStatusReturnsStoppedWhenNotStarted()
    {
        using BackgroundWorker sut = new(100, false, null, _ => Task.CompletedTask);
        IWorkerStatus status = sut.CurrentStatus();
        Assert.AreEqual("Stopped", status.CurrentAction);
        Assert.IsFalse(status.IsRunning);
    }

    [TestMethod]
    public void PauseJobSetsWorkerToPaused()
    {
        using BackgroundWorker sut = new(100, false, null, _ => Task.CompletedTask);
        sut.PauseJob();
        Assert.IsTrue(sut.IsWaiting());
    }

    [TestMethod]
    public void ContinueJobResumesFromPaused()
    {
        using BackgroundWorker sut = new(100, false, null, _ => Task.CompletedTask);
        sut.PauseJob();
        Assert.IsTrue(sut.IsWaiting());

        sut.ContinueJob();
        Assert.IsFalse(sut.IsWaiting());
    }

    [TestMethod]
    public void StartJobExecutesJob()
    {
        bool jobExecuted = false;
        using BackgroundWorker sut = new(10000, false, null, _ =>
        {
            jobExecuted = true;
            return Task.CompletedTask;
        });

        sut.StartJob();

        // allow the async task.Run to complete
        Thread.Sleep(200);
        Assert.IsTrue(jobExecuted);
    }

    [TestMethod]
    public void StopJobPreventsFurtherExecution()
    {
        int callCount = 0;
        using BackgroundWorker sut = new(50, true, null, _ =>
        {
            callCount++;
            return Task.CompletedTask;
        });

        Thread.Sleep(300);
        sut.StopJob();
        // Allow any callbacks already dispatched to the thread pool to drain.
        // _stopped is checked at the top of DoWork so they exit without incrementing.
        Thread.Sleep(100);
        int countAtStop = callCount;

        Thread.Sleep(400);
        Assert.AreEqual(countAtStop, callCount);
    }

    [TestMethod]
    public void StopJobHonoursCancellationToken()
    {
        using CancellationTokenSource cts = new();
        bool wasCancelled = false;

        using BackgroundWorker sut = new(100, false, cts, async token =>
        {
            try
            {
                await Task.Delay(5000, token);
            }
            catch (OperationCanceledException)
            {
                wasCancelled = true;
            }
        });

        // ContinueJob starts the timer without calling StartJob(),
        // which would null the tokenSource and break cancellation.
        sut.ContinueJob();
        Thread.Sleep(200); // allow timer to fire and job to start
        sut.StopJob();     // cancels tokenSource and waits for task to complete

        Assert.IsTrue(wasCancelled);
    }

    [TestMethod]
    public void TwoInstancesDoNotShareActivationState()
    {
        // If IsActivated were still static, starting worker1 would prevent worker2 from ever running.
        int worker1Count = 0;
        int worker2Count = 0;

        using BackgroundWorker worker1 = new(50, true, null, _ => { worker1Count++; return Task.CompletedTask; });
        using BackgroundWorker worker2 = new(50, true, null, _ => { worker2Count++; return Task.CompletedTask; });

        Thread.Sleep(300);

        worker1.StopJob();
        worker2.StopJob();

        Assert.IsTrue(worker1Count > 0, "worker1 should have executed");
        Assert.IsTrue(worker2Count > 0, "worker2 should have executed independently");
    }

    [TestMethod]
    public void CurrentStatusReflectsRunningStateWhenAutoStarted()
    {
        using BackgroundWorker sut = new(10000, true, null, async _ => await Task.Delay(500, CancellationToken.None));
        Thread.Sleep(100);
        IWorkerStatus status = sut.CurrentStatus();
        Assert.IsTrue(status.IsRunning || status.IsWaiting);
    }
}
