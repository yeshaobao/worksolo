using System.Collections.ObjectModel;
using WorkClosure.Models;

namespace WorkClosure.Services;

public sealed class AppState
{
    private readonly StorageService _storageService;
    private bool _initialized;

    public AppState(StorageService storageService)
    {
        _storageService = storageService;
    }

    public ObservableCollection<ProjectRecord> Projects { get; } = [];

    public ObservableCollection<WorkTaskItem> Tasks { get; } = [];

    public AppPreferences Preferences { get; } = new();

    public string DataFilePath => _storageService.DataFilePath;

    public event EventHandler? StateChanged;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        var data = await _storageService.LoadAsync();
        ApplyPreferences(data.Preferences);

        foreach (var project in data.Projects.OrderBy(project => project.Name))
        {
            Projects.Add(project);
        }

        foreach (var task in data.Tasks.OrderByDescending(task => task.UpdatedAt))
        {
            Tasks.Add(task);
        }

        RefreshDerivedFields();
        _initialized = true;
    }

    public IReadOnlyList<ProjectOption> GetProjectOptions()
    {
        var options = new List<ProjectOption>
        {
            new() { Id = null, Name = "未分类" }
        };

        options.AddRange(Projects
            .OrderBy(project => project.IsActive ? 0 : 1)
            .ThenBy(project => project.Name)
            .Select(project => new ProjectOption
            {
                Id = project.Id,
                Name = project.IsActive ? project.Name : $"{project.Name}（已停用）"
            }));

        return options;
    }

    public string GetProjectName(Guid? projectId)
    {
        if (!projectId.HasValue)
        {
            return "未分类";
        }

        return Projects.FirstOrDefault(project => project.Id == projectId)?.Name ?? "未分类";
    }

    public async Task AddQuickTaskAsync(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        var task = new WorkTaskItem
        {
            Title = title.Trim(),
            CreatedAt = DateTimeOffset.Now,
            UpdatedAt = DateTimeOffset.Now,
            Status = WorkTaskStatus.NotStarted,
            Anomaly = TaskAnomaly.None,
            DueDate = DateTimeOffset.Now
        };

        Tasks.Insert(0, task);
        await PersistAndNotifyAsync();
    }

    public async Task SaveTaskAsync(WorkTaskItem draft)
    {
        if (string.IsNullOrWhiteSpace(draft.Title))
        {
            return;
        }

        var existing = Tasks.FirstOrDefault(task => task.Id == draft.Id);
        if (existing is null)
        {
            draft.Touch();
            Tasks.Insert(0, draft);
        }
        else
        {
            existing.Title = draft.Title.Trim();
            existing.ProjectId = draft.ProjectId;
            existing.Anomaly = draft.Anomaly;
            existing.AnomalyNote = draft.AnomalyNote.Trim();
            existing.CreatedAt = draft.CreatedAt;
            existing.DueDate = draft.DueDate;
            existing.StartedAt = draft.StartedAt;
            existing.CompletedAt = draft.CompletedAt;
            existing.Status = draft.Status;
            existing.Touch();
        }

        await PersistAndNotifyAsync();
    }

    public async Task UpdateTaskStatusAsync(WorkTaskItem task, WorkTaskStatus status)
    {
        task.Status = status;
        task.Touch();
        await PersistAndNotifyAsync();
    }

    public async Task DeleteTaskAsync(Guid taskId)
    {
        var existing = Tasks.FirstOrDefault(task => task.Id == taskId);
        if (existing is null)
        {
            return;
        }

        Tasks.Remove(existing);
        await PersistAndNotifyAsync();
    }

    public async Task SaveProjectAsync(ProjectRecord draft)
    {
        var existing = Projects.FirstOrDefault(project => project.Id == draft.Id);
        if (existing is null)
        {
            Projects.Add(draft);
        }
        else
        {
            existing.Name = draft.Name.Trim();
            existing.Description = draft.Description.Trim();
            existing.IsActive = draft.IsActive;
        }

        await PersistAndNotifyAsync();
    }

    public async Task DeleteProjectAsync(Guid projectId)
    {
        var existing = Projects.FirstOrDefault(project => project.Id == projectId);
        if (existing is null)
        {
            return;
        }

        foreach (var task in Tasks.Where(task => task.ProjectId == projectId))
        {
            task.ProjectId = null;
            task.Touch();
        }

        Projects.Remove(existing);
        await PersistAndNotifyAsync();
    }

    public async Task SavePreferencesAsync(AppPreferences draft)
    {
        ApplyPreferences(draft);
        await PersistAndNotifyAsync();
    }

    public async Task PersistAndNotifyAsync()
    {
        RefreshDerivedFields();

        await _storageService.SaveAsync(new AppDataStore
        {
            Preferences = new AppPreferences
            {
                EnableReminders = Preferences.EnableReminders,
                ThemeMode = Preferences.ThemeMode,
                DefaultPageTag = Preferences.DefaultPageTag
            },
            Projects = Projects.Select(project => new ProjectRecord
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt
            }).ToList(),
            Tasks = Tasks.Select(task => task.Clone()).ToList()
        });

        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyPreferences(AppPreferences source)
    {
        Preferences.EnableReminders = source.EnableReminders;
        Preferences.ThemeMode = string.IsNullOrWhiteSpace(source.ThemeMode) ? "system" : source.ThemeMode;
        Preferences.DefaultPageTag = string.IsNullOrWhiteSpace(source.DefaultPageTag) ? "dashboard" : source.DefaultPageTag;
    }

    private void RefreshDerivedFields()
    {
        foreach (var task in Tasks)
        {
            task.ProjectName = GetProjectName(task.ProjectId);
        }
    }
}
