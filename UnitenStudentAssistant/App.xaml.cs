namespace UnitenStudentAssistant;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light;   // our colours are designed for light mode only
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Start on our own splash page. It replaces itself with AppShell when ready,
        // so the splash is never on the back stack.
        return new Window(new Views.SplashPage());
    }
}
