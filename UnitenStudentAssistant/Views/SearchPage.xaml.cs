using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(QueryParam), "q")]
public partial class SearchPage : ContentPage
{
    public string QueryParam { get; set; } = "";      // optional starting text from Home
    private bool loaded;
    private List<Programme> programmes = new();
    private List<UniService> services = new();
    private List<CalendarEvent> events = new();

    public SearchPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!loaded)
        {
            Loading.IsVisible = Loading.IsRunning = true;
            try
            {
                programmes = await FirestoreService.GetProgrammesAsync();
                services = await FirestoreService.GetServicesAsync();
                events = await FirestoreService.GetEventsAsync();
                loaded = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Something went wrong", "Could not load search data. " + ex.Message, "OK");
            }
            finally
            {
                Loading.IsVisible = Loading.IsRunning = false;
            }
        }

        if (!string.IsNullOrEmpty(QueryParam))
        {
            SearchInput.Text = Uri.UnescapeDataString(QueryParam);
            QueryParam = "";       // only once; Back from a result keeps what the user typed
        }
        RunSearch();
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e) => RunSearch();

    private void RunSearch()
    {
        string text = (SearchInput.Text ?? "").Trim();

        var foundProgrammes = new List<Programme>();
        var foundServices = new List<UniService>();
        var foundEvents = new List<CalendarEvent>();

        if (text != "")
        {
            foundProgrammes = programmes.Where(p => Matches(text, p.Name, p.Faculty, p.Description)).ToList();
            foundServices = services.Where(s => Matches(text, s.Name, s.Description)).ToList();
            foundEvents = events.Where(ev => Matches(text, ev.Title, ev.Description)).ToList();
        }

        ShowSection(ProgrammesHeading, ProgrammeResults, foundProgrammes);
        ShowSection(ServicesHeading, ServiceResults, foundServices);
        ShowSection(EventsHeading, EventResults, foundEvents);

        int total = foundProgrammes.Count + foundServices.Count + foundEvents.Count;
        if (text == "")
            ShowMessage("Type to search programmes, services and dates.");
        else if (total == 0)
            ShowMessage($"No results for “{text}”.");
        else
            MessageLabel.IsVisible = false;
    }

    // Case-insensitive "contains" on any of the given fields
    private static bool Matches(string text, params string[] fields) =>
        fields.Any(f => f.Contains(text, StringComparison.OrdinalIgnoreCase));

    // A heading and its list only show when the section has results
    private static void ShowSection<T>(Label heading, VerticalStackLayout list, List<T> items)
    {
        heading.IsVisible = items.Count > 0;
        list.IsVisible = items.Count > 0;
        BindableLayout.SetItemsSource(list, items);
    }

    private void ShowMessage(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }

    private async void OnProgrammeTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is Programme programme)
        {
            await Shell.Current.GoToAsync($"programmedetail?id={programme.Id}");
        }
    }

    private async void OnServiceTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is UniService service)
        {
            await Shell.Current.GoToAsync($"servicedetail?id={service.Id}");
        }
    }

    // Opens the calendar on that month with the day selected
    private async void OnEventTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is CalendarEvent calendarEvent)
        {
            await Shell.Current.GoToAsync($"calendar?date={calendarEvent.Date}");
        }
    }
}
