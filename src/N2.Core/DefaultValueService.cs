using System.Text.Json;
using System.Text.Json.Serialization;

namespace N2.Core;

public class DefaultValueService : IDefaultValueService
{
    public object JsonSerializerOptions => new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode,
        Converters = {
                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
            }
    };
}