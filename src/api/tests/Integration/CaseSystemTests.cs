// Derived from: case-system.feature
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace Api.Tests.Integration;

public class CaseSystemTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CaseSystemTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Derived from: case-system.feature — "Starting a new case returns briefing info"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return201WithCaseInfo_When_CreatingNewCase()
    {
        // Arrange — create own session
        var sessionId = await TestHelper.CreateSessionAsync(_client);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/cases");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("id").GetString().Should().StartWith("case-");
        json.GetProperty("title").GetString().Should().NotBeNullOrEmpty();
        json.GetProperty("briefing").GetString().Should().NotBeNullOrEmpty();
        json.GetProperty("stolenTreasure").TryGetProperty("name", out _).Should().BeTrue();
        json.GetProperty("stolenTreasure").TryGetProperty("description", out _).Should().BeTrue();
        json.TryGetProperty("currentCity", out _).Should().BeTrue();
        json.GetProperty("remainingSteps").GetInt32().Should().Be(10);
        json.GetProperty("status").GetString().Should().Be("active");
    }

    // Derived from: case-system.feature — "New case has correct structure"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_HaveCorrectStructure_When_CaseCreated()
    {
        // Arrange — create own session
        var sessionId = await TestHelper.CreateSessionAsync(_client);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/cases");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;

        // Trail should have 4-6 cities
        if (json.TryGetProperty("trail", out var trail))
        {
            trail.GetArrayLength().Should().BeInRange(4, 6);
        }

        json.GetProperty("remainingSteps").GetInt32().Should().Be(10);
        json.GetProperty("correctSuspectId").GetString().Should().NotBeNullOrEmpty();
        json.GetProperty("currentCityIndex").GetInt32().Should().Be(0);

        var visitedCities = json.GetProperty("visitedCities");
        visitedCities.GetArrayLength().Should().Be(1);

        json.GetProperty("warrantIssued").GetBoolean().Should().BeFalse();
        json.GetProperty("status").GetString().Should().Be("active");
    }

    // Derived from: case-system.feature — "Case completion summary on win"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnWinSummary_When_CaseWon()
    {
        // Arrange — create session, case, travel to final city, win with correct warrant
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.TravelToFinalCityAsync(_client, sessionId, caseId);
        var warrantResp = await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-carmen");
        warrantResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}/summary");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("outcome").GetString().Should().Be("won");
        json.GetProperty("citiesVisited").GetArrayLength().Should().BeGreaterThan(0);
        json.GetProperty("stepsUsed").GetInt32().Should().BeInRange(1, 10);
        json.GetProperty("totalSteps").GetInt32().Should().Be(10);
        json.GetProperty("correctSuspect").TryGetProperty("id", out _).Should().BeTrue();
        json.GetProperty("playerWarrant").TryGetProperty("suspectId", out _).Should().BeTrue();
        json.GetProperty("stolenTreasure").TryGetProperty("name", out _).Should().BeTrue();
        json.GetProperty("stolenTreasure").TryGetProperty("description", out _).Should().BeTrue();
    }

    // Derived from: case-system.feature — "Case completion summary on lose by step exhaustion"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnLoseSummary_When_StepsExhausted()
    {
        // Arrange — create session, case, exhaust all steps
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.ExhaustStepsAsync(_client, sessionId, caseId);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}/summary");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("outcome").GetString().Should().Be("lost");
        json.GetProperty("stepsUsed").GetInt32().Should().Be(10);
        json.GetProperty("correctSuspect").TryGetProperty("name", out _).Should().BeTrue();
        // playerWarrant should be null when no warrant was issued
        json.GetProperty("playerWarrant").ValueKind.Should().Be(JsonValueKind.Null);
    }

    // Derived from: case-system.feature — "Case not found returns 404"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return404_When_CaseNotFound()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/cases/case-nonexistent");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case not found");
    }

    // Derived from: case-system.feature — "Cannot create a case when session already has an active case"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_ActiveCaseAlreadyExists()
    {
        // Arrange — create session with an active case
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act — try to create a second case
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/cases");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Active case already exists");
    }

    // Derived from: case-system.feature — "Actions rejected on completed case"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_ActionsOnCompletedCase()
    {
        // Arrange — create session, case, lose it by exhausting steps
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.ExhaustStepsAsync(_client, sessionId, caseId);

        // Act — try to travel on the completed case
        var cityJson = await TestHelper.GetCityAsync(_client, sessionId, caseId);
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/cases/{caseId}/travel");
        request.Headers.Add("X-Session-Id", sessionId);
        request.Content = JsonContent.Create(new { cityId = "tokyo" });
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is already completed");
    }

    // Derived from: case-system.feature — "Summary not available for active case"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_SummaryRequestedForActiveCase()
    {
        // Arrange — create session and active case
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}/summary");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is still active");
    }
}
