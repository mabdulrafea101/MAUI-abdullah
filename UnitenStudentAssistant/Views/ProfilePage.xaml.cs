using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    // Reload every time, so the changes from Edit Profile show after Back
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var profile = await FirestoreService.GetProfileAsync();
            if (profile == null)
            {
                NameLabel.Text = "Profile not found";
                return;
            }

            InitialsLabel.Text = profile.Initials;
            NameLabel.Text = profile.Name;
            StudentIdLabel.Text = profile.StudentId;
            ProgrammeLabel.Text = profile.Programme;
            EmailLabel.Text = profile.Email;
            PhoneLabel.Text = profile.Phone;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load your profile. " + ex.Message, "OK");
        }
    }

    private async void OnEditClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("editprofile");

    private async void OnAboutClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("about");
}
