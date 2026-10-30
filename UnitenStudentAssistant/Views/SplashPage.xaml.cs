namespace UnitenStudentAssistant.Views;

public partial class SplashPage : ContentPage
{
    private bool started;   // OnAppearing can run more than once; only start once

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (started) return;
        started = true;

        await Task.Delay(2000);
        Application.Current!.Windows[0].Page = new AppShell();
    }
}

