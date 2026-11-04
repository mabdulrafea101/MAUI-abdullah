using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

// Called UniService because "Service" clashes with other names
public class UniService : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("description")] public string Description { get; set; } = "";
    [FirestoreProperty("location")] public string Location { get; set; } = "";
    [FirestoreProperty("hours")] public string Hours { get; set; } = "";
    [FirestoreProperty("contact")] public string Contact { get; set; } = "";
}

