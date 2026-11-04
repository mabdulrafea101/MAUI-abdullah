using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

public class Programme : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("code")] public string Code { get; set; } = "";
    [FirestoreProperty("faculty")] public string Faculty { get; set; } = "";
    [FirestoreProperty("duration")] public string Duration { get; set; } = "";
    [FirestoreProperty("description")] public string Description { get; set; } = "";
    [FirestoreProperty("majors")] public IList<string> Majors { get; set; } = new List<string>();
}

