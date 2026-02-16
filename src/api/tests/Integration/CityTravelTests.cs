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

    public CityTravelTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Derived from: city-travel.feature — "Viewing the current city shows background and NPCs"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnCityInfoAndNpcs_When_GettingCurrentCity()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act
        var json = await TestHelper.GetCityAsync(_client, sessionId, caseId);

        // Assert
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
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act
        var json = await TestHelper.GetCityAsync(_client, sessionId, caseId);

        // Assert
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
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Get travel options to pick a valid destination
        var cityJson = await TestHelper.GetCityAsync(_client, sessionId, caseId);
        var options = cityJson.GetProperty("travelOptions").EnumerateArray().ToList();
        var targetCityId = options[0].GetProperty("cityId").GetString()!;

        // Act
        var response = await TestHelper.TravelAsync(_client, sessionId, caseId, targetCityId);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("city").GetProperty("id").GetString().Should().Be(targetCityId);
        json.GetProperty("remainingSteps").GetInt32().Should().BeLessThan(10);
        json.GetProperty("caseStatus").GetString().Should().Be("active");
    }

    // Derived from: city-travel.feature — "Cannot travel when remaining steps is 0"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_NoRemainingSteps()
    {
        // Arrange — create case and exhaust all steps
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.ExhaustStepsAsync(_client, sessionId, caseId);

        // Act — try to travel again
        var response = await TestHelper.TravelAsync(_client, sessionId, caseId, "tokyo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is already completed");
        json.GetProperty("code").GetString().Should().Be("CASE_COMPLETED");
    }

    // Derived from: city-travel.feature — "Cannot travel to a city not in the offered options"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_DestinationNotInOptions()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Get travel options so we can pick a city NOT in them
        var cityJson = await TestHelper.GetCityAsync(_client, sessionId, caseId);
        var options = cityJson.GetProperty("travelOptions")
            .EnumerateArray()
            .Select(o => o.GetProperty("cityId").GetString())
            .ToHashSet();
        var currentCity = cityJson.GetProperty("city").GetProperty("id").GetString();

        // Pick a city not in options and not current city
        var allCities = new[] { "bangkok", "tokyo", "paris", "cairo", "rio", "new-york", "london", "sydney", "mumbai", "moscow", "nairobi", "istanbul", "mexico-city", "beijing", "rome" };
        var invalidCity = allCities.First(c => !options.Contains(c) && c != currentCity);

        // Act
        var response = await TestHelper.TravelAsync(_client, sessionId, caseId, invalidCity);

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
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Get the current city
        var cityJson = await TestHelper.GetCityAsync(_client, sessionId, caseId);
        var currentCityId = cityJson.GetProperty("city").GetProperty("id").GetString()!;

        // Act — travel to current city
        var response = await TestHelper.TravelAsync(_client, sessionId, caseId, currentCityId);

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
        // Arrange — create case and travel until 1 step remains
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.TravelUntilOneStepAsync(_client, sessionId, caseId);

        // Get a valid travel option for the final move
        var cityJson = await TestHelper.GetCityAsync(_client, sessionId, caseId);
        var options = cityJson.GetProperty("travelOptions").EnumerateArray().ToList();
        options.Should().NotBeEmpty("should have travel options with 1 step remaining");
        var targetCityId = options[0].GetProperty("cityId").GetString()!;

        // Act — this travel should consume the last step
        var response = await TestHelper.TravelAsync(_client, sessionId, caseId, targetCityId);

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
        // Arrange — create case and travel to final city
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.TravelToFinalCityAsync(_client, sessionId, caseId);

        // Act
        var json = await TestHelper.GetCityAsync(_client, sessionId, caseId);

        // Assert
        json.GetProperty("travelOptions").GetArrayLength().Should().Be(0);
        json.GetProperty("isFinalCity").GetBoolean().Should().BeTrue();
    }

    // Derived from: city-travel.feature — "Cannot travel when case is completed"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_TravelOnCompletedCase()
    {
        // Arrange — create case and lose it by exhausting steps
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.ExhaustStepsAsync(_client, sessionId, caseId);

        // Act — attempt travel on completed case
        var response = await TestHelper.TravelAsync(_client, sessionId, caseId, "tokyo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Case is already completed");
        json.GetProperty("code").GetString().Should().Be("CASE_COMPLETED");
    }
}
