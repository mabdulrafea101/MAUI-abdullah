using System.Text;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Helpers;

public static class AiPromptBuilder
{
    public static async Task<string> BuildAsync()
    {
        var profile = await FirestoreService.GetProfileAsync();
        var programmes = await FirestoreService.GetProgrammesAsync();
        var services = await FirestoreService.GetServicesAsync();
        var events = await FirestoreService.GetEventsAsync();

        var text = new StringBuilder();
        text.AppendLine("You are the UNITEN Student Assistant inside a student mobile app.");
        text.AppendLine("Answer ONLY using the facts below. If the answer is not in the facts, say you don't know and suggest contacting the right service.");
        text.AppendLine("Keep answers under 120 words, friendly and plain text (no markdown).");
        text.AppendLine();

        if (profile != null)
            text.AppendLine($"STUDENT: {profile.Name} ({profile.StudentId}), {profile.Programme}");

        text.AppendLine("PROGRAMMES:");
        foreach (var p in programmes)
            text.AppendLine($"- {p.Name}: {p.Description} Faculty: {p.Faculty}. Duration: {p.Duration}. Majors: {string.Join(", ", p.Majors)}.");

        text.AppendLine("SERVICES:");
        foreach (var s in services)
            text.AppendLine($"- {s.Name}: {s.Description} Location: {s.Location}. Hours: {s.Hours}. Contact: {s.Contact}.");

        text.AppendLine("CALENDAR (Semester 1 2026/2027):");
        foreach (var e in events)
            text.AppendLine($"- {e.Date} {e.Title}: {e.Description}");

        // Today's date lets the AI answer "what is next?" questions
        text.AppendLine($"TODAY: {DateTime.Today:yyyy-MM-dd}");
        return text.ToString();
    }
}

