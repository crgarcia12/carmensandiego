using Api.Models;

namespace Api.Services;

public interface INpcChatService
{
    Task<(NpcChatMessage? message, int messageCount, int remainingMessages, string? error, string? code)> ChatAsync(
        GameCase gameCase, string npcId, string message, CancellationToken cancellationToken = default);
    NPC? GetNpc(string npcId);
    bool IsNpcInCity(string npcId, string cityId);
}
