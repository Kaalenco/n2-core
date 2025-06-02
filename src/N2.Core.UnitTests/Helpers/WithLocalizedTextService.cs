using System.Globalization;
using System.Resources;

using Microsoft.Extensions.Caching.Memory;

namespace N2.Core.UnitTests.Helpers;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

[TestClass]
public class WithLocalizedTextService : IDisposable
{
    private LocalizedTextService localizedTextService;
    private MemoryCache memoryCache;
    private bool disposedValue;

    [TestInitialize]
    public void Setup()
    {
        // Initialize the ResourceManager with the actual resources
        ResourceManager resourceManager = new("N2.Core.Resources.Messages", typeof(LocalizedTextService).Assembly);

        // Initialize the MemoryCache
        memoryCache = new MemoryCache(new MemoryCacheOptions());

        // Create an instance of LocalizedTextService
        localizedTextService = new LocalizedTextService(resourceManager, memoryCache);
    }

    [TestMethod]
    public void GetGlobalTextWhenKeyExistsReturnsValue()
    {
        // Arrange
        string key = "Hello";
        string expectedValue = "Hello"; // Value from Resources.resx

        // Act
        string actualValue = localizedTextService.GT(key);

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod]
    public void GetGlobalTextWhenKeyDoesNotExistReturnsMissingPlaceholder()
    {
        // Arrange
        string key = "NonExistentKey";
        string expectedValue = $"[Missing: {key}]";

        // Act
        string actualValue = localizedTextService.GT(key);

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod]
    public void GetGlobalTextWhenCultureIsDutchReturnsLocalizedValue()
    {
        // Arrange
        string key = "Hello";
        string expectedValue = "Hallo"; // Value from Resources.nl.resx

        // Set the culture to Dutch
        localizedTextService.CurrentCulture = new CultureInfo("nl-nl");
        // Act
        string actualValue = localizedTextService.GT(key);

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod]
    public void GetTextWhenKeyExistsReturnsValue()
    {
        // Arrange
        string pageContext = "HomePage";
        string key = "WelcomeMessage";
        string expectedValue = "Welcome to our application!"; // Value from Resources.resx

        // Act
        string actualValue = localizedTextService.GT(pageContext, key);

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    [TestMethod]
    public void GetTextWhenCultureIsDutchReturnsLocalizedValue()
    {
        // Arrange
        string pageContext = "HomePage";
        string key = "WelcomeMessage";
        string expectedValue = "Welkom bij onze applicatie!"; // Value from Resources.nl.resx

        // Set the culture to Dutch
        localizedTextService.CurrentCulture = new CultureInfo("nl-be");

        // Act
        string actualValue = localizedTextService.GT(pageContext, key);

        // Assert
        Assert.AreEqual(expectedValue, actualValue);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                memoryCache?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
