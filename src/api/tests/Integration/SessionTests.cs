// Derived from: session-management.feature
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace Api.Tests.Integration;

public class GameSessionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GameSessionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Derived from: session-management.feature — "Creating a new session"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return201WithSessionId_When_CreatingNewSession()
    {
        // Act
        var response = await _client.PostAsync("/api/sessions", null);

        // Assert — will fail until game session endpoint is implemented
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        var id = json.GetProperty("id").GetString();
        id.Should().NotBeNullOrEmpty();
        id.Should().StartWith("sess-");
        // Validate UUID portion after "sess-"
        Guid.TryParse(id!.Substring(5), out _).Should().BeTrue("session ID should be 'sess-' followed by a UUID");
        json.GetProperty("status").GetString().Should().Be("active");
        json.TryGetProperty("createdAt", out _).Should().BeTrue();
        response.Headers.Contains("X-Session-Id").Should().BeTrue();
        response.Headers.GetValues("X-Session-Id").First().Should().Be(id);
    }

    // Derived from: session-management.feature — "Resuming an existing session"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return200_When_ResumingExistingSession()
    {
        // Arrange — create a session first
        var createResponse = await _client.PostAsync("/api/sessions", null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonDocument.Parse(createBody).RootElement.GetProperty("id").GetString();

        // Act
        var response = await _client.GetAsync($"/api/sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("id").GetString().Should().Be(sessionId);
        json.GetProperty("status").GetString().Should().Be("active");
        json.TryGetProperty("lastAccessedAt", out _).Should().BeTrue();
    }

    // Derived from: session-management.feature — "Session expires after 24 hours of inactivity"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return410_When_SessionExpired()
    {
        // Arrange — expired session
        // Act
        _client.DefaultRequestHeaders.Add("X-Session-Id", "sess-00000000-0000-0000-0000-expired00001");
        var response = await _client.GetAsync("/api/cases/case-001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Gone);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Session expired");
        json.GetProperty("code").GetString().Should().Be("SESSION_EXPIRED");
    }

    // Derived from: session-management.feature — "Maximum concurrent sessions returns HTTP 503"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return503_When_MaxSessionsReached()
    {
        // Act — attempt to create session when at capacity
        var response = await _client.PostAsync("/api/sessions", null);

        // Assert — will fail until max sessions logic is implemented
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Server at capacity");
        json.GetProperty("code").GetString().Should().Be("MAX_SESSIONS_REACHED");
        json.GetProperty("retryAfter").GetInt32().Should().Be(60);
        response.Headers.Contains("Retry-After").Should().BeTrue();
        response.Headers.GetValues("Retry-After").First().Should().Be("60");
    }

    // Derived from: session-management.feature — "Deleting a session"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return204_When_DeletingSession()
    {
        // Arrange — create session
        var createResponse = await _client.PostAsync("/api/sessions", null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createBody = await createResponse.Content.ReadAsStringAsync();
        var sessionId = JsonDocument.Parse(createBody).RootElement.GetProperty("id").GetString();

        // Act
        var response = await _client.DeleteAsync($"/api/sessions/{sessionId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    // Derived from: session-management.feature — "Deleting a nonexistent session returns 404"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return404_When_DeletingNonexistentSession()
    {
        // Act
        var response = await _client.DeleteAsync("/api/sessions/sess-00000000-0000-0000-0000-doesnotexist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Derived from: session-management.feature — "Missing session ID header on protected endpoint returns 401"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return401_When_SessionIdHeaderMissing()
    {
        // Arrange — client without X-Session-Id header
        // Act
        var response = await _client.GetAsync("/api/cases/case-001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Session ID required");
        json.GetProperty("code").GetString().Should().Be("MISSING_SESSION");
    }

    // Derived from: session-management.feature — "Invalid session ID format returns 400"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_SessionIdFormatInvalid()
    {
        // Arrange
        _client.DefaultRequestHeaders.Add("X-Session-Id", "not-a-valid-uuid");

        // Act
        var response = await _client.GetAsync("/api/cases/case-001");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Invalid session ID format");
        json.GetProperty("code").GetString().Should().Be("INVALID_SESSION");
    }
}
