using System.Diagnostics;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Services;

// Fills an empty Firestore with the demo data (docs/DATA-MODEL.md §5).
// A collection is only written when it has no documents, and every
// document has a fixed id, so running this again never makes duplicates.
public static class SeedDataService
{
    private const string Faculty = "College of Computing and Informatics";
    private const string StudentId = "is01084960";

    public static async Task SeedIfEmptyAsync()
    {
        try
        {
            await SeedAsync("profile", new Dictionary<string, StudentProfile>
            {
                ["student"] = new StudentProfile
                {
                    Name = "Abdullah",
                    StudentId = StudentId,
                    Programme = "Bachelor of Information Systems (Hons.)",
                    Email = "is01084960@student.uniten.edu.my",
                    Phone = "012-345 6789"
                }
            });

            await SeedAsync("programmes", new Dictionary<string, Programme>
            {
                ["prog-it"] = MakeProgramme("Bachelor of Information Technology (Hons.)", "BIT",
                    "Builds practical skills in software, networks and data to solve real business problems with technology.",
                    "Software Development", "Networking", "Data Analytics"),
                ["prog-cs"] = MakeProgramme("Bachelor of Computer Science (Hons.)", "BCS",
                    "Covers the theory and practice of computing, from algorithms and programming to intelligent systems.",
                    "Software Engineering", "Artificial Intelligence", "Graphics & Multimedia"),
                ["prog-bc"] = MakeProgramme("Bachelor of Business Computing (Hons.)", "BBC",
                    "Combines computing with business studies to prepare graduates for technology roles in organisations.",
                    "Business Analytics", "E-Business", "IT Management"),
                ["prog-cyber"] = MakeProgramme("Bachelor of Computer Science (Cyber Security) (Hons.)", "BCSS",
                    "Focuses on protecting systems, networks and data, including ethical hacking and incident response.",
                    "Network Security", "Digital Forensics", "Secure Software"),
                ["prog-is"] = MakeProgramme("Bachelor of Information Systems (Hons.)", "BIS",
                    "Teaches how to design and manage information systems that support people and organisations.",
                    "Enterprise Systems", "Information Management", "Systems Analysis")
            });

            await SeedAsync("services", new Dictionary<string, UniService>
            {
                ["svc-registrar"] = MakeService("Registrar",
                    "Course registration, transcripts, enrolment letters and academic records.",
                    "Admin Building, Level 1", "Mon–Fri, 8:30 am – 5:00 pm", "registrar.demo@uniten.edu.my · Ext. 2001"),
                ["svc-finance"] = MakeService("Finance",
                    "Tuition fees, payment plans, receipts and scholarship disbursement.",
                    "Admin Building, Level 2", "Mon–Fri, 8:30 am – 4:30 pm", "finance.demo@uniten.edu.my · Ext. 2002"),
                ["svc-library"] = MakeService("Library",
                    "Borrowing, room booking, online databases and library fines.",
                    "Library Building", "Mon–Sat, 8:00 am – 10:00 pm", "library.demo@uniten.edu.my · Ext. 2003"),
                ["svc-affairs"] = MakeService("Student Affairs",
                    "Clubs, accommodation, counselling and student welfare.",
                    "Student Centre", "Mon–Fri, 9:00 am – 5:00 pm", "affairs.demo@uniten.edu.my · Ext. 2004"),
                ["svc-ict"] = MakeService("ICT Support",
                    "Student email, Wi-Fi, portal passwords and lab computers.",
                    "ICT Centre, Block B", "Mon–Fri, 8:00 am – 6:00 pm", "ict.demo@uniten.edu.my · Ext. 2005")
            });

            await SeedAsync("calendarEvents", new Dictionary<string, CalendarEvent>
            {
                ["evt-01"] = MakeEvent("2026-09-14", "Registration opens", "Registration", "Online course registration opens on the student portal."),
                ["evt-02"] = MakeEvent("2026-09-28", "Semester starts", "Academic", "First day of lectures for Semester 1 2026/2027."),
                ["evt-03"] = MakeEvent("2026-10-09", "Registration deadline", "Registration", "Last day to add or drop courses without penalty."),
                ["evt-04"] = MakeEvent("2026-10-20", "Deepavali holiday", "Holiday", "Public holiday. No classes."),
                ["evt-05"] = MakeEvent("2026-11-09", "Mid-semester break starts", "Break", "One-week mid-semester break begins."),
                ["evt-06"] = MakeEvent("2026-11-16", "Classes resume", "Academic", "Lectures continue after the mid-semester break."),
                ["evt-07"] = MakeEvent("2026-11-27", "Project proposal due", "Academic", "Final-year and course project proposals due."),
                ["evt-08"] = MakeEvent("2026-12-11", "Fee payment deadline", "Registration", "Last day to pay Semester 1 fees."),
                ["evt-09"] = MakeEvent("2026-12-25", "Christmas holiday", "Holiday", "Public holiday. No classes."),
                ["evt-10"] = MakeEvent("2027-01-08", "Last day of lectures", "Academic", "Final week of teaching ends."),
                ["evt-11"] = MakeEvent("2027-01-18", "Final exams start", "Exam", "Final examination period begins."),
                ["evt-12"] = MakeEvent("2027-02-05", "Semester ends", "Academic", "End of Semester 1 2026/2027.")
            });

            await SeedAsync("requests", new Dictionary<string, ServiceRequest>
            {
                ["req-1001"] = MakeRequest(1001, "Registrar", "Request for enrolment letter",
                    "I need an enrolment letter for my scholarship application.",
                    RequestStatus.Completed, MalaysiaTime(10, 1, 10, 0)),
                ["req-1002"] = MakeRequest(1002, "Library", "Library fine appeal",
                    "I returned the book on time but was still charged a late fine.",
                    RequestStatus.Completed, MalaysiaTime(10, 5, 14, 30)),
                ["req-1003"] = MakeRequest(1003, "ICT Support", "Cannot log in to student portal",
                    "My student portal password stopped working after the reset.",
                    RequestStatus.Completed, MalaysiaTime(10, 12, 9, 15)),
                ["req-1004"] = MakeRequest(1004, "Finance", "Scholarship payment not received",
                    "My scholarship for this semester has not been credited to my account yet.",
                    RequestStatus.InProgress, MalaysiaTime(10, 19, 11, 45)),
                ["req-1005"] = MakeRequest(1005, "Student Affairs", "Hostel room change",
                    "I would like to move to a room closer to the library block.",
                    RequestStatus.Pending, MalaysiaTime(10, 26, 16, 20)),
                ["req-1006"] = MakeRequest(1006, "Registrar", "Transcript copy",
                    "I need a certified copy of my latest transcript.",
                    RequestStatus.Pending, MalaysiaTime(10, 28, 8, 50))
            });
        }
        catch (Exception ex)
        {
            // Show alert on screen if seeding fails so we can see the exact error
            Debug.WriteLine("Seeding failed: " + ex.Message);
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (Application.Current?.Windows.Count > 0 && Application.Current.Windows[0].Page != null)
                {
                    await Application.Current.Windows[0].Page!.DisplayAlertAsync("Seeding Error", ex.Message, "OK");
                }
            });
        }
    }

    // Writes the documents only if the collection is empty
    private static async Task SeedAsync<T>(string collection, Dictionary<string, T> documents) where T : class
    {
        if (!await FirestoreService.IsEmptyAsync<T>(collection)) return;

        foreach (var document in documents)
        {
            await FirestoreService.SetDocumentAsync(collection, document.Key, document.Value);
        }
    }

    private static Programme MakeProgramme(string name, string code, string description, params string[] majors) =>
        new Programme
        {
            Name = name,
            Code = code,
            Faculty = Faculty,
            Duration = "4 years",
            Description = description,
            Majors = majors.ToList()
        };

    private static UniService MakeService(string name, string description, string location, string hours, string contact) =>
        new UniService
        {
            Name = name,
            Description = description,
            Location = location,
            Hours = hours,
            Contact = contact
        };

    private static CalendarEvent MakeEvent(string date, string title, string type, string description) =>
        new CalendarEvent
        {
            Date = date,
            Title = title,
            Type = type,
            Description = description
        };

    private static ServiceRequest MakeRequest(int number, string service, string subject, string details,
                                              string status, DateTimeOffset created) =>
        new ServiceRequest
        {
            RequestNo = $"REQ-{number}",
            Service = service,
            Subject = subject,
            Details = details,
            Status = status,
            StudentId = StudentId,
            CreatedAt = created,
            // a request that is no longer Pending was updated one day later
            UpdatedAt = status == RequestStatus.Pending ? created : created.AddDays(1)
        };

    // A moment in 2026, in Malaysia time (UTC+8)
    private static DateTimeOffset MalaysiaTime(int month, int day, int hour, int minute) =>
        new DateTimeOffset(2026, month, day, hour, minute, 0, TimeSpan.FromHours(8));
}

