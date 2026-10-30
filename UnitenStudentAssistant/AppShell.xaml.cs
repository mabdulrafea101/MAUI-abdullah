using UnitenStudentAssistant.Views;

namespace UnitenStudentAssistant;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages that are pushed on top of a tab (see docs/NAVIGATION.md §2)
        Routing.RegisterRoute("programmes", typeof(ProgrammesPage));
        Routing.RegisterRoute("programmedetail", typeof(ProgrammeDetailPage));
        Routing.RegisterRoute("calendar", typeof(CalendarPage));
        Routing.RegisterRoute("search", typeof(SearchPage));
        Routing.RegisterRoute("servicedetail", typeof(ServiceDetailPage));
        Routing.RegisterRoute("newrequest", typeof(NewRequestPage));
        Routing.RegisterRoute("tracking", typeof(RequestTrackingPage));
        Routing.RegisterRoute("requestdetail", typeof(RequestDetailPage));
        Routing.RegisterRoute("editprofile", typeof(EditProfilePage));
        Routing.RegisterRoute("about", typeof(AboutPage));
    }
}
