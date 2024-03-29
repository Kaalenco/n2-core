using N2.Core.Extensions;

namespace N2.Core.UnitTests.Extensions;

[TestClass]
public class GenericClassExtensionTests
{
    [TestMethod]
    public void PropertyMapperWorksWithSameType()
    {
        var source = new SimpleClassA { Name = "John", Age = 25 };
        var target = new SimpleClassA();

        source.MapPropertyValuesByName(target);

        Assert.AreEqual(source.Name, target.Name);
        Assert.AreEqual(source.Age, target.Age);
    }

    [TestMethod]
    public void PropertyMapperWorksWithOtherType()
    {
        var source = new SimpleClassA { Name = "John", Age = 25 };
        var target = new SimpleClassB();

        source.MapPropertyValuesByName(target);

        Assert.AreEqual(source.Name, target.Name);
        Assert.AreEqual(source.Age, target.Age);
    }

    [TestMethod]
    public void PropertyMapperOnlyCopiesProperties()
    {
        var source = new SimpleClassB { Name = "John", Age = 25 };
        var target = new ComplexClassA();

        source.MapPropertyValuesByName(target);

        Assert.AreEqual(source.Name, target.Name);
        Assert.AreEqual(source.Age, target.Age);
        Assert.AreEqual(30, target.NewAge(5));
    }

    [TestMethod]
    public void PropertyMapperOnlyCopiesReadPropertiesFromSource()
    {
        var source = new SimpleClassA { Name = "John", Age = 25 };
        var target = new ComplexClassB();

        source.MapPropertyValuesByName(target);

        Assert.AreEqual(source.Name, target.CurrentName);
        Assert.AreEqual(12, target.Age);
    }

    [TestMethod]
    public void PropertyMapperOnlyCopiesWritePropertiesToTarget()
    {
        var source = new ComplexClassB { Name = "John" };
        source.UpdateAge(25);
        var target = new ComplexClassC();

        source.MapPropertyValuesByName(target);

        Assert.AreEqual("Justin", target.Name);
        Assert.AreEqual(25, target.CurrentAge);
    }

    private sealed class SimpleClassA
    {
        public string? Name { get; set; } = string.Empty;
        public int Age { get; set; } = 10;
    }

    private sealed class SimpleClassB
    {
        public string? Name { get; set; } = string.Empty;
        public int Age { get; set; } = 12;
    }

    private sealed class ComplexClassA
    {
        public string? Name { get; set; } = string.Empty;
        public int Age { get; set; } = 10;
        public int NewAge(int add) => Age += add;
    }

    private sealed class ComplexClassB
    {
        public void UpdateAge(int newAge) => Age = newAge;
        public string CurrentName => Name ?? string.Empty;
        public string? Name { private get; set; } = string.Empty;
        public int Age { get; private set; } = 12;
    }

    private sealed class ComplexClassC
    {
        public int CurrentAge => Age;
        public string? Name { get; internal set; } = "Justin";
        public int Age { private get; set; } = 12;
    }
}
