# DESIGN.md — UNITEN Student Assistant

The visual design system and the layout of every screen. Coding agents must follow this file for anything visual. If a screen is not described here, ask before inventing one.

**Design goal:** simple, clean and friendly. Use flat white cards on a light grey page, a navy header and red only for the main action. Get the look from spacing, rounded corners and colour. Do not use effects, animations or third-party controls.

---

## 1. Colour palette

Define these colours once in `Resources/Styles/Colors.xaml` and use them only through `{StaticResource Name}`. Never type a hex value inside a page.

| Resource key | Hex | Used for |
|---|---|---|
| `Navy` | `#1B2A4A` | Nav bar, Home banner, selected tab, user chat bubble, selected calendar day |
| `UnitenRed` | `#DA291C` | Primary buttons (Submit, Send, Save), event dot on the calendar |
| `UnitenBlue` | `#005DAA` | Links, secondary buttons ("Track my requests"), In Progress text |
| `PageBg` | `#F5F6F8` | Background of every page |
| `CardBg` | `#FFFFFF` | Cards, inputs, tab bar |
| `CardStroke` | `#E3E6EB` | Card stroke, dividers (named `CardStroke` so it does not clash with the `CardBorder` **style**) |
| `TextMain` | `#1F2633` | Titles and body text |
| `TextMuted` | `#6B7280` | Captions, placeholders, dates |
| `AiBubble` | `#EEF0F4` | AI chat bubble background |
| `PendingText` / `PendingBg` | `#B45309` / `#FEF3C7` | "Pending" status pill |
| `ProgressText` / `ProgressBg` | `#005DAA` / `#DBEAFE` | "In Progress" status pill |
| `DoneText` / `DoneBg` | `#15803D` / `#DCFCE7` | "Completed" status pill |

**Rules:**

- Red appears at most once per screen, on the single most important button.
- Status colours are used only on status pills.
- The app is light theme only. Set `Application.Current.UserAppTheme = AppTheme.Light` in `App.xaml.cs` so dark mode on the phone does not break the colours.

## 2. Typography

Use the fonts that ship with the MAUI template: `OpenSansRegular` and `OpenSansSemibold`. Do not add new fonts.

| Style key (in Styles.xaml) | Size | Font | Colour | Used for |
|---|---|---|---|---|
| `TitleLabel` | 22 | Semibold | TextMain | Page heading inside content, detail-page title |
| `SubtitleLabel` | 17 | Semibold | TextMain | Card titles, section headings ("Important dates") |
| `BodyLabel` | 15 | Regular | TextMain | Normal text, descriptions |
| `StrongLabel` | 15 | Semibold | TextMain | Request numbers, event titles, list-item names |
| `CaptionLabel` | 13 | Regular | TextMuted | Dates, faculty, secondary info |
| `SmallLabel` | 12 | Semibold | (set per use) | Status pill text, counts |
| `ErrorLabel` | 13 | Regular | UnitenRed | Form validation message (hidden until needed) |
| `DateDayLabel` | 20 | Semibold | Navy | Day number in the calendar event card's date block |
| `AvatarLabel` | 28 | Semibold | White | Initials inside the Profile circle |

Line height: leave it at the default. For long descriptions, set `LineBreakMode="WordWrap"`.

**Every style key in `Styles.xaml`** (the plan's Task 1 writes them all; do not invent others):

- **Text:** `TitleLabel`, `SubtitleLabel`, `BodyLabel`, `StrongLabel`, `CaptionLabel`, `SmallLabel`, `ErrorLabel`, `DateDayLabel`, `AvatarLabel`
- **Containers:** `CardBorder`, `InputBorder`, `TileBorder`, `StatusPill`, `ChatBubble`, `Divider` (a `BoxView`)
- **Buttons:** `BaseButton` (also the implicit Button style), `PrimaryButton`, `SecondaryButton`, `LinkButton`, `SegmentButton`, `ChipButton`, `CalendarNavButton`
- **Implicit (no key):** `ContentPage`, `Label`, `Entry`, `Editor`, `Picker`, `SearchBar`, `ActivityIndicator`, `Shell`, `Button`

## 3. Spacing, shape and size

- **8-point grid.** Use only 4, 8, 12, 16 or 24 for spacing.
- **Page padding** is `16` on every page.
- **Gap between cards** is `12` (`Spacing="12"` on the stack, or `ItemSpacing` via `LinearItemsLayout`).
- **Card:** `Border`, `BackgroundColor=CardBg`, `Stroke=CardStroke`, `StrokeThickness=1`, `StrokeShape="RoundRectangle 12"`, `Padding="16"`. No shadow. In XAML this is always `Style="{StaticResource CardBorder}"`.
- **Buttons:** height `48`, `CornerRadius="12"`, font 15 Semibold.
  - Primary: red background with white text.
  - Secondary: white background, `BorderColor=UnitenBlue`, `BorderWidth=1`, blue text.
- **Inputs:** wrap each `Entry`, `Editor` and `Picker` in a card-style `Border` with `Padding="12,4"`, so all inputs look the same on Android.
  - Field labels are `CaptionLabel` above the input with a gap of 4.
  - Editor height is `120`.
- **Status pill:** a `Border` with `StrokeShape="RoundRectangle 10"`, `StrokeThickness=0` and `Padding="10,4"`, containing a `SmallLabel`. Colours come from `StatusColors` (see §7).
- **Tap targets** are at least 44 high.

## 4. App icon and splash

- **App icon** (`Resources/AppIcon/appicon.svg` + `appiconfg.svg`): navy rounded background with a white bold "U" in the centre.
  - The background SVG is a navy square.
  - The foreground SVG is the white "U" with roughly 25% padding, so Android's round mask does not cut it.
- **MauiSplashScreen** (`Resources/Splash/splash.svg`): the same white "U" mark. In the `.csproj`, set `Color="#1B2A4A"` so the OS splash is navy.
- **SplashPage** (our own page): navy background. Centred, from top to bottom:
  - the white "U" logo image (120×120)
  - "UNITEN Student Assistant" (22, Semibold, white)
  - "Your campus, in your pocket" (13, white, 80% opacity)
  - a white `ActivityIndicator`

  Because the OS splash is also navy, the change looks seamless.

## 5. Icons

Use our own simple single-colour SVGs in `Resources/Images/`. File names must be lowercase, using only letters, numbers and underscores (a MAUI rule). Size them 24×24 with a `#1B2A4A` stroke; MAUI converts them to PNG automatically, so reference them as `.png` in XAML.

| File | Where |
|---|---|
| `tab_home.svg`, `tab_services.svg`, `tab_ai.svg`, `tab_requests.svg`, `tab_profile.svg` | Tab bar |
| `ic_programmes.svg`, `ic_calendar.svg`, `ic_services.svg`, `ic_requests.svg`, `ic_ai.svg`, `ic_search.svg` | Home tiles |
| `logo_u.svg` | Splash page, About page |

Keep the icons simple: a house, a grid, a chat bubble, a clipboard, a person, a book, a calendar, a magnifier. Draw each one with 1–3 SVG paths, or use plain shapes (rect, circle, line). Do not copy icon packs that have licence terms.

## 6. Shell look

Set these in `Styles.xaml` with implicit `Shell` styles, or as properties on `AppShell.xaml`:

- `Shell.BackgroundColor` and the nav bar are `Navy`; `Shell.TitleColor` and `Shell.ForegroundColor` (back arrow) are white.
- `Shell.TabBarBackgroundColor` is white; `Shell.TabBarTitleColor` (selected) is `Navy`; `Shell.TabBarUnselectedColor` is `TextMuted`.
- Every page has a short `Title`, which becomes the nav-bar text.
- On SplashPage and HomePage, set `Shell.NavBarIsVisible="False"`, because Home draws its own navy banner.

## 7. Status pill helper

`Helpers/StatusColors.cs` is a static class with two methods: `GetText(string status)` and `GetBackground(string status)`.

- Each looks up the matching colour resource by name through `Helpers/AppColors.cs` (`AppColors.Get("PendingText")`), which reads `Application.Current.Resources`. Code-behind that needs a colour (calendar cells, segment buttons, chat bubbles) uses `AppColors.Get(...)` too, never a hex value.
- Status strings are exactly `"Pending"`, `"In Progress"` and `"Completed"` (see docs/DATA-MODEL.md).
- An unknown value falls back to TextMuted text on PageBg.

Pages set the pill in code-behind. In a list `DataTemplate`, the model exposes read-only `StatusTextColor` and `StatusBgColor` properties that call this helper, and the template binds to them.

## 8. Allowed controls

**Allowed:** `Grid`, `VerticalStackLayout`, `HorizontalStackLayout`, `ScrollView`, `Border`, `Label`, `Button`, `ImageButton`, `Image`, `Entry`, `Editor`, `Picker`, `SearchBar`, `CollectionView`, `BindableLayout`, `ActivityIndicator`, `BoxView` (divider lines and the calendar event dot only), `TapGestureRecognizer`, plus our one custom control, `CalendarMonthView`.

**Which list to use:**

- `CollectionView` when the list is the whole page (Programmes, Services, Tracking, AI chat).
- `BindableLayout` on a `VerticalStackLayout` when a short list sits **inside** a `ScrollView` with other content (calendar events, programme majors, search results, recent requests). A `CollectionView` inside a `ScrollView` scrolls badly, so never nest them.

**Not allowed:** `Frame` (obsolete in .NET 9+), `ListView`, `TableView`, `CarouselView`, `RefreshView`, `SwipeView`, Syncfusion/Telerik/DevExpress, CommunityToolkit, custom handlers or renderers, animations, shadows and gradients.

---

## 9. Screen layouts

Notation: `[Card]` is a Border card, `(Red btn)` a primary button, `(Blue btn)` a secondary button and `‹route›` a navigation target. Navigation details are in docs/NAVIGATION.md.

### 9.1 SplashPage

See §4. The page has no nav bar and no tab bar. It is never shown again after it switches to the Shell.

### 9.2 HomePage (tab "Home")

```
┌──────────────────────────────────┐
│ NAVY BANNER (padding 16, 24 top) │
│  Hi, Abdullah                    │  ← 22 white Semibold, name from profile
│  Information Systems · is0108... │  ← 13 white 80%
│  [ SearchBar "Search UNITEN…" ]  │  ← white background, rounded
└──────────────────────────────────┘
  2-column Grid of tiles (gap 12):
  [Programmes] [Calendar]
  [Services ]  [Requests]
  [AI Assist]  [Search  ]
  [Card] "Next up": nearest upcoming calendar event (title + date)
```

- The greeting text is just `Hi, {name}`. No emoji.
- **Tile:** a card 110 high (`TileBorder` style) holding an icon (32), the title (`StrongLabel`) and a one-line caption (`CaptionLabel`). Tapping it navigates (via `TapGestureRecognizer`).
- **"Next up" card:** the first calendar event dated today or later. If there is none, show "No upcoming dates".
- The page is wrapped in a `ScrollView`. The banner sits outside the page padding (full width); everything below it has padding 16.

### 9.3 ProgrammesPage

- A `SearchBar` sits at the top; its `TextChanged` filters the list.
- Below it is a `CollectionView` of cards, each showing:
  - the programme name (`SubtitleLabel`)
  - the faculty (`CaptionLabel`)
  - the duration as a small blue caption ("4 years")
- Empty view: "No programmes match your search."

### 9.4 ProgrammeDetailPage

`ScrollView` → `VerticalStackLayout` (padding 16, spacing 12):

- name (`TitleLabel`)
- faculty (caption)
- a card with two rows: "Duration" with its value, and "Faculty" with its value
- a "About this programme" subtitle followed by the description
- a "Majors / specialisations" subtitle followed by one row per major, prefixed with "•" (a `VerticalStackLayout` with `BindableLayout.ItemsSource` set in code-behind to the list of strings)

### 9.5 CalendarPage

The page is a `ScrollView` → `VerticalStackLayout` (padding 16, spacing 12).

- The heading caption reads "Semester 1 2026/2027" (from the seed data).
- **`CalendarMonthView`** (§10) sits inside one card.
- **"Important dates"** subtitle on the left and a "Show all" link button on the right (hidden until a day is selected).
- Below it, a `VerticalStackLayout` with `BindableLayout` (not a `CollectionView`, because it is inside the `ScrollView`) of event cards:
  - On the left is a date block: day number (20 Semibold, navy) above the month abbreviation (12, muted), 48 wide.
  - On the right are the title (Semibold 15) and `TypeAndDescription` (caption), e.g. "Exam · Final examinations week 1".
- Tapping a day calls `SelectDate` on the calendar and shows only that day's events; the subtitle changes to "Events on 14 Oct" and "Show all" appears. "Show all" calls `ClearSelection()` and returns to the whole month.
- Changing month reloads the list for the new month.
- Empty label (a caption under the list, shown in code-behind when the list is empty): "No events on this day." or "No events this month."

### 9.6 ServicesPage (tab "Services")

A `CollectionView` of cards. Each card has the service icon on the left (`ic_services.png`, 32) with the name (Subtitle) and a one-line description (Caption) beside it. A muted "›" sits on the right.

### 9.7 ServiceDetailPage

- name (Title)
- description (Body)
- a card with three rows, each a muted caption label above a body value: Location, Hours, Contact (email/phone)
- `(Red btn) "Make a request"` → opens the New Request page with this service preselected

### 9.8 ServiceRequestsPage (tab "Requests")

- `(Red btn) "+ New Request"` across the full width
- `(Blue btn) "Track my requests"`
- a "Recent requests" subtitle
- the 3 latest requests as cards (a `BindableLayout` inside the page's `ScrollView`). Each card shows the request number (Semibold) with the status pill on the right, and below it the subject (Body) and date (Caption). Tapping a card opens Tracking on that card's status (see docs/NAVIGATION.md §4).
- Empty: "You have no requests yet."

### 9.9 NewRequestPage

`ScrollView` with padding 16 and spacing 16. Fields:

1. "Service type": a `Picker` listing the service names
2. "Subject": an `Entry` with `MaxLength=80`
3. "Request details": an `Editor` (height 120) with `MaxLength=500`

Then a validation `Label` (red, 13, hidden until needed), then `(Red btn) "Submit request"`.

- While saving, the button is disabled and a small `ActivityIndicator` shows beside it.
- **Validation messages:**
  - "Please choose a service."
  - "Please enter a subject."
  - "Please describe your request (at least 10 characters)."

### 9.10 RequestTrackingPage

- **A row of 3 equal buttons** acting as a segmented control: "Pending (2)", "In Progress (1)", "Completed (3)".
  - The selected one is a navy background with white text; the others are white with navy text and a border.
  - The counts update live.
- **Below:** a `CollectionView` of request cards in the same layout as §9.8, ordered newest first.
- **Empty:** "No pending requests." (the word follows the selected tab)
- The list changes by itself when the status is edited in the Firebase Console. This is the realtime demo.

### 9.11 RequestDetailPage

- request number (Title)
- the status pill (`StatusPill` style with a larger padding of 14,6, holding a `SmallLabel`), left-aligned
- the subject (Subtitle)
- a card with these rows: Service, Submitted (createdAt, `dd MMM yyyy, h:mm tt`), Last updated (updatedAt) and Details (body)
- a muted caption at the bottom: "Status updates automatically."

The page updates live too.

### 9.12 AiAssistantPage (tab "AI")

```
Grid rows: * (chat) / Auto (thinking) / Auto (chips) / Auto (input bar)
- CollectionView of bubbles (Style="ChatBubble": radius 16, padding 12,
  max width 300). User: right-aligned, Navy bg, white text.
  AI: left-aligned, AiBubble bg, TextMain. Error bubbles: AI side, red text.
- "Thinking…" row (ActivityIndicator + caption), visible only while waiting.
- HorizontalStackLayout of 3 suggestion chips. Each chip is a Button with
  Style="ChipButton" (radius 18, height 36, blue text, white bg, blue
  border): "When does the semester start?", "How do I contact
  Finance?", "Which programmes are offered?" — click = send that text.
- Input bar (white): a Divider BoxView on top, then Entry "Ask about
  UNITEN…" + (Red btn) "Send" (width 72).
```

- **First message**, an AI bubble: "Hi! I'm your UNITEN assistant. Ask me about programmes, dates or services."
- **Chip row:** only show it while the chat contains just the welcome bubble. Put it inside a horizontal `ScrollView` so it fits narrow screens.
- **Scrolling:** after each new message, call `ChatList.ScrollTo(messages.Count - 1)`.
- **No key:** when the student sends a question and `secrets.json` has no key, the page adds the error bubble "AI is not set up yet. Add your Gemini key to secrets.json." instead of calling Gemini.

### 9.13 SearchPage

- A `SearchBar` at the top, pre-filled from the `q` parameter. Its `TextChanged` reruns the search.
- Results are grouped as plain sections, not a grouped `CollectionView`. Each heading appears only if its section has results:
  - a "Programmes" subtitle + its result cards
  - a "Services" subtitle + its result cards
  - a "Calendar" subtitle + its result cards

  Each section is a `VerticalStackLayout` with `BindableLayout` whose `ItemsSource` is set in code-behind. This keeps it simple, since there are only ever a few results. Tapping a result opens its detail page (calendar results open `calendar?date=yyyy-MM-dd`).
- With no text: "Type to search programmes, services and dates."
- **What is searched:** case-insensitive `Contains` on:
  - programme name, faculty and description
  - service name and description
  - event title and description

  Requests are not searched.
- With text and no results: "No results for “xyz”."

### 9.14 ProfilePage (tab "Profile")

- Centred at the top:
  - a navy circle `Border` (`StrokeShape="RoundRectangle 40"`, 80×80) holding the initials ("AB", 28, white)
  - the name (Title)
  - the student ID (Caption)
- A card with these rows: Programme, Email, Phone. Each row is a caption label above a body value, with a thin `CardStroke` divider (`BoxView` height 1) between rows.
- `(Blue btn) "Edit profile"` → editprofile
- `(Blue btn) "About this app"` → about

### 9.15 EditProfilePage

Fields, each with a caption label:

- Name (`Entry`)
- Student ID (`Entry`, `IsReadOnly=True`, muted text)
- Programme (`Picker` of programme names)
- Email (`Entry`, `Keyboard=Email`)
- Phone (`Entry`, `Keyboard=Telephone`)

Below them is the validation label, then `(Red btn) "Save changes"`, then `(Blue btn) "Cancel"`.

### 9.16 AboutPage

Everything is centred:

- `logo_u.png` (72×72) centred on a 96×96 navy rounded square (`Border`, `StrokeShape="RoundRectangle 24"`)
- "UNITEN Student Assistant" (Title)
- "Version 1.0" (Caption), from `AppInfo.Current.VersionString`
- a card holding the description (Body): "A simple mobile app that helps UNITEN students find programmes, check the academic calendar, contact university services, submit and track service requests, and ask an AI assistant."
- a card with two rows: Developer → Abdullah (is01084960); Course → CSNB5123
- the caption "Built with .NET MAUI, Firebase Firestore and Google Gemini."

---

## 10. CalendarMonthView (the only custom control)

The control is `Controls/CalendarMonthView.xaml` plus `.xaml.cs`, a `ContentView`. Build it with plain Grid, Label and Button.

**Layout:**

```
Row 0: Grid(3 cols)  [ ‹ ]   October 2026   [ › ]     (buttons 40x40, transparent, navy text)
Row 1: Grid(7 cols)  Sun Mon Tue Wed Thu Fri Sat      (12, muted, centred)
Row 2: Grid(7 cols x 6 rows) of day cells, row height 44
```

**Day cell:** a `VerticalStackLayout` holding a day `Label` (15, centred) and a 6×6 dot (`BoxView`, `CornerRadius=3`, `UnitenRed`). The cell is wrapped in a `Border` (`StrokeShape="RoundRectangle 10"`, `StrokeThickness=0`) with a `TapGestureRecognizer`. Cells are created in C# inside `BuildDays()`, because there are 42 of them.

| State | Look |
|---|---|
| Day in another month | Leave the cell empty (do not draw it) |
| Normal day | Transparent bg, TextMain |
| Today | Text `UnitenBlue`, Semibold |
| Has event(s) | Red dot visible under the number |
| Selected | `Navy` bg, white text; the dot stays visible but white |

**Public API (keep it exactly this small):**

```csharp
public DateTime DisplayMonth { get; }                  // first day of shown month
public void SetEvents(List<CalendarEvent> events)       // redraws dots
public void GoToMonth(DateTime month)                    // used when opened with ?date=
public void SelectDate(DateTime date)                    // highlight one day (no event raised)
public void ClearSelection()                             // remove the highlight ("Show all")
public event EventHandler<DateTime>? DateSelected;      // raised on day tap
public event EventHandler<DateTime>? MonthChanged;      // raised on ‹ / › (first day of new month)
```

**How it works:** a private `BuildDays()` method clears the day grid's `Children` and rebuilds them. It runs on every month change, on `SetEvents` and on each selection change. Rebuilding 42 cells this way is fast enough and easy to read.

- The first cell column is `(int)firstDay.DayOfWeek`, so the week starts on Sunday.
- The selected date resets to "none" whenever the month changes (by ‹ / › or `GoToMonth`).
- `GoToMonth` does **not** raise `MonthChanged`; the page that called it reloads its own list.

## 11. Copy and tone

- Use short, friendly sentence case: "Submit request", not "SUBMIT REQUEST".
- No emoji in UI text.
- Every error is shown with `await DisplayAlertAsync("Something went wrong", "<plain explanation>", "OK")`. In .NET 10 `DisplayAlert` is obsolete; always use `DisplayAlertAsync`.
