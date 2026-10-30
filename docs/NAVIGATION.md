# NAVIGATION.md — screens, routes and flow

This is the single source of truth for how screens connect. Every `GoToAsync` call in the code must match a row in this file. If you add or change a link, update this file first.

## 1. App start

```
OS launch → MauiSplashScreen (navy, from .csproj)
          → App.CreateWindow() returns new Window(new SplashPage())
          → SplashPage.OnAppearing:
                Task seeding     = SeedDataService.SeedIfEmptyAsync()   (errors caught inside, never thrown)
                Task minimumTime = Task.Delay(2000)                     (so the logo is seen)
                Task maximumTime = Task.Delay(8000)                     (never wait more than 8 s)
                await Task.WhenAny(seeding, maximumTime)
                await minimumTime
                Application.Current!.Windows[0].Page = new AppShell();
          → AppShell opens on the first tab: Home
```

**Rules:**

- SplashPage is **not** a Shell page. It is never on the back stack, so pressing Back on Home closes the app as expected.
- If seeding fails (for example, no internet), the app still opens. Pages then show their own load errors.
- If the network is very slow, the 8 s cap opens the app anyway; seeding keeps running in the background and finishes on its own.

## 2. Shell structure

`AppShell.xaml` has a single `TabBar` with five tabs. Each tab is a `ShellContent` with a `Route`, and the tab order follows the PDF's bottom navigation.

| # | Tab title | Route | Page | Icon |
|---|---|---|---|---|
| 1 | Home | `home` | HomePage | tab_home.png |
| 2 | Services | `services` | ServicesPage | tab_services.png |
| 3 | AI | `ai` | AiAssistantPage | tab_ai.png |
| 4 | Requests | `requests` | ServiceRequestsPage | tab_requests.png |
| 5 | Profile | `profile` | ProfilePage | tab_profile.png |

Register the pushed pages once in the `AppShell` constructor:

```csharp
Routing.RegisterRoute("programmes",      typeof(ProgrammesPage));
Routing.RegisterRoute("programmedetail", typeof(ProgrammeDetailPage));
Routing.RegisterRoute("calendar",        typeof(CalendarPage));
Routing.RegisterRoute("search",          typeof(SearchPage));
Routing.RegisterRoute("servicedetail",   typeof(ServiceDetailPage));
Routing.RegisterRoute("newrequest",      typeof(NewRequestPage));
Routing.RegisterRoute("tracking",        typeof(RequestTrackingPage));
Routing.RegisterRoute("requestdetail",   typeof(RequestDetailPage));
Routing.RegisterRoute("editprofile",     typeof(EditProfilePage));
Routing.RegisterRoute("about",           typeof(AboutPage));
```

**Rules:**

- **Relative routes** (no `//`) push a page on top of the **current tab's** stack. The tab bar stays visible and the current tab stays highlighted. Back (the nav-bar arrow or the Android back button) pops it.
- **Absolute routes** (`//services`) switch tabs. Use them only from the Home tiles that point at another tab.

## 3. Route parameters

Pages receive parameters with `[QueryProperty]` on the page class, then use the value in `OnAppearing`. The parameter is set *before* `OnAppearing` runs.

| Route | Parameter | Type | Meaning |
|---|---|---|---|
| `programmedetail` | `id` | string | Firestore document id of the programme |
| `calendar` | `date` | string, optional | `yyyy-MM-dd`; the calendar opens on that month with that day selected |
| `search` | `q` | string, optional | starting search text |
| `servicedetail` | `id` | string | Firestore document id of the service |
| `newrequest` | `service` | string, optional | service name to preselect in the Picker |
| `tracking` | `status` | string, optional | `Pending` / `In Progress` / `Completed`; the default is `Pending` |
| `requestdetail` | `id` | string | Firestore document id of the request |

Escape values that contain spaces or symbols: `$"tracking?status={Uri.EscapeDataString(status)}"`. Shell decodes them before setting the property.

## 4. Complete link table

Every tappable element in the app is listed here.

| From | Element | Action | Goes to |
|---|---|---|---|
| Splash | (automatic, ~2 s) | replace window page | AppShell → Home tab |
| Home | SearchBar, search key pressed | `GoToAsync($"search?q={q}")` | Search |
| Home | Programmes tile | `GoToAsync("programmes")` | Programmes |
| Home | Calendar tile | `GoToAsync("calendar")` | Calendar |
| Home | Services tile | `GoToAsync("//services")` | Services tab |
| Home | Requests tile | `GoToAsync("//requests")` | Requests tab |
| Home | AI Assistant tile | `GoToAsync("//ai")` | AI tab |
| Home | Search tile | `GoToAsync("search")` | Search |
| Home | "Next up" card | `GoToAsync($"calendar?date={yyyy-MM-dd}")` | Calendar, day selected |
| Programmes | programme card | `GoToAsync($"programmedetail?id={id}")` | Programme Detail |
| Programme Detail | back | `..` | Programmes |
| Calendar | day cell | filter the list on the same page | (stays) |
| Calendar | ‹ / › | change month on the same page | (stays) |
| Calendar | "Show all" | clear the day filter | (stays) |
| Services | service card | `GoToAsync($"servicedetail?id={id}")` | Service Detail |
| Service Detail | "Make a request" | `GoToAsync($"newrequest?service={name}")` | New Request (Services tab stack) |
| Requests | "+ New Request" | `GoToAsync("newrequest")` | New Request |
| Requests | "Track my requests" | `GoToAsync("tracking")` | Tracking (Pending) |
| Requests | recent request card | `GoToAsync($"tracking?status={status}")` | Tracking on that status |
| New Request | "Submit request" (valid) | save → alert → clear form → `GoToAsync("tracking?status=Pending")` | Tracking on top of the cleared form |
| Tracking | status buttons | switch the filter on the same page | (stays) |
| Tracking | request card | `GoToAsync($"requestdetail?id={id}")` | Request Detail |
| Request Detail | back | `..` | Tracking |
| AI | chip / Send | call Gemini on the same page | (stays) |
| Search | programme result | `GoToAsync($"programmedetail?id={id}")` | Programme Detail |
| Search | service result | `GoToAsync($"servicedetail?id={id}")` | Service Detail |
| Search | calendar result | `GoToAsync($"calendar?date={yyyy-MM-dd}")` | Calendar, day selected |
| Profile | "Edit profile" | `GoToAsync("editprofile")` | Edit Profile |
| Profile | "About this app" | `GoToAsync("about")` | About |
| Edit Profile | "Save changes" (valid) | save → alert → `GoToAsync("..")` | Profile (reloads in OnAppearing) |
| Edit Profile | "Cancel" | `GoToAsync("..")` | Profile, unchanged |
| About | back | `..` | Profile |

## 5. The main flows (match the wireframe in the PDF)

**Flow A: Browse programmes**

Home → Programmes → (type "comp") → tap "Computer Science" → Programme Detail → Back → Programmes → Back → Home

**Flow B: Submit and track a request (the realtime demo)**

1. Requests tab → "+ New Request".
2. Pick "Finance", type a subject and details, then tap Submit.
3. An alert reads "Request REQ-1007 submitted". Tap OK.
4. The form clears, and Tracking (Pending) is pushed on top. The new REQ shows at the top.
5. Tap it to open Request Detail, where the status is "Pending".
6. The teacher changes `status` to "In Progress" in the Firebase Console. The pill changes on its own within about 1 s.
7. Back → Tracking: the card has moved to the "In Progress" tab and the counts have updated.
8. Back → the empty New Request form → Back → the Requests tab.

**Flow C: Calendar**

Home → Calendar → tap a day with a red dot → that day's events show → › next month → the list updates → "Show all"

**Flow D: Profile**

Profile tab → Edit profile → change the phone → Save → alert → Profile shows the new phone → About → Back → Profile

**Flow E: AI**

AI tab → tap the chip "When does the semester start?" → "Thinking…" → answer bubble → type a question → Send → answer

**Flow F: Search**

Home → type "library" in the SearchBar → Search opens with "library" → tap the Library service → Service Detail → "Make a request" → New Request with "Library" preselected

## 6. Data refresh rules (so screens never show stale data)

| Page | When data loads |
|---|---|
| Home | `OnAppearing`: profile name and the "Next up" event |
| Programmes, Services, Calendar | `OnAppearing`, only if the list is still empty (the data rarely changes) |
| Requests tab | `OnAppearing`: the latest 3 requests, every time |
| Tracking | Listener started in `OnAppearing`, disposed in `OnDisappearing` |
| Request Detail | Listener started in `OnAppearing`, disposed in `OnDisappearing` |
| Profile | `OnAppearing`, every time, so edits show after returning |
| Edit Profile | `OnAppearing`: fills the fields from Firestore |
| New Request | `OnAppearing`: fills the Picker with services (once) and applies the `service` parameter |
| Search | Loads programmes, services and events once, then filters in memory |

## 7. Back-stack examples

| Situation | Stack (bottom → top) | Back goes to |
|---|---|---|
| After submitting a request | Requests › New Request (cleared) › Tracking | the cleared form, then Requests |
| Detail from Search | Home › Search › Programme Detail | Search, then Home |
| Make a request from a service | Services › Service Detail › New Request | Service Detail |
| Switching tabs | Each tab keeps its own stack | — |

Tapping the current tab again pops that tab to its root. This is the default Shell behaviour.
