# DATA-MODEL.md — Firestore collections, C# models and seed data

The app uses **one Cloud Firestore database** with five top-level collections. There is no login: the app has a single student, Abdullah, and Firestore runs in test mode (see docs/SETUP-FIREBASE-GEMINI.md).

> All seed values below (dates, locations, emails, phone numbers) are **demo data for the class project**. They are not official UNITEN information.

## 1. Collections at a glance

| Collection | Doc id | Written by app? | Read by |
|---|---|---|---|
| `programmes` | fixed (`prog-it`, …) | seed only | Programmes, Programme Detail, Search, Edit Profile picker, AI prompt |
| `calendarEvents` | fixed (`evt-01`, …) | seed only | Calendar, Home "Next up", Search, AI prompt |
| `services` | fixed (`svc-registrar`, …) | seed only | Services, Service Detail, Search, New Request picker, AI prompt |
| `requests` | seed: fixed (`req-1001` …); new: auto id | **yes**: New Request adds docs | Requests tab, Tracking (live), Request Detail (live) |
| `profile` | one doc: `student` | **yes**: Edit Profile updates it | Home greeting, Profile, Edit Profile, AI prompt |

**Field naming:** camelCase in Firestore, PascalCase in C#. The mapping is done with `[FirestoreProperty("camelName")]`.

## 2. Field types (and why)

| Kind of value | Firestore type | C# type | Reason |
|---|---|---|---|
| Text | string | `string` | — |
| Calendar day | **string `yyyy-MM-dd`** | `string` (+ a parsed `DateTime` helper) | No timezone surprises, easy to read and edit in the Console, and it sorts correctly as text |
| Moment in time (`createdAt`, `updatedAt`) | timestamp | `DateTimeOffset` | Supported by Plugin.Firebase; used for "newest first" ordering |
| List of text (`majors`) | array | `IList<string>` | Supported by Plugin.Firebase |
| Status | string, exactly one of `Pending`, `In Progress`, `Completed` | `string` | Easy to change by hand in the Console during the demo |

**Status rule:** the spelling and capitalisation must be exact, including the space in `In Progress`. `StatusColors` and the Tracking filter compare with `==`. Any other value shows grey and appears under no tab. Write the three values as constants in `Models/RequestStatus.cs`:

```csharp
public static class RequestStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
}
```

## 3. C# model classes (`Models/`)

Every model:

- implements `IFirestoreObject` (namespace `Plugin.Firebase.Firestore`)
- has a public parameterless constructor (the default one) and public `get; set;` properties
- maps each stored field with an attribute

Properties **without** an attribute are not stored. Use them for display helpers, which are read-only and computed.

```csharp
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

public class CalendarEvent : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("title")] public string Title { get; set; } = "";
    [FirestoreProperty("date")] public string Date { get; set; } = "";        // "yyyy-MM-dd"
    [FirestoreProperty("type")] public string Type { get; set; } = "";        // Academic, Registration, Exam, Break, Holiday
    [FirestoreProperty("description")] public string Description { get; set; } = "";

    // display helpers (not stored)
    public DateTime DateValue => DateTime.ParseExact(Date, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
    public string DayText => DateValue.Day.ToString();
    public string MonthText => DateValue.ToString("MMM");
    public string DateText => DateValue.ToString("dd MMM yyyy");
    public string TypeAndDescription => $"{Type} · {Description}";
}

public class UniService : IFirestoreObject       // "Service" clashes with other names, so UniService
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("description")] public string Description { get; set; } = "";
    [FirestoreProperty("location")] public string Location { get; set; } = "";
    [FirestoreProperty("hours")] public string Hours { get; set; } = "";
    [FirestoreProperty("contact")] public string Contact { get; set; } = "";
}

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

public class ChatMessage            // NOT stored in Firestore: lives only in the AI page's list
{
    public string Text { get; set; } = "";
    public bool IsUser { get; set; }
    public bool IsError { get; set; }       // AI could not answer (no internet, no key, ...)

    // display helpers used by the chat bubble DataTemplate
    public LayoutOptions Side => IsUser ? LayoutOptions.End : LayoutOptions.Start;
    public Color BubbleColor => IsUser ? AppColors.Get("Navy") : AppColors.Get("AiBubble");
    public Color TextColor => IsUser ? Colors.White
                            : IsError ? AppColors.Get("UnitenRed")
                            : AppColors.Get("TextMain");
}
```

`ServiceRequest` and `ChatMessage` use `Color`, `StatusColors` and `AppColors`, so their files need `using UnitenStudentAssistant.Helpers;` (`Color`, `Colors` and `LayoutOptions` are covered by the MAUI implicit usings).

## 4. How the app reads and writes (`Services/FirestoreService.cs`)

The `FirestoreService` static class is the **only** place that touches Firestore. Pages call it and never call `CrossFirebaseFirestore` themselves.

| Method | What it does | Plugin.Firebase calls |
|---|---|---|
| `Task<List<Programme>> GetProgrammesAsync()` | all programmes, ordered by name | `GetCollection("programmes").OrderBy("name").GetDocumentsAsync<Programme>()` |
| `Task<Programme?> GetProgrammeAsync(string id)` | one programme | `GetCollection("programmes").GetDocument(id).GetDocumentSnapshotAsync<Programme>()` → `.Data` |
| `Task<List<CalendarEvent>> GetEventsAsync()` | all events, ordered by date | `OrderBy("date")` |
| `Task<List<UniService>> GetServicesAsync()` | all services, ordered by name | |
| `Task<UniService?> GetServiceAsync(string id)` | one service | |
| `Task<List<ServiceRequest>> GetRecentRequestsAsync(int count)` | newest N requests | `OrderBy("createdAt", true).LimitedTo(count)` |
| `Task<string> AddRequestAsync(string service, string subject, string details)` | creates a request and returns its `requestNo` | count the docs → `REQ-{1000 + count + 1}` → `AddDocumentAsync(new ServiceRequest{...})` |
| `IDisposable ListenToRequests(Action<List<ServiceRequest>> onChanged, Action<Exception> onError)` | live list, newest first | `OrderBy("createdAt", true).AddSnapshotListener<ServiceRequest>(...)` |
| `IDisposable ListenToRequest(string id, Action<ServiceRequest?> onChanged, Action<Exception> onError)` | live single document | `GetDocument(id).AddSnapshotListener<ServiceRequest>(...)` |
| `Task<StudentProfile?> GetProfileAsync()` | the `profile/student` document | |
| `Task UpdateProfileAsync(StudentProfile p)` | saves the edited fields | `GetDocument("student").UpdateDataAsync(("name", p.Name), ("programme", p.Programme), ("email", p.Email), ("phone", p.Phone))` |
| `Task<bool> IsEmptyAsync<T>(string collection)` | used by seeding: does the collection have zero docs? | `GetCollection(c).LimitedTo(1).GetDocumentsAsync<T>(Source.Server)` → `.IsEmpty`. `Source.Server` makes it throw when offline, instead of reading an empty local cache and seeding over real data. |
| `Task SetDocumentAsync(string collection, string id, object data)` | used by seeding: writes one doc with a fixed id | `GetCollection(c).GetDocument(id).SetDataAsync(data)` |

**Snapshot to list:** `snapshot.Documents.Select(d => d.Data).ToList()`.

**Listeners:** listener callbacks may arrive on a background thread. The **page** wraps its UI update in `MainThread.BeginInvokeOnMainThread(...)`, not the service.

**Request number:** "count + 1" is fine for a single-user class project. Two phones submitting at the same moment could get the same number; that is acceptable and documented here on purpose.

**Timestamps:** when the teacher edits `status` in the Console, `updatedAt` does not change by itself. Request Detail still shows the new status live. For the demo, the teacher can also edit `updatedAt`, but this is not needed.

## 5. Seeding (`Services/SeedDataService.cs`)

`SeedDataService.SeedIfEmptyAsync()` is called from SplashPage. For **each** of the five collections:

- If the collection has **zero** documents (`FirestoreService.IsEmptyAsync<T>`), write that collection's seed docs with **fixed ids** (`FirestoreService.SetDocumentAsync`).
- Otherwise, leave it alone.

Fixed ids make seeding safe to run again. To reset the demo, delete a collection in the Console and restart the app.

### 5.1 `profile` (1 doc, id `student`)

| name | studentId | programme | email | phone |
|---|---|---|---|---|
| Abdullah | is01084960 | Bachelor of Information Systems (Hons.) | is01084960@student.uniten.edu.my | 012-345 6789 |

The `programme` value must equal one programme `name` exactly, so the Edit Profile Picker can preselect it.

### 5.2 `programmes` (5 docs)

Every programme has `faculty` = "College of Computing and Informatics" and `duration` = "4 years".

| id | name | code | majors | description |
|---|---|---|---|---|
| prog-it | Bachelor of Information Technology (Hons.) | BIT | Software Development; Networking; Data Analytics | Builds practical skills in software, networks and data to solve real business problems with technology. |
| prog-cs | Bachelor of Computer Science (Hons.) | BCS | Software Engineering; Artificial Intelligence; Graphics & Multimedia | Covers the theory and practice of computing, from algorithms and programming to intelligent systems. |
| prog-bc | Bachelor of Business Computing (Hons.) | BBC | Business Analytics; E-Business; IT Management | Combines computing with business studies to prepare graduates for technology roles in organisations. |
| prog-cyber | Bachelor of Computer Science (Cyber Security) (Hons.) | BCSS | Network Security; Digital Forensics; Secure Software | Focuses on protecting systems, networks and data, including ethical hacking and incident response. |
| prog-is | Bachelor of Information Systems (Hons.) | BIS | Enterprise Systems; Information Management; Systems Analysis | Teaches how to design and manage information systems that support people and organisations. |

### 5.3 `services` (5 docs)

| id | name | description | location | hours | contact |
|---|---|---|---|---|---|
| svc-registrar | Registrar | Course registration, transcripts, enrolment letters and academic records. | Admin Building, Level 1 | Mon–Fri, 8:30 am – 5:00 pm | registrar.demo@uniten.edu.my · Ext. 2001 |
| svc-finance | Finance | Tuition fees, payment plans, receipts and scholarship disbursement. | Admin Building, Level 2 | Mon–Fri, 8:30 am – 4:30 pm | finance.demo@uniten.edu.my · Ext. 2002 |
| svc-library | Library | Borrowing, room booking, online databases and library fines. | Library Building | Mon–Sat, 8:00 am – 10:00 pm | library.demo@uniten.edu.my · Ext. 2003 |
| svc-affairs | Student Affairs | Clubs, accommodation, counselling and student welfare. | Student Centre | Mon–Fri, 9:00 am – 5:00 pm | affairs.demo@uniten.edu.my · Ext. 2004 |
| svc-ict | ICT Support | Student email, Wi-Fi, portal passwords and lab computers. | ICT Centre, Block B | Mon–Fri, 8:00 am – 6:00 pm | ict.demo@uniten.edu.my · Ext. 2005 |

### 5.4 `calendarEvents` (12 docs): "Semester 1 2026/2027"

| id | date | title | type | description |
|---|---|---|---|---|
| evt-01 | 2026-09-14 | Registration opens | Registration | Online course registration opens on the student portal. |
| evt-02 | 2026-09-28 | Semester starts | Academic | First day of lectures for Semester 1 2026/2027. |
| evt-03 | 2026-10-09 | Registration deadline | Registration | Last day to add or drop courses without penalty. |
| evt-04 | 2026-10-20 | Deepavali holiday | Holiday | Public holiday. No classes. |
| evt-05 | 2026-11-09 | Mid-semester break starts | Break | One-week mid-semester break begins. |
| evt-06 | 2026-11-16 | Classes resume | Academic | Lectures continue after the mid-semester break. |
| evt-07 | 2026-11-27 | Project proposal due | Academic | Final-year and course project proposals due. |
| evt-08 | 2026-12-11 | Fee payment deadline | Registration | Last day to pay Semester 1 fees. |
| evt-09 | 2026-12-25 | Christmas holiday | Holiday | Public holiday. No classes. |
| evt-10 | 2027-01-08 | Last day of lectures | Academic | Final week of teaching ends. |
| evt-11 | 2027-01-18 | Final exams start | Exam | Final examination period begins. |
| evt-12 | 2027-02-05 | Semester ends | Academic | End of Semester 1 2026/2027. |

October and November 2026 each have several dots, so the calendar looks alive during the demo (today is around the end of October 2026).

### 5.5 `requests` (6 docs): matches "Pending (2), In Progress (1), Completed (3)"

All seed requests have `studentId` = `is01084960`. `createdAt` uses the time shown in Malaysia time (UTC+8): `new DateTimeOffset(2026, 10, 1, 10, 0, 0, TimeSpan.FromHours(8))`. `updatedAt` is the same moment as `createdAt`, plus 1 day for requests that are not Pending.

| id | requestNo | service | subject | status | createdAt |
|---|---|---|---|---|---|
| req-1001 | REQ-1001 | Registrar | Request for enrolment letter | Completed | 2026-10-01 10:00 |
| req-1002 | REQ-1002 | Library | Library fine appeal | Completed | 2026-10-05 14:30 |
| req-1003 | REQ-1003 | ICT Support | Cannot log in to student portal | Completed | 2026-10-12 09:15 |
| req-1004 | REQ-1004 | Finance | Scholarship payment not received | In Progress | 2026-10-19 11:45 |
| req-1005 | REQ-1005 | Student Affairs | Hostel room change | Pending | 2026-10-26 16:20 |
| req-1006 | REQ-1006 | Registrar | Transcript copy | Pending | 2026-10-28 08:50 |

`details` for every seed request is one plain sentence that expands the subject. Example for REQ-1004: "My scholarship for this semester has not been credited to my account yet."

The first request a student submits becomes **REQ-1007**.

## 6. AI prompt context (`Helpers/AiPromptBuilder.cs`)

`AiPromptBuilder.BuildAsync()` loads the profile, programmes, services and calendar events through `FirestoreService`, and returns **one string** that becomes Gemini's `system_instruction`:

```
You are the UNITEN Student Assistant inside a student mobile app.
Answer ONLY using the facts below. If the answer is not in the facts, say you
don't know and suggest contacting the right service. Keep answers under 120
words, friendly and plain text (no markdown).

STUDENT: Abdullah (is01084960), Bachelor of Information Systems (Hons.)
PROGRAMMES:
- Bachelor of Computer Science (Hons.): <description> Majors: ...
SERVICES:
- Finance: <description> Location: ... Hours: ... Contact: ...
CALENDAR (Semester 1 2026/2027):
- 2026-09-28 Semester starts: <description>
TODAY: 2026-10-30            <- DateTime.Today.ToString("yyyy-MM-dd"), so "next" questions work
```

This is "context stuffing": the whole dataset is small (a few KB), so it fits in every request. That is why the app needs **no RAG and no vector database**. The text is built in the AI page's `OnAppearing` and kept in a private field of that page, so each visit uses fresh data (for example, a profile edited a minute ago).
