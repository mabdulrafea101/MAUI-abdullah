# UNITEN Student Assistant — design spec

- **Date:** 2026-10-30
- **Author:** Abdullah (is01084960)
- **Course:** CSNB5123
- **Status:** approved design (from brainstorming). The implementation plan is in `docs/superpowers/plans/2026-10-30-uniten-student-assistant.md`.
- **Source requirements:** `CSNB5123_Milestone_2_Abdullah_FINAL.pdf` (Milestone 2 proposal: features, wireframes, screen map)

## 1. Goal

A simple, attractive Android app built with .NET MAUI. It gives a UNITEN student one place to:

- browse academic programmes
- check the academic calendar
- find university services
- submit service requests and **track their status live**
- ask an AI assistant about UNITEN

It must satisfy the assignment requirements:

- an app icon
- a splash screen that moves to Home by itself
- multiple connected pages
- storage (an online DB)
- an About page with a description, the developer and the version
- original work with no third-party templates

**Non-goals:** login or accounts, an admin panel inside the app, push notifications, iOS/Windows builds, offline-first sync, and any production-grade security.

## 2. Key decisions

| # | Decision | Why |
|---|---|---|
| D1 | .NET 10 MAUI, **Android only** | Latest MAUI; the demo runs on the emulator; Plugin.Firebase has no Windows support |
| D2 | **Cloud Firestore** via Plugin.Firebase.Firestore 4.0.0 | Satisfies "online DB storage". Snapshot listeners give realtime updates for the status demo. |
| D3 | **Status demo = edit `status` in the Firebase Console** | No admin screen to build. The teacher sees the phone update live. |
| D4 | **FR7 AI = Gemini REST with one API key and context stuffing. No RAG.** | The UNITEN data is a few KB, so all of it fits in the system instruction. RAG and vector DBs would be over-engineering for a class project. |
| D5 | **Code-behind per page + static C# service and helper classes. No MVVM, no DI.** | Matches how the lecturer teaches MAUI; easy for a student to explain |
| D6 | Shell `TabBar` with 5 tabs; other pages are registered routes | Matches the PDF's bottom navigation (Home, Services, AI, Requests, Profile) |
| D7 | Splash is a normal ContentPage set in `CreateWindow`, then swapped for `AppShell` | Auto-navigation without the splash staying in the back stack |
| D8 | Custom **CalendarMonthView** built from Grid and Labels | A calendar is required; the PDF forbids third-party templates; built-in MAUI has no month view |
| D9 | UNITEN red + navy theme, light only, built-in controls | Simple but recognisably UNITEN; no UI kits |
| D10 | Single built-in student profile (Abdullah), with no login | Keeps scope small; Edit Profile still shows a write to the DB |
| D11 | Separate **Edit Profile** screen; after Submit the form is cleared and **Tracking is pushed on top** | User decisions during design review |
| D12 | Manual emulator checklist instead of unit tests | UI-heavy class project; every check maps to a wireframe flow |

## 3. Features → screens (FR numbers follow the PDF)

| FR | Feature | Screens |
|---|---|---|
| FR1 | Home dashboard | HomePage |
| FR2 | Academic programmes (+ details) | ProgrammesPage, ProgrammeDetailPage |
| FR3 | Academic calendar | CalendarPage (+ CalendarMonthView) |
| FR4 | University services | ServicesPage, ServiceDetailPage |
| FR5 | Service requests | ServiceRequestsPage, NewRequestPage |
| FR6 | Request tracking (Pending / In Progress / Completed) | RequestTrackingPage, RequestDetailPage |
| FR7 | AI Student Assistant | AiAssistantPage |
| FR8 | Search | SearchPage |
| FR9 | Student profile | ProfilePage, EditProfilePage |
| FR10 | About | AboutPage |
| — | Required by brief | SplashPage, app icon |

That makes **16 pages** in total. Layouts are in DESIGN.md §9; links in docs/NAVIGATION.md.

## 4. Architecture

```
            ┌────────────── Views (16 pages: XAML + code-behind) ──────────────┐
            │ OnAppearing → load; event handlers → act; GoToAsync → navigate   │
            └───────────────┬───────────────────────────────┬──────────────────┘
                            │ static calls                  │
          ┌─────────────────▼─────────────┐     ┌───────────▼────────────────┐
          │ Services/FirestoreService     │     │ Services/GeminiService     │
          │ Services/SeedDataService      │     │ Helpers/AiPromptBuilder    │
          └─────────────────┬─────────────┘     │ Helpers/AppConfig (key)    │
                            │ Plugin.Firebase    └───────────┬────────────────┘
                     ┌──────▼───────┐                        │ HttpClient (HTTPS)
                     │ Cloud        │◄── teacher edits ──    ▼
                     │ Firestore    │    status in Console   Gemini generateContent
                     └──────────────┘
```

- **Models** are plain classes implementing `IFirestoreObject` (docs/DATA-MODEL.md §3).
- **Helpers:** `StatusColors` (pill colours), `AppColors` (reads a colour resource by name), `AiPromptBuilder` (system instruction text) and `AppConfig` (reads `secrets.json` into an `AppSecrets` object).
- **Controls:** `CalendarMonthView`, the only custom control.

### Error handling

- Every await on the network sits inside `try/catch`.
- **Load failures** show `DisplayAlertAsync` with a plain message.
- **Form validation** shows a red label on the form.
- **AI failures** show an AI-style bubble instead of an alert, so the chat keeps going.
- **Splash seeding failures** are ignored so the app always opens.

### Realtime

`RequestTrackingPage` and `RequestDetailPage`:

- subscribe in `OnAppearing`
- dispose in `OnDisappearing`
- marshal updates to the main thread

The Tracking page uses one listener ordered by `createdAt` and filters by status in memory, so no composite index is needed.

## 5. Data

There are five collections: `programmes`, `calendarEvents`, `services`, `requests` and `profile`. The full schema and seed data are in docs/DATA-MODEL.md.

- **Seeding:** first-run seeding with fixed document ids makes the setup repeatable.
- **Status values:** exactly `Pending`, `In Progress` and `Completed`.
- **Request numbers:** `REQ-{1000 + count + 1}`. The first new request is REQ-1007.

## 6. AI assistant (FR7) in detail

1. When the page is first opened, `AiPromptBuilder.BuildAsync()` reads profile, programmes, services and events. It produces a system instruction: the rules (answer only from facts, under 120 words, plain text) followed by the facts and today's date. The result is kept in a private field of the AI page and rebuilt on every visit (`OnAppearing`), so it always reflects the latest profile.
2. **Each question:** `GeminiService.AskAsync(systemText, history)` POSTs to `v1beta/models/{model}:generateContent`.
   - The key goes in the `x-goog-api-key` header.
   - The body holds `system_instruction` plus `contents`: the conversation so far, with roles `user` and `model`, excluding the welcome bubble.
   - The answer is the text of `candidates[0].content.parts`, skipping any part marked `"thought": true` (newer models can return their reasoning as a separate part).
3. **Config:** the key and model name come from `Resources/Raw/secrets.json`, which is git-ignored. The default model is `gemini-3.8-flash`, which can be changed without code.
4. **Not built:** RAG, embeddings, a vector store, chat-history storage in Firestore, streaming and markdown rendering.

## 7. UI

DESIGN.md is the full source. In summary:

- **Colours:** navy `#1B2A4A` and UNITEN red `#DA291C` on light grey `#F5F6F8`, with white cards (radius 12, 1px border, no shadow).
- **Type:** OpenSans at 22/17/15/13/12.
- **Layout:** an 8-pt spacing grid; one red primary button per screen; status pills in amber, blue and green.
- **Controls:** built-in MAUI controls only (no Frame or ListView, as both are obsolete).

## 8. Testing

`docs/TEST-CHECKLIST.md` defines steps S1–S12. Each plan step ends with its checklist rows on the emulator, then a commit. A teacher demo script is included.

## 9. Risks and mitigations

| Risk | Mitigation |
|---|---|
| Plugin.Firebase build problems on Windows (long paths, native packages) | v4 reduces them; the setup doc has the long-path registry fix; keep the solution path short if needed |
| Firestore test-mode rules expire after 30 days | Setup §2.2 sets the expiry to 2027-03-01; the demo checklist reminds you to check |
| Gemini model name retired or free quota hit | The model is set in `secrets.json`; the error shows as a chat bubble |
| API key in the APK | Accepted for a class project and documented honestly in setup Part 3 |
| Status typo in the Console | Constants in code; the demo script uses the exact strings; an unknown status shows grey |
| Two simultaneous submits get the same REQ number | Accepted (single user); documented in DATA-MODEL §4 |
