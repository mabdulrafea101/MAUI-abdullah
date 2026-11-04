using UnitenStudentAssistant.Helpers;
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(StatusParam), "status")]
public partial class RequestTrackingPage : ContentPage
{
    public string StatusParam { get; set; } = "";       // optional starting tab
    private string selectedStatus = RequestStatus.Pending;
    private List<ServiceRequest> allRequests = new();
    private IDisposable? listener;

    public RequestTrackingPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrEmpty(StatusParam))
        {
            selectedStatus = Uri.UnescapeDataString(StatusParam);
            StatusParam = "";      // only once; coming back from Request Detail keeps the user's tab
        }
        ShowSelectedStatus();

        // One ordered listener and a filter in C#: no Firestore index needed (AGENTS.md §4)
        listener?.Dispose();
        listener = FirestoreService.ListenToRequests(
            requests => MainThread.BeginInvokeOnMainThread(() => ShowRequests(requests)),
            error => MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlertAsync("Live updates stopped", error.Message, "OK")));
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        listener?.Dispose();      // stop listening when the page is hidden
        listener = null;
    }

    private void ShowRequests(List<ServiceRequest> requests)
    {
        allRequests = requests;
        ShowSelectedStatus();
    }

    private void ShowSelectedStatus()
    {
        PendingButton.Text = $"Pending ({CountOf(RequestStatus.Pending)})";
        ProgressButton.Text = $"In Progress ({CountOf(RequestStatus.InProgress)})";
        CompletedButton.Text = $"Completed ({CountOf(RequestStatus.Completed)})";

        StyleSegment(PendingButton, RequestStatus.Pending);
        StyleSegment(ProgressButton, RequestStatus.InProgress);
        StyleSegment(CompletedButton, RequestStatus.Completed);

        // allRequests is already newest first, because the listener orders by createdAt
        RequestList.ItemsSource = allRequests.Where(r => r.Status == selectedStatus).ToList();
        EmptyListLabel.Text = $"No {selectedStatus.ToLower()} requests.";
    }

    private int CountOf(string status) => allRequests.Count(r => r.Status == status);

    private void StyleSegment(Button button, string status)
    {
        bool isSelected = status == selectedStatus;
        button.BackgroundColor = isSelected ? AppColors.Get("Navy") : AppColors.Get("CardBg");
        button.TextColor = isSelected ? Colors.White : AppColors.Get("Navy");
    }

    private void SelectStatus(string status)
    {
        selectedStatus = status;
        ShowSelectedStatus();
    }

    private void OnPendingClicked(object? sender, EventArgs e) => SelectStatus(RequestStatus.Pending);

    private void OnProgressClicked(object? sender, EventArgs e) => SelectStatus(RequestStatus.InProgress);

    private void OnCompletedClicked(object? sender, EventArgs e) => SelectStatus(RequestStatus.Completed);

    private async void OnRequestTapped(object? sender, TappedEventArgs e)
    {
        if (sender is BindableObject bindable && bindable.BindingContext is ServiceRequest request)
        {
            await Shell.Current.GoToAsync($"requestdetail?id={request.Id}");
        }
    }
}
