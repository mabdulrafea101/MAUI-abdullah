# SETUP-FIREBASE-GEMINI.md — one-time setup for Firebase and the Gemini key

This assumes Visual Studio Community with the .NET MAUI workload is already installed, and an Android emulator exists. MAUI installation is **not** covered here.

All three parts take about 20 minutes.

---

## Part 1: Create the MAUI project (once)

1. **Create the project.** In Visual Studio, choose Create a new project → **.NET MAUI App** and set:
   - Name: `UnitenStudentAssistant`
   - Location: this repo folder (`MAUI-abdullah`)
   - Tick **Place solution and project in the same directory**, so the project ends up at `MAUI-abdullah/UnitenStudentAssistant/UnitenStudentAssistant.csproj`
   - Framework: **.NET 10**
   - If the wizard offers "Include sample content", leave it **unticked**
2. **Remove the targets we do not use.** Open `UnitenStudentAssistant.csproj` and change the target line to:

   ```xml
   <TargetFrameworks>net10.0-android</TargetFrameworks>
   ```

   Delete any other `<TargetFrameworks ... Condition=...>` lines (iOS, MacCatalyst, Windows). Plugin.Firebase supports neither Windows nor Mac.
3. **Set the identity values.** In the same file:

   ```xml
   <ApplicationTitle>UNITEN Student Assistant</ApplicationTitle>
   <ApplicationId>com.abdullah.unitenstudentassistant</ApplicationId>
   <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
   <ApplicationVersion>1</ApplicationVersion>
   ```

   Also set `SupportedOSPlatformVersion` for android to **23.0**, which is Plugin.Firebase 4's minimum.
4. **Delete the template's sample code.** Remove `MainPage.xaml` and `MainPage.xaml.cs`, and the dotnet_bot image. The plan replaces them.
5. **Optional: avoid long-path build errors.** If a build later fails with "path too long", run this once in an **admin** PowerShell, then restart Visual Studio:

   ```powershell
   New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force
   ```

## Part 2: Firebase (Firestore)

### 2.1 Create the Firebase project

1. Go to <https://console.firebase.google.com> → **Create a project**.
   - Name: `uniten-student-assistant`.
   - Google Analytics: **off** (not needed).
2. Open **Build → Firestore Database → Create database**, then:
   - **Edition:** Standard (if asked)
   - **Location:** `asia-southeast1 (Singapore)`, the closest to Malaysia. It cannot be changed later.
   - **Rules:** choose **Start in test mode**

### 2.2 Keep test mode working

Test-mode rules contain an expiry date about 30 days ahead. After that date every read fails. Before the demo, open **Firestore → Rules** and set the date past the end of the semester:

```
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /{document=**} {
      allow read, write: if request.time < timestamp.date(2027, 3, 1);
    }
  }
}
```

Then click **Publish**.

These rules let anyone with the project id read and write. That is acceptable for a class demo with fake data. **Do not put real personal data in this database.**

### 2.3 Register the Android app and get `google-services.json`

1. In **Project settings (gear) → General → Your apps**, click the **Android** icon.
2. Fill in the form:
   - **Android package name:** `com.abdullah.unitenstudentassistant`. It must match `ApplicationId` exactly.
   - **App nickname:** UNITEN Student Assistant
   - **SHA-1:** leave empty (only needed for Google sign-in, which we do not use)
3. Download **`google-services.json`** and put it in the project folder, next to the `.csproj`.
4. Skip the console's "Add Firebase SDK" Gradle steps. They are for Android Studio, and NuGet handles this for us.
5. Add this to the `.csproj`:

   ```xml
   <ItemGroup>
     <GoogleServicesJson Include="google-services.json" />
   </ItemGroup>
   ```

   (In Solution Explorer, the file's Build Action then shows **GoogleServicesJson**.)
6. Add the NuGet package. Use Manage NuGet Packages, or the Package Manager Console:

   ```
   Install-Package Plugin.Firebase.Firestore -Version 4.0.0
   ```

### 2.4 Initialise Firebase in `MauiProgram.cs`

This uses the v4 API, which needs an "activity locator" on Android:

```csharp
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
                events.AddAndroid(android => android.OnCreate((activity, _) =>
                    CrossFirebase.Initialize(activity, () => Platform.CurrentActivity!)));
#endif
            });

        return builder.Build();
    }
}
```

### 2.5 First run check

Run the app on the emulator with internet on. In the Firebase Console → Firestore → Data you should see five collections appear: `programmes`, `calendarEvents`, `services`, `requests`, `profile`. These are created by `SeedDataService`.

## Part 3: Gemini API key (for the AI Student Assistant)

1. Go to <https://aistudio.google.com/apikey> and sign in with a Google account → **Create API key**. If asked, pick or create a Google Cloud project; the Firebase project is fine.
2. The free tier is enough for a class demo. Do **not** enable billing.
3. In the project, copy `Resources/Raw/secrets.sample.json` to `Resources/Raw/secrets.json`, and paste the key:

   ```json
   { "GeminiApiKey": "AIza...your key...", "GeminiModel": "gemini-3.8-flash" }
   ```

4. `secrets.json` is in `.gitignore`. Check with `git status` that it is **not** listed before every commit.
5. **If you need a different model:** if Gemini replies with a "model not found" error or a quota error, change `GeminiModel` to another current stable model shown on <https://ai.google.dev/gemini-api/docs/models>, such as `gemini-3.6-flash`. No code change is needed.

### What the app sends

The app makes one HTTPS request per question:

```
POST https://generativelanguage.googleapis.com/v1beta/models/{GeminiModel}:generateContent
Header: x-goog-api-key: {GeminiApiKey}
Body:   { "system_instruction": { "parts": [ { "text": "<UNITEN facts from Firestore>" } ] },
          "contents": [ { "role": "user", "parts": [ { "text": "When does the semester start?" } ] } ] }
Answer: the text of candidates[0].content.parts (parts marked "thought": true are skipped)
```

There is no RAG, no vector database and no second key. The full explanation is in docs/DATA-MODEL.md §6.

**Honest limitation:** the key ships inside the APK, so someone who unpacks the APK could read it. That is acceptable for a class project on a free-tier key. A production app would call Gemini from a server instead. Mention this if the lecturer asks.
