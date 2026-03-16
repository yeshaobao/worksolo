using System.Text.Json;
using WorkClosure.Models;

namespace WorkClosure.Services;

public sealed class StorageService
{
    private const int MaxBackupFiles = 5;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly IReadOnlyList<string> _legacyDataFilePaths;

    public StorageService()
    {
        var localAppDataRoot = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WorkSolo");

        DataDirectoryPath = localAppDataRoot;
        DataFilePath = Path.Combine(DataDirectoryPath, "data.json");
        BackupDirectoryPath = Path.Combine(DataDirectoryPath, "backups");
        _legacyDataFilePaths = BuildLegacyDataFilePaths();
    }

    public string DataDirectoryPath { get; }

    public string DataFilePath { get; }

    public string BackupDirectoryPath { get; }

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

        CreateBackupIfNeeded();

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
        Directory.CreateDirectory(DataDirectoryPath);

        if (File.Exists(DataFilePath))
        {
            return;
        }

        foreach (var legacyFilePath in _legacyDataFilePaths)
        {
            if (!File.Exists(legacyFilePath))
            {
                continue;
            }

            File.Copy(legacyFilePath, DataFilePath, overwrite: false);
            return;
        }
    }

    private void CreateBackupIfNeeded()
    {
        if (!File.Exists(DataFilePath))
        {
            return;
        }

        Directory.CreateDirectory(BackupDirectoryPath);

        var backupFilePath = Path.Combine(
            BackupDirectoryPath,
            $"data-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.json");

        File.Copy(DataFilePath, backupFilePath, overwrite: false);

        var expiredBackups = new DirectoryInfo(BackupDirectoryPath)
            .GetFiles("data-*.json")
            .OrderByDescending(file => file.CreationTimeUtc)
            .Skip(MaxBackupFiles)
            .ToList();

        foreach (var expiredBackup in expiredBackups)
        {
            expiredBackup.Delete();
        }
    }

    private IReadOnlyList<string> BuildLegacyDataFilePaths()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var releaseRoot = ResolveReleaseRoot();
        var projectRoot = ResolveProjectRoot();

        return new[]
        {
            Path.Combine(releaseRoot, "data", "data.json"),
            Path.Combine(projectRoot, "data", "data.json"),
            Path.Combine(localAppData, "WorkClosure", "data.json"),
            Path.Combine(localAppData, "WorkClosure", "data", "data.json")
        }
        .Where(path => !string.Equals(path, DataFilePath, StringComparison.OrdinalIgnoreCase))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToList();
    }

    private static string ResolveReleaseRoot()
    {
        var baseDirectory = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var current = new DirectoryInfo(baseDirectory);

        if (string.Equals(current.Name, "AppLive", StringComparison.OrdinalIgnoreCase) && current.Parent is not null)
        {
            return current.Parent.FullName;
        }

        return current.FullName;
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

        return ResolveReleaseRoot();
    }
}
