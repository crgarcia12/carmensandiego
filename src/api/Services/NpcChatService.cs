using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Models;

namespace Api.Services;

public class NpcChatService : INpcChatService
{
    private readonly GameDataProvider _dataProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NpcChatService> _logger;
    private readonly bool _githubModelsEnabled;
    private readonly string? _githubModelsToken;
    private readonly string _githubModelsEndpoint;
    private readonly string _githubModelsModel;
    private readonly int _githubModelsMaxTokens;
    private readonly double _githubModelsTemperature;

    // Key: "caseId:npcId", Value: chat history
    private readonly ConcurrentDictionary<string, NpcChatHistory> _chatHistories = new();
    private const int MaxMessagesPerNpc = 20;

    public NpcChatService(
        GameDataProvider dataProvider,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<NpcChatService> logger)
    {
        _dataProvider = dataProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _githubModelsEnabled = configuration.GetValue("GitHubModels:Enabled", false);
        var configuredToken = configuration["GitHubModels:Token"]
            ?? Environment.GetEnvironmentVariable("COPILOT_GITHUB_TOKEN")
            ?? Environment.GetEnvironmentVariable("GH_TOKEN")
            ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        _githubModelsToken = !string.IsNullOrWhiteSpace(configuredToken)
            ? configuredToken
            : (_githubModelsEnabled ? TryGetTokenFromGitHubCli() : null);
        _githubModelsEndpoint = configuration["GitHubModels:Endpoint"] ?? string.Empty;
        _githubModelsModel = configuration["GitHubModels:Model"] ?? "openai/gpt-4.1";
        _githubModelsMaxTokens = configuration.GetValue("GitHubModels:MaxTokens", 120);
        _githubModelsTemperature = configuration.GetValue("GitHubModels:Temperature", 0.7d);

        if (_githubModelsEnabled && string.IsNullOrWhiteSpace(_githubModelsToken))
        {
            _logger.LogWarning("GitHubModels is enabled but no token was found. Falling back to built-in NPC responses.");
        }

        if (_githubModelsEnabled && string.IsNullOrWhiteSpace(_githubModelsEndpoint))
        {
            _logger.LogWarning("GitHubModels is enabled but no endpoint was configured. Falling back to built-in NPC responses.");
        }
    }

    private string? TryGetTokenFromGitHubCli()
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gh",
                    Arguments = "auth token",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            if (!process.Start())
            {
                _logger.LogWarning("GitHub Models is enabled but failed to start gh CLI for token lookup.");
                return null;
            }

            if (!process.WaitForExit(3000))
            {
                process.Kill(entireProcessTree: true);
                _logger.LogWarning("GitHub Models is enabled but gh CLI token lookup timed out.");
                return null;
            }

            if (process.ExitCode != 0)
            {
                var errorOutput = process.StandardError.ReadToEnd().Trim();
                _logger.LogWarning(
                    "GitHub Models is enabled but gh CLI token lookup failed with exit code {ExitCode}. {Error}",
                    process.ExitCode,
                    string.IsNullOrWhiteSpace(errorOutput) ? string.Empty : errorOutput);
                return null;
            }

            var ghToken = process.StandardOutput.ReadToEnd().Trim();
            if (string.IsNullOrWhiteSpace(ghToken))
            {
                _logger.LogWarning("GitHub Models is enabled but gh CLI returned an empty token.");
                return null;
            }

            _logger.LogInformation("Using GitHub token from gh CLI for GitHub Models.");
            return ghToken;
        }
        catch (Win32Exception ex)
        {
            _logger.LogWarning(ex, "GitHub Models is enabled but gh CLI is not available.");
            return null;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "GitHub Models is enabled but gh CLI token lookup failed to execute.");
            return null;
        }
    }

    public NPC? GetNpc(string npcId)
    {
        foreach (var city in _dataProvider.Cities)
        {
            var npc = city.Npcs.FirstOrDefault(n => n.Id == npcId);
            if (npc != null) return npc;
        }
        return null;
    }

    public bool IsNpcInCity(string npcId, string cityId)
    {
        var city = _dataProvider.GetCity(cityId);
        return city?.Npcs.Any(n => n.Id == npcId) ?? false;
    }

    public async Task<(NpcChatMessage? message, int messageCount, int remainingMessages, string? error, string? code)> ChatAsync(
        GameCase gameCase, string npcId, string message, CancellationToken cancellationToken = default)
    {
        var key = $"{gameCase.Id}:{npcId}";
        var history = _chatHistories.GetOrAdd(key, _ => new NpcChatHistory());

        if (history.Messages.Count >= MaxMessagesPerNpc)
        {
            return (null, history.Messages.Count, 0, "Conversation limit reached with this NPC", "CHAT_CAP_REACHED");
        }

        var npc = GetNpc(npcId)!;

        // Add player message to history (counts toward cap)
        history.Messages.Add(new NpcChatMessage
        {
            NpcId = npcId,
            NpcName = npc.Name,
            Text = message,
            Timestamp = DateTime.UtcNow
        });

        // Generate NPC response
        var responseText = await GenerateResponseAsync(gameCase, npc, message, cancellationToken);
        var npcMessage = new NpcChatMessage
        {
            NpcId = npcId,
            NpcName = npc.Name,
            Text = responseText,
            Timestamp = DateTime.UtcNow
        };
        history.Messages.Add(npcMessage);

        var remaining = MaxMessagesPerNpc - history.Messages.Count;
        return (npcMessage, history.Messages.Count, Math.Max(0, remaining), null, null);
    }

    private async Task<string> GenerateResponseAsync(GameCase gameCase, NPC npc, string playerMessage, CancellationToken cancellationToken)
    {
        var fallbackResponse = GenerateFallbackResponse(npc, playerMessage);

        if (!_githubModelsEnabled ||           
            string.IsNullOrWhiteSpace(_githubModelsToken) ||
            string.IsNullOrWhiteSpace(_githubModelsEndpoint))
        {
            _logger.LogDebug("Falling back to built-in NPC responses due to missing or incomplete GitHub Models configuration.");
            return fallbackResponse;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _githubModelsEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _githubModelsToken);
            request.Content = JsonContent.Create(new
            {
                model = _githubModelsModel,
                messages = new object[]
                {
                    new { role = "system", content = BuildSystemPrompt(gameCase, npc) },
                    new { role = "user", content = playerMessage }
                },
                max_tokens = _githubModelsMaxTokens,
                temperature = _githubModelsTemperature
            });

            var client = _httpClientFactory.CreateClient();
            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "GitHub Models request failed with status code {StatusCode}. Using built-in NPC response.",
                    response.StatusCode);
                return fallbackResponse;
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var json = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
            var modelResponse = ExtractResponseText(json.RootElement);

            if (string.IsNullOrWhiteSpace(modelResponse))
            {
                _logger.LogWarning("GitHub Models returned an empty message. Using built-in NPC response.");
                return fallbackResponse;
            }

            return modelResponse.Trim();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "GitHub Models request failed. Using built-in NPC response.");
            return fallbackResponse;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "GitHub Models request timed out. Using built-in NPC response.");
            return fallbackResponse;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "GitHub Models response parsing failed. Using built-in NPC response.");
            return fallbackResponse;
        }
    }

    private static string BuildSystemPrompt(GameCase gameCase, NPC npc)
    {
        return $"You are {npc.Name}, a {npc.Role} in {gameCase.CurrentCity}. " +
               "You are an NPC in a Carmen Sandiego detective game. " +
               "Stay in character, keep replies under two sentences, and provide subtle clues without naming the culprit directly.";
    }

    private static string? ExtractResponseText(JsonElement root)
    {
        if (!root.TryGetProperty("choices", out var choices) ||
            choices.ValueKind != JsonValueKind.Array ||
            choices.GetArrayLength() == 0)
        {
            return null;
        }

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message))
        {
            return null;
        }

        if (!message.TryGetProperty("content", out var content) || content.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return content.GetString();
    }

    private static string GenerateFallbackResponse(NPC npc, string playerMessage)
    {
        var responses = new[]
        {
            $"Hmm, interesting question. As a {npc.Role}, I see many things around here.",
            "I did notice someone suspicious passing through recently. They seemed to be in a hurry.",
            "You're asking the right person! I've heard rumors about a mysterious figure in a red coat.",
            "Let me think... Yes, I recall seeing someone matching that description heading to the airport.",
            $"As a {npc.Role} here in the city, I keep my eyes open. There was definitely someone unusual here.",
        };
        var index = Math.Abs(playerMessage.GetHashCode()) % responses.Length;
        return responses[index];
    }
}
