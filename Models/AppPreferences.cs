namespace WorkClosure.Models;

public sealed class AppPreferences
{
    public bool EnableReminders { get; set; } = true;

    public string ThemeMode { get; set; } = "system";

    public string DefaultPageTag { get; set; } = "dashboard";
}
