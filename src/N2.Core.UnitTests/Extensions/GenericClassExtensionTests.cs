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

    [TestMethod]
    public void CopyFromReturnsTargetWithMappedValues()
    {
        var source = new SimpleClassA { Name = "Jane", Age = 30 };
        var target = new SimpleClassA();

        SimpleClassA? result = target.CopyFrom(source);

        Assert.AreSame(target, result);
        Assert.AreEqual("Jane", result!.Name);
        Assert.AreEqual(30, result.Age);
    }

    [TestMethod]
    public void CopyFromReturnsDefaultWhenTargetIsNull()
    {
        var source = new SimpleClassA { Name = "Jane", Age = 30 };
        SimpleClassA? target = null;

        SimpleClassA? result = target!.CopyFrom(source);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void PropertyMapperDoesNothingWhenNoPropertiesMatch()
    {
        var source = new SimpleClassA { Name = "John", Age = 25 };
        var target = new NoOverlapClass();

        source.MapPropertyValuesByName(target);

        Assert.AreEqual("default", target.Description);
        Assert.AreEqual(0, target.Score);
    }

    [TestMethod]
    public void PropertyMapperCopiesNullValues()
    {
        var source = new SimpleClassA { Name = null, Age = 25 };
        var target = new SimpleClassA { Name = "original" };

        source.MapPropertyValuesByName(target);

        Assert.IsNull(target.Name);
    }

    [TestMethod]
    public void PropertyMapperMatchesPropertiesCaseInsensitive()
    {
        var source = new CamelCaseClass { name = "Alice", age = 42 };
        var target = new SimpleClassA();

        source.MapPropertyValuesByName(target);

        Assert.AreEqual("Alice", target.Name);
        Assert.AreEqual(42, target.Age);
    }

    [TestMethod]
    public void PropertyMapperProducesConsistentResultsOnRepeatedCalls()
    {
        var source = new SimpleClassA { Name = "First", Age = 1 };
        var target1 = new SimpleClassB();
        var target2 = new SimpleClassB();

        source.MapPropertyValuesByName(target1);
        source.Name = "Second";
        source.Age = 2;
        source.MapPropertyValuesByName(target2);

        Assert.AreEqual("First", target1.Name);
        Assert.AreEqual(1, target1.Age);
        Assert.AreEqual("Second", target2.Name);
        Assert.AreEqual(2, target2.Age);
    }

    [TestMethod]
    public void SerializeForViewReturnsJsonString()
    {
        var source = new SimpleClassA { Name = "John", Age = 25 };

        string result = source.SerializeForView();

        Assert.IsNotNull(result);
        Assert.Contains("John", result, StringComparison.Ordinal);
        Assert.Contains("25", result, StringComparison.Ordinal);
    }

    [TestMethod]
    public void SerializeForViewReturnsIndentedJson()
    {
        var source = new SimpleClassA { Name = "John", Age = 25 };

        string result = source.SerializeForView();

        Assert.IsTrue(result.Contains('\n', StringComparison.Ordinal), "Expected indented (multi-line) JSON");
    }

    private sealed class NoOverlapClass
    {
        public string Description { get; set; } = "default";
        public int Score { get; set; }
    }

    private sealed class CamelCaseClass
    {
        // Lower-case property names to verify case-insensitive matching
        public string? name { get; set; }
        public int age { get; set; }
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
