using System.Reflection;

namespace WorkClosure.Helpers;

public static class AppInfo
{
    public const string ProductName = "WorkSolo";
    public const string ProductTagline = "个人工作推进器";
    public const string RepositoryUrl = "https://github.com/yeshaobao/worksolo";
    public const string ReleasesUrl = "https://github.com/yeshaobao/worksolo/releases";
    public const string LatestReleaseApiUrl = "https://api.github.com/repos/yeshaobao/worksolo/releases/latest";

    public static string CurrentVersion => FormatVersion(Assembly.GetExecutingAssembly().GetName().Version);

    public static string CurrentVersionTag => $"v{CurrentVersion}";

    public static string ResolveAppRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Run-WorkSolo.cmd")) ||
                File.Exists(Path.Combine(current.FullName, "使用说明.md")) ||
                File.Exists(Path.Combine(current.FullName, "WorkClosure.csproj")))
            {
                return current.FullName;
            }

            if (string.Equals(current.Name, "AppLive", StringComparison.OrdinalIgnoreCase) &&
                current.Parent is not null &&
                File.Exists(Path.Combine(current.Parent.FullName, "Run-WorkSolo.cmd")))
            {
                return current.Parent.FullName;
            }

            current = current.Parent;
        }

        return AppContext.BaseDirectory;
    }

    public static string? ResolveManualPath()
    {
        var candidates = new[]
        {
            Path.Combine(ResolveAppRoot(), "使用说明.md"),
            Path.Combine(AppContext.BaseDirectory, "使用说明.md")
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string FormatVersion(Version? version)
    {
        if (version is null)
        {
            return "未知";
        }

        var parts = new List<int> { version.Major, version.Minor };

        if (version.Build >= 0)
        {
            parts.Add(version.Build);
        }

        if (version.Revision > 0)
        {
            parts.Add(version.Revision);
        }

        return string.Join(".", parts);
    }
}
