using N2.Core.Exceptions;

namespace N2.Core.UnitTests;

[TestClass]
public sealed class ContractTests
{
    [DataTestMethod]
    [DataRow("ValidName")]
    [DataRow("valid_name")]
    [DataRow("_validName")]
    [DataRow("v")]
    [DataRow("V_1")]
    public void ValidNameWithValidInputDoesNotThrow(string validName)
    {
        // Act & Assert
        Contract.ValidName(validName);
        // No exception means test passes
    }

    [DataTestMethod]
    [DataRow(null, "Value cannot be empty or only whitespace.")]
    [DataRow("", "Value cannot be empty or only whitespace.")]
    [DataRow("   ", "Value cannot be empty or only whitespace.")]
    [DataRow("1InvalidName", "Valid name should start with a character")]
    [DataRow(" InvalidName", "Valid name should start with a character")]
    [DataRow("Invalid@Name", "Character at position 7 is not allowed (@).")]
    public void ValidNameWithInvalidInputThrowsContractException(string invalidName, string expectedMessage)
    {
        // Act & Assert
        ContractException exception = Assert.ThrowsException<ContractException>(() => Contract.ValidName(invalidName));
        Assert.AreEqual(expectedMessage, exception.Message);
    }

    [TestMethod]
    public void ValidNameWithTooLongNameThrowsContractException()
    {
        // Arrange
        string tooLongName = new('a', 257);

        // Act & Assert
        ContractException exception = Assert.ThrowsException<ContractException>(() => Contract.ValidName(tooLongName));
        Assert.AreEqual("Name is too long, maximum length is 256 characters.", exception.Message);
    }

    [DataTestMethod]
    [DataRow("object")]
    [DataRow(1)]
    [DataRow(true)]
    public void NotNullWithNonNullValueDoesNotThrow(object value)
    {
        // Act & Assert
        Contract.NotNull(value, nameof(value));
        // No exception means test passes
    }

    [TestMethod]
    public void NotNullWithNullValueThrowsArgumentNullException()
    {
        // Arrange
        object? nullObject = null;
        string paramName = "testParam";

        // Act & Assert
        ArgumentNullException exception = Assert.ThrowsException<ArgumentNullException>(() => Contract.NotNull(nullObject, paramName));
        Assert.AreEqual(paramName, exception.ParamName);
    }

    [DataTestMethod]
    [DataRow('a', true)]
    [DataRow('A', true)]
    [DataRow('z', true)]
    [DataRow('Z', true)]
    [DataRow('1', false)]
    [DataRow('_', false)]
    [DataRow('@', false)]
    [DataRow('\u0100', false)] // Character outside of ASCII range
    public void IsCharReturnsExpectedResult(char character, bool expected)
    {
        // Act
        bool result = Contract.IsChar(character);

        // Assert
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow('0', true)]
    [DataRow('9', true)]
    [DataRow('5', true)]
    [DataRow('a', false)]
    [DataRow('Z', false)]
    [DataRow('_', false)]
    [DataRow('\u0100', false)] // Character outside of ASCII range
    public void IsDigitReturnsExpectedResult(char character, bool expected)
    {
        // Act
        bool result = Contract.IsDigit(character);

        // Assert
        Assert.AreEqual(expected, result);
    }
}
