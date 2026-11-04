using Plugin.Firebase.Firestore;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Services;

// Every Firestore read and write in the app goes through this class.
public static class FirestoreService
{
    private static ICollectionReference Collection(string name) =>
        CrossFirebaseFirestore.Current.GetCollection(name);

    // ----- Programmes, events and services (read only) -----

    public static async Task<List<Programme>> GetProgrammesAsync()
    {
        var snapshot = await Collection("programmes").OrderBy("name").GetDocumentsAsync<Programme>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    public static async Task<Programme?> GetProgrammeAsync(string id)
    {
        var snapshot = await Collection("programmes").GetDocument(id).GetDocumentSnapshotAsync<Programme>();
        return snapshot.Data;   // null if the document does not exist
    }

    public static async Task<List<CalendarEvent>> GetEventsAsync()
    {
        var snapshot = await Collection("calendarEvents").OrderBy("date").GetDocumentsAsync<CalendarEvent>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    public static async Task<List<UniService>> GetServicesAsync()
    {
        var snapshot = await Collection("services").OrderBy("name").GetDocumentsAsync<UniService>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    public static async Task<UniService?> GetServiceAsync(string id)
    {
        var snapshot = await Collection("services").GetDocument(id).GetDocumentSnapshotAsync<UniService>();
        return snapshot.Data;
    }

    // ----- Requests -----

    public static async Task<List<ServiceRequest>> GetRecentRequestsAsync(int count)
    {
        var snapshot = await Collection("requests")
            .OrderBy("createdAt", true)
            .LimitedTo(count)
            .GetDocumentsAsync<ServiceRequest>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    // Creates a new Pending request and returns its number, e.g. "REQ-1007".
    // "count + 1" is fine for a one-student class project (see DATA-MODEL §4).
    public static async Task<string> AddRequestAsync(string service, string subject, string details)
    {
        var all = await Collection("requests").GetDocumentsAsync<ServiceRequest>();
        string requestNo = $"REQ-{1000 + all.Count + 1}";

        var profile = await GetProfileAsync();
        var now = DateTimeOffset.Now;

        var request = new ServiceRequest
        {
            RequestNo = requestNo,
            Service = service,
            Subject = subject,
            Details = details,
            Status = RequestStatus.Pending,
            StudentId = profile?.StudentId ?? "",
            CreatedAt = now,
            UpdatedAt = now
        };

        await Collection("requests").AddDocumentAsync(request);
        return requestNo;
    }

    // Live list of all requests, newest first. Call Dispose() on the result to stop listening.
    public static IDisposable ListenToRequests(Action<List<ServiceRequest>> onChanged, Action<Exception> onError)
    {
        return Collection("requests")
            .OrderBy("createdAt", true)
            .AddSnapshotListener<ServiceRequest>(
                snapshot => onChanged(snapshot.Documents.Select(d => d.Data).ToList()),
                onError);
    }

    // Live single request. onChanged gets null if the document was deleted.
    public static IDisposable ListenToRequest(string id, Action<ServiceRequest?> onChanged, Action<Exception> onError)
    {
        return Collection("requests")
            .GetDocument(id)
            .AddSnapshotListener<ServiceRequest>(snapshot => onChanged(snapshot.Data), onError);
    }

    // ----- Profile (one document: profile/student) -----

    public static async Task<StudentProfile?> GetProfileAsync()
    {
        var snapshot = await Collection("profile").GetDocument("student").GetDocumentSnapshotAsync<StudentProfile>();
        return snapshot.Data;
    }

    public static async Task UpdateProfileAsync(StudentProfile p)
    {
        // studentId is never changed, so it is not in this list
        await Collection("profile").GetDocument("student").UpdateDataAsync(
            ("name", p.Name),
            ("programme", p.Programme),
            ("email", p.Email),
            ("phone", p.Phone));
    }

    // ----- Used only by SeedDataService -----

    // Source.Server: when offline this throws instead of reading an empty cache
    // and seeding over real data.
    public static async Task<bool> IsEmptyAsync<T>(string collection)
    {
        var snapshot = await Collection(collection).LimitedTo(1).GetDocumentsAsync<T>(Source.Server);
        return snapshot.IsEmpty;
    }

    public static async Task SetDocumentAsync(string collection, string id, object data)
    {
        await Collection(collection).GetDocument(id).SetDataAsync(data);
    }
}

