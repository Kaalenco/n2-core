using System.IO.Abstractions;

using Microsoft.Extensions.Configuration;

namespace N2.Core.Entity;

public class EntityConnectionService : IConnectionStringService
{
    private IConfiguration Configuration { get; set; }
    private IDirectoryInfo? DirectoryRoot { get; }
    public string SettingsFileName { get; set; } = "appSettings.json";

    public EntityConnectionService(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public EntityConnectionService()
    {
        FileSystem fileSystem = new();
        string currentFolder = fileSystem.Directory.GetCurrentDirectory();
        DirectoryRoot = fileSystem.DirectoryInfo.New(currentFolder);
        Configuration = LoadConfiguration<EntityConnectionService>();
    }

    public EntityConnectionService(IDirectoryInfo directory)
    {
        Contract.NotNull(directory, nameof(directory));
        if (!directory.Exists)
        {
            throw new EntityConnectionException($"Directory not found: {directory.FullName}");
        }

        DirectoryRoot = directory;
        Configuration = LoadConfiguration<EntityConnectionService>();
    }

    public void Reload<T>() where T : class
    {
        Configuration = LoadConfiguration<T>();
    }

    private IConfiguration LoadConfiguration<T>() where T : class
    {
        if (DirectoryRoot == null)
        {
            throw new EntityConnectionException("DirectoryRoot is not set.");
        }
        else
        {
            string c = DirectoryRoot.FullName;
            return new ConfigurationBuilder()
                .SetBasePath(c)
                .AddJsonFile(SettingsFileName, true)
                .AddUserSecrets<T>()
                .Build();
        }
    }

    public string GetConnectionString(string name) => Configuration.GetConnectionString(name) ?? throw new EntityConnectionException($"Connection string not found: {name}");
}