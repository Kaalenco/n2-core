using System.IO.Abstractions;

using Moq;

namespace N2.Core.UnitTests;

[TestClass]
public class SettingserviceTests
{
    private readonly Mock<IDirectoryInfo> directoryInfoMock = new();
    private readonly SettingsService settings;
    public SettingserviceTests()
    {
        FileSystem fileSystem = new();
        string currentFolder = Directory.GetCurrentDirectory();
        IDirectoryInfo directory = fileSystem.DirectoryInfo.New(currentFolder);
        settings = new SettingsService(directory, null) { SettingsFileName = "testsettings.json" };
        settings.Reload<SettingserviceTests>();
    }

    [TestMethod]
    public void SettingserviceCanInitialize()
    {
        string currentFolder = Directory.GetCurrentDirectory();
        directoryInfoMock.SetupGet(x => x.Exists).Returns(true);
        directoryInfoMock.SetupGet(x => x.FullName).Returns(currentFolder);
        SettingsService testSettings = new(directoryInfoMock.Object, null);
        Assert.IsNotNull(testSettings);
    }

    [TestMethod]
    public void SettingserviceCanGetStructuredSettings()
    {
        SampleServiceSettings emailComServiceSettings = settings.GetConfigSettings<SampleServiceSettings>("EmailComService");
        Assert.IsNotNull(emailComServiceSettings);
    }

    [TestMethod]
    public void SettingserviceReadsFromConfig()
    {
        SampleServiceSettings emailComServiceSettings = settings.GetConfigSettings<SampleServiceSettings>("EmailComService");
        Assert.AreEqual("SecretUser", emailComServiceSettings.UserName);
    }

    [TestMethod]
    public void SettingServiceReturnsDefaults()
    {
        int timeout = settings.GetSetting("Timeout", 567);
        Assert.AreEqual(567, timeout);
    }

    [TestMethod]
    public void SettingServiceReturnsValueFromSetting()
    {
        int timeout = settings.GetSetting("Jwt:TokenLifeTime", 567);
        Assert.AreEqual(-1, timeout);
    }

    [TestMethod]
    public void SettingServiceReturnsValueFromEnvironment()
    {
        StringValue secret = settings.GetConfigSettings<StringValue>("Env:Secret");
        Assert.AreEqual("NotSoSecret", secret.Value);
    }

    private sealed class StringValue
    {
        public string Value { get; set; } = string.Empty;
    }
}


