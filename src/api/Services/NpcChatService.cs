using System.Collections.Concurrent;
using Api.Models;

namespace Api.Services;

public class NpcChatService : INpcChatService
{
    private readonly GameDataProvider _dataProvider;
    // Key: "caseId:npcId", Value: chat history
    private readonly ConcurrentDictionary<string, NpcChatHistory> _chatHistories = new();
    private const int MaxMessagesPerNpc = 20;

    public NpcChatService(GameDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public NPC? GetNpc(string npcId)
    {
        foreach (var city in _dataProvider.Cities)
        {
            var npc = city.Npcs.FirstOrDefault(n => n.Id == npcId);
            if (npc != null) return npc;
        }
        return null;
    }

    public bool IsNpcInCity(string npcId, string cityId)
    {
        var city = _dataProvider.GetCity(cityId);
        return city?.Npcs.Any(n => n.Id == npcId) ?? false;
    }

    public (NpcChatMessage? message, int messageCount, int remainingMessages, string? error, string? code) Chat(
        GameCase gameCase, string npcId, string message)
    {
        var key = $"{gameCase.Id}:{npcId}";
        var history = _chatHistories.GetOrAdd(key, _ => new NpcChatHistory());

        if (history.Messages.Count >= MaxMessagesPerNpc)
        {
            return (null, history.Messages.Count, 0, "Conversation limit reached with this NPC", "CHAT_CAP_REACHED");
        }

        var npc = GetNpc(npcId)!;

        // Add player message to history (counts toward cap)
        history.Messages.Add(new NpcChatMessage
        {
            NpcId = npcId,
            NpcName = npc.Name,
            Text = message,
            Timestamp = DateTime.UtcNow
        });

        // Generate NPC response
        var responseText = GenerateResponse(npc, message);
        var npcMessage = new NpcChatMessage
        {
            NpcId = npcId,
            NpcName = npc.Name,
            Text = responseText,
            Timestamp = DateTime.UtcNow
        };
        history.Messages.Add(npcMessage);

        var remaining = MaxMessagesPerNpc - history.Messages.Count;
        return (npcMessage, history.Messages.Count, Math.Max(0, remaining), null, null);
    }

    private static string GenerateResponse(NPC npc, string playerMessage)
    {
        var responses = new[]
        {
            $"Hmm, interesting question. As a {npc.Role}, I see many things around here.",
            $"I did notice someone suspicious passing through recently. They seemed to be in a hurry.",
            $"You're asking the right person! I've heard rumors about a mysterious figure in a red coat.",
            $"Let me think... Yes, I recall seeing someone matching that description heading to the airport.",
            $"As a {npc.Role} here in the city, I keep my eyes open. There was definitely someone unusual here.",
        };
        var index = Math.Abs(playerMessage.GetHashCode()) % responses.Length;
        return responses[index];
    }
}
