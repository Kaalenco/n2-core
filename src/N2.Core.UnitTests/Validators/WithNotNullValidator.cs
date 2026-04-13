using N2.Core.Extensions;

using Microsoft.Extensions.DependencyInjection;

using N2.Core.Validators;

namespace N2.Core.UnitTests.Validators;

// Top-level class so AddValidators (which skips nested types) picks it up
internal sealed class TestModelValidator : IRuntimeValidator<TestModel>
{
    public void Validate(TestModel item)
    {
        ArgumentNullException.ThrowIfNull(item);
    }
}

internal sealed class TestModel { }

[TestClass]
public class WithNotNullValidator
{
    [TestMethod]
    public void ValidateWithNonNullDoesNotThrow()
    {
        NotNullValidator<TestModel> sut = new();
        sut.Validate(new TestModel());
    }

    [TestMethod]
    public void ValidateWithNullThrowsArgumentNullException()
    {
        NotNullValidator<TestModel> sut = new();
        Assert.Throws<ArgumentNullException>(() => sut.Validate(null!));
    }

    [TestMethod]
    public void ValidateWithNullStringThrowsArgumentNullException()
    {
        NotNullValidator<string> sut = new();
        Assert.Throws<ArgumentNullException>(() => sut.Validate(null!));
    }

    [TestMethod]
    public void ValidateWithNonNullStringDoesNotThrow()
    {
        NotNullValidator<string> sut = new();
        sut.Validate("hello");
    }

    [TestMethod]
    public void AddValidatorsRegistersConcreteValidator()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddValidators(typeof(WithNotNullValidator));
        ServiceProvider sp = sc.BuildServiceProvider();

        IRuntimeValidator<TestModel>? validator = sp.GetService<IRuntimeValidator<TestModel>>();

        Assert.IsNotNull(validator);
        Assert.IsInstanceOfType<TestModelValidator>(validator);
    }

    [TestMethod]
    public void AddValidatorsDoesNotRegisterDuplicate()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddValidators(typeof(WithNotNullValidator));
        sc.AddValidators(typeof(WithNotNullValidator));

        int count = sc.Count(s => s.ServiceType == typeof(IRuntimeValidator<TestModel>));

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void AddValidatorsRegisteredValidatorPassesForValidInput()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddValidators(typeof(WithNotNullValidator));
        ServiceProvider sp = sc.BuildServiceProvider();

        IRuntimeValidator<TestModel> validator = sp.GetRequiredService<IRuntimeValidator<TestModel>>();
        validator.Validate(new TestModel()); // must not throw
    }

    [TestMethod]
    public void AddValidatorsRegisteredValidatorThrowsForNull()
    {
        IServiceCollection sc = new ServiceCollection();
        sc.AddValidators(typeof(WithNotNullValidator));
        ServiceProvider sp = sc.BuildServiceProvider();

        IRuntimeValidator<TestModel> validator = sp.GetRequiredService<IRuntimeValidator<TestModel>>();
        Assert.Throws<ArgumentNullException>(() => validator.Validate(null!));
    }
}
