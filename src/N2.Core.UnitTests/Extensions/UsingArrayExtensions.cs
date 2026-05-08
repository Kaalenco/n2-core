using N2.Core.Extensions;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public sealed class UsingArrayExtensions
{
    [TestMethod]
    public void ClearNullArrayDoesNotThrow()
    {
        ArrayExtensions.Clear<int>(null!);
    }

    [TestMethod]
    public void ClearSetsAllIntElementsToZero()
    {
        int[] arr = [1, 2, 3, 4, 5];
        arr.Clear();
        foreach (int item in arr)
        {
            Assert.AreEqual(0, item);
        }
    }

    [TestMethod]
    public void ClearPreservesArrayLength()
    {
        int[] arr = [10, 20, 30];
        arr.Clear();
        Assert.AreEqual(3, arr.Length);
    }

    [TestMethod]
    public void ClearEmptyArrayDoesNotThrow()
    {
        int[] arr = [];
        arr.Clear();
        Assert.AreEqual(0, arr.Length);
    }

    [TestMethod]
    public void ClearWorksForStructElements()
    {
        bool[] arr = [true, false, true];
        arr.Clear();
        Assert.IsFalse(arr[0]);
        Assert.IsFalse(arr[1]);
        Assert.IsFalse(arr[2]);
    }
}
