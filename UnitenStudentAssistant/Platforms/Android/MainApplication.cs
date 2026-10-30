using Android.App;
using Android.Runtime;

namespace UnitenStudentAssistant;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
        {
            Android.Util.Log.Error("UnitenCrash", args.Exception?.ToString() ?? "Unknown Android error");
        };

        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            Android.Util.Log.Error("UnitenCrash", args.ExceptionObject?.ToString() ?? "Unknown AppDomain error");
        };
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
