using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(ServiceParam), "service")]
public partial class NewRequestPage : ContentPage
{
    public string ServiceParam { get; set; } = "";      // optional service name to preselect
    private bool servicesLoaded;

    public NewRequestPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!servicesLoaded)
        {
            try
            {
                var services = await FirestoreService.GetServicesAsync();
                ServicePicker.ItemsSource = services.Select(s => s.Name).ToList();
                servicesLoaded = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Something went wrong", "Could not load services. " + ex.Message, "OK");
            }
        }

        if (!string.IsNullOrEmpty(ServiceParam))
        {
            // Unescape is harmless if Shell has already decoded the value
            ServicePicker.SelectedItem = Uri.UnescapeDataString(ServiceParam);
            ServiceParam = "";     // only once, so the cleared form stays cleared after Back
        }
    }

    private async void OnSubmitClicked(object? sender, EventArgs e)
    {
        string error = Validate();
        if (error != "")
        {
            ValidationLabel.Text = error;      // keep what the user typed
            ValidationLabel.IsVisible = true;
            return;
        }
        ValidationLabel.IsVisible = false;

        SubmitButton.IsEnabled = false;       // a double tap cannot create two requests
        Saving.IsVisible = Saving.IsRunning = true;
        try
        {
            string requestNo = await FirestoreService.AddRequestAsync(
                (string)ServicePicker.SelectedItem,
                SubjectEntry.Text?.Trim() ?? "",
                DetailsEditor.Text?.Trim() ?? "");

            await DisplayAlertAsync("Thank you", $"Request {requestNo} submitted.", "OK");
            ClearForm();
            await Shell.Current.GoToAsync("tracking?status=Pending");    // pushed on top of this form
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not submit your request. " + ex.Message, "OK");
        }
        finally
        {
            SubmitButton.IsEnabled = true;
            Saving.IsVisible = Saving.IsRunning = false;
        }
    }

    // Returns the first problem, or "" when the form is fine
    private string Validate()
    {
        if (ServicePicker.SelectedIndex < 0)
            return "Please choose a service.";
        if (string.IsNullOrWhiteSpace(SubjectEntry.Text))
            return "Please enter a subject.";
        if ((DetailsEditor.Text ?? "").Trim().Length < 10)
            return "Please describe your request (at least 10 characters).";
        return "";
    }

    private void ClearForm()
    {
        ServicePicker.SelectedIndex = -1;
        SubjectEntry.Text = "";
        DetailsEditor.Text = "";
        ValidationLabel.IsVisible = false;
    }
}
