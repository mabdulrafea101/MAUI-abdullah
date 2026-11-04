namespace UnitenStudentAssistant.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        // ApplicationDisplayVersion in the .csproj, so "Version 1.0"
        VersionLabel.Text = $"Version {AppInfo.Current.VersionString}";
    }
}
