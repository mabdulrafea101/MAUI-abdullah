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

