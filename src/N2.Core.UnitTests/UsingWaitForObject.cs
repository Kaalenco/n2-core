namespace N2.Core.UnitTests;

[TestClass]
public class UsingWaitForObject
{
    [TestMethod]
    public async Task WaitAsyncReturnsTrueWhenConditionIsImmediatelyAvailable()
    {
        object available = new();
        using WaitForObject sut = new(() => available, timeout: 5);
        bool result = await sut.WaitAsync();
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task WaitAsyncReturnsFalseWhenConditionNeverMet()
    {
        using WaitForObject sut = new(() => null, timeout: 1);
        bool result = await sut.WaitAsync();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task WaitAsyncReturnsTrueWhenConditionEventuallyMet()
    {
        int counter = 0;
        using WaitForObject sut = new(() => counter >= 1 ? (object)counter : null, timeout: 5);

        _ = Task.Run(async () =>
        {
            await Task.Delay(300);
            Interlocked.Increment(ref counter);
        });

        bool result = await sut.WaitAsync();
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task WaitAsyncReturnsFalseWhenConditionMetAfterTimeout()
    {
        int counter = 0;
        using WaitForObject sut = new(() => counter >= 1 ? (object)counter : null, timeout: 1);

        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            Interlocked.Increment(ref counter);
        });

        bool result = await sut.WaitAsync();
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void CanDisposeWithoutWaiting()
    {
        WaitForObject sut = new(() => null, timeout: 30);
        sut.Dispose();
        // no exception expected
#pragma warning disable MSTEST0032 // We expect the Dispose method to complete without throwing an exception, so we can assert true here.
        Assert.IsTrue(true, "Dispose should not throw an exception");
    }
}
