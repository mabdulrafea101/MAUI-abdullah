# TEST-CHECKLIST.md — manual checks on the Android emulator

There is no automated test project; this is a UI class project. After each plan task, run the app on the emulator (F5 in Visual Studio) and tick the rows for that task. **Section S*n* belongs to plan Task *n*. A task is done only when its rows pass.** Builds that compile but fail a row are not done.

**Before testing:** the emulator has internet, Firestore rules are not expired (see setup §2.2), and `secrets.json` exists for the AI rows.

## Task-by-task checks

### S1: Project skeleton, theme, icon, splash, Shell

- [ ] `dotnet build -f net10.0-android` finishes with 0 errors.
- [ ] The app icon in the emulator launcher is navy with a white "U".
- [ ] On launch the OS splash is navy, followed by our SplashPage (logo, title, spinner) for about 2 s.
- [ ] The app then shows the Shell with 5 tabs: Home, Services, AI, Requests, Profile, each with its icon. Each tab opens its placeholder page with the right title.
- [ ] The selected tab is navy, the others grey; the nav bar is navy with white text.
- [ ] Pressing Android Back on Home closes the app. It does **not** return to the splash.
- [ ] With the emulator in dark mode, the app still uses the light colours.

### S2: Firebase connection, models and seed

- [ ] The first run creates the 5 collections in the Firestore Console, with the counts from DATA-MODEL §5 (5 programmes, 12 events, 5 services, 6 requests, 1 profile).
- [ ] In the Console, `requests/req-1004` has `status` = `In Progress`, and `createdAt`/`updatedAt` are **timestamp** fields (not strings).
- [ ] A second run creates **no** duplicates (the counts stay the same).
- [ ] Airplane mode on, then start the app: the splash still moves to Home within about 8 s, with no crash.

### S3: Programmes and Programme Detail (Flow A)

Open Programmes from the Home placeholder's temporary button (Task 1) until Home is built in Task 6.

- [ ] The list shows 5 programmes, sorted by name, each with faculty and "4 years".
- [ ] Typing "cyber" leaves only the Cyber Security programme. Typing "zzz" shows the empty message.
- [ ] Tapping a card opens its detail: name, faculty, duration, description and a bullet list of majors.
- [ ] Back returns to the list with the search text still there.

### S4: Services and Service Detail

- [ ] The Services tab lists 5 services, each with icon, name, one-line description and "›".
- [ ] Tapping Finance shows its description, location, hours and contact.
- [ ] "Make a request" opens the New Request page (still a placeholder until Task 7). Back returns to Service Detail.

### S5: Calendar (Flow C)

Open Calendar from the Home placeholder's temporary button until Home is built.

- [ ] The current month shows with the correct first weekday. Today's number is blue.
- [ ] Days with events show a red dot (October 2026: 9, 20; November 2026: 9, 16, 27; December 2026: 11, 25).
- [ ] With no day selected, "Important dates" lists every event of the shown month.
- [ ] Tapping 9 Nov makes it navy, the subtitle reads "Events on 9 Nov", only that event is listed and "Show all" appears.
- [ ] Tapping a day with no events shows "No events on this day."
- [ ] "Show all" removes the highlight and lists every event for the month again.
- [ ] › and ‹ change the month and the list. The selection is cleared.

### S6: Home

- [ ] The banner reads "Hi, Abdullah" with the programme and student ID underneath. The temporary buttons from Task 1 are gone.
- [ ] The 6 tiles show in 2 columns with icons. Each opens the page in NAVIGATION §4:
  - [ ] Programmes and Calendar are pushed, and the Home tab stays selected. Back returns to Home.
  - [ ] Services, Requests and AI switch tabs.
  - [ ] Search pushes the Search page (a placeholder until Task 10).
- [ ] The "Next up" card shows the first event dated today or later (on 30 Oct 2026 that is "Mid-semester break starts", 09 Nov 2026).
- [ ] Tapping "Next up" opens Calendar on November 2026 with the 9th selected.

### S7: Service Requests tab and New Request (Flow B, part 1)

- [ ] The Requests tab shows "+ New Request" (red), "Track my requests" (blue) and the 3 newest requests (REQ-1006, 1005, 1004) with coloured status pills.
- [ ] **Submit with nothing filled:** "Please choose a service." shows in red and nothing is saved.
- [ ] Service chosen, subject empty: "Please enter a subject." shows.
- [ ] Subject filled but details under 10 characters: the details message shows. The typed text is kept.
- [ ] **A valid submit:**
  - [ ] The button disables and the small spinner shows while saving.
  - [ ] An alert titled "Thank you" reads "Request REQ-1007 submitted."
  - [ ] After OK, the form is cleared and the Tracking page is pushed on top (a placeholder until Task 8).
  - [ ] The Firestore Console shows the new document with `requestNo` REQ-1007, `status` Pending and timestamp dates.
- [ ] Back from Tracking shows the cleared form. Back again shows the Requests tab, whose recent list now starts with REQ-1007.
- [ ] Double-tapping Submit quickly creates only **one** request.
- [ ] From Services → Finance → "Make a request", New Request opens with **Finance** preselected.

### S8: Request Tracking and Request Detail: realtime (Flow B, part 2)

- [ ] "Track my requests" opens Tracking on **Pending**, with counts Pending (3), In Progress (1) and Completed (3) after REQ-1007 was added.
- [ ] Switching the status buttons filters the list, newest first. A tab with no requests shows "No pending requests." / "No in progress requests." / "No completed requests."
- [ ] On the Requests tab, tapping a recent card opens Tracking with **that card's status** selected. After S7, all three recent cards are Pending. After the live-detail row below, tap the changed card to see it open on **In Progress**.
- [ ] Tapping a card opens Request Detail with the correct number, pill, subject, service, dates and details.
- [ ] **Live detail:** with Request Detail open, change `status` to `In Progress` in the Firebase Console. The pill turns blue within about 2 s without touching the phone.
- [ ] **Live list:** Back to Tracking. The card has moved to In Progress and the counts updated. Change it in the Console to `Completed` while Tracking is open, and the list updates live.
- [ ] After New Request's submit, the pushed Tracking page shows Pending with REQ-1008 (or the next number) at the top.
- [ ] Leaving and reopening Tracking several times causes no duplicate updates and no crash, which shows the listener is disposed.

### S9: Profile, Edit Profile, About (Flow D)

- [ ] Profile shows the "AB" circle, the name, ID, programme, email and phone, with thin dividers.
- [ ] Edit profile shows the fields filled, with Student ID read-only and the Programme picker preselected.
- [ ] Clearing Name and tapping Save shows a validation message, and nothing is saved.
- [ ] Changing the phone and tapping Save shows the alert, returns to Profile and displays the new phone. The Firestore Console shows the new phone too.
- [ ] Cancel returns without saving.
- [ ] Home's greeting reflects a changed name.
- [ ] About shows the logo, the name, "Version 1.0", the description, a Developer row (Abdullah (is01084960)) and a Course row (CSNB5123).

### S10: Search (Flow F)

- [ ] Empty text shows the hint message.
- [ ] "fee" shows a **Services** section (Finance, because its description says "Tuition fees") and a **Calendar** section (Fee payment deadline), and **no** Programmes heading. Each section heading only shows when it has results.
- [ ] "xyz" shows "No results for “xyz”."
- [ ] Tapping a programme result opens Programme Detail; a service result opens Service Detail; an event result opens Calendar on that month with the day selected (e.g. "Fee payment deadline" → December 2026, the 11th).
- [ ] On Home, typing "library" in the SearchBar and pressing search opens Search with "library" filled in. Library → "Make a request" → New Request with Library preselected.

### S11: AI Student Assistant (Flow E)

- [ ] The welcome bubble and 3 suggestion chips show.
- [ ] Tapping "When does the semester start?" shows the user bubble on the right (navy), then "Thinking…", then an AI bubble on the left that mentions **28 September 2026**.
- [ ] Asking "How do I contact Finance?" gives an answer that includes the Finance location or contact from the seed data.
- [ ] Asking something unrelated ("Who won the World Cup?") gets a polite "I don't know, I only help with UNITEN" style answer.
- [ ] The chips hide after the first question. The list scrolls to the newest bubble. Send is disabled while waiting.
- [ ] Airplane mode, then ask: a friendly error bubble (red text) appears, with no crash.
- [ ] Rename `secrets.json` (e.g. to `secrets.off.json`), rebuild, then ask a question: the "AI is not set up yet…" bubble appears in red text and nothing crashes. Rename it back and rebuild afterwards.

### S12: Final pass

- [ ] Every row in NAVIGATION §4 has been tapped at least once.
- [ ] No screen has text cut off or overlapping on a small emulator (Pixel 4a-sized) or a large one (Pixel 8 Pro-sized).
- [ ] No `Frame`, `ListView`, `DisplayAlert(` (without Async), MVVM or extra NuGet packages are in the code: search the solution.
- [ ] No hex colour appears in any file under `Views/` or `Controls/`.
- [ ] `git status` does not list `secrets.json` or `google-services.json`.
- [ ] The teacher demo script below runs from start to finish without a hitch.

---

## Teacher demo script (about 5 minutes)

**Before class:**

- Firestore rules are not expired.
- The emulator is already booted.
- The app is installed.
- The Firebase Console is open on `requests` in a browser next to the emulator.

1. **Launch:** show the splash, which auto-moves to Home. Point out the icon in the launcher. (Requirements: icon, splash screen.)
2. **Home:** the greeting comes from Firestore. Tap Programmes, search "cyber" and open the detail. Go back.
3. **Calendar:** show the red dots, tap 9 Nov and show "Mid-semester break starts", then change month.
4. **Services:** open Finance, then tap "Make a request". The picker is preselected.
5. **Submit:** type a subject and details, then Submit. The alert shows the REQ number, and Tracking opens on Pending.
6. **The realtime demo:**
   - Open the new request's detail.
   - In the Firebase Console, change `status` to **In Progress**. The pill turns blue live.
   - Go back to Tracking: the counts moved.
   - Change it to **Completed** in the Console while Tracking is open, and it moves again.
7. **AI:** tap the chip "When does the semester start?". Explain: "No RAG. The app reads our Firestore data, puts it into Gemini's system instruction and calls one REST API with one key."
8. **Profile:** Edit profile, change the phone, then Save. Show the change in the Console. Open **About** (description, developer, version). (Requirement: About page.)
9. **Storage:** show the 5 collections in the Firestore Console. (Requirement: online database storage.)
