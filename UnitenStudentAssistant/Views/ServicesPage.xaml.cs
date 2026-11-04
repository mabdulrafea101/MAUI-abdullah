using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class ServicesPage : ContentPage
{
    private List<UniService> services = new();

    public ServicesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (services.Count > 0) return;     // load once

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            services = await FirestoreService.GetServicesAsync();
            ServiceList.ItemsSource = services;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load services. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private async void OnServiceTapped(object sender, TappedEventArgs e)
    {
        var service = (UniService)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"servicedetail?id={service.Id}");
    }
}
