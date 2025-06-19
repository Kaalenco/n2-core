namespace N2.Core.UnitTests;

[TestClass]
public class NCoreSemaphoreTest
{
    [TestMethod]
    public void InitializeNCoreSemaphore()
    {
        _ = new Semaphore(0, 1);
        _ = new Semaphore(-1, 0);
        _ = new Semaphore(-2, -1);
        _ = new Semaphore(0, 1, "someName");
        _ = new Semaphore(0, "otherName");
        _ = new Semaphore(0, 5, null);
        _ = new Semaphore(0, 5, "");
        _ = new Semaphore(4, "");
        _ = new Semaphore(3, null);
        Assert.IsTrue(true, "Initialization is successful");
    }

    [TestMethod]
    public void InitializeNCoreSemaphoreFail()
    {
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { _ = new Semaphore(0, 0); });
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { _ = new Semaphore(4, 0); });
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { _ = new Semaphore(4, 4); });
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => { _ = new Semaphore(0, -1); });
    }

    [TestMethod]
    public void NCoreSemaphoreTimeoutTest()
    {
        Semaphore s = new(0, 1);
        bool first = s.WaitOne(TimeSpan.FromMilliseconds(100));
        bool second = s.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.IsTrue(first);
        Assert.IsFalse(second);
        Assert.AreEqual(1, s.CurrentValue);
        s.Release();
        Assert.AreEqual(0, s.CurrentValue);
    }

    [DataTestMethod]
    [DataRow(0, 2)]
    [DataRow(-1, 1)]
    [DataRow(-2, 0)]
    public void NCoreSemaphoreMaxcountTest(int start, int max)
    {
        Semaphore s = new(start, max);
        bool first = s.WaitOne(TimeSpan.FromMilliseconds(100));
        bool second = s.WaitOne(TimeSpan.FromMilliseconds(100));
        bool third = s.WaitOne(TimeSpan.FromMilliseconds(100));
        Assert.IsTrue(first);
        Assert.IsTrue(second);
        Assert.IsFalse(third);
        Assert.AreEqual(max, s.CurrentValue);
        s.Release();
        Assert.AreEqual(max - 1, s.CurrentValue);
        s.Release();
        Assert.AreEqual(start, s.CurrentValue);
    }
}
