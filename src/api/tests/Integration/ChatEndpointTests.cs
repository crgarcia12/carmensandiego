// Derived from: npc-conversations.feature
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace Api.Tests.Integration;

public class NpcConversationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public NpcConversationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>Gets an NPC ID in the current city for the given case.</summary>
    private async Task<string> GetNpcInCurrentCityAsync(string sessionId, string caseId)
    {
        var cityJson = await TestHelper.GetCityAsync(_client, sessionId, caseId);
        var npcs = cityJson.GetProperty("npcs").EnumerateArray().ToList();
        return npcs[0].GetProperty("id").GetString()!;
    }

    // Derived from: npc-conversations.feature — "Sending a message to an NPC returns an AI response"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnNpcResponse_When_SendingValidMessage()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var npcId = await GetNpcInCurrentCityAsync(sessionId, caseId);

        // Act
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, npcId, "Have you seen anyone suspicious?");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("npcMessage").GetProperty("npcId").GetString().Should().Be(npcId);
        json.GetProperty("npcMessage").GetProperty("npcName").GetString().Should().NotBeNullOrEmpty();
        json.GetProperty("npcMessage").GetProperty("text").GetString().Should().NotBeNullOrEmpty();
        json.GetProperty("npcMessage").TryGetProperty("timestamp", out _).Should().BeTrue();
        json.GetProperty("chatHistory").TryGetProperty("messageCount", out _).Should().BeTrue();
        json.GetProperty("chatHistory").TryGetProperty("remainingMessages", out _).Should().BeTrue();
    }

    // Derived from: npc-conversations.feature — "Chat input is limited to 280 characters"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_MessageExceeds280Characters()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var npcId = await GetNpcInCurrentCityAsync(sessionId, caseId);
        var longMessage = new string('A', 281);

        // Act
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, npcId, longMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("280 character limit");
        json.GetProperty("code").GetString().Should().Be("MESSAGE_TOO_LONG");
    }

    // Derived from: npc-conversations.feature — "Chat input at exactly 280 characters is accepted"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Accept_When_MessageIsExactly280Characters()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var npcId = await GetNpcInCurrentCityAsync(sessionId, caseId);
        var exactMessage = new string('A', 280);

        // Act
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, npcId, exactMessage);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("npcMessage").GetProperty("text").GetString().Should().NotBeNullOrEmpty();
    }

    // Derived from: npc-conversations.feature — "Conversation history is limited to 20 messages"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return429_When_MessageCapReached()
    {
        // Arrange — send 10 messages to fill the cap (10 player + 10 NPC = 20 total)
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var npcId = await GetNpcInCurrentCityAsync(sessionId, caseId);

        for (int i = 0; i < 10; i++)
        {
            var resp = await TestHelper.ChatAsync(_client, sessionId, caseId, npcId, $"Message {i}");
            resp.StatusCode.Should().Be(HttpStatusCode.OK, $"Pre-fill message {i} failed");
        }

        // Act — 11th message should be rejected (20 messages already in history)
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, npcId, "One more question");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Conversation limit reached");
        json.GetProperty("code").GetString().Should().Be("CHAT_CAP_REACHED");
    }

    // Derived from: npc-conversations.feature — "Empty message is rejected"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_MessageIsEmpty()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var npcId = await GetNpcInCurrentCityAsync(sessionId, caseId);

        // Act
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, npcId, "");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Message cannot be empty");
        json.GetProperty("code").GetString().Should().Be("EMPTY_MESSAGE");
    }

    // Derived from: npc-conversations.feature — "Chat with nonexistent NPC returns 404"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return404_When_NpcDoesNotExist()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, "npc-nonexistent", "Hello");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("NPC not found");
        json.GetProperty("code").GetString().Should().Be("NPC_NOT_FOUND");
    }

    // Derived from: npc-conversations.feature — "Cannot chat with an NPC in a different city"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_NpcIsInDifferentCity()
    {
        // Arrange — get the NPC list for current city, then pick an NPC NOT in that city
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var cityJson = await TestHelper.GetCityAsync(_client, sessionId, caseId);
        var currentCityNpcs = cityJson.GetProperty("npcs")
            .EnumerateArray()
            .Select(n => n.GetProperty("id").GetString())
            .ToHashSet();

        // Pick an NPC that exists but is NOT in the current city
        var allNpcs = new[] { "npc-somchai", "npc-yuki", "npc-pierre", "npc-hassan", "npc-carlos", "npc-mike", "npc-arthur", "npc-bruce", "npc-priya", "npc-ivan" };
        var wrongCityNpc = allNpcs.First(n => !currentCityNpcs.Contains(n));

        // Act
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, wrongCityNpc, "Hello");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("NPC is not in your current city");
        json.GetProperty("code").GetString().Should().Be("NPC_WRONG_CITY");
    }

    // Derived from: npc-conversations.feature — "Cannot chat when case is completed"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_CaseIsCompleted()
    {
        // Arrange — create case, get NPC, then lose the case
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var npcId = await GetNpcInCurrentCityAsync(sessionId, caseId);
        // Lose the case by issuing wrong warrant
        await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-vic");

        // Act — try to chat on completed case
        var response = await TestHelper.ChatAsync(_client, sessionId, caseId, npcId, "Hello");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is already completed");
        json.GetProperty("code").GetString().Should().Be("CASE_COMPLETED");
    }
}
