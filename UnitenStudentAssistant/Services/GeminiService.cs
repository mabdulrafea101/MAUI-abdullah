using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnitenStudentAssistant.Helpers;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Services;

public static class GeminiService
{
    // One HttpClient for the whole app. After 30 s it gives up with TaskCanceledException.
    private static readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public static async Task<string> AskAsync(string systemText, List<ChatMessage> history)
    {
        var secrets = await AppConfig.GetSecretsAsync();

        // The conversation so far: "user" for the student, "model" for the AI
        var contents = new JsonArray();
        foreach (var message in history)
        {
            contents.Add(new JsonObject
            {
                ["role"] = message.IsUser ? "user" : "model",
                ["parts"] = new JsonArray(new JsonObject { ["text"] = message.Text })
            });
        }

        var body = new JsonObject
        {
            ["system_instruction"] = new JsonObject
            {
                ["parts"] = new JsonArray(new JsonObject { ["text"] = systemText })
            },
            ["contents"] = contents
        };

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{secrets.GeminiModel}:generateContent";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", secrets.GeminiApiKey);     // header, never in the URL
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Gemini error {(int)response.StatusCode}: {ReadErrorMessage(json)}");

        return ReadAnswer(json);
    }

    // The answer is the text of candidates[0].content.parts.
    // Parts marked "thought": true are the model's reasoning, so skip them.
    private static string ReadAnswer(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var answer = new StringBuilder();

        if (doc.RootElement.TryGetProperty("candidates", out var candidates)
            && candidates.ValueKind == JsonValueKind.Array
            && candidates.GetArrayLength() > 0
            && candidates[0].TryGetProperty("content", out var content)
            && content.TryGetProperty("parts", out var parts)
            && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                bool isThought = part.TryGetProperty("thought", out var thought)
                                 && thought.ValueKind == JsonValueKind.True;
                if (!isThought && part.TryGetProperty("text", out var text)
                               && text.ValueKind == JsonValueKind.String)
                {
                    answer.Append(text.GetString());
                }
            }
        }

        string result = answer.ToString().Trim();
        return result != "" ? result : "Sorry, I could not answer that. Please try asking in a different way.";
    }

    // Gemini errors look like { "error": { "code": 404, "message": "..." } }
    private static string ReadErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString() ?? "no details";
            }
        }
        catch (JsonException)
        {
            // not JSON, so fall through
        }
        return "no details";
    }
}

