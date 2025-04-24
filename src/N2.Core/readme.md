# N2.Core

[![.NET Build and test](https://github.com/Kaalenco/n2-core/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Kaalenco/n2-core/actions/workflows/dotnet.yml)

Generic core utilities for any project.

## Usage

Specific usage examples are provided in the code snippets below. The library is designed to be easy to use and integrate into your projects.

### LocalizedTextService

The `LocalizedTextService` provides localized text based on resource files and supports caching for improved performance.
The service uses `IMemoryCache` to cache resource lookups, preventing repeated loading of the same key for the same culture.
The information is retrieved from a resource file, which is a .resx file that contains key-value pairs for different cultures.
Configure the dependencies in your `Startup.cs` or `Program.cs` file.

#### Example

```csharp
var resourceManager = new ResourceManager("N2.Core.Resources", typeof(LocalizedTextService).Assembly);
var memoryCache = new MemoryCache(new MemoryCacheOptions());
var textService = new LocalizedTextService(resourceManager, memoryCache);

string globalText = textService.GetGlobalText("Hello"); // Cached after the first call

````

