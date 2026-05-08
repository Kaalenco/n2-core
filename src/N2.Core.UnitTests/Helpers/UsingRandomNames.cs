using N2.Core.Helpers;

namespace N2.Core.UnitTests.Helpers;

[TestClass]
public sealed class UsingRandomNames
{
    private static readonly HashSet<string> KnownNames = new(StringComparer.Ordinal)
    {
        "Angelique", "Balthazar", "Cassandra", "Dante", "Evangeline",
        "Felix", "Gabrielle", "Hector", "Isolde", "Jasper",
        "Kassandra", "Lysander", "Morgana", "Nathaniel", "Ophelia",
        "Percival", "Quintessa", "Raphael", "Seraphina", "Tristan",
        "Ursula", "Valerian", "Wolfgang", "Xanthe", "Ysabel", "Zephyr"
    };

    [TestMethod]
    public void CreateRandomNameReturnsNonEmptyString()
    {
        string name = RandomNames.CreateRandomName();
        Assert.IsFalse(string.IsNullOrEmpty(name));
    }

    [TestMethod]
    public void CreateRandomNameReturnsNameFromKnownSet()
    {
        string name = RandomNames.CreateRandomName();
        Assert.IsTrue(KnownNames.Contains(name), $"Unexpected name returned: '{name}'");
    }

    [TestMethod]
    public void CreateRandomNameRepeatedCallsAlwaysReturnKnownName()
    {
        for (int i = 0; i < 30; i++)
        {
            string name = RandomNames.CreateRandomName();
            Assert.IsTrue(KnownNames.Contains(name), $"Unexpected name on iteration {i}: '{name}'");
        }
    }
}
