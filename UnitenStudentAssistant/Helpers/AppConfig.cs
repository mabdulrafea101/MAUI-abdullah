using System.Diagnostics;
using System.Text.Json;

namespace UnitenStudentAssistant.Helpers;

public class AppSecrets
{
    public string GeminiApiKey { get; set; } = "";
    public string GeminiModel { get; set; } = "gemini-3.8-flash";

    // False when secrets.json is missing or still holds the sample text
    public bool HasKey => GeminiApiKey != "" && GeminiApiKey != "PASTE-YOUR-KEY-HERE";
}

public static class AppConfig
{
    private static AppSecrets? cached;

    // Reads Resources/Raw/secrets.json once. A missing or broken file gives an empty AppSecrets.
    public static async Task<AppSecrets> GetSecretsAsync()
    {
        if (cached != null) return cached;

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("secrets.json");
            using var doc = await JsonDocument.ParseAsync(stream);
            cached = new AppSecrets
            {
                GeminiApiKey = ReadString(doc.RootElement, "GeminiApiKey", ""),
                GeminiModel = ReadString(doc.RootElement, "GeminiModel", "gemini-3.8-flash")
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"secrets.json could not be read: {ex.Message}");   // never log the key itself
            cached = new AppSecrets();
        }
        return cached;
    }

    private static string ReadString(JsonElement root, string name, string fallback)
    {
        if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            string text = (value.GetString() ?? "").Trim();
            if (text != "") return text;
        }
        return fallback;
    }
}
