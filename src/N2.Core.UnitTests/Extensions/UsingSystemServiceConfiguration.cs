using Microsoft.Extensions.DependencyInjection;

using System.IO.Abstractions;

using N2.Core.Commands;
using N2.Core.Extensions;
using N2.Core.SystemAbstractions;
using N2.Core.UnitTests.Models;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public sealed class UsingSystemServiceConfiguration
{
    // -------------------------------------------------------------------------
    // AddSystemServices
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddSystemServices_RegistersIFileSystem()
    {
        ServiceProvider sp = new ServiceCollection().AddSystemServices().BuildServiceProvider();
        Assert.IsNotNull(sp.GetService<IFileSystem>());
    }

    [TestMethod]
    public void AddSystemServices_RegistersITimeSystem()
    {
        ServiceProvider sp = new ServiceCollection().AddSystemServices().BuildServiceProvider();
        Assert.IsNotNull(sp.GetService<ITimeSystem>());
    }

    [TestMethod]
    public void AddSystemServices_RegistersIActivityLoggerFactory()
    {
        ServiceProvider sp = new ServiceCollection().AddSystemServices().BuildServiceProvider();
        Assert.IsNotNull(sp.GetService<IActivityLoggerFactory>());
    }

    [TestMethod]
    public void AddSystemServices_CalledTwice_DoesNotDuplicateRegistrations()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddSystemServices();
        sc.AddSystemServices();
        int count = sc.Count(s => s.ServiceType == typeof(IFileSystem));
        Assert.AreEqual(1, count);
    }

    // -------------------------------------------------------------------------
    // AddCommandHandlers
    // -------------------------------------------------------------------------

    [TestMethod]
    public void AddCommandHandlers_WithType_RegistersHandlerInterface()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddCommandHandlers(typeof(TestCommandHandler));
        Assert.IsTrue(sc.Any(s => s.ServiceType == typeof(ICommandHandler<TestCommand>)));
    }

    [TestMethod]
    public void AddCommandHandlers_WithType_DoesNotRegisterDuplicate()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddCommandHandlers(typeof(TestCommandHandler));
        sc.AddCommandHandlers(typeof(TestCommandHandler));
        int count = sc.Count(s => s.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void AddCommandHandlers_WithAssembly_RegistersHandlerInterface()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddCommandHandlers(typeof(TestCommandHandler).Assembly);
        Assert.IsTrue(sc.Any(s => s.ServiceType == typeof(ICommandHandler<TestCommand>)));
    }

    [TestMethod]
    public void AddCommandHandlers_WithNullTypeArray_ReturnsServiceCollection()
    {
        IServiceCollection sc = new ServiceCollection();
        IServiceCollection result = sc.AddCommandHandlers((Type[])null!);
        Assert.AreSame(sc, result);
    }

    // -------------------------------------------------------------------------
    // IsAssignableTo
    // -------------------------------------------------------------------------

    [TestMethod]
    public void IsAssignableTo_DerivedType_ReturnsTrue()
    {
        Assert.IsTrue(typeof(ArgumentNullException).IsAssignableTo(typeof(Exception)));
    }

    [TestMethod]
    public void IsAssignableTo_SameType_ReturnsTrue()
    {
        Assert.IsTrue(typeof(Exception).IsAssignableTo(typeof(Exception)));
    }

    [TestMethod]
    public void IsAssignableTo_UnrelatedType_ReturnsFalse()
    {
        Assert.IsFalse(typeof(string).IsAssignableTo(typeof(Exception)));
    }
}
