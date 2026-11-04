using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(RequestId), "id")]
public partial class RequestDetailPage : ContentPage
{
    public string RequestId { get; set; } = "";
    private IDisposable? listener;

    public RequestDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        listener?.Dispose();
        listener = FirestoreService.ListenToRequest(
            RequestId,
            request => MainThread.BeginInvokeOnMainThread(async () => await ShowRequestAsync(request)),
            error => MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlertAsync("Live updates stopped", error.Message, "OK")));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        listener?.Dispose();      // stop listening when the page is hidden
        listener = null;
    }

    // Runs on first load and again every time the document changes in Firestore
    private async Task ShowRequestAsync(ServiceRequest? request)
    {
        if (request == null)
        {
            await DisplayAlertAsync("Request not found", "This request no longer exists.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        RequestNoLabel.Text = request.RequestNo;
        StatusLabel.Text = request.Status;
        StatusLabel.TextColor = request.StatusTextColor;
        StatusBadge.BackgroundColor = request.StatusBgColor;
        SubjectLabel.Text = request.Subject;
        ServiceLabel.Text = request.Service;
        CreatedLabel.Text = request.CreatedText;
        UpdatedLabel.Text = request.UpdatedText;
        DetailsLabel.Text = request.Details;
    }
}
