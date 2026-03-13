namespace WorkClosure.Models;

public sealed class AppDataStore
{
    public List<ProjectRecord> Projects { get; set; } = [];

    public List<WorkTaskItem> Tasks { get; set; } = [];

    public AppPreferences Preferences { get; set; } = new();
}
