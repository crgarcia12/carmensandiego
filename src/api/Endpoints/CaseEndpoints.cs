using Api.Services;

namespace Api.Endpoints;

public static class CaseEndpoints
{
    public static void MapCaseEndpoints(this WebApplication app)
    {
        app.MapPost("/api/cases", CreateCase);
        app.MapGet("/api/cases/{id}", GetCase);
        app.MapGet("/api/cases/{id}/summary", GetCaseSummary);
    }

    private static IResult CreateCase(HttpContext context, ICaseService caseService, IGameSessionService sessionService)
    {
        var sessionId = context.Request.Headers["X-Session-Id"].FirstOrDefault()!;

        // Check if session already has an active case
        var activeCaseId = caseService.GetActiveCaseId(sessionId);
        if (activeCaseId != null)
        {
            var activeCase = caseService.GetCase(activeCaseId);
            if (activeCase != null && activeCase.Status == "active")
            {
                return Results.Conflict(new { error = "Active case already exists" });
            }
        }

        var gameCase = caseService.CreateCase(sessionId);

        return Results.Json(new
        {
            id = gameCase.Id,
            title = gameCase.Title,
            briefing = gameCase.Briefing,
            stolenTreasure = new { name = gameCase.StolenTreasure.Name, description = gameCase.StolenTreasure.Description },
            currentCity = gameCase.CurrentCity,
            remainingSteps = gameCase.RemainingSteps,
            status = gameCase.Status,
            trail = gameCase.Trail,
            correctSuspectId = gameCase.CorrectSuspectId,
            currentCityIndex = gameCase.CurrentCityIndex,
            visitedCities = gameCase.VisitedCities,
            warrantIssued = gameCase.WarrantIssued
        }, statusCode: 201);
    }

    private static IResult GetCase(string id, ICaseService caseService)
    {
        var gameCase = caseService.GetCase(id);
        if (gameCase == null)
        {
            return Results.NotFound(new { error = "Case not found" });
        }

        return Results.Ok(new
        {
            id = gameCase.Id,
            title = gameCase.Title,
            briefing = gameCase.Briefing,
            stolenTreasure = new { name = gameCase.StolenTreasure.Name, description = gameCase.StolenTreasure.Description },
            currentCity = gameCase.CurrentCity,
            remainingSteps = gameCase.RemainingSteps,
            status = gameCase.Status,
            trail = gameCase.Trail,
            correctSuspectId = gameCase.CorrectSuspectId,
            currentCityIndex = gameCase.CurrentCityIndex,
            visitedCities = gameCase.VisitedCities,
            warrantIssued = gameCase.WarrantIssued
        });
    }

    private static IResult GetCaseSummary(string id, ICaseService caseService, ISuspectService suspectService)
    {
        var gameCase = caseService.GetCase(id);
        if (gameCase == null)
        {
            return Results.NotFound(new { error = "Case not found" });
        }

        if (gameCase.Status == "active")
        {
            return Results.BadRequest(new { error = "Case is still active" });
        }

        var correctSuspect = suspectService.GetSuspect(gameCase.CorrectSuspectId);

        object? playerWarrant = null;
        if (gameCase.Warrant != null)
        {
            playerWarrant = new { suspectId = gameCase.Warrant.SuspectId };
        }

        return Results.Ok(new
        {
            outcome = gameCase.Status,
            citiesVisited = gameCase.VisitedCities,
            stepsUsed = 10 - gameCase.RemainingSteps,
            totalSteps = 10,
            correctSuspect = correctSuspect != null ? new { id = correctSuspect.Id, name = correctSuspect.Name } : null,
            playerWarrant = playerWarrant,
            stolenTreasure = new { name = gameCase.StolenTreasure.Name, description = gameCase.StolenTreasure.Description }
        });
    }
}
