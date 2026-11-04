using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

public class StudentProfile : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("studentId")] public string StudentId { get; set; } = "";
    [FirestoreProperty("programme")] public string Programme { get; set; } = "";
    [FirestoreProperty("email")] public string Email { get; set; } = "";
    [FirestoreProperty("phone")] public string Phone { get; set; } = "";

    // display helper (not stored): "Abdullah" -> "AB", "Ali Bakar" -> "AB"
    public string Initials
    {
        get
        {
            var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "?";
            if (words.Length == 1) return words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpper();
            return (words[0].Substring(0, 1) + words[1].Substring(0, 1)).ToUpper();
        }
    }
}

