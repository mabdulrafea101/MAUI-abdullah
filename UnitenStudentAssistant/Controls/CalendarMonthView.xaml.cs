using Microsoft.Maui.Controls.Shapes;
using UnitenStudentAssistant.Helpers;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Controls;

// A simple month calendar: 42 possible cells, rebuilt whenever something changes.
public partial class CalendarMonthView : ContentView
{
    private List<CalendarEvent> events = new();
    private DateTime? selectedDate;

    public DateTime DisplayMonth { get; private set; }      // always the 1st of the shown month

    public event EventHandler<DateTime>? DateSelected;       // a day was tapped
    public event EventHandler<DateTime>? MonthChanged;       // ‹ or › was tapped

    public CalendarMonthView()
    {
        InitializeComponent();
        DisplayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        BuildDays();
    }

    public void SetEvents(List<CalendarEvent> newEvents)
    {
        events = newEvents;
        BuildDays();
    }

    // Used when the page is opened with ?date=. Does not raise MonthChanged.
    public void GoToMonth(DateTime month)
    {
        DisplayMonth = new DateTime(month.Year, month.Month, 1);
        selectedDate = null;
        BuildDays();
    }

    public void SelectDate(DateTime date)
    {
        selectedDate = date.Date;
        BuildDays();
    }

    public void ClearSelection()
    {
        selectedDate = null;
        BuildDays();
    }

    private void OnPreviousClicked(object sender, EventArgs e) => ChangeMonth(-1);

    private void OnNextClicked(object sender, EventArgs e) => ChangeMonth(1);

    private void ChangeMonth(int step)
    {
        DisplayMonth = DisplayMonth.AddMonths(step);
        selectedDate = null;
        BuildDays();
        MonthChanged?.Invoke(this, DisplayMonth);
    }

    private void BuildDays()
    {
        MonthLabel.Text = DisplayMonth.ToString("MMMM yyyy");
        DaysGrid.Children.Clear();

        int firstColumn = (int)DisplayMonth.DayOfWeek;     // 0 = Sunday
        int daysInMonth = DateTime.DaysInMonth(DisplayMonth.Year, DisplayMonth.Month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(DisplayMonth.Year, DisplayMonth.Month, day);
            int index = firstColumn + day - 1;              // position in the 7 x 6 grid
            DaysGrid.Add(CreateDayCell(date), index % 7, index / 7);
        }
    }

    private Border CreateDayCell(DateTime date)
    {
        bool isToday = date == DateTime.Today;
        bool isSelected = date == selectedDate;
        string key = date.ToString("yyyy-MM-dd");
        bool hasEvent = events.Any(ev => ev.Date == key);

        var dayLabel = new Label
        {
            Text = date.Day.ToString(),
            FontSize = 15,
            FontFamily = isToday || isSelected ? "OpenSansSemibold" : "OpenSansRegular",
            TextColor = isSelected ? Colors.White
                      : isToday ? AppColors.Get("UnitenBlue")
                      : AppColors.Get("TextMain"),
            HorizontalOptions = LayoutOptions.Center
        };

        // The dot is always there (transparent when there is no event),
        // so every day number sits at the same height.
        var dot = new BoxView
        {
            WidthRequest = 6,
            HeightRequest = 6,
            CornerRadius = 3,
            HorizontalOptions = LayoutOptions.Center,
            Color = !hasEvent ? Colors.Transparent
                  : isSelected ? Colors.White
                  : AppColors.Get("UnitenRed")
        };

        var cell = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            BackgroundColor = isSelected ? AppColors.Get("Navy") : Colors.Transparent,
            Content = new VerticalStackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center,
                Children = { dayLabel, dot }
            }
        };

        // Cells are made in code, so the tap handler is a short lambda here
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => DateSelected?.Invoke(this, date);
        cell.GestureRecognizers.Add(tap);

        return cell;
    }
}

