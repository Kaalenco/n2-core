using Microsoft.Extensions.Configuration;

using Moq;

using System.IO.Abstractions;

using N2.Core.Entity;

namespace N2.Core.UnitTests.Entity;

[TestClass]
public sealed class WithEntityConnectionService
{
    private static IConfiguration BuildConfig(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    // -------------------------------------------------------------------------
    // IConfiguration constructor — GetConnectionString
    // -------------------------------------------------------------------------

    [TestMethod]
    public void GetConnectionString_ReturnsConfiguredValue()
    {
        IConfiguration config = BuildConfig(new() { ["ConnectionStrings:MyDb"] = "Server=.;Database=Test" });
        EntityConnectionService sut = new(config);
        Assert.AreEqual("Server=.;Database=Test", sut.GetConnectionString("MyDb"));
    }

    [TestMethod]
    public void GetConnectionString_MissingKey_ThrowsEntityConnectionException()
    {
        IConfiguration config = BuildConfig(new());
        EntityConnectionService sut = new(config);
        Assert.Throws<EntityConnectionException>(() => sut.GetConnectionString("Missing"));
    }

    [TestMethod]
    public void GetConnectionString_EmptyStringValue_ThrowsEntityConnectionException()
    {
        IConfiguration config = BuildConfig(new() { ["ConnectionStrings:Empty"] = null });
        EntityConnectionService sut = new(config);
        Assert.Throws<EntityConnectionException>(() => sut.GetConnectionString("Empty"));
    }

    // -------------------------------------------------------------------------
    // IDirectoryInfo constructor — guard clauses
    // -------------------------------------------------------------------------

    [TestMethod]
    public void DirectoryInfoCtor_NullDirectory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityConnectionService((IDirectoryInfo)null!));
    }

    [TestMethod]
    public void DirectoryInfoCtor_NonExistentDirectory_ThrowsEntityConnectionException()
    {
        Mock<IDirectoryInfo> dir = new();
        dir.Setup(d => d.Exists).Returns(false);
        dir.Setup(d => d.FullName).Returns("/nonexistent");
        Assert.Throws<EntityConnectionException>(() => new EntityConnectionService(dir.Object));
    }

    // -------------------------------------------------------------------------
    // Reload — requires DirectoryRoot; throws when constructed via IConfiguration
    // -------------------------------------------------------------------------

    [TestMethod]
    public void Reload_WithoutDirectoryRoot_ThrowsEntityConnectionException()
    {
        IConfiguration config = BuildConfig(new());
        EntityConnectionService sut = new(config);
        Assert.Throws<EntityConnectionException>(() => sut.Reload<EntityConnectionService>());
    }
}
