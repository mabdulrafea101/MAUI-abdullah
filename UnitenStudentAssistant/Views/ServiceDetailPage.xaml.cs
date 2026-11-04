using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(ServiceId), "id")]
public partial class ServiceDetailPage : ContentPage
{
    public string ServiceId { get; set; } = "";
    private UniService? service;

    public ServiceDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (service != null) return;     // already loaded (coming back from New Request)

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            service = await FirestoreService.GetServiceAsync(ServiceId);
            if (service == null)
            {
                await DisplayAlertAsync("Not found", "This service could not be found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            NameLabel.Text = service.Name;
            DescriptionLabel.Text = service.Description;
            LocationLabel.Text = service.Location;
            HoursLabel.Text = service.Hours;
            ContactLabel.Text = service.Contact;
            MakeRequestButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load the service. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private async void OnMakeRequestClicked(object sender, EventArgs e)
    {
        if (service == null) return;
        // The name has spaces ("Student Affairs"), so escape it
        await Shell.Current.GoToAsync($"newrequest?service={Uri.EscapeDataString(service.Name)}");
    }
}
