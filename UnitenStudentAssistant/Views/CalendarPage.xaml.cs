using System.Globalization;
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(DateParam), "date")]
public partial class CalendarPage : ContentPage
{
    public string DateParam { get; set; } = "";     // optional "yyyy-MM-dd" from Home or Search
    private List<CalendarEvent> allEvents = new();

    public CalendarPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        bool firstLoad = allEvents.Count == 0;

        if (firstLoad)
        {
            Loading.IsVisible = Loading.IsRunning = true;
            try
            {
                allEvents = await FirestoreService.GetEventsAsync();
                MonthView.SetEvents(allEvents);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Something went wrong", "Could not load the calendar. " + ex.Message, "OK");
            }
            finally
            {
                Loading.IsVisible = Loading.IsRunning = false;
            }
        }

        if (DateTime.TryParseExact(DateParam, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out DateTime date))
        {
            // Opened from "Next up" or a Search result: jump to that day
            MonthView.GoToMonth(date);
            MonthView.SelectDate(date);
            ShowEventsForDay(date);
            DateParam = "";       // use it only once
        }
        else if (firstLoad)
        {
            ShowEventsForMonth();
        }
    }

    private void OnDateSelected(object? sender, DateTime date)
    {
        MonthView.SelectDate(date);
        ShowEventsForDay(date);
    }

    private void OnMonthChanged(object? sender, DateTime month)
    {
        ShowEventsForMonth();
    }

    private void OnShowAllClicked(object sender, EventArgs e)
    {
        MonthView.ClearSelection();
        ShowEventsForMonth();
    }

    private void ShowEventsForMonth()
    {
        string month = MonthView.DisplayMonth.ToString("yyyy-MM");      // dates are "yyyy-MM-dd" text
        ListTitleLabel.Text = "Important dates";
        ShowAllButton.IsVisible = false;
        ShowList(allEvents.Where(ev => ev.Date.StartsWith(month)).ToList(), "No events this month.");
    }

    private void ShowEventsForDay(DateTime date)
    {
        string day = date.ToString("yyyy-MM-dd");
        ListTitleLabel.Text = $"Events on {date:d MMM}";
        ShowAllButton.IsVisible = true;
        ShowList(allEvents.Where(ev => ev.Date == day).ToList(), "No events on this day.");
    }

    private void ShowList(List<CalendarEvent> list, string emptyText)
    {
        BindableLayout.SetItemsSource(EventList, list);
        EmptyLabel.Text = emptyText;
        EmptyLabel.IsVisible = list.Count == 0;
    }
}
