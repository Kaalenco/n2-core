using System.Text.Json;
using System.Text.Json.Serialization;

namespace N2.Core;

public class DefaultValueService : IDefaultValueService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
        Converters = {
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
            }
    };

    public object JsonSerializerOptions => _jsonSerializerOptions;
}