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

    private async void OnChipClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn)
        {
            await SendAsync(btn.Text);
        }
    }

    private async void OnSendClicked(object? sender, EventArgs e) =>
        await SendAsync(QuestionEntry.Text);

    private async void OnQuestionCompleted(object? sender, EventArgs e) =>
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
