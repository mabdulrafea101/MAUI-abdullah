using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class ServiceRequestsPage : ContentPage
{
    public ServiceRequestsPage()
    {
        InitializeComponent();
    }

    // Reload every time, so a request submitted a moment ago shows at the top
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var recent = await FirestoreService.GetRecentRequestsAsync(3);
            BindableLayout.SetItemsSource(RecentList, recent);
            EmptyLabel.IsVisible = recent.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load your requests. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private async void OnNewRequestClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("newrequest");

    private async void OnTrackClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("tracking");

    // Opens Tracking on the tapped card's status ("In Progress" has a space, so escape it)
    private async void OnRecentTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is ServiceRequest request)
        {
            await Shell.Current.GoToAsync($"tracking?status={Uri.EscapeDataString(request.Status)}");
        }
    }
}
