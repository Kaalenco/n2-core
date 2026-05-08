namespace N2.Core.UnitTests;

[TestClass]
public sealed class WithSelectableItem
{
    // -------------------------------------------------------------------------
    // Two-argument constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void TwoArgCtorSetsPublicIdAndName()
    {
        Guid id = Guid.NewGuid();
        SelectableItem sut = new(id, "Alpha");
        Assert.AreEqual(id, sut.PublicId);
        Assert.AreEqual("Alpha", sut.Name);
    }

    [TestMethod]
    public void TwoArgCtorDefaultsDescriptionToEmpty()
    {
        SelectableItem sut = new(Guid.NewGuid(), "Beta");
        Assert.AreEqual(string.Empty, sut.Description);
    }

    [TestMethod]
    public void TwoArgCtorDefaultsSelectedToFalse()
    {
        SelectableItem sut = new(Guid.NewGuid(), "Gamma");
        Assert.IsFalse(sut.Selected);
    }

    // -------------------------------------------------------------------------
    // Three-argument constructor
    // -------------------------------------------------------------------------

    [TestMethod]
    public void ThreeArgCtorSetsAllProperties()
    {
        Guid id = Guid.NewGuid();
        SelectableItem sut = new(id, "Delta", "a selection item");
        Assert.AreEqual(id, sut.PublicId);
        Assert.AreEqual("Delta", sut.Name);
        Assert.AreEqual("a selection item", sut.Description);
    }
}
