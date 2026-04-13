namespace N2.Core.UnitTests;

[TestClass]
public sealed class WithValidator
{
#pragma warning disable CA1812 // This class is only used for testing and is not instantiated directly, so we can suppress the warning about it being unused.
    private sealed class TestModel { }

    private Validator validator = null!;

    [TestInitialize]
    public void Initialize()
    {
        validator = new Validator();
    }

    [TestMethod]
    public void NewValidatorIsValid()
    {
        Assert.IsTrue(validator.Valid);
        Assert.IsEmpty(validator.Results);
    }

    // NotNullOrEmpty

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void NotNullOrEmptyWithNullOrEmptyAddsResult(string? value)
    {
        validator.NotNullOrEmpty<TestModel>(value!, "must have value");

        Assert.IsFalse(validator.Valid);
        Assert.HasCount(1, validator.Results);
        Assert.AreEqual(ErrorCode.ValueNullOrEmpty, validator.Results[0].ErrorCode);
        Assert.AreEqual("must have value", validator.Results[0].Message);
        Assert.AreEqual(nameof(TestModel), validator.Results[0].TypeName);
    }

    [TestMethod]
    public void NotNullOrEmptyWithValueDoesNotAddResult()
    {
        validator.NotNullOrEmpty<TestModel>("hello", "must have value");

        Assert.IsTrue(validator.Valid);
    }

    // LengthMustBeEqualTo

    [TestMethod]
    public void LengthMustBeEqualToWithCorrectLengthDoesNotAddResult()
    {
        validator.LengthMustBeEqualTo<TestModel>("abc", 3, "wrong length");

        Assert.IsTrue(validator.Valid);
    }

    [TestMethod]
    [DataRow("ab", 3)]
    [DataRow("abcd", 3)]
    public void LengthMustBeEqualToWithWrongLengthAddsResult(string value, int length)
    {
        validator.LengthMustBeEqualTo<TestModel>(value, length, "wrong length");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.StringLenght, validator.Results[0].ErrorCode);
        Assert.AreEqual("wrong length", validator.Results[0].Message);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void LengthMustBeEqualToWithNullOrEmptyAddsNullOrEmptyResult(string? value)
    {
        validator.LengthMustBeEqualTo<TestModel>(value!, 3, "wrong length");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.ValueNullOrEmpty, validator.Results[0].ErrorCode);
    }

    // MinimumValue

    [TestMethod]
    public void MinimumValueAtMinimumDoesNotAddResult()
    {
        validator.MinimumValue<TestModel>(5, 5, "too small");

        Assert.IsTrue(validator.Valid);
    }

    [TestMethod]
    public void MinimumValueAboveMinimumDoesNotAddResult()
    {
        validator.MinimumValue<TestModel>(10, 5, "too small");

        Assert.IsTrue(validator.Valid);
    }

    [TestMethod]
    public void MinimumValueBelowMinimumAddsResult()
    {
        validator.MinimumValue<TestModel>(4, 5, "too small");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.MinimumValue, validator.Results[0].ErrorCode);
        Assert.AreEqual("too small", validator.Results[0].Message);
        Assert.AreEqual(nameof(TestModel), validator.Results[0].TypeName);
    }

    // ZeroOrPositive

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(100)]
    public void ZeroOrPositiveWithZeroOrPositiveDoesNotAddResult(int value)
    {
        validator.ZeroOrPositive<TestModel>(value, "must be positive");

        Assert.IsTrue(validator.Valid);
    }

    [TestMethod]
    public void ZeroOrPositiveWithNegativeAddsResult()
    {
        validator.ZeroOrPositive<TestModel>(-1, "must be positive");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.MinimumValue, validator.Results[0].ErrorCode);
        Assert.AreEqual("must be positive", validator.Results[0].Message);
    }

    // StartLowerThenEnd (int)

    [TestMethod]
    public void StartLowerThenEndIntWithStartLowerThanEndDoesNotAddResult()
    {
        validator.StartLowerThenEnd<TestModel>(1, 10, "start must be lower");

        Assert.IsTrue(validator.Valid);
    }

    [TestMethod]
    public void StartLowerThenEndIntWithStartEqualToEndAddsResult()
    {
        validator.StartLowerThenEnd<TestModel>(5, 5, "start must be lower");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.StartLowerThenEnd, validator.Results[0].ErrorCode);
        Assert.AreEqual("start must be lower", validator.Results[0].Message);
    }

    [TestMethod]
    public void StartLowerThenEndIntWithStartGreaterThanEndAddsResult()
    {
        validator.StartLowerThenEnd<TestModel>(10, 1, "start must be lower");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.StartLowerThenEnd, validator.Results[0].ErrorCode);
    }

    // StartLowerThenEnd (DateTime)

    [TestMethod]
    public void StartLowerThenEndDateTimeWithStartBeforeEndDoesNotAddResult()
    {
        validator.StartLowerThenEnd<TestModel>(new DateTime(2025, 1, 1), new DateTime(2025, 12, 31), "start must be lower");

        Assert.IsTrue(validator.Valid);
    }

    [TestMethod]
    public void StartLowerThenEndDateTimeWithStartEqualToEndAddsResult()
    {
        var date = new DateTime(2025, 6, 15);
        validator.StartLowerThenEnd<TestModel>(date, date, "start must be lower");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.StartLowerThenEnd, validator.Results[0].ErrorCode);
        Assert.AreEqual("start must be lower", validator.Results[0].Message);
    }

    [TestMethod]
    public void StartLowerThenEndDateTimeWithStartAfterEndAddsResult()
    {
        validator.StartLowerThenEnd<TestModel>(new DateTime(2025, 12, 31), new DateTime(2025, 1, 1), "start must be lower");

        Assert.IsFalse(validator.Valid);
        Assert.AreEqual(ErrorCode.StartLowerThenEnd, validator.Results[0].ErrorCode);
    }

    // Multiple violations

    [TestMethod]
    public void MultipleViolationsAccumulateResults()
    {
        validator.NotNullOrEmpty<TestModel>("", "field1 required");
        validator.MinimumValue<TestModel>(0, 1, "field2 too small");
        validator.ZeroOrPositive<TestModel>(-5, "field3 negative");

        Assert.IsFalse(validator.Valid);
        Assert.HasCount(3, validator.Results);
    }
}
