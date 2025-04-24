# csharp codingstyle

## Generic guidelines

use common c# coding guidelines, except when the guidelines in this file prevent that.

Use 4 characters for indentation.

Avoid the use of underscores entirely in variables, classes, method names or unit tests.

Always use CamelCase for private fields in classes.

Always use PascalCase for methods and properties.

Async methods should NOT have a postfix of 'Async'.

## Web API

When creating a Web API, use the following conventions:

- Use the `IActionResult` return type for all controller methods.
- Use `IEnumerable<T>` for collections in the return type.
- Always Use minimal APIs.
- Use Command and Query pattern for all endpoints.
- Use commandhandlers and queryhandlers to handle commands and queries.
- Query and command classes should be in the same namespace as the controller.
- A response should be a DTO class with the same name as the command or query class, but with a postfix of 'Response'.
- An error should be a DTO class with the same name as the command or query class, but with a postfix of 'Error'.

### Example

This example shows a full implementation of a command and query handler for a weather service. The
minimal front end is using a weather forecast service to get the weather forecast for a given location.

```csharp
using N2.Core; // for IHandle, IRequest, IResponse

namespace N2.Weather;

public class ForecastError
{
    public string Message { get; set; } = string.Empty;
    public static ForecastError Create(Exception e)
    {
        return new ForecastError { Message = e.Message };
    }
}

public class WeatherForecastResponse : IResponse
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}

public class WeatherForecastRequest : IRequest
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class WeatherForecastHandler : IHandle<WeatherForecastRequest, WeatherForecastResponse>
{
    private readonly Random rng = new();

    private readonly string[] summaries =
        [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    public Task<WeatherForecastResponse> Handle(WeatherForecastRequest request)
    {
        return GetForecast(request.Latitude, request.Longitude);
    }

    /// <summary>
    /// Gets the weather forecast for a given latitude and longitude.
    /// </summary>
    private Task<WeatherForecastResponse> GetForecast( double latitude, double longitude )
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180.");
        }
        var result = new WeatherForecastResponse()
        {
            Date = DateTime.Now,
            TemperatureC = rng.Next(-20, 55),
            Summary = summaries[rng.Next(summaries.Length)]
        };

        return Task.FromResult(result);
    }
}

public static class ServiceCollectionExtensions
{
    public static void AddWeatherForecast(this IServiceCollection services)
    {
        services.TryAddScoped<IHandle<WeatherForecastRequest, WeatherForecastResponse>, WeatherForecastHandler>();
    }
}

public static class WeatherForecast
{
    public static void AddWeatherForecast(this WebApplication app)
    {
        app.MapGet("/forecast", GetForecast)
            .WithName("GetWeatherForecast")
            .WithDescription("The weather forecast can be used to get a forecast for s specific geo location.");
    }

    public static async Task<Results<Ok<WeatherForecastResponse>, BadRequest<ForecastError>, ProblemHttpResult>> GetForecast(
        [FromQuery(Name = "latitude")] double latitude,
        [FromQuery(Name = "longitude")] double longitude,
        [FromServices] IHandle<WeatherForecastRequest, WeatherForecastResponse> weatherForecastService)
    {
        try
        {
            var request = new WeatherForecastRequest
            {
                Latitude = latitude,
                Longitude = longitude
            };
            var forecast = await weatherForecastService.Handle(request);

            return TypedResults.Ok(forecast);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return TypedResults.BadRequest(ForecastError.Create(ex));
        }
    }
}


```

## Testing

Testing should be done using the MSTest framework.

A testclass should start with 'With' followed by the class name, or start with 'Using' followed by the class name for disposable types.
