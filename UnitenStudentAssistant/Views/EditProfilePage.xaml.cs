using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class EditProfilePage : ContentPage
{
    private StudentProfile? profile;
    private bool filled;

    public EditProfilePage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (filled) return;      // fill once; do not overwrite what the user is typing

        SaveButton.IsEnabled = false;      // nothing to save until the fields are filled
        try
        {
            var programmes = await FirestoreService.GetProgrammesAsync();
            ProgrammePicker.ItemsSource = programmes.Select(p => p.Name).ToList();

            profile = await FirestoreService.GetProfileAsync();
            if (profile != null)
            {
                NameEntry.Text = profile.Name;
                StudentIdEntry.Text = profile.StudentId;
                ProgrammePicker.SelectedItem = profile.Programme;    // must equal a programme name exactly
                EmailEntry.Text = profile.Email;
                PhoneEntry.Text = profile.Phone;
                filled = true;
                SaveButton.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load your profile. " + ex.Message, "OK");
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        string error = Validate();
        if (error != "")
        {
            ValidationLabel.Text = error;      // keep what the user typed
            ValidationLabel.IsVisible = true;
            return;
        }
        ValidationLabel.IsVisible = false;

        if (profile == null)
        {
            await DisplayAlertAsync("Something went wrong", "Your profile is not loaded yet.", "OK");
            return;
        }

        SaveButton.IsEnabled = false;
        Saving.IsVisible = Saving.IsRunning = true;
        try
        {
            profile.Name = NameEntry.Text?.Trim() ?? "";
            profile.Programme = (string)ProgrammePicker.SelectedItem;
            profile.Email = EmailEntry.Text?.Trim() ?? "";
            profile.Phone = PhoneEntry.Text?.Trim() ?? "";
            await FirestoreService.UpdateProfileAsync(profile);

            await DisplayAlertAsync("Saved", "Your profile has been updated.", "OK");
            await Shell.Current.GoToAsync("..");      // Profile reloads in its OnAppearing
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not save your profile. " + ex.Message, "OK");
        }
        finally
        {
            SaveButton.IsEnabled = true;
            Saving.IsVisible = Saving.IsRunning = false;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync("..");

    // Returns the first problem, or "" when the form is fine
    private string Validate()
    {
        string email = (EmailEntry.Text ?? "").Trim();

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
            return "Please enter your name.";
        if (ProgrammePicker.SelectedIndex < 0)
            return "Please choose your programme.";
        if (!email.Contains('@') || !email.Contains('.'))
            return "Please enter a valid email address.";
        if ((PhoneEntry.Text ?? "").Trim().Length < 9)
            return "Please enter a valid phone number.";
        return "";
    }
}
