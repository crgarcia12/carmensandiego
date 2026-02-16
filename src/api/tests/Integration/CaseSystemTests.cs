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
    private const string TestSessionId = "sess-00000000-0000-0000-0000-000000000001";

    public CaseSystemTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Session-Id", TestSessionId);
    }

    // Derived from: case-system.feature — "Starting a new case returns briefing info"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return201WithCaseInfo_When_CreatingNewCase()
    {
        // Act
        var response = await _client.PostAsync("/api/cases", null);

        // Assert — will fail until POST /api/cases is implemented
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
        // Act
        var response = await _client.PostAsync("/api/cases", null);

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
        // Arrange — assume case-win-001 exists in final city with correct suspect
        var caseId = "case-win-001";

        // Act
        var response = await _client.GetAsync($"/api/cases/{caseId}/summary");

        // Assert — will fail until summary endpoint and win logic are implemented
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
        // Arrange — assume case-lose-001 exists with 0 steps remaining
        var caseId = "case-lose-001";

        // Act
        var response = await _client.GetAsync($"/api/cases/{caseId}/summary");

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
        // Act
        var response = await _client.GetAsync("/api/cases/case-nonexistent");

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
        // Arrange — session already has an active case
        // Act
        var response = await _client.PostAsync("/api/cases", null);

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
        // Arrange — case is already completed
        var caseId = "case-done-001";
        var travelRequest = new { cityId = "tokyo" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{caseId}/travel", travelRequest);

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
        // Arrange — case is still active
        var caseId = "case-active-001";

        // Act
        var response = await _client.GetAsync($"/api/cases/{caseId}/summary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is still active");
    }
}
