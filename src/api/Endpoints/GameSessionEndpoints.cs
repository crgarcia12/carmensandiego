using Api.Services;

namespace Api.Endpoints;

public static class GameSessionEndpoints
{
    public static void MapGameSessionEndpoints(this WebApplication app)
    {
        app.MapPost("/api/sessions", CreateSession);
        app.MapGet("/api/sessions/{id}", ResumeSession);
        app.MapDelete("/api/sessions/{id}", DeleteSession);
    }

    private static async Task CreateSession(HttpContext context, IGameSessionService sessionService)
    {
        if (sessionService.SessionCount >= 1000)
        {
            context.Response.StatusCode = 503;
            context.Response.Headers["Retry-After"] = "60";
            await context.Response.WriteAsJsonAsync(new { error = "Server at capacity", code = "MAX_SESSIONS_REACHED", retryAfter = 60 });
            return;
        }

        var session = sessionService.CreateSession();
        context.Response.StatusCode = 201;
        context.Response.Headers["X-Session-Id"] = session.Id;
        await context.Response.WriteAsJsonAsync(new { id = session.Id, status = session.Status, createdAt = session.CreatedAt });
    }

    private static IResult ResumeSession(string id, IGameSessionService sessionService)
    {
        if (sessionService.IsExpired(id))
        {
            return Results.Json(
                new { error = "Session expired", code = "SESSION_EXPIRED" },
                statusCode: 410);
        }

        var session = sessionService.GetSession(id);
        if (session == null)
        {
            return Results.NotFound(new { error = "Session not found" });
        }

        session.LastAccessedAt = DateTime.UtcNow;

        return Results.Ok(new { id = session.Id, status = session.Status, lastAccessedAt = session.LastAccessedAt });
    }

    private static IResult DeleteSession(string id, IGameSessionService sessionService)
    {
        if (!sessionService.DeleteSession(id))
        {
            return Results.NotFound(new { error = "Session not found" });
        }
        return Results.NoContent();
    }
}
