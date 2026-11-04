using System.Globalization;
using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

public class CalendarEvent : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("title")] public string Title { get; set; } = "";
    [FirestoreProperty("date")] public string Date { get; set; } = "";        // "yyyy-MM-dd"
    [FirestoreProperty("type")] public string Type { get; set; } = "";        // Academic, Registration, Exam, Break, Holiday
    [FirestoreProperty("description")] public string Description { get; set; } = "";

    // display helpers (not stored)
    public DateTime DateValue => DateTime.ParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    public string DayText => DateValue.Day.ToString();
    public string MonthText => DateValue.ToString("MMM");
    public string DateText => DateValue.ToString("dd MMM yyyy");
    public string TypeAndDescription => $"{Type} · {Description}";
}

