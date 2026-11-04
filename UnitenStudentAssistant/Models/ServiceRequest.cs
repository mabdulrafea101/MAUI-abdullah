using Plugin.Firebase.Firestore;
using UnitenStudentAssistant.Helpers;

namespace UnitenStudentAssistant.Models;

public class ServiceRequest : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("requestNo")] public string RequestNo { get; set; } = "";   // "REQ-1007"
    [FirestoreProperty("service")] public string Service { get; set; } = "";       // service name
    [FirestoreProperty("subject")] public string Subject { get; set; } = "";
    [FirestoreProperty("details")] public string Details { get; set; } = "";
    [FirestoreProperty("status")] public string Status { get; set; } = RequestStatus.Pending;
    [FirestoreProperty("studentId")] public string StudentId { get; set; } = "";
    [FirestoreProperty("createdAt")] public DateTimeOffset CreatedAt { get; set; }
    [FirestoreProperty("updatedAt")] public DateTimeOffset UpdatedAt { get; set; }

    // display helpers (not stored)
    public string CreatedText => CreatedAt.ToLocalTime().ToString("dd MMM yyyy, h:mm tt");
    public string UpdatedText => UpdatedAt.ToLocalTime().ToString("dd MMM yyyy, h:mm tt");
    public Color StatusTextColor => StatusColors.GetText(Status);
    public Color StatusBgColor => StatusColors.GetBackground(Status);
}

