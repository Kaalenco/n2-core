using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace N2.Core;

public class NullConfiguration : IConfiguration
{
    private readonly ConcurrentDictionary<string, string> _values = new();
    public string? this[string key]
    {
        get
        {
            return _values[key];
        }
        set
        {
            if (value == null)
            {
                _values.TryRemove(key, out _);
            }
            else
            {
                _values.TryAdd(key, value);
            }
        }
    }

    public IEnumerable<IConfigurationSection> GetChildren() => throw new NotImplementedException();
    public IChangeToken GetReloadToken() => throw new NotImplementedException();
    public IConfigurationSection GetSection(string key) => throw new NotImplementedException();
}