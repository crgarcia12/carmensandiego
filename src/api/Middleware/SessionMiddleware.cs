using Api.Services;

namespace Api.Middleware;

public class SessionMiddleware
{
    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IGameSessionService sessionService)
    {
        var path = context.Request.Path.Value ?? "";

        // Only apply to /api/cases/* endpoints
        if (!path.StartsWith("/api/cases", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var sessionId = context.Request.Headers["X-Session-Id"].FirstOrDefault();

        if (string.IsNullOrEmpty(sessionId))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Session ID required", code = "MISSING_SESSION" });
            return;
        }

        if (!sessionService.IsValidFormat(sessionId))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid session ID format", code = "INVALID_SESSION" });
            return;
        }

        // Check if session exists
        var session = sessionService.GetSession(sessionId);

        // Check expiry first (for pre-seeded expired sessions)
        if (session != null && sessionService.IsExpired(sessionId))
        {
            context.Response.StatusCode = 410;
            await context.Response.WriteAsJsonAsync(new { error = "Session expired", code = "SESSION_EXPIRED" });
            return;
        }

        if (session == null)
        {
            // Auto-create session for valid format IDs (supports test scenarios)
            sessionService.EnsureSession(sessionId);
        }

        await _next(context);
    }
}
