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

