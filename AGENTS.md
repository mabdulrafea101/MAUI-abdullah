# AGENTS.md — rules for anyone (human or AI) writing code in this repo

**UNITEN Student Assistant** is a .NET MAUI Android app for the CSNB5123 class project, built by Abdullah (is01084960). It is a **BSCS class project**: the code must stay simple enough for a second-year student to read, explain to a lecturer and change in class.

Read these before writing code:

| File | What it decides |
|---|---|
| `docs/superpowers/specs/2026-10-30-uniten-student-assistant-design.md` | What we build and why (the spec) |
| `DESIGN.md` | Colours, fonts, spacing, allowed controls, the layout of every screen |
| `docs/NAVIGATION.md` | Every route, parameter and tap target; the back-stack behaviour |
| `docs/DATA-MODEL.md` | Firestore collections, C# model classes, seed data, AI prompt |
| `docs/SETUP-FIREBASE-GEMINI.md` | Firebase project, `google-services.json`, Gemini key |
| `docs/superpowers/plans/2026-10-30-uniten-student-assistant.md` | The step-by-step build plan. Follow it in order. |
| `docs/TEST-CHECKLIST.md` | Manual emulator checks after each step, and the demo script |

If code and docs disagree, **the docs win**. Fix the code, or update the doc first in the same commit with a reason.

---

## 1. Tech stack (fixed; do not add to it)

| Thing | Choice |
|---|---|
| Framework | .NET 10, .NET MAUI (`Microsoft.Maui.Controls` 10.0.x, as installed by Visual Studio) |
| Target | **Android only**: `<TargetFrameworks>net10.0-android</TargetFrameworks>`, `SupportedOSPlatformVersion` 23.0. Tested on the Android emulator. |
| Database | Cloud Firestore via **`Plugin.Firebase.Firestore` 4.0.0** (it brings in `Plugin.Firebase.Core`). Nothing else from Firebase. |
| AI | Google Gemini **REST** `generateContent` through `HttpClient` and `System.Text.Json`. No SDK package. |
| UI | Built-in MAUI controls only, plus our own `CalendarMonthView` |

**NuGet packages allowed:** the template's packages plus `Plugin.Firebase.Firestore`. Nothing else: no CommunityToolkit, no MVVM libraries, no UI kits, no Newtonsoft.

## 2. Architecture: code-behind, no MVVM

Our lecturer teaches MAUI **without MVVM**. We keep that, but stay tidy:

```
UnitenStudentAssistant/
  App.xaml(.cs)            light theme, CreateWindow → SplashPage
  AppShell.xaml(.cs)       TabBar + Routing.RegisterRoute (see docs/NAVIGATION.md)
  MauiProgram.cs           UseMauiApp, fonts, Firebase init (Android lifecycle)
  Models/                  plain data classes (IFirestoreObject) + RequestStatus constants
  Services/                static classes: FirestoreService, GeminiService, SeedDataService
  Helpers/                 static classes: StatusColors, AppColors, AiPromptBuilder, AppConfig (+ AppSecrets)
  Controls/                CalendarMonthView.xaml(.cs)
  Views/                   one .xaml + .xaml.cs per screen (16 pages)
  Resources/Styles/        Colors.xaml, Styles.xaml
  Resources/Images/        our SVG icons
  Resources/Raw/           secrets.json (git-ignored), secrets.sample.json
```

**Rules:**

1. **Each page = XAML layout + code-behind logic.**
   - The code-behind loads data, fills controls and handles events.
   - There are no ViewModels, `ICommand`s, `INotifyPropertyChanged` classes or `BindingContext = this` tricks.
2. **Logic that is not about one screen goes into a plain static C# class** in `Services/` or `Helpers/`, in a `.cs` file with no XAML. Pages call it directly, e.g. `await FirestoreService.GetProgrammesAsync()`.
3. **No dependency injection.**
   - Do not register pages or services in `MauiProgram`.
   - Shell creates pages from `Routing.RegisterRoute`.
   - Services are `static`.
4. **Binding is used only inside `DataTemplate`s** (`{Binding Name}` on list items). Everything else is set in code: `TitleLabel.Text = programme.Name;`, `ProgrammeList.ItemsSource = programmes;`.
5. **Events, not commands.**
   - Use `Clicked="OnSubmitClicked"`, `TextChanged="OnSearchTextChanged"` and `Tapped="OnProgrammeTapped"`.
   - Event handlers are `private async void On...(object sender, EventArgs e)`.
6. **Load in `OnAppearing`, clean up in `OnDisappearing`.** Live Firestore listeners must be disposed in `OnDisappearing` (see §4).
7. **Navigation only with `Shell.Current.GoToAsync(...)`**, using exactly the routes in docs/NAVIGATION.md. Never use `Navigation.PushAsync` or `new SomePage()` (the only exceptions are `new AppShell()` and `new SplashPage()` in App and SplashPage).
8. **Route parameters with `[QueryProperty]`** on the page class:

   ```csharp
   [QueryProperty(nameof(ProgrammeId), "id")]
   public partial class ProgrammeDetailPage : ContentPage
   {
       public string ProgrammeId { get; set; } = "";
       ...
   }
   ```

## 3. The canonical page (copy this shape for every list page)

```xml
<!-- Views/ProgrammesPage.xaml -->
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             x:Class="UnitenStudentAssistant.Views.ProgrammesPage"
             Title="Programmes"
             BackgroundColor="{StaticResource PageBg}">
    <Grid RowDefinitions="Auto,*" Padding="16" RowSpacing="12">
        <SearchBar x:Name="ProgrammeSearch" Placeholder="Search programmes"
                   TextChanged="OnSearchTextChanged" />
        <ActivityIndicator x:Name="Loading" Grid.Row="1" IsRunning="False"
                           IsVisible="False" Color="{StaticResource Navy}" />
        <CollectionView x:Name="ProgrammeList" Grid.Row="1">
            <CollectionView.ItemsLayout>
                <LinearItemsLayout Orientation="Vertical" ItemSpacing="12" />
            </CollectionView.ItemsLayout>
            <CollectionView.EmptyView>
                <Label Text="No programmes match your search." Style="{StaticResource CaptionLabel}"
                       HorizontalOptions="Center" Margin="0,24" />
            </CollectionView.EmptyView>
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:Programme">
                    <Border Style="{StaticResource CardBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnProgrammeTapped" />
                        </Border.GestureRecognizers>
                        <VerticalStackLayout Spacing="4">
                            <Label Text="{Binding Name}" Style="{StaticResource SubtitleLabel}" />
                            <Label Text="{Binding Faculty}" Style="{StaticResource CaptionLabel}" />
                            <Label Text="{Binding Duration}" Style="{StaticResource CaptionLabel}"
                                   TextColor="{StaticResource UnitenBlue}" />
                        </VerticalStackLayout>
                    </Border>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

```csharp
// Views/ProgrammesPage.xaml.cs
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class ProgrammesPage : ContentPage
{
    private List<Programme> allProgrammes = new();

    public ProgrammesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (allProgrammes.Count > 0) return;          // load once

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            allProgrammes = await FirestoreService.GetProgrammesAsync();
            ProgrammeList.ItemsSource = allProgrammes;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load programmes. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string text = (e.NewTextValue ?? "").Trim().ToLower();
        ProgrammeList.ItemsSource = allProgrammes
            .Where(p => p.Name.ToLower().Contains(text) || p.Faculty.ToLower().Contains(text))
            .ToList();
    }

    private async void OnProgrammeTapped(object sender, TappedEventArgs e)
    {
        var programme = (Programme)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"programmedetail?id={programme.Id}");
    }
}
```

**Why tap on the Border instead of `SelectionMode`:** a tap gives no grey "selected" flash and nothing to reset afterwards, and the handler reads the tapped item from `BindingContext`. Leave `CollectionView` at its default `SelectionMode="None"`.

**Why `x:DataType` on every `DataTemplate`:** it turns on compiled bindings, so a typo such as `{Binding Nmae}` is a build error instead of a silently empty label. It needs the `xmlns:models` line at the top of the page.

**Why this order (spinner, try, catch, finally):** if the internet is off, the page shows a friendly alert instead of crashing.

## 4. Realtime pages (Request Tracking, Request Detail)

These two pages are the teacher demo, so this exact pattern must be followed:

```csharp
private IDisposable? listener;

protected override void OnAppearing()
{
    base.OnAppearing();
    listener?.Dispose();      // never keep two listeners alive
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
```

- `ShowRequests` stores the list in a field, updates the three count labels and the selected tab's `ItemsSource`.
- The filter is done in C#: `all.Where(r => r.Status == selectedStatus)`.
- Do not use Firestore `where` queries here. A where query plus an order would need a composite index; one ordered listener plus an in-memory filter needs none.

## 5. Code style

- **C# version:** the template default. Turn nullable on (the template already does). Use file-scoped namespaces: `namespace UnitenStudentAssistant.Views;`.
- **Names:**
  - Pages are `XxxPage`.
  - XAML element names are PascalCase nouns (`ProgrammeList`, `SubjectEntry`, `SubmitButton`).
  - Private fields are camelCase with no underscore (`allProgrammes`), matching the class style.
- **Methods** stay short. When one passes ~30 lines, split it into a private helper method in the same page (e.g. `ShowRequests`, `BuildDays`).
- **Comments** explain *why*, in plain English, and stay sparse. Students must be able to read the code aloud.
- **LINQ:** `Where`, `Select`, `OrderBy`, `Take`, `ToList`, `Count`, `Any` and `FirstOrDefault` are allowed. Nothing more clever.
- **Async:**
  - Every Firestore or HTTP call is `await`ed inside `try/catch`.
  - Never use `.Result` or `.Wait()`.
  - `async void` only for event handlers and `OnAppearing`.
- **Strings:** use interpolation (`$"REQ-{number}"`). Date formats come from docs/DATA-MODEL.md.
- **Alerts:** `await DisplayAlertAsync(title, message, "OK")`. `DisplayAlert` is obsolete in .NET 10.
- **No magic colours or sizes in pages.** Use `{StaticResource ...}` and the styles from `Resources/Styles/Styles.xaml` (DESIGN.md §1–3).
- **Forms:** when a form is invalid, show the message in the page's red validation `Label` instead of an alert, and keep the user's input.
- **Buttons:** while saving, set `SubmitButton.IsEnabled = false` and re-enable it in `finally`, so a double tap cannot create two requests.

## 6. UI rules (short version of DESIGN.md)

- **Allowed:** Grid, VerticalStackLayout, HorizontalStackLayout, ScrollView, Border, Label, Button, ImageButton, Image, Entry, Editor, Picker, SearchBar, CollectionView, BindableLayout (short lists inside a ScrollView), ActivityIndicator, BoxView (divider lines and the calendar dot only), TapGestureRecognizer, and `CalendarMonthView`.
- **Never put a `CollectionView` inside a `ScrollView`.** Use `BindableLayout` on a `VerticalStackLayout` there instead (DESIGN.md §8).
- **Colours in C#:** use `AppColors.Get("Navy")` (from `Helpers/AppColors.cs`), never `Color.FromArgb("#...")`.
- **Forbidden:** `Frame` and `ListView` (both obsolete), TableView, CarouselView, RefreshView, SwipeView, any third-party control, custom handlers/renderers, animations, shadows and gradients.
- **Cards** are always `Style="{StaticResource CardBorder}"`. Primary buttons are `PrimaryButton` (red); secondary buttons are `SecondaryButton` (blue outline).
- **One red button per screen at most.**
- **Light theme only.**

## 7. Secrets and git

- The Gemini key lives in `Resources/Raw/secrets.json`, which is **git-ignored**. The file to commit is `Resources/Raw/secrets.sample.json`:

  ```json
  { "GeminiApiKey": "PASTE-YOUR-KEY-HERE", "GeminiModel": "gemini-3.8-flash" }
  ```

- `Helpers/AppConfig.cs` reads it once with `FileSystem.OpenAppPackageFileAsync("secrets.json")` and parses it with `JsonDocument` (no reflection, so it is safe in Release builds). If the file or key is missing, the AI page shows a friendly bubble: "AI is not set up yet. Add your Gemini key to secrets.json."
- `google-services.json` is also git-ignored. Each developer downloads their own from the Firebase Console.
- **Never** paste keys into code, docs, commit messages or chat logs.
- **Commits:**
  - One commit per plan step, with a message like `feat: programmes list and detail` or `fix: tracking counts not updating`.
  - Do not push unless Abdullah asks.

## 8. How to check your work

There is no unit-test project, by design: this is a UI class project. After every plan step:

1. Run `dotnet build -f net10.0-android` from the project folder. It must finish with **0 errors**. Fix new warnings about obsolete APIs.
2. Run the app on the Android emulator from Visual Studio (F5).
3. Do the matching rows in `docs/TEST-CHECKLIST.md` and tick them.
4. Only then commit.

If you are an AI agent and cannot run the emulator, say so plainly, and list which checklist rows still need a human. Do not claim a screen "works" because it builds.

## 9. Things that commonly go wrong (and the fix)

| Symptom | Cause | Fix |
|---|---|---|
| Crash on start: "Default FirebaseApp is not initialized" | `google-services.json` missing or the wrong build action | Put it in the project root with build action **GoogleServicesJson**; the package name must equal `ApplicationId` |
| Build fails with very long paths | Windows 260-character path limit (Firebase native packages) | Enable Windows long paths (see setup doc) or move the solution to a short folder such as `C:\dev\` |
| Lists empty, no error | Firestore rules not in test mode, or the seed has not run | Check the Console rules; restart the app with internet on |
| Tracking does not update live | Listener disposed too early, or the UI updated off the main thread | Follow §4 exactly |
| Status shows grey and is missing from every tab | Status typed with a different spelling in the Console | Use exactly `Pending`, `In Progress`, `Completed` |
| SVG icon not showing | File name has capitals or a dash | Lowercase letters, digits and `_` only; reference it as `.png` |
| AI says "not set up" | `secrets.json` missing from `Resources/Raw` | Copy the sample, add the key, rebuild |
