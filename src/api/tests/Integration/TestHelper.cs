using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace Api.Tests.Integration;

/// <summary>
/// Shared helpers for integration tests — each test creates its own session and case.
/// </summary>
public static class TestHelper
{
    /// <summary>Creates a new session via the API. Returns the session ID.</summary>
    public static async Task<string> CreateSessionAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/sessions", null);
        response.StatusCode.Should().Be(HttpStatusCode.Created, "CreateSession helper failed");
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        return json.GetProperty("id").GetString()!;
    }

    /// <summary>Creates a new case for the given session. Returns (caseId, caseJson).</summary>
    public static async Task<(string CaseId, JsonElement Json)> CreateCaseAsync(HttpClient client, string sessionId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/cases");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Created, "CreateCase helper failed");
        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body).RootElement;
        return (json.GetProperty("id").GetString()!, json);
    }

    /// <summary>Gets the current city info for a case. Returns the JSON response.</summary>
    public static async Task<JsonElement> GetCityAsync(HttpClient client, string sessionId, string caseId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}/city");
        request.Headers.Add("X-Session-Id", sessionId);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK, "GetCity helper failed");
        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement;
    }

    /// <summary>Travels to a city. Returns the response.</summary>
    public static async Task<HttpResponseMessage> TravelAsync(HttpClient client, string sessionId, string caseId, string cityId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/cases/{caseId}/travel");
        request.Headers.Add("X-Session-Id", sessionId);
        request.Content = JsonContent.Create(new { cityId });
        return await client.SendAsync(request);
    }

    /// <summary>Travels along the trail until the player reaches the final city.</summary>
    public static async Task TravelToFinalCityAsync(HttpClient client, string sessionId, string caseId)
    {
        // Get case info to learn the trail
        using var caseReq = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}");
        caseReq.Headers.Add("X-Session-Id", sessionId);
        var caseResp = await client.SendAsync(caseReq);
        var caseBody = await caseResp.Content.ReadAsStringAsync();
        var caseJson = JsonDocument.Parse(caseBody).RootElement;
        var trail = caseJson.GetProperty("trail").EnumerateArray().Select(e => e.GetString()!).ToList();

        // Travel along each trail city from index 1 onward
        for (int i = 1; i < trail.Count; i++)
        {
            var resp = await TravelAsync(client, sessionId, caseId, trail[i]);
            resp.StatusCode.Should().Be(HttpStatusCode.OK, $"Travel to {trail[i]} failed");
        }
    }

    /// <summary>Travels until exactly 1 step remains.</summary>
    public static async Task TravelUntilOneStepAsync(HttpClient client, string sessionId, string caseId)
    {
        // Get case info to learn the trail so we can avoid trail cities
        using var caseReq = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}");
        caseReq.Headers.Add("X-Session-Id", sessionId);
        var caseResp = await client.SendAsync(caseReq);
        var caseBody = await caseResp.Content.ReadAsStringAsync();
        var caseJson = JsonDocument.Parse(caseBody).RootElement;
        var trail = caseJson.GetProperty("trail").EnumerateArray().Select(e => e.GetString()!).ToHashSet();

        for (int i = 0; i < 9; i++)
        {
            var cityJson = await GetCityAsync(client, sessionId, caseId);
            var remaining = cityJson.GetProperty("remainingSteps").GetInt32();
            if (remaining <= 1) break;
            var currentCityId = cityJson.GetProperty("city").GetProperty("id").GetString();
            var options = cityJson.GetProperty("travelOptions").EnumerateArray().ToList();
            if (options.Count == 0) break;

            // Prefer a non-trail, non-current city to avoid advancing or revisiting
            var targetCityId = options
                .Select(o => o.GetProperty("cityId").GetString()!)
                .FirstOrDefault(c => c != currentCityId && !trail.Contains(c));
            // Fall back to any non-current city
            targetCityId ??= options
                .Select(o => o.GetProperty("cityId").GetString()!)
                .First(c => c != currentCityId);

            var resp = await TravelAsync(client, sessionId, caseId, targetCityId);
            resp.StatusCode.Should().Be(HttpStatusCode.OK, $"Travel step {i + 1} failed");
        }
    }

    /// <summary>Travels until 0 steps remain (exhausts all steps).</summary>
    public static async Task ExhaustStepsAsync(HttpClient client, string sessionId, string caseId)
    {
        // Get case info to learn the trail so we can avoid trail cities
        using var caseReq = new HttpRequestMessage(HttpMethod.Get, $"/api/cases/{caseId}");
        caseReq.Headers.Add("X-Session-Id", sessionId);
        var caseResp = await client.SendAsync(caseReq);
        var caseBody = await caseResp.Content.ReadAsStringAsync();
        var caseJson = JsonDocument.Parse(caseBody).RootElement;
        var trail = caseJson.GetProperty("trail").EnumerateArray().Select(e => e.GetString()!).ToHashSet();

        for (int i = 0; i < 10; i++)
        {
            var cityJson = await GetCityAsync(client, sessionId, caseId);
            var remaining = cityJson.GetProperty("remainingSteps").GetInt32();
            if (remaining <= 0) break;
            var currentCityId = cityJson.GetProperty("city").GetProperty("id").GetString();
            var options = cityJson.GetProperty("travelOptions").EnumerateArray().ToList();
            if (options.Count == 0) break;

            // Prefer a non-trail, non-current city to avoid advancing or revisiting
            var targetCityId = options
                .Select(o => o.GetProperty("cityId").GetString()!)
                .FirstOrDefault(c => c != currentCityId && !trail.Contains(c));
            // Fall back to any non-current city
            targetCityId ??= options
                .Select(o => o.GetProperty("cityId").GetString()!)
                .First(c => c != currentCityId);

            var resp = await TravelAsync(client, sessionId, caseId, targetCityId);
            if (!resp.IsSuccessStatusCode) break;
            var respBody = await resp.Content.ReadAsStringAsync();
            var respJson = JsonDocument.Parse(respBody).RootElement;
            if (respJson.GetProperty("caseStatus").GetString() == "lost") break;
        }
    }

    /// <summary>Issues a warrant. Returns the response.</summary>
    public static async Task<HttpResponseMessage> IssueWarrantAsync(HttpClient client, string sessionId, string caseId, string suspectId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/cases/{caseId}/warrant");
        request.Headers.Add("X-Session-Id", sessionId);
        request.Content = JsonContent.Create(new { suspectId });
        return await client.SendAsync(request);
    }

    /// <summary>Sends a chat message to an NPC. Returns the response.</summary>
    public static async Task<HttpResponseMessage> ChatAsync(HttpClient client, string sessionId, string caseId, string npcId, string message)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/cases/{caseId}/npcs/{npcId}/chat");
        request.Headers.Add("X-Session-Id", sessionId);
        request.Content = JsonContent.Create(new { message });
        return await client.SendAsync(request);
    }
}
