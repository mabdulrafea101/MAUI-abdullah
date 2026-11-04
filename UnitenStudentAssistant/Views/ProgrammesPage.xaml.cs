using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class ProgrammesPage : ContentPage
{
    private List<Programme> allProgrammes = new();

    public ProgrammesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (allProgrammes.Count > 0) return;          // load once; Back from the detail keeps the search

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            allProgrammes = await FirestoreService.GetProgrammesAsync();
            ProgrammeList.ItemsSource = allProgrammes;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load programmes. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string text = (e.NewTextValue ?? "").Trim().ToLower();
        ProgrammeList.ItemsSource = allProgrammes
            .Where(p => p.Name.ToLower().Contains(text) || p.Faculty.ToLower().Contains(text))
            .ToList();
    }

    private async void OnProgrammeTapped(object sender, TappedEventArgs e)
    {
        var programme = (Programme)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"programmedetail?id={programme.Id}");
    }
}
