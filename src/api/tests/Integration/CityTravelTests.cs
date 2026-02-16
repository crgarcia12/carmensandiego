// Derived from: city-travel.feature
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace Api.Tests.Integration;

public class CityTravelTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string TestSessionId = "sess-00000000-0000-0000-0000-000000000001";
    private const string TestCaseId = "case-travel-001";

    public CityTravelTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Session-Id", TestSessionId);
    }

    // Derived from: city-travel.feature — "Viewing the current city shows background and NPCs"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnCityInfoAndNpcs_When_GettingCurrentCity()
    {
        // Act
        var response = await _client.GetAsync($"/api/cases/{TestCaseId}/city");

        // Assert — will fail until city endpoint is implemented
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;

        var city = json.GetProperty("city");
        city.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
        city.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
        city.TryGetProperty("region", out _).Should().BeTrue();
        city.TryGetProperty("continent", out _).Should().BeTrue();
        city.TryGetProperty("backgroundKey", out _).Should().BeTrue();

        var npcs = json.GetProperty("npcs");
        npcs.GetArrayLength().Should().BeInRange(2, 3);
        foreach (var npc in npcs.EnumerateArray())
        {
            npc.TryGetProperty("id", out _).Should().BeTrue();
            npc.TryGetProperty("name", out _).Should().BeTrue();
            npc.TryGetProperty("role", out _).Should().BeTrue();
        }

        json.GetProperty("remainingSteps").GetInt32().Should().BeGreaterThan(0);
        json.TryGetProperty("isFinalCity", out _).Should().BeTrue();
    }

    // Derived from: city-travel.feature — "Travel options show 3 cities including 1 correct and 2 decoys"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Show3TravelOptions_When_GettingCurrentCity()
    {
        // Act
        var response = await _client.GetAsync($"/api/cases/{TestCaseId}/city");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;

        var travelOptions = json.GetProperty("travelOptions");
        travelOptions.GetArrayLength().Should().Be(3);
        foreach (var option in travelOptions.EnumerateArray())
        {
            option.TryGetProperty("cityId", out _).Should().BeTrue();
            option.TryGetProperty("cityName", out _).Should().BeTrue();
            option.TryGetProperty("description", out _).Should().BeTrue();
        }
    }

    // Derived from: city-travel.feature — "Traveling to a valid city decrements steps"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_DecrementSteps_When_TravelingToValidCity()
    {
        // Arrange
        var request = new { cityId = "tokyo" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/travel", request);

        // Assert — will fail until travel endpoint is implemented
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("city").GetProperty("id").GetString().Should().Be("tokyo");
        json.GetProperty("remainingSteps").GetInt32().Should().BeLessThan(10);
        json.GetProperty("caseStatus").GetString().Should().Be("active");
    }

    // Derived from: city-travel.feature — "Cannot travel when remaining steps is 0"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_NoRemainingSteps()
    {
        // Arrange — case has 0 remaining steps
        var request = new { cityId = "tokyo" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/travel", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("No remaining steps");
        json.GetProperty("code").GetString().Should().Be("NO_STEPS");
    }

    // Derived from: city-travel.feature — "Cannot travel to a city not in the offered options"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_DestinationNotInOptions()
    {
        // Arrange — london is not in the travel options
        var request = new { cityId = "london" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/travel", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Invalid travel destination");
        json.GetProperty("code").GetString().Should().Be("INVALID_DESTINATION");
    }

    // Derived from: city-travel.feature — "Cannot travel to the current city"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_TravelingToCurrentCity()
    {
        // Arrange — player is already in bangkok
        var request = new { cityId = "bangkok" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/travel", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Already in this city");
        json.GetProperty("code").GetString().Should().Be("SAME_CITY");
    }

    // Derived from: city-travel.feature — "Lose condition triggered when steps reach 0"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_LoseCase_When_StepsReachZero()
    {
        // Arrange — case has 1 remaining step, no warrant issued
        var request = new { cityId = "tokyo" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/travel", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("remainingSteps").GetInt32().Should().Be(0);
        json.GetProperty("caseStatus").GetString().Should().Be("lost");
    }

    // Derived from: city-travel.feature — "No travel options at the final trail city"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnEmptyTravelOptions_When_AtFinalCity()
    {
        // Arrange — player is at final city
        // Act
        var response = await _client.GetAsync($"/api/cases/{TestCaseId}/city");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("travelOptions").GetArrayLength().Should().Be(0);
        json.GetProperty("isFinalCity").GetBoolean().Should().BeTrue();
    }

    // Derived from: city-travel.feature — "Cannot travel when case is completed"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_TravelOnCompletedCase()
    {
        // Arrange — case is already won/lost
        var request = new { cityId = "tokyo" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/travel", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is already completed");
        json.GetProperty("code").GetString().Should().Be("CASE_COMPLETED");
    }
}
