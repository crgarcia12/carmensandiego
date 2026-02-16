using Api.Services;

namespace Api.Endpoints;

public static class CityEndpoints
{
    public static void MapCityEndpoints(this WebApplication app)
    {
        app.MapGet("/api/cases/{id}/city", GetCurrentCity);
        app.MapPost("/api/cases/{id}/travel", TravelToCity);
    }

    private static IResult GetCurrentCity(string id, ICaseService caseService)
    {
        var gameCase = caseService.GetCase(id);
        if (gameCase == null)
        {
            return Results.NotFound(new { error = "Case not found" });
        }

        var city = caseService.GetCity(gameCase.CurrentCity);
        if (city == null)
        {
            return Results.NotFound(new { error = "City not found" });
        }

        var isFinalCity = gameCase.CurrentCityIndex >= gameCase.Trail.Count - 1;
        var travelOptions = caseService.GetTravelOptions(gameCase);

        return Results.Ok(new
        {
            city = new
            {
                id = city.Id,
                name = city.Name,
                region = city.Region,
                continent = city.Continent,
                backgroundKey = city.BackgroundKey
            },
            npcs = city.Npcs.Select(n => new { id = n.Id, name = n.Name, role = n.Role }),
            travelOptions = travelOptions.Select(t => new { cityId = t.CityId, cityName = t.CityName, description = t.Description }),
            remainingSteps = gameCase.RemainingSteps,
            isFinalCity = isFinalCity
        });
    }

    private static async Task<IResult> TravelToCity(string id, HttpContext context, ICaseService caseService)
    {
        var gameCase = caseService.GetCase(id);
        if (gameCase == null)
        {
            return Results.NotFound(new { error = "Case not found" });
        }

        var request = await context.Request.ReadFromJsonAsync<TravelRequest>();
        if (request == null || string.IsNullOrEmpty(request.CityId))
        {
            return Results.BadRequest(new { error = "City ID is required", code = "MISSING_CITY_ID" });
        }

        var (success, error, code) = caseService.Travel(gameCase, request.CityId);
        if (!success)
        {
            var statusCode = code switch
            {
                "CASE_COMPLETED" => 409,
                "NO_STEPS" => 409,
                "SAME_CITY" => 400,
                "INVALID_DESTINATION" => 400,
                _ => 400
            };
            return Results.Json(new { error, code }, statusCode: statusCode);
        }

        var city = caseService.GetCity(gameCase.CurrentCity);

        return Results.Ok(new
        {
            city = city != null ? new
            {
                id = city.Id,
                name = city.Name,
                region = city.Region,
                continent = city.Continent,
                backgroundKey = city.BackgroundKey
            } : null,
            remainingSteps = gameCase.RemainingSteps,
            caseStatus = gameCase.Status
        });
    }
}

public class TravelRequest
{
    public string CityId { get; set; } = string.Empty;
}
