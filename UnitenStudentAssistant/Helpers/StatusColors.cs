using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Helpers;

// Colours for the status pill. Any unknown status shows grey (DESIGN.md §7).
public static class StatusColors
{
    public static Color GetText(string status) => status switch
    {
        RequestStatus.Pending => AppColors.Get("PendingText"),
        RequestStatus.InProgress => AppColors.Get("ProgressText"),
        RequestStatus.Completed => AppColors.Get("DoneText"),
        _ => AppColors.Get("TextMuted")
    };

    public static Color GetBackground(string status) => status switch
    {
        RequestStatus.Pending => AppColors.Get("PendingBg"),
        RequestStatus.InProgress => AppColors.Get("ProgressBg"),
        RequestStatus.Completed => AppColors.Get("DoneBg"),
        _ => AppColors.Get("PageBg")
    };
}

