using Moq;
using System.IO.Abstractions;

namespace N2.Core.UnitTests;

[TestClass]
public class SettingserviceTests
{
    private readonly Mock<IDirectoryInfo> directoryInfoMock = new Mock<IDirectoryInfo>();
    private readonly ISettingsService settings;
    public SettingserviceTests()
    {
        var fileSystem = new FileSystem();
        var currentFolder = Directory.GetCurrentDirectory();
        var directory = fileSystem.DirectoryInfo.New(currentFolder);
        settings = new SettingsService(directory, null) {  SettingsFileName = "testsettings.json" };
        settings.Reload<SettingserviceTests>();
    }

    [TestMethod]
    public void SettingserviceCanInitialize()
    {
        var currentFolder = Directory.GetCurrentDirectory();
        directoryInfoMock.SetupGet(x => x.Exists).Returns(true);
        directoryInfoMock.SetupGet(x => x.FullName).Returns(currentFolder);
        var testSettings = new SettingsService(directoryInfoMock.Object, null);
        Assert.IsNotNull(testSettings);
    }

    [TestMethod]
    public void SettingserviceCanGetStructuredSettings()
    {
        var emailComServiceSettings = settings.GetConfigSettings<SampleServiceSettings>("EmailComService");
        Assert.IsNotNull(emailComServiceSettings);
    }

    [TestMethod]
    public void SettingserviceReadsFromConfig()
    {
        var emailComServiceSettings = settings.GetConfigSettings<SampleServiceSettings>("EmailComService");
        Assert.AreEqual("SecretUser", emailComServiceSettings.UserName);
    }

    [TestMethod]
    public void SettingServiceReturnsDefaults()
    {
        var timeout = settings.GetSetting("Timeout", 567);
        Assert.AreEqual(567, timeout);
    }

    [TestMethod]
    public void SettingServiceReturnsValueFromSetting()
    {
        var timeout = settings.GetSetting("Jwt:TokenLifeTime", 567);
        Assert.AreEqual(-1, timeout);
    }

}
