using Api.Services;

namespace Api.Endpoints;

public static class NpcEndpoints
{
    public static void MapNpcEndpoints(this WebApplication app)
    {
        app.MapPost("/api/cases/{id}/npcs/{npcId}/chat", ChatWithNpc);
    }

    private static async Task<IResult> ChatWithNpc(string id, string npcId, HttpContext context,
        ICaseService caseService, INpcChatService npcChatService)
    {
        var gameCase = caseService.GetCase(id);
        if (gameCase == null)
        {
            return Results.NotFound(new { error = "Case not found" });
        }

        if (gameCase.Status != "active")
        {
            return Results.Conflict(new { error = "Case is already completed", code = "CASE_COMPLETED" });
        }

        var request = await context.Request.ReadFromJsonAsync<NpcChatRequest>();
        var message = request?.Message;

        if (string.IsNullOrEmpty(message))
        {
            return Results.BadRequest(new { error = "Message cannot be empty", code = "EMPTY_MESSAGE" });
        }

        if (message.Length > 280)
        {
            return Results.BadRequest(new { error = "Message exceeds 280 character limit", code = "MESSAGE_TOO_LONG" });
        }

        var npc = npcChatService.GetNpc(npcId);
        if (npc == null)
        {
            return Results.NotFound(new { error = "NPC not found", code = "NPC_NOT_FOUND" });
        }

        if (!npcChatService.IsNpcInCity(npcId, gameCase.CurrentCity))
        {
            return Results.BadRequest(new { error = "NPC is not in your current city", code = "NPC_WRONG_CITY" });
        }

        var (npcMessage, messageCount, remainingMessages, error, code) =
            npcChatService.Chat(gameCase, npcId, message);

        if (npcMessage == null)
        {
            var statusCode = code switch
            {
                "CHAT_CAP_REACHED" => 429,
                _ => 400
            };
            return Results.Json(new { error, code }, statusCode: statusCode);
        }

        return Results.Ok(new
        {
            npcMessage = new
            {
                npcId = npcMessage.NpcId,
                npcName = npcMessage.NpcName,
                text = npcMessage.Text,
                timestamp = npcMessage.Timestamp
            },
            chatHistory = new
            {
                messageCount = messageCount,
                remainingMessages = remainingMessages
            }
        });
    }
}

public class NpcChatRequest
{
    public string Message { get; set; } = string.Empty;
}
