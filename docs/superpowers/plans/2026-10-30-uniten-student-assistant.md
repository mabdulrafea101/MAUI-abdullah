# UNITEN Student Assistant Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a simple, attractive .NET MAUI Android app (16 pages) that lets a UNITEN student browse programmes, read the academic calendar, find services, submit service requests, watch their status change live from Firestore, and ask a Gemini-powered assistant about UNITEN.

**Architecture:** Each page is XAML plus code-behind (no MVVM, no DI). All Firestore access goes through one static `FirestoreService`; Gemini is one static `GeminiService` calling the REST API with `HttpClient`. Shell provides a 5-tab `TabBar` and registered routes for pushed pages; a plain `SplashPage` seeds Firestore and then swaps in `AppShell`.

**Tech Stack:** .NET 10 MAUI (Android only), Plugin.Firebase.Firestore 4.0.0, Cloud Firestore (test mode), Google Gemini REST `generateContent`, System.Text.Json.

**Spec:** `docs/superpowers/specs/2026-10-30-uniten-student-assistant-design.md`. Read it together with `AGENTS.md`, `DESIGN.md`, `docs/NAVIGATION.md`, `docs/DATA-MODEL.md` and `docs/TEST-CHECKLIST.md`. If this plan and those docs disagree, the docs win; fix the plan in the same commit.

## Global Constraints

- Target: `<TargetFrameworks>net10.0-android</TargetFrameworks>`, `SupportedOSPlatformVersion` for android = `23.0`, `ApplicationId` = `com.abdullah.unitenstudentassistant`, `ApplicationDisplayVersion` = `1.0`.
- NuGet: the template's packages plus **`Plugin.Firebase.Firestore` 4.0.0** only. No CommunityToolkit, no MVVM library, no UI kit, no Newtonsoft.
- No MVVM, no `ICommand`, no `INotifyPropertyChanged`, no dependency injection. Bindings only inside `DataTemplate`s with `x:DataType`.
- Allowed controls only (DESIGN.md §8). Never `Frame`, `ListView`, `TableView`, `CarouselView`, `RefreshView`, `SwipeView`. Never a `CollectionView` inside a `ScrollView`.
- Alerts: `DisplayAlertAsync` only. `DisplayAlert` is obsolete in .NET 10.
- No hex colour in any file under `Views/` or `Controls/`. XAML uses `{StaticResource Key}`; C# uses `AppColors.Get("Key")`.
- Routes, parameters and tap targets exactly as `docs/NAVIGATION.md` §2–§4.
- Status strings exactly `Pending`, `In Progress`, `Completed` (constants in `Models/RequestStatus.cs`).
- `Resources/Raw/secrets.json` and `google-services.json` are never committed. Commit `secrets.sample.json` only.
- UI copy in English, sentence case, no emoji. At most one red (`PrimaryButton`) button per screen.
- Light theme only (`UserAppTheme = AppTheme.Light`).

## How to run and verify each task

All commands run from the **repo root** `MAUI-abdullah/` unless stated. The project folder is `UnitenStudentAssistant/` (the solution file sits in the same folder because "Place solution and project in the same directory" was ticked).

- **Build:** `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android` must end with `0 Error(s)`.
- **Run:** open the solution in Visual Studio, pick the Android emulator, press F5.
- **Check:** tick the rows of `docs/TEST-CHECKLIST.md` section S*n* for Task *n*.
- **Secret guard before every commit:** `git diff --cached --name-only` must not print `secrets.json` or `google-services.json`. A one-line check:

  ```bash
  git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
  ```

There is no unit-test project (spec decision D12). Each task's "test" is its checklist section on the emulator. An AI agent that cannot run the emulator must say so and list the rows a human still has to tick; it must not claim a screen works because it builds.

## File structure

```
UnitenStudentAssistant/
  UnitenStudentAssistant.csproj        Task 1 (targets, icon, splash), Task 2 (package, google-services.json)
  google-services.json                 Task 2 (downloaded, git-ignored)
  App.xaml / App.xaml.cs               Task 1: merge styles, light theme, CreateWindow -> SplashPage
  AppShell.xaml / AppShell.xaml.cs     Task 1: TabBar (5 tabs) + 10 RegisterRoute calls
  MauiProgram.cs                       Task 2: Firebase init in the Android lifecycle
  Platforms/Android/Resources/values/colors.xml   Task 1: navy/red Android colours
  Models/
    RequestStatus.cs                   Task 2: the 3 status constants
    Programme.cs                       Task 2
    CalendarEvent.cs                   Task 2
    UniService.cs                      Task 2
    ServiceRequest.cs                  Task 2
    StudentProfile.cs                  Task 2
    ChatMessage.cs                     Task 2 (not stored; AI page only)
  Helpers/
    AppColors.cs                       Task 2: AppColors.Get("Key") -> Color from App resources
    StatusColors.cs                    Task 2: status -> pill colours
    AppConfig.cs                       Task 11: AppSecrets + AppConfig.GetSecretsAsync()
    AiPromptBuilder.cs                 Task 11: Firestore data -> system instruction text
  Services/
    FirestoreService.cs                Task 2: the only class that touches Firestore
    SeedDataService.cs                 Task 2: first-run seed with fixed ids
    GeminiService.cs                   Task 11: one REST call per question
  Controls/
    CalendarMonthView.xaml(.cs)        Task 5: the only custom control
  Views/                               Task 1 creates all 16 as placeholders; later tasks replace them
    SplashPage            Task 1, final in Task 2
    HomePage              Task 6
    ProgrammesPage        Task 3      ProgrammeDetailPage   Task 3
    ServicesPage          Task 4      ServiceDetailPage     Task 4
    CalendarPage          Task 5
    ServiceRequestsPage   Task 7      NewRequestPage        Task 7
    RequestTrackingPage   Task 8      RequestDetailPage     Task 8
    ProfilePage           Task 9      EditProfilePage       Task 9      AboutPage   Task 9
    SearchPage            Task 10
    AiAssistantPage       Task 11
  Resources/
    AppIcon/appicon.svg, appiconfg.svg Task 1
    Splash/splash.svg                  Task 1
    Images/*.svg                       Task 1: 5 tab icons, 6 tile icons, logo_u
    Styles/Colors.xaml, Styles.xaml    Task 1 (replace the template files)
    Raw/secrets.sample.json            Task 11 (committed)
    Raw/secrets.json                   Task 11 (local only, git-ignored)
```

**Task order and why:** each task ends with something you can tap on the emulator. Programmes (3), Services (4) and Calendar (5) come before Home (6) so the Home tiles have real pages to open. The Requests tab and New Request (7) come before Tracking (8) so there is a way to reach Tracking. Search (10) comes after every page it links to. AI (11) is last because it reads all the other data.

---

### Task 1: Project skeleton, theme, icons, splash and Shell

**Files:**
- Modify: `UnitenStudentAssistant/UnitenStudentAssistant.csproj`
- Modify: `UnitenStudentAssistant/Platforms/Android/Resources/values/colors.xml`
- Replace: `UnitenStudentAssistant/Resources/Styles/Colors.xaml`, `UnitenStudentAssistant/Resources/Styles/Styles.xaml`
- Replace: `UnitenStudentAssistant/Resources/AppIcon/appicon.svg`, `appiconfg.svg`, `UnitenStudentAssistant/Resources/Splash/splash.svg`
- Create: 12 SVGs in `UnitenStudentAssistant/Resources/Images/`
- Modify: `UnitenStudentAssistant/App.xaml`, `App.xaml.cs`, `AppShell.xaml`, `AppShell.xaml.cs`
- Create: `UnitenStudentAssistant/Views/SplashPage.xaml(.cs)` and 15 placeholder pages in `UnitenStudentAssistant/Views/`
- Delete: `MainPage.xaml`, `MainPage.xaml.cs`, `Resources/Images/dotnet_bot.png`

**Interfaces:**
- Consumes: nothing.
- Produces: every colour key and style key listed in DESIGN.md §1–§3; the 16 page classes in namespace `UnitenStudentAssistant.Views` (later tasks replace their content but keep the class names); the routes `home`, `services`, `ai`, `requests`, `profile` (tabs) and `programmes`, `programmedetail`, `calendar`, `search`, `servicedetail`, `newrequest`, `tracking`, `requestdetail`, `editprofile`, `about` (pushed).

- [ ] **Step 1: Create the project**

Follow `docs/SETUP-FIREBASE-GEMINI.md` Part 1 exactly: a **.NET MAUI App** named `UnitenStudentAssistant` in the repo folder, .NET 10, "Place solution and project in the same directory" ticked, "Include sample content" unticked. Then edit the `.csproj` as Part 1 says (Android only, identity values, min SDK 23).

- [ ] **Step 2: Set the icon, splash and image items in the `.csproj`**

Find the template's resources `<ItemGroup>` and make it exactly this. The only changes are the two navy `Color` values and the deleted `dotnet_bot.png` line.

```xml
<ItemGroup>
    <!-- App Icon -->
    <MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="#1B2A4A" />

    <!-- Splash Screen -->
    <MauiSplashScreen Include="Resources\Splash\splash.svg" Color="#1B2A4A" BaseSize="128,128" />

    <!-- Images -->
    <MauiImage Include="Resources\Images\*" />

    <!-- Custom Fonts -->
    <MauiFont Include="Resources\Fonts\*" />

    <!-- Raw Assets (also remove the "CopyToOutputDirectory" items if the template has any) -->
    <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

Delete `MainPage.xaml`, `MainPage.xaml.cs` and `Resources/Images/dotnet_bot.png`.

- [ ] **Step 3: Android colours**

Replace `Platforms/Android/Resources/values/colors.xml`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<resources>
    <color name="colorPrimary">#1B2A4A</color>
    <color name="colorPrimaryDark">#1B2A4A</color>
    <color name="colorAccent">#DA291C</color>
</resources>
```

- [ ] **Step 4: Colours resource dictionary**

Replace `Resources/Styles/Colors.xaml` completely:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<?xaml-comp compile="true" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- Brand -->
    <Color x:Key="Navy">#1B2A4A</Color>
    <Color x:Key="UnitenRed">#DA291C</Color>
    <Color x:Key="UnitenBlue">#005DAA</Color>

    <!-- Surfaces and text -->
    <Color x:Key="PageBg">#F5F6F8</Color>
    <Color x:Key="CardBg">#FFFFFF</Color>
    <Color x:Key="CardStroke">#E3E6EB</Color>
    <Color x:Key="TextMain">#1F2633</Color>
    <Color x:Key="TextMuted">#6B7280</Color>
    <Color x:Key="AiBubble">#EEF0F4</Color>

    <!-- Status pills -->
    <Color x:Key="PendingText">#B45309</Color>
    <Color x:Key="PendingBg">#FEF3C7</Color>
    <Color x:Key="ProgressText">#005DAA</Color>
    <Color x:Key="ProgressBg">#DBEAFE</Color>
    <Color x:Key="DoneText">#15803D</Color>
    <Color x:Key="DoneBg">#DCFCE7</Color>

</ResourceDictionary>
```

- [ ] **Step 5: Styles resource dictionary**

Replace `Resources/Styles/Styles.xaml` completely. Styles are declared before the styles that are `BasedOn` them.

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<?xaml-comp compile="true" ?>
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">

    <!-- ===== Implicit styles (no key): apply to every control of that type ===== -->

    <Style TargetType="ContentPage" ApplyToDerivedTypes="True">
        <Setter Property="BackgroundColor" Value="{StaticResource PageBg}" />
    </Style>

    <Style TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
    </Style>

    <Style TargetType="Entry">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
        <Setter Property="PlaceholderColor" Value="{StaticResource TextMuted}" />
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="MinimumHeightRequest" Value="44" />
    </Style>

    <Style TargetType="Editor">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
        <Setter Property="PlaceholderColor" Value="{StaticResource TextMuted}" />
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="MinimumHeightRequest" Value="44" />
    </Style>

    <Style TargetType="Picker">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
        <Setter Property="TitleColor" Value="{StaticResource TextMuted}" />
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="MinimumHeightRequest" Value="44" />
    </Style>

    <Style TargetType="SearchBar">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
        <Setter Property="PlaceholderColor" Value="{StaticResource TextMuted}" />
        <Setter Property="CancelButtonColor" Value="{StaticResource TextMuted}" />
        <Setter Property="BackgroundColor" Value="{StaticResource CardBg}" />
    </Style>

    <Style TargetType="ActivityIndicator">
        <Setter Property="Color" Value="{StaticResource Navy}" />
    </Style>

    <Style TargetType="Shell" ApplyToDerivedTypes="True">
        <Setter Property="Shell.BackgroundColor" Value="{StaticResource Navy}" />
        <Setter Property="Shell.ForegroundColor" Value="White" />
        <Setter Property="Shell.TitleColor" Value="White" />
        <Setter Property="Shell.UnselectedColor" Value="{StaticResource TextMuted}" />
        <Setter Property="Shell.NavBarHasShadow" Value="False" />
        <Setter Property="Shell.TabBarBackgroundColor" Value="{StaticResource CardBg}" />
        <Setter Property="Shell.TabBarForegroundColor" Value="{StaticResource Navy}" />
        <Setter Property="Shell.TabBarTitleColor" Value="{StaticResource Navy}" />
        <Setter Property="Shell.TabBarUnselectedColor" Value="{StaticResource TextMuted}" />
    </Style>

    <!-- ===== Text styles ===== -->

    <Style x:Key="TitleLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansSemibold" />
        <Setter Property="FontSize" Value="22" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
    </Style>

    <Style x:Key="SubtitleLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansSemibold" />
        <Setter Property="FontSize" Value="17" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
    </Style>

    <Style x:Key="BodyLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
        <Setter Property="LineBreakMode" Value="WordWrap" />
    </Style>

    <Style x:Key="StrongLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansSemibold" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
    </Style>

    <Style x:Key="CaptionLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="13" />
        <Setter Property="TextColor" Value="{StaticResource TextMuted}" />
    </Style>

    <Style x:Key="SmallLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansSemibold" />
        <Setter Property="FontSize" Value="12" />
        <Setter Property="TextColor" Value="{StaticResource TextMain}" />
    </Style>

    <Style x:Key="ErrorLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="13" />
        <Setter Property="TextColor" Value="{StaticResource UnitenRed}" />
        <Setter Property="IsVisible" Value="False" />
    </Style>

    <Style x:Key="DateDayLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansSemibold" />
        <Setter Property="FontSize" Value="20" />
        <Setter Property="TextColor" Value="{StaticResource Navy}" />
        <Setter Property="HorizontalOptions" Value="Center" />
    </Style>

    <Style x:Key="AvatarLabel" TargetType="Label">
        <Setter Property="FontFamily" Value="OpenSansSemibold" />
        <Setter Property="FontSize" Value="28" />
        <Setter Property="TextColor" Value="White" />
        <Setter Property="HorizontalOptions" Value="Center" />
        <Setter Property="VerticalOptions" Value="Center" />
    </Style>

    <!-- ===== Containers ===== -->

    <Style x:Key="CardBorder" TargetType="Border">
        <Setter Property="BackgroundColor" Value="{StaticResource CardBg}" />
        <Setter Property="Stroke" Value="{StaticResource CardStroke}" />
        <Setter Property="StrokeThickness" Value="1" />
        <Setter Property="StrokeShape" Value="RoundRectangle 12" />
        <Setter Property="Padding" Value="16" />
    </Style>

    <Style x:Key="InputBorder" TargetType="Border" BasedOn="{StaticResource CardBorder}">
        <Setter Property="Padding" Value="12,4" />
    </Style>

    <Style x:Key="TileBorder" TargetType="Border" BasedOn="{StaticResource CardBorder}">
        <Setter Property="HeightRequest" Value="110" />
        <Setter Property="Padding" Value="12" />
    </Style>

    <Style x:Key="StatusPill" TargetType="Border">
        <Setter Property="StrokeThickness" Value="0" />
        <Setter Property="StrokeShape" Value="RoundRectangle 10" />
        <Setter Property="Padding" Value="10,4" />
        <Setter Property="HorizontalOptions" Value="End" />
        <Setter Property="VerticalOptions" Value="Center" />
    </Style>

    <Style x:Key="ChatBubble" TargetType="Border">
        <Setter Property="StrokeThickness" Value="0" />
        <Setter Property="StrokeShape" Value="RoundRectangle 16" />
        <Setter Property="Padding" Value="12" />
        <Setter Property="MaximumWidthRequest" Value="300" />
    </Style>

    <Style x:Key="Divider" TargetType="BoxView">
        <Setter Property="HeightRequest" Value="1" />
        <Setter Property="Color" Value="{StaticResource CardStroke}" />
    </Style>

    <!-- ===== Buttons ===== -->

    <Style x:Key="BaseButton" TargetType="Button">
        <Setter Property="FontFamily" Value="OpenSansSemibold" />
        <Setter Property="FontSize" Value="15" />
        <Setter Property="CornerRadius" Value="12" />
        <Setter Property="HeightRequest" Value="48" />
        <Setter Property="Padding" Value="16,0" />
        <Setter Property="BackgroundColor" Value="{StaticResource Navy}" />
        <Setter Property="TextColor" Value="White" />
        <Setter Property="VisualStateManager.VisualStateGroups">
            <VisualStateGroupList>
                <VisualStateGroup x:Name="CommonStates">
                    <VisualState x:Name="Normal" />
                    <VisualState x:Name="Disabled">
                        <VisualState.Setters>
                            <Setter Property="Opacity" Value="0.5" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>
    </Style>

    <!-- A Button without a Style looks like BaseButton -->
    <Style TargetType="Button" BasedOn="{StaticResource BaseButton}" />

    <Style x:Key="PrimaryButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="BackgroundColor" Value="{StaticResource UnitenRed}" />
    </Style>

    <Style x:Key="SecondaryButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="BackgroundColor" Value="{StaticResource CardBg}" />
        <Setter Property="TextColor" Value="{StaticResource UnitenBlue}" />
        <Setter Property="BorderColor" Value="{StaticResource UnitenBlue}" />
        <Setter Property="BorderWidth" Value="1" />
    </Style>

    <Style x:Key="LinkButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource UnitenBlue}" />
        <Setter Property="FontSize" Value="13" />
        <Setter Property="HeightRequest" Value="36" />
        <Setter Property="Padding" Value="8,0" />
    </Style>

    <Style x:Key="SegmentButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="HeightRequest" Value="40" />
        <Setter Property="CornerRadius" Value="10" />
        <Setter Property="FontSize" Value="13" />
        <Setter Property="Padding" Value="4,0" />
        <Setter Property="BorderColor" Value="{StaticResource Navy}" />
        <Setter Property="BorderWidth" Value="1" />
        <Setter Property="BackgroundColor" Value="{StaticResource CardBg}" />
        <Setter Property="TextColor" Value="{StaticResource Navy}" />
    </Style>

    <Style x:Key="ChipButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="FontFamily" Value="OpenSansRegular" />
        <Setter Property="FontSize" Value="13" />
        <Setter Property="HeightRequest" Value="36" />
        <Setter Property="CornerRadius" Value="18" />
        <Setter Property="Padding" Value="12,0" />
        <Setter Property="BackgroundColor" Value="{StaticResource CardBg}" />
        <Setter Property="TextColor" Value="{StaticResource UnitenBlue}" />
        <Setter Property="BorderColor" Value="{StaticResource UnitenBlue}" />
        <Setter Property="BorderWidth" Value="1" />
    </Style>

    <Style x:Key="CalendarNavButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="WidthRequest" Value="40" />
        <Setter Property="HeightRequest" Value="40" />
        <Setter Property="Padding" Value="0" />
        <Setter Property="FontSize" Value="22" />
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource Navy}" />
    </Style>

</ResourceDictionary>
```

- [ ] **Step 6: App icon and splash SVGs**

`Resources/AppIcon/appicon.svg` (navy background):

```xml
<svg xmlns="http://www.w3.org/2000/svg" width="456" height="456" viewBox="0 0 456 456">
  <rect width="456" height="456" fill="#1B2A4A" />
</svg>
```

`Resources/AppIcon/appiconfg.svg` and `Resources/Splash/splash.svg` (the same white "U", centred with plenty of padding):

```xml
<svg xmlns="http://www.w3.org/2000/svg" width="456" height="456" viewBox="0 0 456 456">
  <path d="M148 120 V260 a80 80 0 0 0 160 0 V120" fill="none" stroke="#FFFFFF"
        stroke-width="44" stroke-linecap="round" />
</svg>
```

- [ ] **Step 7: Image SVGs**

Create these files in `Resources/Images/`. Names are lowercase with underscores only (a MAUI rule); XAML references them as `.png`. Shell tints the tab icons itself, so their stroke colour does not matter there.

Tab icons use this wrapper (24×24). Put the shapes from the table inside it:

```xml
<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24"
     fill="none" stroke="#1B2A4A" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
  <!-- shapes here -->
</svg>
```

Tile icons (`ic_*.svg`) use the same wrapper but with `width="32" height="32"` (keep `viewBox="0 0 24 24"`), so they render at 32 px.

| File | Shapes inside the wrapper |
|---|---|
| `tab_home.svg` | `<path d="M3 11 L12 4 L21 11" />` `<path d="M5 10 V20 H19 V10" />` `<path d="M10 20 V14 H14 V20" />` |
| `tab_services.svg` | `<rect x="4" y="4" width="7" height="7" rx="1.5" />` `<rect x="13" y="4" width="7" height="7" rx="1.5" />` `<rect x="4" y="13" width="7" height="7" rx="1.5" />` `<rect x="13" y="13" width="7" height="7" rx="1.5" />` |
| `tab_ai.svg` | `<path d="M4 5 H20 V16 H10 L5 20 V16 H4 Z" />` `<circle cx="9" cy="10.5" r="0.8" fill="#1B2A4A" />` `<circle cx="12" cy="10.5" r="0.8" fill="#1B2A4A" />` `<circle cx="15" cy="10.5" r="0.8" fill="#1B2A4A" />` |
| `tab_requests.svg` | `<rect x="5" y="4" width="14" height="17" rx="2" />` `<path d="M9 3 H15 V6 H9 Z" />` `<path d="M8 11 H16 M8 15 H13" />` |
| `tab_profile.svg` | `<circle cx="12" cy="8" r="4" />` `<path d="M4 21 C4 16 8 14 12 14 C16 14 20 16 20 21" />` |
| `ic_programmes.svg` | `<path d="M3 5 C6 4 9 4 12 6 C15 4 18 4 21 5 V19 C18 18 15 18 12 20 C9 18 6 18 3 19 Z" />` `<path d="M12 6 V20" />` |
| `ic_calendar.svg` | `<rect x="3" y="5" width="18" height="16" rx="2" />` `<path d="M3 10 H21 M8 3 V7 M16 3 V7" />` |
| `ic_services.svg` | the same four `<rect>`s as `tab_services.svg` |
| `ic_requests.svg` | the same shapes as `tab_requests.svg` |
| `ic_ai.svg` | the same shapes as `tab_ai.svg` |
| `ic_search.svg` | `<circle cx="11" cy="11" r="6" />` `<path d="M16 16 L21 21" />` |

`logo_u.svg` (white "U", used on navy backgrounds; 120 px base size):

```xml
<svg xmlns="http://www.w3.org/2000/svg" width="120" height="120" viewBox="0 0 456 456">
  <path d="M148 120 V260 a80 80 0 0 0 160 0 V120" fill="none" stroke="#FFFFFF"
        stroke-width="44" stroke-linecap="round" />
</svg>
```

- [ ] **Step 8: App**

`App.xaml` (keep the template's merged dictionaries; it should look like this):

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.App">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="Resources/Styles/Colors.xaml" />
                <ResourceDictionary Source="Resources/Styles/Styles.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>
```

`App.xaml.cs`:

```csharp
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
```

- [ ] **Step 9: AppShell**

`AppShell.xaml`:

```xml
<?xml version="1.0" encoding="UTF-8" ?>
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:UnitenStudentAssistant.Views"
       x:Class="UnitenStudentAssistant.AppShell"
       Shell.FlyoutBehavior="Disabled">

    <TabBar>
        <ShellContent Title="Home" Route="home" Icon="tab_home.png"
                      ContentTemplate="{DataTemplate views:HomePage}" />
        <ShellContent Title="Services" Route="services" Icon="tab_services.png"
                      ContentTemplate="{DataTemplate views:ServicesPage}" />
        <ShellContent Title="AI" Route="ai" Icon="tab_ai.png"
                      ContentTemplate="{DataTemplate views:AiAssistantPage}" />
        <ShellContent Title="Requests" Route="requests" Icon="tab_requests.png"
                      ContentTemplate="{DataTemplate views:ServiceRequestsPage}" />
        <ShellContent Title="Profile" Route="profile" Icon="tab_profile.png"
                      ContentTemplate="{DataTemplate views:ProfilePage}" />
    </TabBar>

</Shell>
```

`AppShell.xaml.cs`:

```csharp
using UnitenStudentAssistant.Views;

namespace UnitenStudentAssistant;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Pages that are pushed on top of a tab (see docs/NAVIGATION.md §2)
        Routing.RegisterRoute("programmes", typeof(ProgrammesPage));
        Routing.RegisterRoute("programmedetail", typeof(ProgrammeDetailPage));
        Routing.RegisterRoute("calendar", typeof(CalendarPage));
        Routing.RegisterRoute("search", typeof(SearchPage));
        Routing.RegisterRoute("servicedetail", typeof(ServiceDetailPage));
        Routing.RegisterRoute("newrequest", typeof(NewRequestPage));
        Routing.RegisterRoute("tracking", typeof(RequestTrackingPage));
        Routing.RegisterRoute("requestdetail", typeof(RequestDetailPage));
        Routing.RegisterRoute("editprofile", typeof(EditProfilePage));
        Routing.RegisterRoute("about", typeof(AboutPage));
    }
}
```

- [ ] **Step 10: SplashPage (first version)**

`Views/SplashPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.SplashPage"
             BackgroundColor="{StaticResource Navy}"
             Shell.NavBarIsVisible="False">
    <VerticalStackLayout VerticalOptions="Center" HorizontalOptions="Center" Spacing="16">
        <Image Source="logo_u.png" WidthRequest="120" HeightRequest="120" HorizontalOptions="Center" />
        <Label Text="UNITEN Student Assistant" Style="{StaticResource TitleLabel}"
               TextColor="White" HorizontalOptions="Center" />
        <Label Text="Your campus, in your pocket" Style="{StaticResource CaptionLabel}"
               TextColor="White" Opacity="0.8" HorizontalOptions="Center" />
        <ActivityIndicator IsRunning="True" Color="White" HorizontalOptions="Center" Margin="0,8,0,0" />
    </VerticalStackLayout>
</ContentPage>
```

`Views/SplashPage.xaml.cs` (Task 2 adds seeding):

```csharp
namespace UnitenStudentAssistant.Views;

public partial class SplashPage : ContentPage
{
    private bool started;   // OnAppearing can run more than once; only start once

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (started) return;
        started = true;

        await Task.Delay(2000);
        Application.Current!.Windows[0].Page = new AppShell();
    }
}
```

- [ ] **Step 11: 15 placeholder pages**

Create each page below in `Views/` (in Visual Studio: right-click `Views` → Add → New Item → **.NET MAUI ContentPage (XAML)**), then replace both files with this template, substituting `{Class}` and `{Title}` from the table.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.{Class}"
             Title="{Title}">
    <VerticalStackLayout Padding="16" Spacing="12">
        <Label Text="{Title}: coming soon" Style="{StaticResource CaptionLabel}" />
    </VerticalStackLayout>
</ContentPage>
```

```csharp
namespace UnitenStudentAssistant.Views;

public partial class {Class} : ContentPage
{
    public {Class}()
    {
        InitializeComponent();
    }
}
```

| `{Class}` | `{Title}` | Built in |
|---|---|---|
| `HomePage` | Home | Task 6 (temporary version below) |
| `ProgrammesPage` | Programmes | Task 3 |
| `ProgrammeDetailPage` | Programme | Task 3 |
| `ServicesPage` | Services | Task 4 |
| `ServiceDetailPage` | Service | Task 4 |
| `CalendarPage` | Academic calendar | Task 5 |
| `ServiceRequestsPage` | Requests | Task 7 |
| `NewRequestPage` | New request | Task 7 |
| `RequestTrackingPage` | Track requests | Task 8 |
| `RequestDetailPage` | Request | Task 8 |
| `ProfilePage` | Profile | Task 9 |
| `EditProfilePage` | Edit profile | Task 9 |
| `AboutPage` | About | Task 9 |
| `SearchPage` | Search | Task 10 |
| `AiAssistantPage` | AI assistant | Task 11 |

`HomePage` gets two **temporary** buttons so Programmes and Calendar can be tested before Task 6. Use this instead of the template for `Views/HomePage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.HomePage"
             Title="Home">
    <VerticalStackLayout Padding="16" Spacing="12">
        <Label Text="Home: coming soon" Style="{StaticResource CaptionLabel}" />
        <!-- TEMPORARY until Task 6 -->
        <Button Text="Programmes (temporary)" Clicked="OnProgrammesClicked" />
        <Button Text="Calendar (temporary)" Clicked="OnCalendarClicked" />
    </VerticalStackLayout>
</ContentPage>
```

and `Views/HomePage.xaml.cs`:

```csharp
namespace UnitenStudentAssistant.Views;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();
    }

    // TEMPORARY until Task 6
    private async void OnProgrammesClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("programmes");

    private async void OnCalendarClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("calendar");
}
```

- [ ] **Step 12: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `Build succeeded` with `0 Error(s)`. If an error says a `StaticResource` was not found, the key is misspelled or declared after its first use in `Styles.xaml`.

- [ ] **Step 13: Run checklist S1 on the emulator**

Tick every row of `docs/TEST-CHECKLIST.md` S1.

- [ ] **Step 14: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: project skeleton with theme, icons, splash and shell tabs"
```

---

### Task 2: Firebase connection, models, helpers and seed data

**Files:**
- Modify: `UnitenStudentAssistant/UnitenStudentAssistant.csproj` (package + `GoogleServicesJson` item)
- Add (local only, git-ignored): `UnitenStudentAssistant/google-services.json`
- Modify: `UnitenStudentAssistant/MauiProgram.cs`
- Create: `UnitenStudentAssistant/Models/RequestStatus.cs`, `Programme.cs`, `CalendarEvent.cs`, `UniService.cs`, `ServiceRequest.cs`, `StudentProfile.cs`, `ChatMessage.cs`
- Create: `UnitenStudentAssistant/Helpers/AppColors.cs`, `StatusColors.cs`
- Create: `UnitenStudentAssistant/Services/FirestoreService.cs`, `SeedDataService.cs`
- Modify: `UnitenStudentAssistant/Views/SplashPage.xaml.cs`

**Interfaces:**
- Consumes: colour keys from Task 1 (`PendingText`, `PendingBg`, `ProgressText`, `ProgressBg`, `DoneText`, `DoneBg`, `TextMuted`, `PageBg`, `Navy`, `AiBubble`, `UnitenRed`, `TextMain`).
- Produces (later tasks call exactly these):
  - Models in `UnitenStudentAssistant.Models`: `RequestStatus.Pending/InProgress/Completed`; `Programme`, `CalendarEvent` (`DateValue`, `DayText`, `MonthText`, `DateText`, `TypeAndDescription`), `UniService`, `ServiceRequest` (`CreatedText`, `UpdatedText`, `StatusTextColor`, `StatusBgColor`), `StudentProfile` (`Initials`), `ChatMessage` (`Text`, `IsUser`, `IsError`, `Side`, `BubbleColor`, `TextColor`).
  - `Helpers.AppColors.Get(string key) : Color`; `Helpers.StatusColors.GetText(string) : Color`, `GetBackground(string) : Color`.
  - `Services.FirestoreService`: `GetProgrammesAsync() : Task<List<Programme>>`, `GetProgrammeAsync(string id) : Task<Programme?>`, `GetEventsAsync() : Task<List<CalendarEvent>>`, `GetServicesAsync() : Task<List<UniService>>`, `GetServiceAsync(string id) : Task<UniService?>`, `GetRecentRequestsAsync(int count) : Task<List<ServiceRequest>>`, `AddRequestAsync(string service, string subject, string details) : Task<string>` (returns the `requestNo`), `ListenToRequests(Action<List<ServiceRequest>> onChanged, Action<Exception> onError) : IDisposable`, `ListenToRequest(string id, Action<ServiceRequest?> onChanged, Action<Exception> onError) : IDisposable`, `GetProfileAsync() : Task<StudentProfile?>`, `UpdateProfileAsync(StudentProfile p) : Task`, `IsEmptyAsync<T>(string collection) : Task<bool>`, `SetDocumentAsync(string collection, string id, object data) : Task`.
  - `Services.SeedDataService.SeedIfEmptyAsync() : Task` (never throws).

- [ ] **Step 1: Firebase project, `google-services.json` and the package**

Follow `docs/SETUP-FIREBASE-GEMINI.md` Part 2, sections 2.1–2.3: create the Firebase project and Firestore in test mode, register the Android app with package name `com.abdullah.unitenstudentassistant`, put `google-services.json` next to the `.csproj`, add the `<GoogleServicesJson Include="google-services.json" />` item group, and install the package. From the repo root the CLI equivalent is:

```bash
dotnet add UnitenStudentAssistant/UnitenStudentAssistant.csproj package Plugin.Firebase.Firestore --version 4.0.0
```

Check that git ignores the file: `git check-ignore UnitenStudentAssistant/google-services.json` must print the path.

- [ ] **Step 2: Initialise Firebase in `MauiProgram.cs`**

Replace the whole file:

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
#if ANDROID
using Plugin.Firebase.Core.Platforms.Android;
#endif

namespace UnitenStudentAssistant;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                // Firebase must start when the Android activity is created, before any page uses Firestore
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                    CrossFirebase.Initialize(activity, () => Platform.CurrentActivity!)));
#endif
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

There is no `builder.Services.Add...` line on purpose: the app uses no dependency injection (AGENTS.md §2).

- [ ] **Step 3: Models**

Create one file per class in `Models/`. Fields map to Firestore with `[FirestoreProperty]`; properties without an attribute are display helpers and are not stored (DATA-MODEL §3).

`Models/RequestStatus.cs`:

```csharp
namespace UnitenStudentAssistant.Models;

// The only three status values. They must match the Firestore Console spelling exactly.
public static class RequestStatus
{
    public const string Pending = "Pending";
    public const string InProgress = "In Progress";
    public const string Completed = "Completed";
}
```

`Models/Programme.cs`:

```csharp
using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

public class Programme : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("code")] public string Code { get; set; } = "";
    [FirestoreProperty("faculty")] public string Faculty { get; set; } = "";
    [FirestoreProperty("duration")] public string Duration { get; set; } = "";
    [FirestoreProperty("description")] public string Description { get; set; } = "";
    [FirestoreProperty("majors")] public IList<string> Majors { get; set; } = new List<string>();
}
```

`Models/CalendarEvent.cs`:

```csharp
using System.Globalization;
using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

public class CalendarEvent : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("title")] public string Title { get; set; } = "";
    [FirestoreProperty("date")] public string Date { get; set; } = "";        // "yyyy-MM-dd"
    [FirestoreProperty("type")] public string Type { get; set; } = "";        // Academic, Registration, Exam, Break, Holiday
    [FirestoreProperty("description")] public string Description { get; set; } = "";

    // display helpers (not stored)
    public DateTime DateValue => DateTime.ParseExact(Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
    public string DayText => DateValue.Day.ToString();
    public string MonthText => DateValue.ToString("MMM");
    public string DateText => DateValue.ToString("dd MMM yyyy");
    public string TypeAndDescription => $"{Type} · {Description}";
}
```

`Models/UniService.cs`:

```csharp
using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

// Called UniService because "Service" clashes with other names
public class UniService : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("description")] public string Description { get; set; } = "";
    [FirestoreProperty("location")] public string Location { get; set; } = "";
    [FirestoreProperty("hours")] public string Hours { get; set; } = "";
    [FirestoreProperty("contact")] public string Contact { get; set; } = "";
}
```

`Models/ServiceRequest.cs`:

```csharp
using Plugin.Firebase.Firestore;
using UnitenStudentAssistant.Helpers;

namespace UnitenStudentAssistant.Models;

public class ServiceRequest : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("requestNo")] public string RequestNo { get; set; } = "";   // "REQ-1007"
    [FirestoreProperty("service")] public string Service { get; set; } = "";       // service name
    [FirestoreProperty("subject")] public string Subject { get; set; } = "";
    [FirestoreProperty("details")] public string Details { get; set; } = "";
    [FirestoreProperty("status")] public string Status { get; set; } = RequestStatus.Pending;
    [FirestoreProperty("studentId")] public string StudentId { get; set; } = "";
    [FirestoreProperty("createdAt")] public DateTimeOffset CreatedAt { get; set; }
    [FirestoreProperty("updatedAt")] public DateTimeOffset UpdatedAt { get; set; }

    // display helpers (not stored)
    public string CreatedText => CreatedAt.ToLocalTime().ToString("dd MMM yyyy, h:mm tt");
    public string UpdatedText => UpdatedAt.ToLocalTime().ToString("dd MMM yyyy, h:mm tt");
    public Color StatusTextColor => StatusColors.GetText(Status);
    public Color StatusBgColor => StatusColors.GetBackground(Status);
}
```

`Models/StudentProfile.cs`:

```csharp
using Plugin.Firebase.Firestore;

namespace UnitenStudentAssistant.Models;

public class StudentProfile : IFirestoreObject
{
    [FirestoreDocumentId] public string Id { get; set; } = "";
    [FirestoreProperty("name")] public string Name { get; set; } = "";
    [FirestoreProperty("studentId")] public string StudentId { get; set; } = "";
    [FirestoreProperty("programme")] public string Programme { get; set; } = "";
    [FirestoreProperty("email")] public string Email { get; set; } = "";
    [FirestoreProperty("phone")] public string Phone { get; set; } = "";

    // display helper (not stored): "Abdullah" -> "AB", "Ali Bakar" -> "AB"
    public string Initials
    {
        get
        {
            var words = Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return "?";
            if (words.Length == 1) return words[0].Substring(0, Math.Min(2, words[0].Length)).ToUpper();
            return (words[0].Substring(0, 1) + words[1].Substring(0, 1)).ToUpper();
        }
    }
}
```

`Models/ChatMessage.cs`:

```csharp
using UnitenStudentAssistant.Helpers;

namespace UnitenStudentAssistant.Models;

// NOT stored in Firestore: it only lives in the AI page's list
public class ChatMessage
{
    public string Text { get; set; } = "";
    public bool IsUser { get; set; }
    public bool IsError { get; set; }       // the AI could not answer (no internet, no key, ...)

    // display helpers used by the chat bubble DataTemplate
    public LayoutOptions Side => IsUser ? LayoutOptions.End : LayoutOptions.Start;
    public Color BubbleColor => IsUser ? AppColors.Get("Navy") : AppColors.Get("AiBubble");
    public Color TextColor => IsUser ? Colors.White
                            : IsError ? AppColors.Get("UnitenRed")
                            : AppColors.Get("TextMain");
}
```

- [ ] **Step 4: Colour helpers**

`Helpers/AppColors.cs`:

```csharp
namespace UnitenStudentAssistant.Helpers;

// Reads a colour from Resources/Styles/Colors.xaml by its key,
// so C# code never has to type a hex value.
public static class AppColors
{
    public static Color Get(string key)
    {
        if (Application.Current != null
            && Application.Current.Resources.TryGetValue(key, out object value)
            && value is Color color)
        {
            return color;
        }

        return Colors.Gray;   // a misspelled key shows grey instead of crashing
    }
}
```

`Helpers/StatusColors.cs`:

```csharp
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
```

- [ ] **Step 5: `FirestoreService`**

`Services/FirestoreService.cs`. This is the only file that touches Firestore (DATA-MODEL §4):

```csharp
using Plugin.Firebase.Firestore;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Services;

// Every Firestore read and write in the app goes through this class.
public static class FirestoreService
{
    private static ICollectionReference Collection(string name) =>
        CrossFirebaseFirestore.Current.GetCollection(name);

    // ----- Programmes, events and services (read only) -----

    public static async Task<List<Programme>> GetProgrammesAsync()
    {
        var snapshot = await Collection("programmes").OrderBy("name").GetDocumentsAsync<Programme>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    public static async Task<Programme?> GetProgrammeAsync(string id)
    {
        var snapshot = await Collection("programmes").GetDocument(id).GetDocumentSnapshotAsync<Programme>();
        return snapshot.Data;   // null if the document does not exist
    }

    public static async Task<List<CalendarEvent>> GetEventsAsync()
    {
        var snapshot = await Collection("calendarEvents").OrderBy("date").GetDocumentsAsync<CalendarEvent>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    public static async Task<List<UniService>> GetServicesAsync()
    {
        var snapshot = await Collection("services").OrderBy("name").GetDocumentsAsync<UniService>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    public static async Task<UniService?> GetServiceAsync(string id)
    {
        var snapshot = await Collection("services").GetDocument(id).GetDocumentSnapshotAsync<UniService>();
        return snapshot.Data;
    }

    // ----- Requests -----

    public static async Task<List<ServiceRequest>> GetRecentRequestsAsync(int count)
    {
        var snapshot = await Collection("requests")
            .OrderBy("createdAt", true)
            .LimitedTo(count)
            .GetDocumentsAsync<ServiceRequest>();
        return snapshot.Documents.Select(d => d.Data).ToList();
    }

    // Creates a new Pending request and returns its number, e.g. "REQ-1007".
    // "count + 1" is fine for a one-student class project (see DATA-MODEL §4).
    public static async Task<string> AddRequestAsync(string service, string subject, string details)
    {
        var all = await Collection("requests").GetDocumentsAsync<ServiceRequest>();
        string requestNo = $"REQ-{1000 + all.Count + 1}";

        var profile = await GetProfileAsync();
        var now = DateTimeOffset.Now;

        var request = new ServiceRequest
        {
            RequestNo = requestNo,
            Service = service,
            Subject = subject,
            Details = details,
            Status = RequestStatus.Pending,
            StudentId = profile?.StudentId ?? "",
            CreatedAt = now,
            UpdatedAt = now
        };

        await Collection("requests").AddDocumentAsync(request);
        return requestNo;
    }

    // Live list of all requests, newest first. Call Dispose() on the result to stop listening.
    public static IDisposable ListenToRequests(Action<List<ServiceRequest>> onChanged, Action<Exception> onError)
    {
        return Collection("requests")
            .OrderBy("createdAt", true)
            .AddSnapshotListener<ServiceRequest>(
                snapshot => onChanged(snapshot.Documents.Select(d => d.Data).ToList()),
                onError);
    }

    // Live single request. onChanged gets null if the document was deleted.
    public static IDisposable ListenToRequest(string id, Action<ServiceRequest?> onChanged, Action<Exception> onError)
    {
        return Collection("requests")
            .GetDocument(id)
            .AddSnapshotListener<ServiceRequest>(snapshot => onChanged(snapshot.Data), onError);
    }

    // ----- Profile (one document: profile/student) -----

    public static async Task<StudentProfile?> GetProfileAsync()
    {
        var snapshot = await Collection("profile").GetDocument("student").GetDocumentSnapshotAsync<StudentProfile>();
        return snapshot.Data;
    }

    public static async Task UpdateProfileAsync(StudentProfile p)
    {
        // studentId is never changed, so it is not in this list
        await Collection("profile").GetDocument("student").UpdateDataAsync(
            ("name", p.Name),
            ("programme", p.Programme),
            ("email", p.Email),
            ("phone", p.Phone));
    }

    // ----- Used only by SeedDataService -----

    // Source.Server: when offline this throws instead of reading an empty cache
    // and seeding over real data.
    public static async Task<bool> IsEmptyAsync<T>(string collection)
    {
        var snapshot = await Collection(collection).LimitedTo(1).GetDocumentsAsync<T>(Source.Server);
        return snapshot.IsEmpty;
    }

    public static async Task SetDocumentAsync(string collection, string id, object data)
    {
        await Collection(collection).GetDocument(id).SetDataAsync(data);
    }
}
```

- [ ] **Step 6: `SeedDataService`**

`Services/SeedDataService.cs`. All values come from DATA-MODEL §5; if you change one here, change the doc too.

```csharp
using System.Diagnostics;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Services;

// Fills an empty Firestore with the demo data (docs/DATA-MODEL.md §5).
// A collection is only written when it has no documents, and every
// document has a fixed id, so running this again never makes duplicates.
public static class SeedDataService
{
    private const string Faculty = "College of Computing and Informatics";
    private const string StudentId = "is01084960";

    public static async Task SeedIfEmptyAsync()
    {
        try
        {
            await SeedAsync("profile", new Dictionary<string, StudentProfile>
            {
                ["student"] = new StudentProfile
                {
                    Name = "Abdullah",
                    StudentId = StudentId,
                    Programme = "Bachelor of Information Systems (Hons.)",
                    Email = "is01084960@student.uniten.edu.my",
                    Phone = "012-345 6789"
                }
            });

            await SeedAsync("programmes", new Dictionary<string, Programme>
            {
                ["prog-it"] = MakeProgramme("Bachelor of Information Technology (Hons.)", "BIT",
                    "Builds practical skills in software, networks and data to solve real business problems with technology.",
                    "Software Development", "Networking", "Data Analytics"),
                ["prog-cs"] = MakeProgramme("Bachelor of Computer Science (Hons.)", "BCS",
                    "Covers the theory and practice of computing, from algorithms and programming to intelligent systems.",
                    "Software Engineering", "Artificial Intelligence", "Graphics & Multimedia"),
                ["prog-bc"] = MakeProgramme("Bachelor of Business Computing (Hons.)", "BBC",
                    "Combines computing with business studies to prepare graduates for technology roles in organisations.",
                    "Business Analytics", "E-Business", "IT Management"),
                ["prog-cyber"] = MakeProgramme("Bachelor of Computer Science (Cyber Security) (Hons.)", "BCSS",
                    "Focuses on protecting systems, networks and data, including ethical hacking and incident response.",
                    "Network Security", "Digital Forensics", "Secure Software"),
                ["prog-is"] = MakeProgramme("Bachelor of Information Systems (Hons.)", "BIS",
                    "Teaches how to design and manage information systems that support people and organisations.",
                    "Enterprise Systems", "Information Management", "Systems Analysis")
            });

            await SeedAsync("services", new Dictionary<string, UniService>
            {
                ["svc-registrar"] = MakeService("Registrar",
                    "Course registration, transcripts, enrolment letters and academic records.",
                    "Admin Building, Level 1", "Mon–Fri, 8:30 am – 5:00 pm", "registrar.demo@uniten.edu.my · Ext. 2001"),
                ["svc-finance"] = MakeService("Finance",
                    "Tuition fees, payment plans, receipts and scholarship disbursement.",
                    "Admin Building, Level 2", "Mon–Fri, 8:30 am – 4:30 pm", "finance.demo@uniten.edu.my · Ext. 2002"),
                ["svc-library"] = MakeService("Library",
                    "Borrowing, room booking, online databases and library fines.",
                    "Library Building", "Mon–Sat, 8:00 am – 10:00 pm", "library.demo@uniten.edu.my · Ext. 2003"),
                ["svc-affairs"] = MakeService("Student Affairs",
                    "Clubs, accommodation, counselling and student welfare.",
                    "Student Centre", "Mon–Fri, 9:00 am – 5:00 pm", "affairs.demo@uniten.edu.my · Ext. 2004"),
                ["svc-ict"] = MakeService("ICT Support",
                    "Student email, Wi-Fi, portal passwords and lab computers.",
                    "ICT Centre, Block B", "Mon–Fri, 8:00 am – 6:00 pm", "ict.demo@uniten.edu.my · Ext. 2005")
            });

            await SeedAsync("calendarEvents", new Dictionary<string, CalendarEvent>
            {
                ["evt-01"] = MakeEvent("2026-09-14", "Registration opens", "Registration", "Online course registration opens on the student portal."),
                ["evt-02"] = MakeEvent("2026-09-28", "Semester starts", "Academic", "First day of lectures for Semester 1 2026/2027."),
                ["evt-03"] = MakeEvent("2026-10-09", "Registration deadline", "Registration", "Last day to add or drop courses without penalty."),
                ["evt-04"] = MakeEvent("2026-10-20", "Deepavali holiday", "Holiday", "Public holiday. No classes."),
                ["evt-05"] = MakeEvent("2026-11-09", "Mid-semester break starts", "Break", "One-week mid-semester break begins."),
                ["evt-06"] = MakeEvent("2026-11-16", "Classes resume", "Academic", "Lectures continue after the mid-semester break."),
                ["evt-07"] = MakeEvent("2026-11-27", "Project proposal due", "Academic", "Final-year and course project proposals due."),
                ["evt-08"] = MakeEvent("2026-12-11", "Fee payment deadline", "Registration", "Last day to pay Semester 1 fees."),
                ["evt-09"] = MakeEvent("2026-12-25", "Christmas holiday", "Holiday", "Public holiday. No classes."),
                ["evt-10"] = MakeEvent("2027-01-08", "Last day of lectures", "Academic", "Final week of teaching ends."),
                ["evt-11"] = MakeEvent("2027-01-18", "Final exams start", "Exam", "Final examination period begins."),
                ["evt-12"] = MakeEvent("2027-02-05", "Semester ends", "Academic", "End of Semester 1 2026/2027.")
            });

            await SeedAsync("requests", new Dictionary<string, ServiceRequest>
            {
                ["req-1001"] = MakeRequest(1001, "Registrar", "Request for enrolment letter",
                    "I need an enrolment letter for my scholarship application.",
                    RequestStatus.Completed, MalaysiaTime(10, 1, 10, 0)),
                ["req-1002"] = MakeRequest(1002, "Library", "Library fine appeal",
                    "I returned the book on time but was still charged a late fine.",
                    RequestStatus.Completed, MalaysiaTime(10, 5, 14, 30)),
                ["req-1003"] = MakeRequest(1003, "ICT Support", "Cannot log in to student portal",
                    "My student portal password stopped working after the reset.",
                    RequestStatus.Completed, MalaysiaTime(10, 12, 9, 15)),
                ["req-1004"] = MakeRequest(1004, "Finance", "Scholarship payment not received",
                    "My scholarship for this semester has not been credited to my account yet.",
                    RequestStatus.InProgress, MalaysiaTime(10, 19, 11, 45)),
                ["req-1005"] = MakeRequest(1005, "Student Affairs", "Hostel room change",
                    "I would like to move to a room closer to the library block.",
                    RequestStatus.Pending, MalaysiaTime(10, 26, 16, 20)),
                ["req-1006"] = MakeRequest(1006, "Registrar", "Transcript copy",
                    "I need a certified copy of my latest transcript.",
                    RequestStatus.Pending, MalaysiaTime(10, 28, 8, 50))
            });
        }
        catch (Exception ex)
        {
            // No internet or expired rules: the app still opens, and each page shows its own error
            Debug.WriteLine("Seeding failed: " + ex.Message);
        }
    }

    // Writes the documents only if the collection is empty
    private static async Task SeedAsync<T>(string collection, Dictionary<string, T> documents) where T : class
    {
        if (!await FirestoreService.IsEmptyAsync<T>(collection)) return;

        foreach (var document in documents)
        {
            await FirestoreService.SetDocumentAsync(collection, document.Key, document.Value);
        }
    }

    private static Programme MakeProgramme(string name, string code, string description, params string[] majors) =>
        new Programme
        {
            Name = name,
            Code = code,
            Faculty = Faculty,
            Duration = "4 years",
            Description = description,
            Majors = majors.ToList()
        };

    private static UniService MakeService(string name, string description, string location, string hours, string contact) =>
        new UniService
        {
            Name = name,
            Description = description,
            Location = location,
            Hours = hours,
            Contact = contact
        };

    private static CalendarEvent MakeEvent(string date, string title, string type, string description) =>
        new CalendarEvent
        {
            Date = date,
            Title = title,
            Type = type,
            Description = description
        };

    private static ServiceRequest MakeRequest(int number, string service, string subject, string details,
                                              string status, DateTimeOffset created) =>
        new ServiceRequest
        {
            RequestNo = $"REQ-{number}",
            Service = service,
            Subject = subject,
            Details = details,
            Status = status,
            StudentId = StudentId,
            CreatedAt = created,
            // a request that is no longer Pending was updated one day later
            UpdatedAt = status == RequestStatus.Pending ? created : created.AddDays(1)
        };

    // A moment in 2026, in Malaysia time (UTC+8)
    private static DateTimeOffset MalaysiaTime(int month, int day, int hour, int minute) =>
        new DateTimeOffset(2026, month, day, hour, minute, 0, TimeSpan.FromHours(8));
}
```

- [ ] **Step 7: SplashPage (final version)**

Replace `Views/SplashPage.xaml.cs` (the XAML from Task 1 stays):

```csharp
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class SplashPage : ContentPage
{
    private bool started;   // OnAppearing can run more than once; only start once

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (started) return;
        started = true;

        Task seeding = SeedDataService.SeedIfEmptyAsync();   // never throws: errors are caught inside
        Task minimumTime = Task.Delay(2000);                 // show the logo for at least 2 s
        Task maximumTime = Task.Delay(8000);                 // never wait longer than 8 s

        await Task.WhenAny(seeding, maximumTime);
        await minimumTime;

        // Swap the whole window to the Shell, so Back on Home closes the app
        Application.Current!.Windows[0].Page = new AppShell();
    }
}
```

- [ ] **Step 8: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`. If the build fails with "path too long", see AGENTS.md §9 (enable Windows long paths or move the repo to `C:\dev\`).

- [ ] **Step 9: Run checklist S2 on the emulator**

Start the app once with internet on, then open the Firestore Console and tick every row of `docs/TEST-CHECKLIST.md` S2 (5 collections, counts, `req-1004` fields are timestamps, no duplicates on a second run, airplane-mode start still reaches Home).

- [ ] **Step 10: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: firestore connection, models and first-run seed data"
```

---

### Task 3: Programmes and Programme Detail (Flow A)

**Files:**
- Replace: `UnitenStudentAssistant/Views/ProgrammesPage.xaml`, `ProgrammesPage.xaml.cs`
- Replace: `UnitenStudentAssistant/Views/ProgrammeDetailPage.xaml`, `ProgrammeDetailPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.GetProgrammesAsync()`, `FirestoreService.GetProgrammeAsync(string id)`, model `Programme`; styles `CardBorder`, `TitleLabel`, `SubtitleLabel`, `BodyLabel`, `CaptionLabel`.
- Produces: route `programmedetail?id={id}` working (used again by Search in Task 10).

- [ ] **Step 1: ProgrammesPage (the canonical list page)**

`Views/ProgrammesPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             x:Class="UnitenStudentAssistant.Views.ProgrammesPage"
             Title="Programmes">
    <Grid RowDefinitions="Auto,*" Padding="16" RowSpacing="12">
        <SearchBar x:Name="ProgrammeSearch" Placeholder="Search programmes"
                   TextChanged="OnSearchTextChanged" />
        <ActivityIndicator x:Name="Loading" Grid.Row="1" IsRunning="False" IsVisible="False"
                           VerticalOptions="Start" />
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

`Views/ProgrammesPage.xaml.cs`:

```csharp
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
        if (allProgrammes.Count > 0) return;          // load once; Back from the detail keeps the search

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

- [ ] **Step 2: ProgrammeDetailPage**

`Views/ProgrammeDetailPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.ProgrammeDetailPage"
             Title="Programme">
    <Grid>
        <ScrollView>
            <VerticalStackLayout Padding="16" Spacing="12">
                <Label x:Name="NameLabel" Style="{StaticResource TitleLabel}" />
                <Label x:Name="FacultyLabel" Style="{StaticResource CaptionLabel}" />

                <Border Style="{StaticResource CardBorder}">
                    <Grid ColumnDefinitions="Auto,*" RowDefinitions="Auto,Auto"
                          ColumnSpacing="16" RowSpacing="8">
                        <Label Text="Duration" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="DurationLabel" Grid.Column="1" Style="{StaticResource BodyLabel}" />
                        <Label Text="Faculty" Grid.Row="1" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="FacultyValueLabel" Grid.Row="1" Grid.Column="1"
                               Style="{StaticResource BodyLabel}" />
                    </Grid>
                </Border>

                <Label Text="About this programme" Style="{StaticResource SubtitleLabel}" />
                <Label x:Name="DescriptionLabel" Style="{StaticResource BodyLabel}" />

                <Label Text="Majors / specialisations" Style="{StaticResource SubtitleLabel}" />
                <VerticalStackLayout x:Name="MajorsList" Spacing="4">
                    <BindableLayout.ItemTemplate>
                        <DataTemplate x:DataType="x:String">
                            <Label Text="{Binding StringFormat='• {0}'}" Style="{StaticResource BodyLabel}" />
                        </DataTemplate>
                    </BindableLayout.ItemTemplate>
                </VerticalStackLayout>
            </VerticalStackLayout>
        </ScrollView>

        <ActivityIndicator x:Name="Loading" IsRunning="False" IsVisible="False"
                           HorizontalOptions="Center" VerticalOptions="Center" />
    </Grid>
</ContentPage>
```

`Views/ProgrammeDetailPage.xaml.cs`:

```csharp
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(ProgrammeId), "id")]
public partial class ProgrammeDetailPage : ContentPage
{
    public string ProgrammeId { get; set; } = "";
    private bool loaded;

    public ProgrammeDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (loaded) return;

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var programme = await FirestoreService.GetProgrammeAsync(ProgrammeId);
            if (programme == null)
            {
                await DisplayAlertAsync("Not found", "This programme could not be found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            ShowProgramme(programme);
            loaded = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load the programme. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private void ShowProgramme(Programme programme)
    {
        NameLabel.Text = programme.Name;
        FacultyLabel.Text = programme.Faculty;
        DurationLabel.Text = programme.Duration;
        FacultyValueLabel.Text = programme.Faculty;
        DescriptionLabel.Text = programme.Description;
        BindableLayout.SetItemsSource(MajorsList, programme.Majors);
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`. A compiled-binding error such as "Binding: Property "Nmae" not found" means a typo in a `{Binding}`.

- [ ] **Step 4: Run checklist S3 on the emulator**

Open Programmes with the temporary button on Home and tick every row of `docs/TEST-CHECKLIST.md` S3.

- [ ] **Step 5: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: programmes list with search and programme detail"
```

---

### Task 4: Services and Service Detail

**Files:**
- Replace: `UnitenStudentAssistant/Views/ServicesPage.xaml`, `ServicesPage.xaml.cs`
- Replace: `UnitenStudentAssistant/Views/ServiceDetailPage.xaml`, `ServiceDetailPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.GetServicesAsync()`, `FirestoreService.GetServiceAsync(string id)`, model `UniService`; image `ic_services.png`; styles `CardBorder`, `PrimaryButton`.
- Produces: route `servicedetail?id={id}` working (Search uses it in Task 10); a call to `newrequest?service={escaped name}` that Task 7 reads.

- [ ] **Step 1: ServicesPage**

`Views/ServicesPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             x:Class="UnitenStudentAssistant.Views.ServicesPage"
             Title="Services">
    <Grid Padding="16">
        <ActivityIndicator x:Name="Loading" IsRunning="False" IsVisible="False" VerticalOptions="Start" />
        <CollectionView x:Name="ServiceList">
            <CollectionView.ItemsLayout>
                <LinearItemsLayout Orientation="Vertical" ItemSpacing="12" />
            </CollectionView.ItemsLayout>
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:UniService">
                    <Border Style="{StaticResource CardBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnServiceTapped" />
                        </Border.GestureRecognizers>
                        <Grid ColumnDefinitions="Auto,*,Auto" ColumnSpacing="12">
                            <Image Source="ic_services.png" WidthRequest="32" HeightRequest="32"
                                   VerticalOptions="Center" />
                            <VerticalStackLayout Grid.Column="1" Spacing="2" VerticalOptions="Center">
                                <Label Text="{Binding Name}" Style="{StaticResource SubtitleLabel}" />
                                <Label Text="{Binding Description}" Style="{StaticResource CaptionLabel}"
                                       MaxLines="1" LineBreakMode="TailTruncation" />
                            </VerticalStackLayout>
                            <Label Grid.Column="2" Text="›" Style="{StaticResource SubtitleLabel}"
                                   TextColor="{StaticResource TextMuted}" VerticalOptions="Center" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

`Views/ServicesPage.xaml.cs`:

```csharp
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class ServicesPage : ContentPage
{
    private List<UniService> services = new();

    public ServicesPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (services.Count > 0) return;     // load once

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            services = await FirestoreService.GetServicesAsync();
            ServiceList.ItemsSource = services;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load services. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private async void OnServiceTapped(object sender, TappedEventArgs e)
    {
        var service = (UniService)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"servicedetail?id={service.Id}");
    }
}
```

- [ ] **Step 2: ServiceDetailPage**

`Views/ServiceDetailPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.ServiceDetailPage"
             Title="Service">
    <Grid>
        <ScrollView>
            <VerticalStackLayout Padding="16" Spacing="12">
                <Label x:Name="NameLabel" Style="{StaticResource TitleLabel}" />
                <Label x:Name="DescriptionLabel" Style="{StaticResource BodyLabel}" />

                <Border Style="{StaticResource CardBorder}">
                    <VerticalStackLayout Spacing="12">
                        <VerticalStackLayout Spacing="4">
                            <Label Text="Location" Style="{StaticResource CaptionLabel}" />
                            <Label x:Name="LocationLabel" Style="{StaticResource BodyLabel}" />
                        </VerticalStackLayout>
                        <VerticalStackLayout Spacing="4">
                            <Label Text="Hours" Style="{StaticResource CaptionLabel}" />
                            <Label x:Name="HoursLabel" Style="{StaticResource BodyLabel}" />
                        </VerticalStackLayout>
                        <VerticalStackLayout Spacing="4">
                            <Label Text="Contact" Style="{StaticResource CaptionLabel}" />
                            <Label x:Name="ContactLabel" Style="{StaticResource BodyLabel}" />
                        </VerticalStackLayout>
                    </VerticalStackLayout>
                </Border>

                <Button x:Name="MakeRequestButton" Text="Make a request" Style="{StaticResource PrimaryButton}"
                        IsEnabled="False" Clicked="OnMakeRequestClicked" Margin="0,12,0,0" />
            </VerticalStackLayout>
        </ScrollView>

        <ActivityIndicator x:Name="Loading" IsRunning="False" IsVisible="False"
                           HorizontalOptions="Center" VerticalOptions="Center" />
    </Grid>
</ContentPage>
```

`Views/ServiceDetailPage.xaml.cs`:

```csharp
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(ServiceId), "id")]
public partial class ServiceDetailPage : ContentPage
{
    public string ServiceId { get; set; } = "";
    private UniService? service;

    public ServiceDetailPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (service != null) return;     // already loaded (coming back from New Request)

        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            service = await FirestoreService.GetServiceAsync(ServiceId);
            if (service == null)
            {
                await DisplayAlertAsync("Not found", "This service could not be found.", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            NameLabel.Text = service.Name;
            DescriptionLabel.Text = service.Description;
            LocationLabel.Text = service.Location;
            HoursLabel.Text = service.Hours;
            ContactLabel.Text = service.Contact;
            MakeRequestButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load the service. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private async void OnMakeRequestClicked(object sender, EventArgs e)
    {
        if (service == null) return;
        // The name has spaces ("Student Affairs"), so escape it
        await Shell.Current.GoToAsync($"newrequest?service={Uri.EscapeDataString(service.Name)}");
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`.

- [ ] **Step 4: Run checklist S4 on the emulator**

Tick every row of `docs/TEST-CHECKLIST.md` S4. "Make a request" opens the New Request placeholder until Task 7.

- [ ] **Step 5: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: services list and service detail"
```

---

### Task 5: CalendarMonthView and Calendar page (Flow C)

**Files:**
- Create: `UnitenStudentAssistant/Controls/CalendarMonthView.xaml`, `CalendarMonthView.xaml.cs` (in Visual Studio: Add → New Item → **.NET MAUI ContentView (XAML)**)
- Replace: `UnitenStudentAssistant/Views/CalendarPage.xaml`, `CalendarPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.GetEventsAsync()`, model `CalendarEvent` (`Date`, `DayText`, `MonthText`, `Title`, `TypeAndDescription`), `AppColors.Get`; styles `CalendarNavButton`, `SubtitleLabel`, `SmallLabel`, `DateDayLabel`, `StrongLabel`, `LinkButton`.
- Produces: the control API from DESIGN.md §10 exactly: `DisplayMonth`, `SetEvents(List<CalendarEvent>)`, `GoToMonth(DateTime)`, `SelectDate(DateTime)`, `ClearSelection()`, events `DateSelected` and `MonthChanged` (both `EventHandler<DateTime>`). The route `calendar?date=yyyy-MM-dd` working (used by Home in Task 6 and Search in Task 10).

- [ ] **Step 1: CalendarMonthView layout**

`Controls/CalendarMonthView.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Controls.CalendarMonthView">
    <VerticalStackLayout Spacing="8">
        <!-- Row 0: ‹  October 2026  › -->
        <Grid ColumnDefinitions="40,*,40">
            <Button Text="‹" Style="{StaticResource CalendarNavButton}" Clicked="OnPreviousClicked" />
            <Label x:Name="MonthLabel" Grid.Column="1" Style="{StaticResource SubtitleLabel}"
                   HorizontalOptions="Center" VerticalOptions="Center" />
            <Button Text="›" Grid.Column="2" Style="{StaticResource CalendarNavButton}" Clicked="OnNextClicked" />
        </Grid>

        <!-- Row 1: weekday names (the week starts on Sunday) -->
        <Grid ColumnDefinitions="*,*,*,*,*,*,*">
            <Label Grid.Column="0" Text="Sun" Style="{StaticResource SmallLabel}" TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
            <Label Grid.Column="1" Text="Mon" Style="{StaticResource SmallLabel}" TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
            <Label Grid.Column="2" Text="Tue" Style="{StaticResource SmallLabel}" TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
            <Label Grid.Column="3" Text="Wed" Style="{StaticResource SmallLabel}" TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
            <Label Grid.Column="4" Text="Thu" Style="{StaticResource SmallLabel}" TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
            <Label Grid.Column="5" Text="Fri" Style="{StaticResource SmallLabel}" TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
            <Label Grid.Column="6" Text="Sat" Style="{StaticResource SmallLabel}" TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
        </Grid>

        <!-- Row 2: up to 6 weeks of day cells, filled in by BuildDays() -->
        <Grid x:Name="DaysGrid"
              ColumnDefinitions="*,*,*,*,*,*,*"
              RowDefinitions="44,44,44,44,44,44"
              ColumnSpacing="2" RowSpacing="2" />
    </VerticalStackLayout>
</ContentView>
```

- [ ] **Step 2: CalendarMonthView logic**

`Controls/CalendarMonthView.xaml.cs`:

```csharp
using Microsoft.Maui.Controls.Shapes;
using UnitenStudentAssistant.Helpers;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Controls;

// A simple month calendar: 42 possible cells, rebuilt whenever something changes.
public partial class CalendarMonthView : ContentView
{
    private List<CalendarEvent> events = new();
    private DateTime? selectedDate;

    public DateTime DisplayMonth { get; private set; }      // always the 1st of the shown month

    public event EventHandler<DateTime>? DateSelected;       // a day was tapped
    public event EventHandler<DateTime>? MonthChanged;       // ‹ or › was tapped

    public CalendarMonthView()
    {
        InitializeComponent();
        DisplayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        BuildDays();
    }

    public void SetEvents(List<CalendarEvent> newEvents)
    {
        events = newEvents;
        BuildDays();
    }

    // Used when the page is opened with ?date=. Does not raise MonthChanged.
    public void GoToMonth(DateTime month)
    {
        DisplayMonth = new DateTime(month.Year, month.Month, 1);
        selectedDate = null;
        BuildDays();
    }

    public void SelectDate(DateTime date)
    {
        selectedDate = date.Date;
        BuildDays();
    }

    public void ClearSelection()
    {
        selectedDate = null;
        BuildDays();
    }

    private void OnPreviousClicked(object sender, EventArgs e) => ChangeMonth(-1);

    private void OnNextClicked(object sender, EventArgs e) => ChangeMonth(1);

    private void ChangeMonth(int step)
    {
        DisplayMonth = DisplayMonth.AddMonths(step);
        selectedDate = null;
        BuildDays();
        MonthChanged?.Invoke(this, DisplayMonth);
    }

    private void BuildDays()
    {
        MonthLabel.Text = DisplayMonth.ToString("MMMM yyyy");
        DaysGrid.Children.Clear();

        int firstColumn = (int)DisplayMonth.DayOfWeek;     // 0 = Sunday
        int daysInMonth = DateTime.DaysInMonth(DisplayMonth.Year, DisplayMonth.Month);

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(DisplayMonth.Year, DisplayMonth.Month, day);
            int index = firstColumn + day - 1;              // position in the 7 x 6 grid
            DaysGrid.Add(CreateDayCell(date), index % 7, index / 7);
        }
    }

    private Border CreateDayCell(DateTime date)
    {
        bool isToday = date == DateTime.Today;
        bool isSelected = date == selectedDate;
        string key = date.ToString("yyyy-MM-dd");
        bool hasEvent = events.Any(ev => ev.Date == key);

        var dayLabel = new Label
        {
            Text = date.Day.ToString(),
            FontSize = 15,
            FontFamily = isToday || isSelected ? "OpenSansSemibold" : "OpenSansRegular",
            TextColor = isSelected ? Colors.White
                      : isToday ? AppColors.Get("UnitenBlue")
                      : AppColors.Get("TextMain"),
            HorizontalOptions = LayoutOptions.Center
        };

        // The dot is always there (transparent when there is no event),
        // so every day number sits at the same height.
        var dot = new BoxView
        {
            WidthRequest = 6,
            HeightRequest = 6,
            CornerRadius = 3,
            HorizontalOptions = LayoutOptions.Center,
            Color = !hasEvent ? Colors.Transparent
                  : isSelected ? Colors.White
                  : AppColors.Get("UnitenRed")
        };

        var cell = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 10 },
            BackgroundColor = isSelected ? AppColors.Get("Navy") : Colors.Transparent,
            Content = new VerticalStackLayout
            {
                Spacing = 2,
                VerticalOptions = LayoutOptions.Center,
                Children = { dayLabel, dot }
            }
        };

        // Cells are made in code, so the tap handler is a short lambda here
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => DateSelected?.Invoke(this, date);
        cell.GestureRecognizers.Add(tap);

        return cell;
    }
}
```

- [ ] **Step 3: CalendarPage layout**

`Views/CalendarPage.xaml`. The control is named `MonthView` (not `Calendar`, which would clash with `System.Globalization.Calendar`):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             xmlns:controls="clr-namespace:UnitenStudentAssistant.Controls"
             x:Class="UnitenStudentAssistant.Views.CalendarPage"
             Title="Academic calendar">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="12">
            <Label Text="Semester 1 2026/2027" Style="{StaticResource CaptionLabel}" />

            <Border Style="{StaticResource CardBorder}">
                <controls:CalendarMonthView x:Name="MonthView"
                                            DateSelected="OnDateSelected"
                                            MonthChanged="OnMonthChanged" />
            </Border>

            <Grid ColumnDefinitions="*,Auto">
                <Label x:Name="ListTitleLabel" Text="Important dates" Style="{StaticResource SubtitleLabel}"
                       VerticalOptions="Center" />
                <Button x:Name="ShowAllButton" Grid.Column="1" Text="Show all" Style="{StaticResource LinkButton}"
                        IsVisible="False" Clicked="OnShowAllClicked" />
            </Grid>

            <ActivityIndicator x:Name="Loading" IsRunning="False" IsVisible="False" />

            <!-- BindableLayout, not CollectionView, because we are inside a ScrollView -->
            <VerticalStackLayout x:Name="EventList" Spacing="12">
                <BindableLayout.ItemTemplate>
                    <DataTemplate x:DataType="models:CalendarEvent">
                        <Border Style="{StaticResource CardBorder}">
                            <Grid ColumnDefinitions="48,*" ColumnSpacing="12">
                                <VerticalStackLayout VerticalOptions="Center">
                                    <Label Text="{Binding DayText}" Style="{StaticResource DateDayLabel}" />
                                    <Label Text="{Binding MonthText}" Style="{StaticResource SmallLabel}"
                                           TextColor="{StaticResource TextMuted}" HorizontalOptions="Center" />
                                </VerticalStackLayout>
                                <VerticalStackLayout Grid.Column="1" Spacing="2" VerticalOptions="Center">
                                    <Label Text="{Binding Title}" Style="{StaticResource StrongLabel}" />
                                    <Label Text="{Binding TypeAndDescription}" Style="{StaticResource CaptionLabel}"
                                           LineBreakMode="WordWrap" />
                                </VerticalStackLayout>
                            </Grid>
                        </Border>
                    </DataTemplate>
                </BindableLayout.ItemTemplate>
            </VerticalStackLayout>

            <Label x:Name="EmptyLabel" Style="{StaticResource CaptionLabel}"
                   HorizontalOptions="Center" IsVisible="False" />
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

- [ ] **Step 4: CalendarPage logic**

`Views/CalendarPage.xaml.cs`:

```csharp
using System.Globalization;
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(DateParam), "date")]
public partial class CalendarPage : ContentPage
{
    public string DateParam { get; set; } = "";     // optional "yyyy-MM-dd" from Home or Search
    private List<CalendarEvent> allEvents = new();

    public CalendarPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        bool firstLoad = allEvents.Count == 0;

        if (firstLoad)
        {
            Loading.IsVisible = Loading.IsRunning = true;
            try
            {
                allEvents = await FirestoreService.GetEventsAsync();
                MonthView.SetEvents(allEvents);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Something went wrong", "Could not load the calendar. " + ex.Message, "OK");
            }
            finally
            {
                Loading.IsVisible = Loading.IsRunning = false;
            }
        }

        if (DateTime.TryParseExact(DateParam, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                   DateTimeStyles.None, out DateTime date))
        {
            // Opened from "Next up" or a Search result: jump to that day
            MonthView.GoToMonth(date);
            MonthView.SelectDate(date);
            ShowEventsForDay(date);
            DateParam = "";       // use it only once
        }
        else if (firstLoad)
        {
            ShowEventsForMonth();
        }
    }

    private void OnDateSelected(object? sender, DateTime date)
    {
        MonthView.SelectDate(date);
        ShowEventsForDay(date);
    }

    private void OnMonthChanged(object? sender, DateTime month)
    {
        ShowEventsForMonth();
    }

    private void OnShowAllClicked(object sender, EventArgs e)
    {
        MonthView.ClearSelection();
        ShowEventsForMonth();
    }

    private void ShowEventsForMonth()
    {
        string month = MonthView.DisplayMonth.ToString("yyyy-MM");      // dates are "yyyy-MM-dd" text
        ListTitleLabel.Text = "Important dates";
        ShowAllButton.IsVisible = false;
        ShowList(allEvents.Where(ev => ev.Date.StartsWith(month)).ToList(), "No events this month.");
    }

    private void ShowEventsForDay(DateTime date)
    {
        string day = date.ToString("yyyy-MM-dd");
        ListTitleLabel.Text = $"Events on {date:d MMM}";
        ShowAllButton.IsVisible = true;
        ShowList(allEvents.Where(ev => ev.Date == day).ToList(), "No events on this day.");
    }

    private void ShowList(List<CalendarEvent> list, string emptyText)
    {
        BindableLayout.SetItemsSource(EventList, list);
        EmptyLabel.Text = emptyText;
        EmptyLabel.IsVisible = list.Count == 0;
    }
}
```

- [ ] **Step 5: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`.

- [ ] **Step 6: Run checklist S5 on the emulator**

Open Calendar with the temporary button on Home and tick every row of `docs/TEST-CHECKLIST.md` S5.

- [ ] **Step 7: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: custom month calendar with event dots and day filter"
```

---

### Task 6: Home

**Files:**
- Replace: `UnitenStudentAssistant/Views/HomePage.xaml`, `HomePage.xaml.cs` (this removes the two temporary buttons from Task 1)

**Interfaces:**
- Consumes: `FirestoreService.GetProfileAsync()`, `FirestoreService.GetEventsAsync()` (sorted by date), `CalendarEvent.DateText`; images `ic_programmes.png`, `ic_calendar.png`, `ic_services.png`, `ic_requests.png`, `ic_ai.png`, `ic_search.png`; styles `TileBorder`, `InputBorder`, `StrongLabel`; routes `programmes`, `calendar`, `calendar?date=`, `search`, `search?q=`, `//services`, `//requests`, `//ai`.
- Produces: nothing new for later tasks. Search (Task 10) reads the `q` parameter sent from here.

- [ ] **Step 1: HomePage layout**

`Views/HomePage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.HomePage"
             Title="Home"
             Shell.NavBarIsVisible="False">
    <ScrollView>
        <VerticalStackLayout>

            <!-- Navy banner: full width, outside the page padding -->
            <VerticalStackLayout BackgroundColor="{StaticResource Navy}" Padding="16,24,16,16" Spacing="4">
                <Label x:Name="GreetingLabel" Text="Hi" Style="{StaticResource TitleLabel}" TextColor="White" />
                <Label x:Name="StudentInfoLabel" Style="{StaticResource CaptionLabel}"
                       TextColor="White" Opacity="0.8" />
                <Border Style="{StaticResource InputBorder}" Padding="0" Margin="0,12,0,0">
                    <SearchBar x:Name="HomeSearch" Placeholder="Search UNITEN…"
                               SearchButtonPressed="OnSearchButtonPressed" />
                </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Padding="16" Spacing="12">

                <!-- 2 x 3 tiles -->
                <Grid ColumnDefinitions="*,*" RowDefinitions="Auto,Auto,Auto" ColumnSpacing="12" RowSpacing="12">

                    <Border Grid.Row="0" Grid.Column="0" Style="{StaticResource TileBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnProgrammesTapped" />
                        </Border.GestureRecognizers>
                        <VerticalStackLayout Spacing="4" VerticalOptions="Center">
                            <Image Source="ic_programmes.png" WidthRequest="32" HeightRequest="32" HorizontalOptions="Start" />
                            <Label Text="Programmes" Style="{StaticResource StrongLabel}" />
                            <Label Text="Browse degrees" Style="{StaticResource CaptionLabel}"
                                   MaxLines="1" LineBreakMode="TailTruncation" />
                        </VerticalStackLayout>
                    </Border>

                    <Border Grid.Row="0" Grid.Column="1" Style="{StaticResource TileBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnCalendarTapped" />
                        </Border.GestureRecognizers>
                        <VerticalStackLayout Spacing="4" VerticalOptions="Center">
                            <Image Source="ic_calendar.png" WidthRequest="32" HeightRequest="32" HorizontalOptions="Start" />
                            <Label Text="Calendar" Style="{StaticResource StrongLabel}" />
                            <Label Text="Important dates" Style="{StaticResource CaptionLabel}"
                                   MaxLines="1" LineBreakMode="TailTruncation" />
                        </VerticalStackLayout>
                    </Border>

                    <Border Grid.Row="1" Grid.Column="0" Style="{StaticResource TileBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnServicesTapped" />
                        </Border.GestureRecognizers>
                        <VerticalStackLayout Spacing="4" VerticalOptions="Center">
                            <Image Source="ic_services.png" WidthRequest="32" HeightRequest="32" HorizontalOptions="Start" />
                            <Label Text="Services" Style="{StaticResource StrongLabel}" />
                            <Label Text="Offices and help" Style="{StaticResource CaptionLabel}"
                                   MaxLines="1" LineBreakMode="TailTruncation" />
                        </VerticalStackLayout>
                    </Border>

                    <Border Grid.Row="1" Grid.Column="1" Style="{StaticResource TileBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnRequestsTapped" />
                        </Border.GestureRecognizers>
                        <VerticalStackLayout Spacing="4" VerticalOptions="Center">
                            <Image Source="ic_requests.png" WidthRequest="32" HeightRequest="32" HorizontalOptions="Start" />
                            <Label Text="Requests" Style="{StaticResource StrongLabel}" />
                            <Label Text="Submit and track" Style="{StaticResource CaptionLabel}"
                                   MaxLines="1" LineBreakMode="TailTruncation" />
                        </VerticalStackLayout>
                    </Border>

                    <Border Grid.Row="2" Grid.Column="0" Style="{StaticResource TileBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnAiTapped" />
                        </Border.GestureRecognizers>
                        <VerticalStackLayout Spacing="4" VerticalOptions="Center">
                            <Image Source="ic_ai.png" WidthRequest="32" HeightRequest="32" HorizontalOptions="Start" />
                            <Label Text="AI Assistant" Style="{StaticResource StrongLabel}" />
                            <Label Text="Ask a question" Style="{StaticResource CaptionLabel}"
                                   MaxLines="1" LineBreakMode="TailTruncation" />
                        </VerticalStackLayout>
                    </Border>

                    <Border Grid.Row="2" Grid.Column="1" Style="{StaticResource TileBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnSearchTapped" />
                        </Border.GestureRecognizers>
                        <VerticalStackLayout Spacing="4" VerticalOptions="Center">
                            <Image Source="ic_search.png" WidthRequest="32" HeightRequest="32" HorizontalOptions="Start" />
                            <Label Text="Search" Style="{StaticResource StrongLabel}" />
                            <Label Text="Find anything" Style="{StaticResource CaptionLabel}"
                                   MaxLines="1" LineBreakMode="TailTruncation" />
                        </VerticalStackLayout>
                    </Border>
                </Grid>

                <!-- Next up -->
                <Label Text="Next up" Style="{StaticResource SubtitleLabel}" Margin="0,8,0,0" />
                <Border Style="{StaticResource CardBorder}">
                    <Border.GestureRecognizers>
                        <TapGestureRecognizer Tapped="OnNextUpTapped" />
                    </Border.GestureRecognizers>
                    <VerticalStackLayout Spacing="4">
                        <Label x:Name="NextUpTitleLabel" Text="Loading…" Style="{StaticResource StrongLabel}" />
                        <Label x:Name="NextUpDateLabel" Style="{StaticResource CaptionLabel}" />
                    </VerticalStackLayout>
                </Border>

            </VerticalStackLayout>
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

- [ ] **Step 2: HomePage logic**

`Views/HomePage.xaml.cs`:

```csharp
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class HomePage : ContentPage
{
    private CalendarEvent? nextEvent;

    public HomePage()
    {
        InitializeComponent();
    }

    // Runs every time Home is shown, so a name changed in Edit Profile appears here
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var profile = await FirestoreService.GetProfileAsync();
            if (profile != null)
            {
                GreetingLabel.Text = $"Hi, {profile.Name}";
                StudentInfoLabel.Text = $"{profile.Programme} · {profile.StudentId}";
            }

            // Events come sorted by date, and "yyyy-MM-dd" text compares in date order
            var events = await FirestoreService.GetEventsAsync();
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            nextEvent = events.FirstOrDefault(ev => string.CompareOrdinal(ev.Date, today) >= 0);

            if (nextEvent != null)
            {
                NextUpTitleLabel.Text = nextEvent.Title;
                NextUpDateLabel.Text = nextEvent.DateText;
            }
            else
            {
                NextUpTitleLabel.Text = "No upcoming dates";
                NextUpDateLabel.Text = "";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load your home page. " + ex.Message, "OK");
        }
    }

    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        string text = (HomeSearch.Text ?? "").Trim();
        HomeSearch.Text = "";
        HomeSearch.Unfocus();
        await Shell.Current.GoToAsync($"search?q={Uri.EscapeDataString(text)}");
    }

    // Pushed pages: the Home tab stays selected
    private async void OnProgrammesTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("programmes");

    private async void OnCalendarTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("calendar");

    private async void OnSearchTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("search");

    // Absolute routes (//): switch to another tab
    private async void OnServicesTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//services");

    private async void OnRequestsTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//requests");

    private async void OnAiTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//ai");

    private async void OnNextUpTapped(object sender, TappedEventArgs e)
    {
        if (nextEvent == null) return;
        await Shell.Current.GoToAsync($"calendar?date={nextEvent.Date}");
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`. An error about `OnProgrammesClicked` means the old temporary XAML is still there.

- [ ] **Step 4: Run checklist S6 on the emulator**

Tick every row of `docs/TEST-CHECKLIST.md` S6. The Search tile and the SearchBar open the Search placeholder until Task 10.

- [ ] **Step 5: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: home with greeting, tiles and next-up date"
```

---

### Task 7: Requests tab and New Request (Flow B, part 1)

**Files:**
- Replace: `UnitenStudentAssistant/Views/ServiceRequestsPage.xaml`, `ServiceRequestsPage.xaml.cs`
- Replace: `UnitenStudentAssistant/Views/NewRequestPage.xaml`, `NewRequestPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.GetRecentRequestsAsync(int)`, `FirestoreService.GetServicesAsync()`, `FirestoreService.AddRequestAsync(string service, string subject, string details)`; model `ServiceRequest` (`RequestNo`, `Status`, `Subject`, `CreatedText`, `StatusTextColor`, `StatusBgColor`); `RequestStatus`; styles `StatusPill`, `InputBorder`, `ErrorLabel`, `PrimaryButton`, `SecondaryButton`.
- Produces: the request card layout that Task 8 repeats; the calls `tracking`, `tracking?status={escaped status}` and `tracking?status=Pending` that Task 8 reads.

- [ ] **Step 1: ServiceRequestsPage layout**

`Views/ServiceRequestsPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             x:Class="UnitenStudentAssistant.Views.ServiceRequestsPage"
             Title="Requests">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="12">
            <Button Text="+ New Request" Style="{StaticResource PrimaryButton}" Clicked="OnNewRequestClicked" />
            <Button Text="Track my requests" Style="{StaticResource SecondaryButton}" Clicked="OnTrackClicked" />

            <Label Text="Recent requests" Style="{StaticResource SubtitleLabel}" Margin="0,8,0,0" />
            <ActivityIndicator x:Name="Loading" IsRunning="False" IsVisible="False" />

            <!-- BindableLayout, not CollectionView, because we are inside a ScrollView -->
            <VerticalStackLayout x:Name="RecentList" Spacing="12">
                <BindableLayout.ItemTemplate>
                    <DataTemplate x:DataType="models:ServiceRequest">
                        <Border Style="{StaticResource CardBorder}">
                            <Border.GestureRecognizers>
                                <TapGestureRecognizer Tapped="OnRecentTapped" />
                            </Border.GestureRecognizers>
                            <Grid ColumnDefinitions="*,Auto" RowDefinitions="Auto,Auto,Auto" RowSpacing="4">
                                <Label Text="{Binding RequestNo}" Style="{StaticResource StrongLabel}"
                                       VerticalOptions="Center" />
                                <Border Grid.Column="1" Style="{StaticResource StatusPill}"
                                        BackgroundColor="{Binding StatusBgColor}">
                                    <Label Text="{Binding Status}" Style="{StaticResource SmallLabel}"
                                           TextColor="{Binding StatusTextColor}" />
                                </Border>
                                <Label Grid.Row="1" Grid.ColumnSpan="2" Text="{Binding Subject}"
                                       Style="{StaticResource BodyLabel}" />
                                <Label Grid.Row="2" Grid.ColumnSpan="2" Text="{Binding CreatedText}"
                                       Style="{StaticResource CaptionLabel}" />
                            </Grid>
                        </Border>
                    </DataTemplate>
                </BindableLayout.ItemTemplate>
            </VerticalStackLayout>

            <Label x:Name="EmptyLabel" Text="You have no requests yet." Style="{StaticResource CaptionLabel}"
                   HorizontalOptions="Center" IsVisible="False" />
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

- [ ] **Step 2: ServiceRequestsPage logic**

`Views/ServiceRequestsPage.xaml.cs`:

```csharp
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class ServiceRequestsPage : ContentPage
{
    public ServiceRequestsPage()
    {
        InitializeComponent();
    }

    // Reload every time, so a request submitted a moment ago shows at the top
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Loading.IsVisible = Loading.IsRunning = true;
        try
        {
            var recent = await FirestoreService.GetRecentRequestsAsync(3);
            BindableLayout.SetItemsSource(RecentList, recent);
            EmptyLabel.IsVisible = recent.Count == 0;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Something went wrong", "Could not load your requests. " + ex.Message, "OK");
        }
        finally
        {
            Loading.IsVisible = Loading.IsRunning = false;
        }
    }

    private async void OnNewRequestClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("newrequest");

    private async void OnTrackClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("tracking");

    // Opens Tracking on the tapped card's status ("In Progress" has a space, so escape it)
    private async void OnRecentTapped(object sender, TappedEventArgs e)
    {
        var request = (ServiceRequest)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"tracking?status={Uri.EscapeDataString(request.Status)}");
    }
}
```

- [ ] **Step 3: NewRequestPage layout**

`Views/NewRequestPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.NewRequestPage"
             Title="New request">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="16">

            <VerticalStackLayout Spacing="4">
                <Label Text="Service type" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Picker x:Name="ServicePicker" Title="Choose a service" />
                </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Spacing="4">
                <Label Text="Subject" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Entry x:Name="SubjectEntry" Placeholder="Short summary of your request" MaxLength="80" />
                </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Spacing="4">
                <Label Text="Request details" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Editor x:Name="DetailsEditor" Placeholder="Describe what you need"
                            HeightRequest="120" MaxLength="500" />
                </Border>
            </VerticalStackLayout>

            <Label x:Name="ValidationLabel" Style="{StaticResource ErrorLabel}" />

            <Grid ColumnDefinitions="*,Auto" ColumnSpacing="12">
                <Button x:Name="SubmitButton" Text="Submit request" Style="{StaticResource PrimaryButton}"
                        Clicked="OnSubmitClicked" />
                <ActivityIndicator x:Name="Saving" Grid.Column="1" IsRunning="False" IsVisible="False"
                                   WidthRequest="24" HeightRequest="24" VerticalOptions="Center" />
            </Grid>

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

- [ ] **Step 4: NewRequestPage logic**

`Views/NewRequestPage.xaml.cs`:

```csharp
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

        if (ServiceParam != "")
        {
            // Unescape is harmless if Shell has already decoded the value
            ServicePicker.SelectedItem = Uri.UnescapeDataString(ServiceParam);
            ServiceParam = "";     // only once, so the cleared form stays cleared after Back
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
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
                SubjectEntry.Text.Trim(),
                DetailsEditor.Text.Trim());

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
```

- [ ] **Step 5: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`.

- [ ] **Step 6: Run checklist S7 on the emulator**

Tick every row of `docs/TEST-CHECKLIST.md` S7. Tracking is still a placeholder, so the push after Submit shows "Track requests: coming soon".

- [ ] **Step 7: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: requests tab and new request form with validation"
```

---

### Task 8: Request Tracking and Request Detail, realtime (Flow B, part 2)

**Files:**
- Replace: `UnitenStudentAssistant/Views/RequestTrackingPage.xaml`, `RequestTrackingPage.xaml.cs`
- Replace: `UnitenStudentAssistant/Views/RequestDetailPage.xaml`, `RequestDetailPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.ListenToRequests(...)`, `FirestoreService.ListenToRequest(...)`, `RequestStatus`, `AppColors.Get`, model `ServiceRequest`; styles `SegmentButton`, `StatusPill`; the `status` values sent by Task 7.
- Produces: route `requestdetail?id={id}` working. These two pages are the teacher's realtime demo (AGENTS.md §4).

- [ ] **Step 1: RequestTrackingPage layout**

`Views/RequestTrackingPage.xaml`. The request card is the same as on the Requests tab:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             x:Class="UnitenStudentAssistant.Views.RequestTrackingPage"
             Title="Track requests">
    <Grid RowDefinitions="Auto,*" Padding="16" RowSpacing="12">

        <!-- Three buttons that act as a segmented control -->
        <Grid ColumnDefinitions="*,*,*" ColumnSpacing="8">
            <Button x:Name="PendingButton" Style="{StaticResource SegmentButton}" Clicked="OnPendingClicked" />
            <Button x:Name="ProgressButton" Grid.Column="1" Style="{StaticResource SegmentButton}" Clicked="OnProgressClicked" />
            <Button x:Name="CompletedButton" Grid.Column="2" Style="{StaticResource SegmentButton}" Clicked="OnCompletedClicked" />
        </Grid>

        <CollectionView x:Name="RequestList" Grid.Row="1">
            <CollectionView.ItemsLayout>
                <LinearItemsLayout Orientation="Vertical" ItemSpacing="12" />
            </CollectionView.ItemsLayout>
            <CollectionView.EmptyView>
                <Label x:Name="EmptyListLabel" Style="{StaticResource CaptionLabel}"
                       HorizontalOptions="Center" Margin="0,24" />
            </CollectionView.EmptyView>
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:ServiceRequest">
                    <Border Style="{StaticResource CardBorder}">
                        <Border.GestureRecognizers>
                            <TapGestureRecognizer Tapped="OnRequestTapped" />
                        </Border.GestureRecognizers>
                        <Grid ColumnDefinitions="*,Auto" RowDefinitions="Auto,Auto,Auto" RowSpacing="4">
                            <Label Text="{Binding RequestNo}" Style="{StaticResource StrongLabel}"
                                   VerticalOptions="Center" />
                            <Border Grid.Column="1" Style="{StaticResource StatusPill}"
                                    BackgroundColor="{Binding StatusBgColor}">
                                <Label Text="{Binding Status}" Style="{StaticResource SmallLabel}"
                                       TextColor="{Binding StatusTextColor}" />
                            </Border>
                            <Label Grid.Row="1" Grid.ColumnSpan="2" Text="{Binding Subject}"
                                   Style="{StaticResource BodyLabel}" />
                            <Label Grid.Row="2" Grid.ColumnSpan="2" Text="{Binding CreatedText}"
                                   Style="{StaticResource CaptionLabel}" />
                        </Grid>
                    </Border>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
    </Grid>
</ContentPage>
```

- [ ] **Step 2: RequestTrackingPage logic (the live list)**

`Views/RequestTrackingPage.xaml.cs`:

```csharp
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

        if (StatusParam != "")
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

    private void OnPendingClicked(object sender, EventArgs e) => SelectStatus(RequestStatus.Pending);

    private void OnProgressClicked(object sender, EventArgs e) => SelectStatus(RequestStatus.InProgress);

    private void OnCompletedClicked(object sender, EventArgs e) => SelectStatus(RequestStatus.Completed);

    private async void OnRequestTapped(object sender, TappedEventArgs e)
    {
        var request = (ServiceRequest)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"requestdetail?id={request.Id}");
    }
}
```

- [ ] **Step 3: RequestDetailPage layout**

`Views/RequestDetailPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.RequestDetailPage"
             Title="Request">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="12">
            <Label x:Name="RequestNoLabel" Text="Loading…" Style="{StaticResource TitleLabel}" />

            <!-- Large status pill -->
            <Border x:Name="StatusBadge" Style="{StaticResource StatusPill}"
                    Padding="14,6" HorizontalOptions="Start">
                <Label x:Name="StatusLabel" Style="{StaticResource SmallLabel}" FontSize="13" />
            </Border>

            <Label x:Name="SubjectLabel" Style="{StaticResource SubtitleLabel}" />

            <Border Style="{StaticResource CardBorder}">
                <VerticalStackLayout Spacing="12">
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Service" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="ServiceLabel" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Submitted" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="CreatedLabel" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Last updated" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="UpdatedLabel" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Details" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="DetailsLabel" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                </VerticalStackLayout>
            </Border>

            <Label Text="Status updates automatically." Style="{StaticResource CaptionLabel}"
                   HorizontalOptions="Center" />
        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

- [ ] **Step 4: RequestDetailPage logic (the live document)**

`Views/RequestDetailPage.xaml.cs`:

```csharp
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
```

- [ ] **Step 5: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`.

- [ ] **Step 6: Run checklist S8 on the emulator (the realtime demo)**

Keep the Firebase Console open on `requests` next to the emulator and tick every row of `docs/TEST-CHECKLIST.md` S8, including the live detail, live list and "no duplicate updates after reopening" rows.

- [ ] **Step 7: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: live request tracking and request detail"
```

---

### Task 9: Profile, Edit Profile and About (Flow D)

**Files:**
- Replace: `UnitenStudentAssistant/Views/ProfilePage.xaml`, `ProfilePage.xaml.cs`
- Replace: `UnitenStudentAssistant/Views/EditProfilePage.xaml`, `EditProfilePage.xaml.cs`
- Replace: `UnitenStudentAssistant/Views/AboutPage.xaml`, `AboutPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.GetProfileAsync()`, `FirestoreService.UpdateProfileAsync(StudentProfile p)`, `FirestoreService.GetProgrammesAsync()`; model `StudentProfile` (with `Initials`); styles `AvatarLabel`, `TitleLabel`, `CaptionLabel`, `BodyLabel`, `CardBorder`, `InputBorder`, `Divider`, `ErrorLabel`, `PrimaryButton`, `SecondaryButton`; image `logo_u.png`; routes `editprofile`, `about`.
- Produces: nothing new for later tasks. Home (Task 6) already reloads the profile in `OnAppearing`, so an edited name shows there too.

- [ ] **Step 1: ProfilePage layout**

`Views/ProfilePage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.ProfilePage"
             Title="Profile">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="12">

            <!-- Navy circle with the initials -->
            <Border BackgroundColor="{StaticResource Navy}" StrokeThickness="0"
                    StrokeShape="RoundRectangle 40" WidthRequest="80" HeightRequest="80"
                    HorizontalOptions="Center" Margin="0,8,0,0">
                <Label x:Name="InitialsLabel" Text="?" Style="{StaticResource AvatarLabel}" />
            </Border>
            <Label x:Name="NameLabel" Text="Loading…" Style="{StaticResource TitleLabel}"
                   HorizontalOptions="Center" />
            <Label x:Name="StudentIdLabel" Style="{StaticResource CaptionLabel}"
                   HorizontalOptions="Center" />

            <Border Style="{StaticResource CardBorder}">
                <VerticalStackLayout Spacing="12">
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Programme" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="ProgrammeLabel" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                    <BoxView Style="{StaticResource Divider}" />
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Email" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="EmailLabel" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                    <BoxView Style="{StaticResource Divider}" />
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Phone" Style="{StaticResource CaptionLabel}" />
                        <Label x:Name="PhoneLabel" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                </VerticalStackLayout>
            </Border>

            <Button Text="Edit profile" Style="{StaticResource SecondaryButton}" Clicked="OnEditClicked" />
            <Button Text="About this app" Style="{StaticResource SecondaryButton}" Clicked="OnAboutClicked" />

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

- [ ] **Step 2: ProfilePage logic**

`Views/ProfilePage.xaml.cs`:

```csharp
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

    private async void OnEditClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("editprofile");

    private async void OnAboutClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("about");
}
```

- [ ] **Step 3: EditProfilePage layout**

`Views/EditProfilePage.xaml`. The form follows the same pattern as New Request (Task 7):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.EditProfilePage"
             Title="Edit profile">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="16">

            <VerticalStackLayout Spacing="4">
                <Label Text="Name" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Entry x:Name="NameEntry" Placeholder="Your full name" MaxLength="60" />
                </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Spacing="4">
                <Label Text="Student ID" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Entry x:Name="StudentIdEntry" IsReadOnly="True"
                           TextColor="{StaticResource TextMuted}" />
                </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Spacing="4">
                <Label Text="Programme" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Picker x:Name="ProgrammePicker" Title="Choose your programme" />
                </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Spacing="4">
                <Label Text="Email" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Entry x:Name="EmailEntry" Keyboard="Email" Placeholder="name@student.uniten.edu.my" />
                </Border>
            </VerticalStackLayout>

            <VerticalStackLayout Spacing="4">
                <Label Text="Phone" Style="{StaticResource CaptionLabel}" />
                <Border Style="{StaticResource InputBorder}">
                    <Entry x:Name="PhoneEntry" Keyboard="Telephone" Placeholder="012-345 6789" MaxLength="20" />
                </Border>
            </VerticalStackLayout>

            <Label x:Name="ValidationLabel" Style="{StaticResource ErrorLabel}" />

            <Grid ColumnDefinitions="*,Auto" ColumnSpacing="12">
                <Button x:Name="SaveButton" Text="Save changes" Style="{StaticResource PrimaryButton}"
                        Clicked="OnSaveClicked" />
                <ActivityIndicator x:Name="Saving" Grid.Column="1" IsRunning="False" IsVisible="False"
                                   WidthRequest="24" HeightRequest="24" VerticalOptions="Center" />
            </Grid>
            <Button Text="Cancel" Style="{StaticResource SecondaryButton}" Clicked="OnCancelClicked" />

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

- [ ] **Step 4: EditProfilePage logic**

`Views/EditProfilePage.xaml.cs`:

```csharp
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

    private async void OnSaveClicked(object sender, EventArgs e)
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
            profile.Name = NameEntry.Text.Trim();
            profile.Programme = (string)ProgrammePicker.SelectedItem;
            profile.Email = EmailEntry.Text.Trim();
            profile.Phone = PhoneEntry.Text.Trim();
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

    private async void OnCancelClicked(object sender, EventArgs e) =>
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
```

- [ ] **Step 5: AboutPage**

`Views/AboutPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="UnitenStudentAssistant.Views.AboutPage"
             Title="About">
    <ScrollView>
        <VerticalStackLayout Padding="16" Spacing="12">

            <!-- Logo on a navy rounded square -->
            <Border BackgroundColor="{StaticResource Navy}" StrokeThickness="0"
                    StrokeShape="RoundRectangle 24" WidthRequest="96" HeightRequest="96"
                    HorizontalOptions="Center" Margin="0,8,0,0">
                <Image Source="logo_u.png" WidthRequest="72" HeightRequest="72"
                       HorizontalOptions="Center" VerticalOptions="Center" />
            </Border>

            <Label Text="UNITEN Student Assistant" Style="{StaticResource TitleLabel}"
                   HorizontalOptions="Center" />
            <Label x:Name="VersionLabel" Style="{StaticResource CaptionLabel}"
                   HorizontalOptions="Center" />

            <Border Style="{StaticResource CardBorder}">
                <Label Style="{StaticResource BodyLabel}"
                       Text="A simple mobile app that helps UNITEN students find programmes, check the academic calendar, contact university services, submit and track service requests, and ask an AI assistant." />
            </Border>

            <Border Style="{StaticResource CardBorder}">
                <VerticalStackLayout Spacing="12">
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Developer" Style="{StaticResource CaptionLabel}" />
                        <Label Text="Abdullah (is01084960)" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                    <BoxView Style="{StaticResource Divider}" />
                    <VerticalStackLayout Spacing="4">
                        <Label Text="Course" Style="{StaticResource CaptionLabel}" />
                        <Label Text="CSNB5123" Style="{StaticResource BodyLabel}" />
                    </VerticalStackLayout>
                </VerticalStackLayout>
            </Border>

            <Label Text="Built with .NET MAUI, Firebase Firestore and Google Gemini."
                   Style="{StaticResource CaptionLabel}" HorizontalOptions="Center"
                   HorizontalTextAlignment="Center" />

        </VerticalStackLayout>
    </ScrollView>
</ContentPage>
```

`Views/AboutPage.xaml.cs`:

```csharp
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
```

- [ ] **Step 6: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`.

- [ ] **Step 7: Run checklist S9 on the emulator**

Tick every row of `docs/TEST-CHECKLIST.md` S9. Keep the Firestore Console open on `profile/student` to see the saved phone.

- [ ] **Step 8: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: profile, edit profile and about pages"
```

---

### Task 10: Search (Flow F)

**Files:**
- Replace: `UnitenStudentAssistant/Views/SearchPage.xaml`, `SearchPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.GetProgrammesAsync()`, `GetServicesAsync()`, `GetEventsAsync()`; models `Programme`, `UniService`, `CalendarEvent` (with `DayText`, `MonthText`, `DateText`); the `q` parameter sent by Home (Task 6); routes `programmedetail?id=`, `servicedetail?id=`, `calendar?date=`.
- Produces: route `search` / `search?q=` working.

- [ ] **Step 1: SearchPage layout**

`Views/SearchPage.xaml`. The results are inside a `ScrollView`, so each section uses `BindableLayout`, not a `CollectionView` (AGENTS.md §6):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             x:Class="UnitenStudentAssistant.Views.SearchPage"
             Title="Search">
    <Grid RowDefinitions="Auto,*" Padding="16" RowSpacing="12">
        <SearchBar x:Name="SearchInput" Placeholder="Search programmes, services and dates"
                   TextChanged="OnSearchTextChanged" />

        <ScrollView Grid.Row="1">
            <VerticalStackLayout Spacing="12">
                <Label x:Name="MessageLabel" Style="{StaticResource CaptionLabel}"
                       HorizontalOptions="Center" Margin="0,24" />

                <!-- Programmes -->
                <Label x:Name="ProgrammesHeading" Text="Programmes" Style="{StaticResource SubtitleLabel}"
                       IsVisible="False" />
                <VerticalStackLayout x:Name="ProgrammeResults" Spacing="12">
                    <BindableLayout.ItemTemplate>
                        <DataTemplate x:DataType="models:Programme">
                            <Border Style="{StaticResource CardBorder}">
                                <Border.GestureRecognizers>
                                    <TapGestureRecognizer Tapped="OnProgrammeTapped" />
                                </Border.GestureRecognizers>
                                <VerticalStackLayout Spacing="4">
                                    <Label Text="{Binding Name}" Style="{StaticResource StrongLabel}" />
                                    <Label Text="{Binding Faculty}" Style="{StaticResource CaptionLabel}" />
                                </VerticalStackLayout>
                            </Border>
                        </DataTemplate>
                    </BindableLayout.ItemTemplate>
                </VerticalStackLayout>

                <!-- Services -->
                <Label x:Name="ServicesHeading" Text="Services" Style="{StaticResource SubtitleLabel}"
                       IsVisible="False" />
                <VerticalStackLayout x:Name="ServiceResults" Spacing="12">
                    <BindableLayout.ItemTemplate>
                        <DataTemplate x:DataType="models:UniService">
                            <Border Style="{StaticResource CardBorder}">
                                <Border.GestureRecognizers>
                                    <TapGestureRecognizer Tapped="OnServiceTapped" />
                                </Border.GestureRecognizers>
                                <VerticalStackLayout Spacing="4">
                                    <Label Text="{Binding Name}" Style="{StaticResource StrongLabel}" />
                                    <Label Text="{Binding Description}" Style="{StaticResource CaptionLabel}"
                                           MaxLines="2" LineBreakMode="TailTruncation" />
                                </VerticalStackLayout>
                            </Border>
                        </DataTemplate>
                    </BindableLayout.ItemTemplate>
                </VerticalStackLayout>

                <!-- Calendar -->
                <Label x:Name="EventsHeading" Text="Calendar" Style="{StaticResource SubtitleLabel}"
                       IsVisible="False" />
                <VerticalStackLayout x:Name="EventResults" Spacing="12">
                    <BindableLayout.ItemTemplate>
                        <DataTemplate x:DataType="models:CalendarEvent">
                            <Border Style="{StaticResource CardBorder}">
                                <Border.GestureRecognizers>
                                    <TapGestureRecognizer Tapped="OnEventTapped" />
                                </Border.GestureRecognizers>
                                <Grid ColumnDefinitions="48,*" ColumnSpacing="12">
                                    <VerticalStackLayout VerticalOptions="Center">
                                        <Label Text="{Binding DayText}" Style="{StaticResource DateDayLabel}" />
                                        <Label Text="{Binding MonthText}" Style="{StaticResource CaptionLabel}"
                                               HorizontalOptions="Center" />
                                    </VerticalStackLayout>
                                    <VerticalStackLayout Grid.Column="1" Spacing="2" VerticalOptions="Center">
                                        <Label Text="{Binding Title}" Style="{StaticResource StrongLabel}" />
                                        <Label Text="{Binding DateText}" Style="{StaticResource CaptionLabel}" />
                                    </VerticalStackLayout>
                                </Grid>
                            </Border>
                        </DataTemplate>
                    </BindableLayout.ItemTemplate>
                </VerticalStackLayout>
            </VerticalStackLayout>
        </ScrollView>

        <ActivityIndicator x:Name="Loading" Grid.Row="1" IsRunning="False" IsVisible="False"
                           VerticalOptions="Start" />
    </Grid>
</ContentPage>
```

- [ ] **Step 2: SearchPage logic**

`Views/SearchPage.xaml.cs`. The data is loaded once and then filtered in memory (NAVIGATION.md §6):

```csharp
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

[QueryProperty(nameof(QueryParam), "q")]
public partial class SearchPage : ContentPage
{
    public string QueryParam { get; set; } = "";      // optional starting text from Home
    private bool loaded;
    private List<Programme> programmes = new();
    private List<UniService> services = new();
    private List<CalendarEvent> events = new();

    public SearchPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!loaded)
        {
            Loading.IsVisible = Loading.IsRunning = true;
            try
            {
                programmes = await FirestoreService.GetProgrammesAsync();
                services = await FirestoreService.GetServicesAsync();
                events = await FirestoreService.GetEventsAsync();
                loaded = true;
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync("Something went wrong", "Could not load search data. " + ex.Message, "OK");
            }
            finally
            {
                Loading.IsVisible = Loading.IsRunning = false;
            }
        }

        if (QueryParam != "")
        {
            SearchInput.Text = Uri.UnescapeDataString(QueryParam);
            QueryParam = "";       // only once; Back from a result keeps what the user typed
        }
        RunSearch();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) => RunSearch();

    private void RunSearch()
    {
        string text = (SearchInput.Text ?? "").Trim();

        var foundProgrammes = new List<Programme>();
        var foundServices = new List<UniService>();
        var foundEvents = new List<CalendarEvent>();

        if (text != "")
        {
            foundProgrammes = programmes.Where(p => Matches(text, p.Name, p.Faculty, p.Description)).ToList();
            foundServices = services.Where(s => Matches(text, s.Name, s.Description)).ToList();
            foundEvents = events.Where(ev => Matches(text, ev.Title, ev.Description)).ToList();
        }

        ShowSection(ProgrammesHeading, ProgrammeResults, foundProgrammes);
        ShowSection(ServicesHeading, ServiceResults, foundServices);
        ShowSection(EventsHeading, EventResults, foundEvents);

        int total = foundProgrammes.Count + foundServices.Count + foundEvents.Count;
        if (text == "")
            ShowMessage("Type to search programmes, services and dates.");
        else if (total == 0)
            ShowMessage($"No results for “{text}”.");
        else
            MessageLabel.IsVisible = false;
    }

    // Case-insensitive "contains" on any of the given fields
    private static bool Matches(string text, params string[] fields) =>
        fields.Any(f => f.Contains(text, StringComparison.OrdinalIgnoreCase));

    // A heading and its list only show when the section has results
    private static void ShowSection<T>(Label heading, VerticalStackLayout list, List<T> items)
    {
        heading.IsVisible = items.Count > 0;
        list.IsVisible = items.Count > 0;
        BindableLayout.SetItemsSource(list, items);
    }

    private void ShowMessage(string message)
    {
        MessageLabel.Text = message;
        MessageLabel.IsVisible = true;
    }

    private async void OnProgrammeTapped(object sender, TappedEventArgs e)
    {
        var programme = (Programme)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"programmedetail?id={programme.Id}");
    }

    private async void OnServiceTapped(object sender, TappedEventArgs e)
    {
        var service = (UniService)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"servicedetail?id={service.Id}");
    }

    // Opens the calendar on that month with the day selected
    private async void OnEventTapped(object sender, TappedEventArgs e)
    {
        var calendarEvent = (CalendarEvent)((BindableObject)sender).BindingContext;
        await Shell.Current.GoToAsync($"calendar?date={calendarEvent.Date}");
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`.

- [ ] **Step 4: Run checklist S10 on the emulator**

Tick every row of `docs/TEST-CHECKLIST.md` S10, including the Home → "library" → Library → "Make a request" chain (Flow F).

- [ ] **Step 5: Commit**

```bash
git add UnitenStudentAssistant
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: search across programmes, services and calendar"
```

---

### Task 11: AI Student Assistant with Gemini (Flow E, FR7)

**Files:**
- Create: `UnitenStudentAssistant/Resources/Raw/secrets.sample.json` (committed)
- Create: `UnitenStudentAssistant/Resources/Raw/secrets.json` (local only, git-ignored)
- Create: `UnitenStudentAssistant/Helpers/AppConfig.cs`
- Create: `UnitenStudentAssistant/Helpers/AiPromptBuilder.cs`
- Create: `UnitenStudentAssistant/Services/GeminiService.cs`
- Replace: `UnitenStudentAssistant/Views/AiAssistantPage.xaml`, `AiAssistantPage.xaml.cs`

**Interfaces:**
- Consumes: `FirestoreService.GetProfileAsync()`, `GetProgrammesAsync()`, `GetServicesAsync()`, `GetEventsAsync()`; model `ChatMessage` (with `Side`, `BubbleColor`, `TextColor`, `IsUser`, `IsError`); styles `ChatBubble`, `ChipButton`, `PrimaryButton`, `Divider`, `BodyLabel`, `CaptionLabel`.
- Produces:
  - `class AppSecrets { string GeminiApiKey; string GeminiModel; bool HasKey; }`
  - `static Task<AppSecrets> AppConfig.GetSecretsAsync()`, read once and cached
  - `static Task<string> AiPromptBuilder.BuildAsync()`, which returns the system instruction text (DATA-MODEL.md §6)
  - `static Task<string> GeminiService.AskAsync(string systemText, List<ChatMessage> history)`, which returns the answer text and throws on HTTP or network errors

This is "context stuffing", not RAG: the app puts its own small Firestore dataset into Gemini's system instruction on every question (spec §6).

- [ ] **Step 1: Get a key and create the secrets files**

Follow `docs/SETUP-FIREBASE-GEMINI.md` Part 3 to create a Gemini API key.

Create `Resources/Raw/secrets.sample.json` (this one **is** committed):

```json
{ "GeminiApiKey": "PASTE-YOUR-KEY-HERE", "GeminiModel": "gemini-3.8-flash" }
```

Copy it to `Resources/Raw/secrets.json` in the same folder and paste your real key into the copy. Then confirm git ignores the copy:

```bash
git check-ignore -v UnitenStudentAssistant/Resources/Raw/secrets.json
```

Expected: one line naming `.gitignore` and the `secrets` pattern. **If it prints nothing, stop** and fix `.gitignore` before going on.

- [ ] **Step 2: `AppConfig` (reads the key)**

`Helpers/AppConfig.cs`. It uses `JsonDocument`, not reflection, so it also works in Release builds (AGENTS.md §7):

```csharp
using System.Diagnostics;
using System.Text.Json;

namespace UnitenStudentAssistant.Helpers;

public class AppSecrets
{
    public string GeminiApiKey { get; set; } = "";
    public string GeminiModel { get; set; } = "gemini-3.8-flash";

    // False when secrets.json is missing or still holds the sample text
    public bool HasKey => GeminiApiKey != "" && GeminiApiKey != "PASTE-YOUR-KEY-HERE";
}

public static class AppConfig
{
    private static AppSecrets? cached;

    // Reads Resources/Raw/secrets.json once. A missing or broken file gives an empty AppSecrets.
    public static async Task<AppSecrets> GetSecretsAsync()
    {
        if (cached != null) return cached;

        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("secrets.json");
            using var doc = await JsonDocument.ParseAsync(stream);
            cached = new AppSecrets
            {
                GeminiApiKey = ReadString(doc.RootElement, "GeminiApiKey", ""),
                GeminiModel = ReadString(doc.RootElement, "GeminiModel", "gemini-3.8-flash")
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"secrets.json could not be read: {ex.Message}");   // never log the key itself
            cached = new AppSecrets();
        }
        return cached;
    }

    private static string ReadString(JsonElement root, string name, string fallback)
    {
        if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
        {
            string text = (value.GetString() ?? "").Trim();
            if (text != "") return text;
        }
        return fallback;
    }
}
```

- [ ] **Step 3: `AiPromptBuilder` (Firestore data to system instruction)**

`Helpers/AiPromptBuilder.cs`. The rules text is copied from DATA-MODEL.md §6. If you change one, change the other:

```csharp
using System.Text;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Helpers;

public static class AiPromptBuilder
{
    public static async Task<string> BuildAsync()
    {
        var profile = await FirestoreService.GetProfileAsync();
        var programmes = await FirestoreService.GetProgrammesAsync();
        var services = await FirestoreService.GetServicesAsync();
        var events = await FirestoreService.GetEventsAsync();

        var text = new StringBuilder();
        text.AppendLine("You are the UNITEN Student Assistant inside a student mobile app.");
        text.AppendLine("Answer ONLY using the facts below. If the answer is not in the facts, say you don't know and suggest contacting the right service.");
        text.AppendLine("Keep answers under 120 words, friendly and plain text (no markdown).");
        text.AppendLine();

        if (profile != null)
            text.AppendLine($"STUDENT: {profile.Name} ({profile.StudentId}), {profile.Programme}");

        text.AppendLine("PROGRAMMES:");
        foreach (var p in programmes)
            text.AppendLine($"- {p.Name}: {p.Description} Faculty: {p.Faculty}. Duration: {p.Duration}. Majors: {string.Join(", ", p.Majors)}.");

        text.AppendLine("SERVICES:");
        foreach (var s in services)
            text.AppendLine($"- {s.Name}: {s.Description} Location: {s.Location}. Hours: {s.Hours}. Contact: {s.Contact}.");

        text.AppendLine("CALENDAR (Semester 1 2026/2027):");
        foreach (var e in events)
            text.AppendLine($"- {e.Date} {e.Title}: {e.Description}");

        // Today's date lets the AI answer "what is next?" questions
        text.AppendLine($"TODAY: {DateTime.Today:yyyy-MM-dd}");
        return text.ToString();
    }
}
```

- [ ] **Step 4: `GeminiService` (one REST call per question)**

`Services/GeminiService.cs`. It sends `POST v1beta/models/{model}:generateContent` with the key in the `x-goog-api-key` header (spec §6; SETUP-FIREBASE-GEMINI.md Part 3):

```csharp
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using UnitenStudentAssistant.Helpers;
using UnitenStudentAssistant.Models;

namespace UnitenStudentAssistant.Services;

public static class GeminiService
{
    // One HttpClient for the whole app. After 30 s it gives up with TaskCanceledException.
    private static readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public static async Task<string> AskAsync(string systemText, List<ChatMessage> history)
    {
        var secrets = await AppConfig.GetSecretsAsync();

        // The conversation so far: "user" for the student, "model" for the AI
        var contents = new JsonArray();
        foreach (var message in history)
        {
            contents.Add(new JsonObject
            {
                ["role"] = message.IsUser ? "user" : "model",
                ["parts"] = new JsonArray(new JsonObject { ["text"] = message.Text })
            });
        }

        var body = new JsonObject
        {
            ["system_instruction"] = new JsonObject
            {
                ["parts"] = new JsonArray(new JsonObject { ["text"] = systemText })
            },
            ["contents"] = contents
        };

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{secrets.GeminiModel}:generateContent";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("x-goog-api-key", secrets.GeminiApiKey);     // header, never in the URL
        request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

        using var response = await http.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Gemini error {(int)response.StatusCode}: {ReadErrorMessage(json)}");

        return ReadAnswer(json);
    }

    // The answer is the text of candidates[0].content.parts.
    // Parts marked "thought": true are the model's reasoning, so skip them.
    private static string ReadAnswer(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var answer = new StringBuilder();

        if (doc.RootElement.TryGetProperty("candidates", out var candidates)
            && candidates.ValueKind == JsonValueKind.Array
            && candidates.GetArrayLength() > 0
            && candidates[0].TryGetProperty("content", out var content)
            && content.TryGetProperty("parts", out var parts)
            && parts.ValueKind == JsonValueKind.Array)
        {
            foreach (var part in parts.EnumerateArray())
            {
                bool isThought = part.TryGetProperty("thought", out var thought)
                                 && thought.ValueKind == JsonValueKind.True;
                if (!isThought && part.TryGetProperty("text", out var text)
                               && text.ValueKind == JsonValueKind.String)
                {
                    answer.Append(text.GetString());
                }
            }
        }

        string result = answer.ToString().Trim();
        return result != "" ? result : "Sorry, I could not answer that. Please try asking in a different way.";
    }

    // Gemini errors look like { "error": { "code": 404, "message": "..." } }
    private static string ReadErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message)
                && message.ValueKind == JsonValueKind.String)
            {
                return message.GetString() ?? "no details";
            }
        }
        catch (JsonException)
        {
            // not JSON, so fall through
        }
        return "no details";
    }
}
```

- [ ] **Step 5: AiAssistantPage layout**

`Views/AiAssistantPage.xaml`. The bubble's `HorizontalOptions` sits on the inner `Border`, not on the template's root, because a `CollectionView` stretches the root of each item to the full width:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:models="clr-namespace:UnitenStudentAssistant.Models"
             x:Class="UnitenStudentAssistant.Views.AiAssistantPage"
             Title="AI assistant">
    <Grid RowDefinitions="*,Auto,Auto,Auto">

        <!-- Chat bubbles -->
        <CollectionView x:Name="ChatList" Margin="16,16,16,0">
            <CollectionView.ItemsLayout>
                <LinearItemsLayout Orientation="Vertical" ItemSpacing="8" />
            </CollectionView.ItemsLayout>
            <CollectionView.ItemTemplate>
                <DataTemplate x:DataType="models:ChatMessage">
                    <Grid>
                        <Border Style="{StaticResource ChatBubble}"
                                HorizontalOptions="{Binding Side}"
                                BackgroundColor="{Binding BubbleColor}">
                            <Label Text="{Binding Text}" Style="{StaticResource BodyLabel}"
                                   TextColor="{Binding TextColor}" />
                        </Border>
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>

        <!-- Visible only while waiting for Gemini -->
        <HorizontalStackLayout x:Name="ThinkingRow" Grid.Row="1" Padding="16,4" Spacing="8" IsVisible="False">
            <ActivityIndicator IsRunning="True" WidthRequest="20" HeightRequest="20" />
            <Label Text="Thinking…" Style="{StaticResource CaptionLabel}" VerticalOptions="Center" />
        </HorizontalStackLayout>

        <!-- Suggestion chips: shown only while the chat holds just the welcome bubble -->
        <ScrollView x:Name="ChipRow" Grid.Row="2" Orientation="Horizontal"
                    HorizontalScrollBarVisibility="Never">
            <HorizontalStackLayout Padding="16,8" Spacing="8">
                <Button Text="When does the semester start?" Style="{StaticResource ChipButton}" Clicked="OnChipClicked" />
                <Button Text="How do I contact Finance?" Style="{StaticResource ChipButton}" Clicked="OnChipClicked" />
                <Button Text="Which programmes are offered?" Style="{StaticResource ChipButton}" Clicked="OnChipClicked" />
            </HorizontalStackLayout>
        </ScrollView>

        <!-- Input bar -->
        <VerticalStackLayout Grid.Row="3" BackgroundColor="{StaticResource CardBg}">
            <BoxView Style="{StaticResource Divider}" />
            <Grid ColumnDefinitions="*,72" ColumnSpacing="8" Padding="16,8">
                <Entry x:Name="QuestionEntry" Placeholder="Ask about UNITEN…" ReturnType="Send"
                       Completed="OnQuestionCompleted" VerticalOptions="Center" />
                <Button x:Name="SendButton" Grid.Column="1" Text="Send" Style="{StaticResource PrimaryButton}"
                        Padding="0" Clicked="OnSendClicked" />
            </Grid>
        </VerticalStackLayout>

    </Grid>
</ContentPage>
```

- [ ] **Step 6: AiAssistantPage logic**

`Views/AiAssistantPage.xaml.cs`. The chat lives only in this page's list and is never stored in Firestore (spec §6):

```csharp
using System.Collections.ObjectModel;
using System.Diagnostics;
using UnitenStudentAssistant.Helpers;
using UnitenStudentAssistant.Models;
using UnitenStudentAssistant.Services;

namespace UnitenStudentAssistant.Views;

public partial class AiAssistantPage : ContentPage
{
    private readonly ObservableCollection<ChatMessage> messages = new();
    private readonly ChatMessage welcome = new()
    {
        Text = "Hi! I'm your UNITEN assistant. Ask me about programmes, dates or services."
    };
    private string systemText = "";

    public AiAssistantPage()
    {
        InitializeComponent();
        messages.Add(welcome);
        ChatList.ItemsSource = messages;      // ObservableCollection: new bubbles appear by themselves
    }

    // Rebuild the facts on every visit, so a profile edited a minute ago is included
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            systemText = await AiPromptBuilder.BuildAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"AI facts not loaded: {ex.Message}");   // SendAsync tries again
        }
    }

    private async void OnChipClicked(object sender, EventArgs e) =>
        await SendAsync(((Button)sender).Text);

    private async void OnSendClicked(object sender, EventArgs e) =>
        await SendAsync(QuestionEntry.Text);

    private async void OnQuestionCompleted(object sender, EventArgs e) =>
        await SendAsync(QuestionEntry.Text);

    private async Task SendAsync(string? question)
    {
        string text = (question ?? "").Trim();
        if (text == "" || !SendButton.IsEnabled) return;     // empty, or still waiting for an answer

        QuestionEntry.Text = "";
        AddMessage(new ChatMessage { Text = text, IsUser = true });
        SetWaiting(true);
        try
        {
            var secrets = await AppConfig.GetSecretsAsync();
            if (!secrets.HasKey)
            {
                AddMessage(Error("AI is not set up yet. Add your Gemini key to secrets.json."));
                return;
            }

            if (systemText == "")
                systemText = await AiPromptBuilder.BuildAsync();     // the first load failed

            string answer = await GeminiService.AskAsync(systemText, BuildHistory());
            AddMessage(new ChatMessage { Text = answer });
        }
        catch (HttpRequestException)
        {
            AddMessage(Error("Sorry, I could not reach the AI right now. Check your internet and try again."));
        }
        catch (TaskCanceledException)
        {
            AddMessage(Error("The AI took too long to answer. Please try again."));
        }
        catch (Exception ex)
        {
            AddMessage(Error("Sorry, the AI could not answer: " + ex.Message));
        }
        finally
        {
            SetWaiting(false);
        }
    }

    // What Gemini sees: the real conversation, without the welcome bubble and error bubbles.
    // A question that only got an error is left out too, so user/model turns keep alternating.
    private List<ChatMessage> BuildHistory()
    {
        var history = new List<ChatMessage>();
        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];
            if (message == welcome || message.IsError) continue;

            bool answeredWithError = i + 1 < messages.Count && messages[i + 1].IsError;
            if (message.IsUser && answeredWithError) continue;

            history.Add(message);
        }
        return history;
    }

    private void AddMessage(ChatMessage message)
    {
        messages.Add(message);
        ChipRow.IsVisible = messages.Count <= 1;              // chips only next to the welcome bubble
        ChatList.ScrollTo(messages.Count - 1);                // show the newest bubble
    }

    private void SetWaiting(bool waiting)
    {
        ThinkingRow.IsVisible = waiting;
        SendButton.IsEnabled = !waiting;
    }

    private static ChatMessage Error(string text) => new() { Text = text, IsError = true };
}
```

- [ ] **Step 7: Build**

Run: `dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android`
Expected: `0 Error(s)`.

- [ ] **Step 8: Run checklist S11 on the emulator**

Tick every row of `docs/TEST-CHECKLIST.md` S11. For the "no key" row, rename `secrets.json` to `secrets.off.json`. `.gitignore` covers `secrets*.json`, so the renamed copy cannot be committed. Rename it back and rebuild afterwards.

If every question returns "Gemini error 404" or a quota error, change `GeminiModel` in `secrets.json` as described in SETUP-FIREBASE-GEMINI.md Part 3, step 5. No code change is needed.

- [ ] **Step 9: Commit (check the secret guard output carefully here)**

```bash
git add UnitenStudentAssistant
git status --short UnitenStudentAssistant/Resources/Raw
git diff --cached --name-only | grep -E "(^|/)secrets\.json$|google-services\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "feat: AI student assistant with Gemini and Firestore context"
```

`git status` must show `secrets.sample.json` as added (`A`) and must **not** list `secrets.json`. If the guard prints `STOP`, run `git restore --staged UnitenStudentAssistant/Resources/Raw/secrets.json` and fix `.gitignore` before committing.

---

### Task 12: Final pass, demo rehearsal and docs sync

**Files:**
- Modify: only files that the checks below flag
- Modify: `docs/*.md`, `AGENTS.md`, `DESIGN.md` if the code ended up different from them

**Interfaces:**
- Consumes: everything.
- Produces: an app that passes `docs/TEST-CHECKLIST.md` S12 and the teacher demo script.

- [ ] **Step 1: Search for forbidden code**

Run these from the repo root in Git Bash:

```bash
grep -rnE "<Frame|<ListView|<TableView|<CarouselView|<RefreshView|<SwipeView|DisplayAlert\(|INotifyPropertyChanged|ICommand|CommunityToolkit" UnitenStudentAssistant --include=*.cs --include=*.xaml --exclude-dir=bin --exclude-dir=obj
grep -rnE "#[0-9A-Fa-f]{6}" UnitenStudentAssistant/Views UnitenStudentAssistant/Controls
grep -rn "Color.FromArgb" UnitenStudentAssistant --include=*.cs --exclude-dir=bin --exclude-dir=obj
grep -n "PackageReference" UnitenStudentAssistant/UnitenStudentAssistant.csproj
```

Expected:

- The first three commands print **nothing**.
- The last command prints only `Microsoft.Maui.Controls`, `Microsoft.Extensions.Logging.Debug` and `Plugin.Firebase.Firestore`.

Fix anything else, then rebuild.

- [ ] **Step 2: Clean build**

```bash
dotnet clean UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android
dotnet build UnitenStudentAssistant/UnitenStudentAssistant.csproj -f net10.0-android
```

Expected: `0 Error(s)` and no warnings that mention `obsolete`.

- [ ] **Step 3: Run checklist S12 and walk every link**

Tick every row of `docs/TEST-CHECKLIST.md` S12. Work down the NAVIGATION.md §4 table row by row and tap each element once. Do this on a small emulator (Pixel 4a size) and a large one (Pixel 8 Pro size).

- [ ] **Step 4: Rehearse the teacher demo**

Run the "Teacher demo script" at the end of `docs/TEST-CHECKLIST.md` from start to finish, with the Firebase Console open next to the emulator. Time it: it should take about 5 minutes.

To start from clean seed data, delete the `requests` collection in the Console and restart the app. The seed recreates REQ-1001 to REQ-1006, so the first new request is REQ-1007 again.

- [ ] **Step 5: Sync the docs**

If anything in the code differs from `AGENTS.md`, `DESIGN.md`, `docs/NAVIGATION.md` or `docs/DATA-MODEL.md`, update the doc so it matches. Examples: a renamed control, a changed message, a new field. The docs are what the teacher and future agents read.

- [ ] **Step 6: Final secret check and commit**

```bash
git status
git add UnitenStudentAssistant docs AGENTS.md DESIGN.md README.md .gitignore
git diff --cached --name-only | grep -E "(^|/)secrets[^/]*\.json$|google-services\.json$" | grep -v "secrets\.sample\.json$" && echo "STOP: secret staged" || echo "OK: no secrets staged"
git commit -m "chore: final pass, demo rehearsal and docs sync"
```

Read the `git status` output before adding anything. Do not push unless Abdullah asks.

---

## Done when

- [ ] All 12 tasks are committed, and `git log --oneline` shows one commit per task.
- [ ] Every row of `docs/TEST-CHECKLIST.md` S1–S12 is ticked on the emulator by a person, not only built.
- [ ] Flows A–F in `docs/NAVIGATION.md` §5 run exactly as written.
- [ ] `git log --all --name-only` never lists `secrets.json` or `google-services.json`.
