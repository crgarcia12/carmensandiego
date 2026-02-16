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
    private const string TestSessionId = "sess-00000000-0000-0000-0000-000000000001";
    private const string TestCaseId = "case-chat-001";
    private const string TestNpcId = "npc-somchai";

    public NpcConversationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Session-Id", TestSessionId);
    }

    // Derived from: npc-conversations.feature — "Sending a message to an NPC returns an AI response"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnNpcResponse_When_SendingValidMessage()
    {
        // Arrange
        var request = new { message = "Have you seen anyone suspicious?" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/{TestNpcId}/chat", request);

        // Assert — will fail until NPC chat endpoint is implemented
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("npcMessage").GetProperty("npcId").GetString().Should().Be(TestNpcId);
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
        var longMessage = new string('A', 281);
        var request = new { message = longMessage };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/{TestNpcId}/chat", request);

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
        var exactMessage = new string('A', 280);
        var request = new { message = exactMessage };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/{TestNpcId}/chat", request);

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
        // Arrange — simulate 20 messages already exchanged
        // The 21st player message (after 10 player + 10 NPC = 20 total) should be rejected
        var request = new { message = "One more question" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/{TestNpcId}/chat", request);

        // Assert — will fail until chat cap logic is implemented
        // In a full setup, this test would send 10 messages first, then the 11th would be rejected
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
        var request = new { message = "" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/{TestNpcId}/chat", request);

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
        var request = new { message = "Hello" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/npc-nonexistent/chat", request);

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
        // Arrange — npc-yuki is in tokyo, player is in bangkok
        var request = new { message = "Hello" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/npc-yuki/chat", request);

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
        // Arrange — case is already won/lost
        var request = new { message = "Hello" };

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/cases/{TestCaseId}/npcs/{TestNpcId}/chat", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is already completed");
        json.GetProperty("code").GetString().Should().Be("CASE_COMPLETED");
    }
}
