# UNITEN Student Assistant

A .NET MAUI Android app for CSNB5123, built by Abdullah (is01084960). It covers programmes, the academic calendar, university services, service requests with **live** status tracking via Cloud Firestore, and an AI assistant powered by Google Gemini.

## Read in this order

1. [Design spec](docs/superpowers/specs/2026-10-30-uniten-student-assistant-design.md): what we build and why
2. [AGENTS.md](AGENTS.md): code rules (code-behind, no MVVM, allowed controls)
3. [DESIGN.md](DESIGN.md): colours, fonts and every screen's layout
4. [docs/NAVIGATION.md](docs/NAVIGATION.md): routes, taps and back-stack behaviour
5. [docs/DATA-MODEL.md](docs/DATA-MODEL.md): Firestore collections, models and seed data
6. [docs/SETUP-FIREBASE-GEMINI.md](docs/SETUP-FIREBASE-GEMINI.md): one-time Firebase and Gemini setup
7. [Implementation plan](docs/superpowers/plans/2026-10-30-uniten-student-assistant.md): build it step by step
8. [docs/TEST-CHECKLIST.md](docs/TEST-CHECKLIST.md): emulator checks and the demo script

Other files:

- `CSNB5123_Milestone_2_Abdullah_FINAL.pdf`: the original Milestone 2 proposal
- `UNITEN-Student-Assistant-Build-Plan.html`: a visual overview of complexity and the realtime demo

## Stack

.NET 10 MAUI (Android) · Plugin.Firebase.Firestore 4.0.0 · Gemini REST API (one key, no RAG)
