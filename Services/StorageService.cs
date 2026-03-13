using System.Text.Json;
using WorkClosure.Models;

namespace WorkClosure.Services;

public sealed class StorageService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _legacyDataFilePath;

    public StorageService()
    {
        var projectRoot = ResolveProjectRoot();
        DataDirectoryPath = Path.Combine(projectRoot, "data");
        DataFilePath = Path.Combine(DataDirectoryPath, "data.json");

        var legacyFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WorkClosure");
        _legacyDataFilePath = Path.Combine(legacyFolder, "data.json");
    }

    public string DataDirectoryPath { get; }

    public string DataFilePath { get; }

    public async Task<AppDataStore> LoadAsync()
    {
        EnsureDataLocation();

        if (!File.Exists(DataFilePath))
        {
            return new AppDataStore();
        }

        await using var stream = File.OpenRead(DataFilePath);
        return await JsonSerializer.DeserializeAsync<AppDataStore>(stream, SerializerOptions) ?? new AppDataStore();
    }

    public async Task SaveAsync(AppDataStore payload)
    {
        Directory.CreateDirectory(DataDirectoryPath);

        var normalized = new AppDataStore
        {
            Preferences = new AppPreferences
            {
                EnableReminders = payload.Preferences.EnableReminders,
                ThemeMode = payload.Preferences.ThemeMode,
                DefaultPageTag = payload.Preferences.DefaultPageTag
            },
            Projects = payload.Projects.Select(project => new ProjectRecord
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt
            }).ToList(),
            Tasks = payload.Tasks.Select(task => task.Clone()).ToList()
        };

        await using var stream = File.Create(DataFilePath);
        await JsonSerializer.SerializeAsync(stream, normalized, SerializerOptions);
    }

    private void EnsureDataLocation()
    {
        if (File.Exists(DataFilePath))
        {
            return;
        }

        if (!File.Exists(_legacyDataFilePath))
        {
            Directory.CreateDirectory(DataDirectoryPath);
            return;
        }

        Directory.CreateDirectory(DataDirectoryPath);

        try
        {
            File.Move(_legacyDataFilePath, DataFilePath);
        }
        catch (IOException)
        {
            File.Copy(_legacyDataFilePath, DataFilePath, overwrite: false);
            File.Delete(_legacyDataFilePath);
        }
        catch (UnauthorizedAccessException)
        {
            File.Copy(_legacyDataFilePath, DataFilePath, overwrite: false);
        }
    }

    private static string ResolveProjectRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "WorkClosure.csproj")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
