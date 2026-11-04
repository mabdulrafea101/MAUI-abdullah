using System.Diagnostics;
using UnitenStudentAssistant.Services;

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

        try
        {
            Task seeding = SeedDataService.SeedIfEmptyAsync();   // never throws: errors are caught inside
            Task minimumTime = Task.Delay(2000);                 // show the logo for at least 2 s
            Task maximumTime = Task.Delay(8000);                 // never wait longer than 8 s

            await Task.WhenAny(seeding, maximumTime);
            await minimumTime;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var shell = new AppShell();

                    if (Application.Current?.Windows.Count > 0)
                    {
                        Application.Current.Windows[0].Page = shell;
                    }
                    else if (Window != null)
                    {
                        Window.Page = shell;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"AppShell creation error: {ex}");
                    await DisplayAlertAsync("Startup Error", $"Could not load AppShell: {ex.Message}", "OK");
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Splash error: {ex}");
            await DisplayAlertAsync("Splash Error", ex.Message, "OK");
        }
    }
}
