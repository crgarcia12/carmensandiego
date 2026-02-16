using Api.Models;

namespace Api.Services;

public interface IGameSessionService
{
    GameSession? GetSession(string sessionId);
    GameSession CreateSession();
    bool DeleteSession(string sessionId);
    bool IsExpired(string sessionId);
    bool IsValidFormat(string sessionId);
    int SessionCount { get; }
    void EnsureSession(string sessionId);
}
