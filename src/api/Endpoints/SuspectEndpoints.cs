using Api.Services;

namespace Api.Endpoints;

public static class SuspectEndpoints
{
    public static void MapSuspectEndpoints(this WebApplication app)
    {
        app.MapGet("/api/cases/{id}/suspects", GetSuspects);
        app.MapPost("/api/cases/{id}/warrant", IssueWarrant);
    }

    private static IResult GetSuspects(string id, ICaseService caseService, ISuspectService suspectService)
    {
        var gameCase = caseService.GetCase(id);
        if (gameCase == null)
        {
            return Results.NotFound(new { error = "Case not found" });
        }

        var suspects = suspectService.GetAllSuspects();
        return Results.Ok(new
        {
            suspects = suspects.Select(s => new
            {
                id = s.Id,
                name = s.Name,
                photoKey = s.PhotoKey,
                traits = new
                {
                    hairColor = s.Traits.HairColor,
                    eyeColor = s.Traits.EyeColor,
                    hobby = s.Traits.Hobby,
                    favoriteFood = s.Traits.FavoriteFood,
                    vehicle = s.Traits.Vehicle,
                    distinguishingFeature = s.Traits.DistinguishingFeature
                }
            })
        });
    }

    private static async Task<IResult> IssueWarrant(string id, HttpContext context,
        ICaseService caseService, ISuspectService suspectService)
    {
        var gameCase = caseService.GetCase(id);
        if (gameCase == null)
        {
            return Results.NotFound(new { error = "Case not found" });
        }

        var request = await context.Request.ReadFromJsonAsync<WarrantRequest>();
        var suspectId = request?.SuspectId;

        if (string.IsNullOrEmpty(suspectId))
        {
            return Results.BadRequest(new { error = "Suspect ID is required", code = "MISSING_SUSPECT_ID" });
        }

        var suspect = suspectService.GetSuspect(suspectId);
        if (suspect == null)
        {
            return Results.BadRequest(new { error = "Suspect not found", code = "INVALID_SUSPECT" });
        }

        if (gameCase.WarrantIssued)
        {
            return Results.Conflict(new { error = "Warrant already issued for this case", code = "WARRANT_ALREADY_ISSUED" });
        }

        gameCase.WarrantIssued = true;
        gameCase.Warrant = new Api.Models.Warrant
        {
            SuspectId = suspectId,
            CityId = gameCase.CurrentCity,
            IssuedAt = DateTime.UtcNow
        };

        var isCorrectSuspect = suspectId == gameCase.CorrectSuspectId;
        var finalTrailCity = gameCase.Trail.Last();
        var isCorrectCity = gameCase.CurrentCity == finalTrailCity;

        if (isCorrectSuspect && isCorrectCity)
        {
            gameCase.Status = "won";
            return Results.Ok(new
            {
                result = "won",
                caseStatus = "won",
                message = "You caught Carmen Sandiego! The stolen treasure has been recovered.",
                warrant = new
                {
                    suspectId = gameCase.Warrant.SuspectId,
                    cityId = gameCase.Warrant.CityId,
                    issuedAt = gameCase.Warrant.IssuedAt
                }
            });
        }

        gameCase.Status = "lost";

        if (!isCorrectSuspect)
        {
            var correctSuspect = suspectService.GetSuspect(gameCase.CorrectSuspectId);
            return Results.Ok(new
            {
                result = "lost",
                reason = "wrong_suspect",
                caseStatus = "lost",
                correctSuspect = new { id = correctSuspect!.Id, name = correctSuspect.Name }
            });
        }

        // Correct suspect, wrong city
        return Results.Ok(new
        {
            result = "lost",
            reason = "wrong_city",
            caseStatus = "lost",
            warrant = new { cityId = gameCase.Warrant.CityId }
        });
    }
}

public class WarrantRequest
{
    public string? SuspectId { get; set; }
}
