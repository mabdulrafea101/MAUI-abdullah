using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(ProgrammeId), "id")]
public partial class ProgrammeDetailPage : ContentPage
{
    public string ProgrammeId { get; set; } = "";
    private bool loaded;

    public ProgrammeDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (loaded) return;

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var programme = await FirestoreService.GetProgrammeAsync(ProgrammeId);
            if (programme == null)
            {
                await DisplayAlertAsync("Not found", "This programme could not be found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            ShowProgramme(programme);
            loaded = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load the programme. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private void ShowProgramme(Programme programme)
    {
        NameLabel.Text = programme.Name;
        FacultyLabel.Text = programme.Faculty;
        DurationLabel.Text = programme.Duration;
        FacultyValueLabel.Text = programme.Faculty;
        DescriptionLabel.Text = programme.Description;
        BindableLayout.SetItemsSource(MajorsList, programme.Majors);
    }
}
