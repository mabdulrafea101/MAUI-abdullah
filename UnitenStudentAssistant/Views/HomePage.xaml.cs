namespace UnitenStudentAssistant.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    // TEMPORARY until Task 6
    private async void OnProgrammesClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("programmes");

    private async void OnCalendarClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("calendar");
}
