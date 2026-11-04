namespace UnitenStudentAssistant.Models;

// The only three status values. They must match the Firestore Console spelling exactly.
public static class RequestStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
}

