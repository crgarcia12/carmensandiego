using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Api.Models;

namespace Api.Services;

public class InMemoryGameSessionService : IGameSessionService
{
    private readonly ConcurrentDictionary<string, GameSession> _sessions = new();
    private const int MaxSessions = 1000;
    private static readonly Regex SessionIdPattern = new(@"^sess-[0-9a-zA-Z]{8}-[0-9a-zA-Z]{4}-[0-9a-zA-Z]{4}-[0-9a-zA-Z]{4}-[0-9a-zA-Z]{12}$");

    public InMemoryGameSessionService()
    {
        // Pre-seed an expired session for testing
        _sessions["sess-00000000-0000-0000-0000-expired00001"] = new GameSession
        {
            Id = "sess-00000000-0000-0000-0000-expired00001",
            Status = "active",
            CreatedAt = DateTime.UtcNow.AddHours(-25),
            LastAccessedAt = DateTime.UtcNow.AddHours(-25)
        };
    }

    public int SessionCount => _sessions.Count;

    public bool IsValidFormat(string sessionId)
    {
        return SessionIdPattern.IsMatch(sessionId);
    }

    public GameSession CreateSession()
    {
        var session = new GameSession
        {
            Id = $"sess-{Guid.NewGuid()}",
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            LastAccessedAt = DateTime.UtcNow
        };
        _sessions[session.Id] = session;
        return session;
    }

    public GameSession? GetSession(string sessionId)
    {
        _sessions.TryGetValue(sessionId, out var session);
        return session;
    }

    public void TouchSession(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.LastAccessedAt = DateTime.UtcNow;
        }
    }

    public bool DeleteSession(string sessionId)
    {
        return _sessions.TryRemove(sessionId, out _);
    }

    public bool IsExpired(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            return (DateTime.UtcNow - session.LastAccessedAt).TotalHours >= 24;
        }
        // Session doesn't exist — treat as expired
        return true;
    }

    public void EnsureSession(string sessionId)
    {
        if (!_sessions.ContainsKey(sessionId))
        {
            _sessions[sessionId] = new GameSession
            {
                Id = sessionId,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow
            };
        }
        else
        {
            _sessions[sessionId].LastAccessedAt = DateTime.UtcNow;
        }
    }
}
