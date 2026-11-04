using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class HomePage : ContentPage
{
    private CalendarEvent? nextEvent;

    public HomePage()
    {
        InitializeComponent();
    }

    // Runs every time Home is shown, so a name changed in Edit Profile appears here
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var profile = await FirestoreService.GetProfileAsync();
            if (profile != null)
            {
                GreetingLabel.Text = $"Hi, {profile.Name}";
                StudentInfoLabel.Text = $"{profile.Programme} · {profile.StudentId}";
            }

            // Events come sorted by date, and "yyyy-MM-dd" text compares in date order
            var events = await FirestoreService.GetEventsAsync();
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            nextEvent = events.FirstOrDefault(ev => string.CompareOrdinal(ev.Date, today) >= 0);

            if (nextEvent != null)
            {
                NextUpTitleLabel.Text = nextEvent.Title;
                NextUpDateLabel.Text = nextEvent.DateText;
            }
            else
            {
                NextUpTitleLabel.Text = "No upcoming dates";
                NextUpDateLabel.Text = "";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load your home page. " + ex.Message, "OK");
        }
    }

    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        string text = (HomeSearch.Text ?? "").Trim();
        HomeSearch.Text = "";
        HomeSearch.Unfocus();
        await Shell.Current.GoToAsync($"search?q={Uri.EscapeDataString(text)}");
    }

    // Pushed pages: the Home tab stays selected
    private async void OnProgrammesTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("programmes");

    private async void OnCalendarTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("calendar");

    private async void OnSearchTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("search");

    // Absolute routes (//): switch to another tab
    private async void OnServicesTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//services");

    private async void OnRequestsTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//requests");

    private async void OnAiTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//ai");

    private async void OnNextUpTapped(object sender, TappedEventArgs e)
    {
        if (nextEvent == null) return;
        await Shell.Current.GoToAsync($"calendar?date={nextEvent.Date}");
    }
}
