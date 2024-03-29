using Dpi.Repository.Exceptions;
using Microsoft.Extensions.Configuration;
using System.IO.Abstractions;

namespace N2.Core;

public class SettingsService : ISettingsService
{
    private readonly IExceptionFactory exceptionFactory;

    private IConfiguration Configuration { get; set; }
    public IDirectoryInfo? DirectoryRoot { get; }
    public string SettingsFileName { get; set; } = "appSettings.json";

    public SettingsService(
        IConfiguration configuration,
        IExceptionFactory exceptionFactory)
    {
        this.exceptionFactory = exceptionFactory;
        Configuration = configuration;
    }

    public SettingsService()
    {
        var fileSystem = new FileSystem();
        var currentFolder = Directory.GetCurrentDirectory();
        exceptionFactory = new N2CoreExceptionFactory();
        DirectoryRoot = fileSystem.DirectoryInfo.New(currentFolder);
        Configuration = LoadConfiguration<SettingsService>();
    }

    public SettingsService(IDirectoryInfo directory, IExceptionFactory? exceptionFactory)
    {
        this.exceptionFactory = exceptionFactory ?? new N2CoreExceptionFactory();
        this.exceptionFactory.ThrowIfNull(directory);
        if (!directory.Exists)
        {
            this.exceptionFactory.ThrowDirectoryNotFoundException(directory.FullName);
        }

        DirectoryRoot = directory;
        Configuration = LoadConfiguration<SettingsService>();
    }

    public void Reload<T>() where T : class
    {
        Configuration = LoadConfiguration<T>();
    }

    private IConfiguration LoadConfiguration<T>() where T : class
    {
        if (DirectoryRoot == null)
        {
            exceptionFactory.ThrowInvalidOperationException("DirectoryRoot is not set.");
            return new NullConfiguration();
        }
        else
        {
            var c = DirectoryRoot.FullName;
            return new ConfigurationBuilder()
                .SetBasePath(c)
                .AddJsonFile(SettingsFileName, true)
                .AddUserSecrets<T>()
                .Build();
        }
    }

    public TConfig GetConfigSettings<TConfig>(string sectionName) where TConfig : class, new()
    {
        var settings = new TConfig();
        Configuration.GetSection(sectionName).Bind(settings);
        return settings;
    }

    public TConfig GetConfigSettings<TConfig>() where TConfig : class, new()
    {
        var sectionName = typeof(TConfig).Name ?? throw new ConfigurationException("Type name not found.");
        return GetConfigSettings<TConfig>(sectionName);
    }

    public TValue GetSetting<TValue>(string name, TValue defaultValue) where TValue : struct
    {
        var result = Configuration.GetValue(typeof(TValue), name, defaultValue);
        if (result == null) return defaultValue;        
        return (TValue)result;
    }

    public string GetConnectionString(string name) => Configuration.GetConnectionString(name) ?? throw new ConfigurationException($"Connection string not found: {name}");
}
