using Api.Models;

namespace Api.Services;

public interface INpcChatService
{
    (NpcChatMessage? message, int messageCount, int remainingMessages, string? error, string? code) Chat(
        GameCase gameCase, string npcId, string message);
    NPC? GetNpc(string npcId);
    bool IsNpcInCity(string npcId, string cityId);
}
