// Derived from: suspect-dossier.feature
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace Api.Tests.Integration;

public class SuspectDossierTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string TestSessionId = "sess-00000000-0000-0000-0000-000000000001";
    private const string TestCaseId = "case-warrant-001";

    public SuspectDossierTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Add("X-Session-Id", TestSessionId);
    }

    // Derived from: suspect-dossier.feature — "Viewing the full suspect dossier returns 10 or more suspects"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnAtLeast10Suspects_When_GettingDossier()
    {
        // Act
        var response = await _client.GetAsync($"/api/cases/{TestCaseId}/suspects");

        // Assert — will fail until suspects endpoint is implemented
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        var suspects = json.GetProperty("suspects");
        suspects.GetArrayLength().Should().BeGreaterThanOrEqualTo(10);
    }

    // Derived from: suspect-dossier.feature — "Each suspect has all required traits"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_HaveAllRequiredTraits_ForEachSuspect()
    {
        // Act
        var response = await _client.GetAsync($"/api/cases/{TestCaseId}/suspects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        var suspects = json.GetProperty("suspects");

        foreach (var suspect in suspects.EnumerateArray())
        {
            suspect.GetProperty("id").GetString().Should().NotBeNullOrEmpty();
            suspect.GetProperty("name").GetString().Should().NotBeNullOrEmpty();
            suspect.GetProperty("photoKey").GetString().Should().NotBeNullOrEmpty();

            var traits = suspect.GetProperty("traits");
            traits.GetProperty("hairColor").GetString().Should().NotBeNullOrEmpty();
            traits.GetProperty("eyeColor").GetString().Should().NotBeNullOrEmpty();
            traits.GetProperty("hobby").GetString().Should().NotBeNullOrEmpty();
            traits.GetProperty("favoriteFood").GetString().Should().NotBeNullOrEmpty();
            traits.GetProperty("vehicle").GetString().Should().NotBeNullOrEmpty();
            traits.GetProperty("distinguishingFeature").GetString().Should().NotBeNullOrEmpty();
        }
    }

    // Derived from: suspect-dossier.feature — "Issuing a warrant for the correct suspect in the correct city wins the case"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_WinCase_When_CorrectWarrantInCorrectCity()
    {
        // Arrange — player in final trail city, correct suspect is suspect-carmen
        var request = new { suspectId = "suspect-carmen" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/warrant", request);

        // Assert — will fail until warrant endpoint is implemented
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("result").GetString().Should().Be("won");
        json.GetProperty("caseStatus").GetString().Should().Be("won");
        json.GetProperty("message").GetString().Should().Contain("Carmen Sandiego");
        json.GetProperty("warrant").GetProperty("suspectId").GetString().Should().Be("suspect-carmen");
        json.GetProperty("warrant").TryGetProperty("cityId", out _).Should().BeTrue();
        json.GetProperty("warrant").TryGetProperty("issuedAt", out _).Should().BeTrue();
    }

    // Derived from: suspect-dossier.feature — "Issuing a warrant for the wrong suspect loses the case"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_LoseCase_When_WrongSuspect()
    {
        // Arrange — correct suspect is suspect-carmen, but player picks suspect-vic
        var request = new { suspectId = "suspect-vic" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/warrant", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("result").GetString().Should().Be("lost");
        json.GetProperty("reason").GetString().Should().Be("wrong_suspect");
        json.GetProperty("caseStatus").GetString().Should().Be("lost");
        json.GetProperty("correctSuspect").GetProperty("id").GetString().Should().Be("suspect-carmen");
        json.GetProperty("correctSuspect").GetProperty("name").GetString().Should().Contain("Carmen");
    }

    // Derived from: suspect-dossier.feature — "Issuing a warrant for the right suspect in the wrong city loses the case"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_LoseCase_When_RightSuspectWrongCity()
    {
        // Arrange — player is in tokyo (not the final trail city)
        var request = new { suspectId = "suspect-carmen" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/warrant", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("result").GetString().Should().Be("lost");
        json.GetProperty("reason").GetString().Should().Be("wrong_city");
        json.GetProperty("caseStatus").GetString().Should().Be("lost");
    }

    // Derived from: suspect-dossier.feature — "Cannot issue a warrant twice for the same case"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return409_When_WarrantAlreadyIssued()
    {
        // Arrange — warrant was already issued
        var request = new { suspectId = "suspect-vic" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/warrant", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Warrant already issued");
        json.GetProperty("code").GetString().Should().Be("WARRANT_ALREADY_ISSUED");
    }

    // Derived from: suspect-dossier.feature — "Invalid suspect ID returns 400"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_SuspectIdInvalid()
    {
        // Arrange
        var request = new { suspectId = "suspect-nonexistent" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/warrant", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Suspect not found");
        json.GetProperty("code").GetString().Should().Be("INVALID_SUSPECT");
    }

    // Derived from: suspect-dossier.feature — "Missing suspect ID in request body returns 400"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_Return400_When_SuspectIdMissing()
    {
        // Arrange — empty body, no suspectId
        var request = new { };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/cases/{TestCaseId}/warrant", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("error").GetString().Should().Contain("Suspect ID is required");
        json.GetProperty("code").GetString().Should().Be("MISSING_SUSPECT_ID");
    }

    // Derived from: suspect-dossier.feature — "Dossier is accessible even after case completion"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnDossier_When_CaseCompleted()
    {
        // Arrange — case is already completed (won)
        // Act
        var response = await _client.GetAsync($"/api/cases/{TestCaseId}/suspects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("suspects").GetArrayLength().Should().BeGreaterThanOrEqualTo(10);
    }
}
