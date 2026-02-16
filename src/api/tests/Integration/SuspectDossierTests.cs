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

    public SuspectDossierTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // Derived from: suspect-dossier.feature — "Viewing the full suspect dossier returns 10 or more suspects"
    [Fact]
    [Trait("Category", "Integration")]
    public async Task Should_ReturnAtLeast10Suspects_When_GettingDossier()
    {
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}/suspects");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
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
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}/suspects");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

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
        // Arrange — create case, travel to final city, then issue correct warrant
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.TravelToFinalCityAsync(_client, sessionId, caseId);

        // Act — issue warrant for the correct suspect (all generated cases use suspect-carmen)
        var response = await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-carmen");

        // Assert
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
        // Arrange — create case, issue wrong warrant
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act — issue warrant for wrong suspect
        var response = await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-vic");

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
        // Arrange — create case (player starts at first city, not final city)
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act — issue warrant for correct suspect but from wrong city (starting city)
        var response = await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-carmen");

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
        // Arrange — create case and issue a warrant first
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        var firstWarrant = await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-vic");
        firstWarrant.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act — try to issue a second warrant
        var response = await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-vic");

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
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act
        var response = await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-nonexistent");

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
        // Arrange
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);

        // Act — send empty body
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/cases/{caseId}/warrant");
        request.Headers.Add("X-Session-Id", sessionId);
        request.Content = JsonContent.Create(new { });
        var response = await _client.SendAsync(request);

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
        // Arrange — create case, complete it by issuing a warrant
        var sessionId = await TestHelper.CreateSessionAsync(_client);
        var (caseId, _) = await TestHelper.CreateCaseAsync(_client, sessionId);
        await TestHelper.IssueWarrantAsync(_client, sessionId, caseId, "suspect-vic");

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}/suspects");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("suspects").GetArrayLength().Should().BeGreaterThanOrEqualTo(10);
    }
}
