using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using WorkClosure.Helpers;
using WorkClosure.Models;

namespace WorkClosure.ViewModels;

public sealed class UpdatesViewModel : ObservableObject
{
    private static readonly HttpClient HttpClient = CreateHttpClient();

    private string _latestVersionText = "尚未检查";
    private string _updateStatus = "点击“检查更新”可前往 GitHub 拉取最新发布信息。";
    private string _lastCheckedText = "最近检查：尚未执行";
    private string _releaseNotesPreview = "当前页面内已提供本地版本历史，可先查看本次和上一版本变化。";
    private string _latestReleaseUrl = AppInfo.ReleasesUrl;
    private bool _isChecking;

    public UpdatesViewModel()
    {
        CheckForUpdatesCommand = new RelayCommand(() => _ = CheckForUpdatesAsync());
        OpenReleasesCommand = new RelayCommand(() => OpenUrl(_latestReleaseUrl));
        OpenRepositoryCommand = new RelayCommand(() => OpenUrl(AppInfo.RepositoryUrl));

        VersionHistory =
        [
            new ReleaseHistoryItem
            {
                Version = "v1.0.1",
                ReleaseDate = "2026-03-16",
                Title = "版本固化与更新中心",
                Summary = "固化程序集版本号，新增“检查更新 / 版本历史”页面，并修复使用文档路径与版本展示。",
                TagText = "当前版本"
            },
            new ReleaseHistoryItem
            {
                Version = "v1.0.0",
                ReleaseDate = "2026-03-16",
                Title = "首个正式版本",
                Summary = "提供工作台、事项与项目管理、总结复盘、本地数据迁移和 GitHub 发布包。",
                TagText = "首发版本"
            }
        ];
    }

    public string ProductName => AppInfo.ProductName;

    public string CurrentVersionText => $"当前安装版本 {AppInfo.CurrentVersionTag}";

    public string CurrentChannelText => "更新渠道：GitHub Releases";

    public string LatestVersionText
    {
        get => _latestVersionText;
        set => SetProperty(ref _latestVersionText, value);
    }

    public string UpdateStatus
    {
        get => _updateStatus;
        set => SetProperty(ref _updateStatus, value);
    }

    public string LastCheckedText
    {
        get => _lastCheckedText;
        set => SetProperty(ref _lastCheckedText, value);
    }

    public string ReleaseNotesPreview
    {
        get => _releaseNotesPreview;
        set => SetProperty(ref _releaseNotesPreview, value);
    }

    public ObservableCollection<ReleaseHistoryItem> VersionHistory { get; }

    public RelayCommand CheckForUpdatesCommand { get; }

    public RelayCommand OpenReleasesCommand { get; }

    public RelayCommand OpenRepositoryCommand { get; }

    private async Task CheckForUpdatesAsync()
    {
        if (_isChecking)
        {
            return;
        }

        try
        {
            _isChecking = true;
            UpdateStatus = "正在检查 GitHub 最新版本...";
            ReleaseNotesPreview = "正在拉取最新发布信息，请稍候。";

            using var response = await HttpClient.GetAsync(AppInfo.LatestReleaseApiUrl);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            var root = document.RootElement;
            var tagName = root.TryGetProperty("tag_name", out var tagElement)
                ? tagElement.GetString()
                : null;
            var releaseUrl = root.TryGetProperty("html_url", out var urlElement)
                ? urlElement.GetString()
                : null;
            var body = root.TryGetProperty("body", out var bodyElement)
                ? bodyElement.GetString()
                : null;
            var publishedAt = root.TryGetProperty("published_at", out var publishedElement)
                ? publishedElement.GetDateTimeOffset().ToLocalTime()
                : (DateTimeOffset?)null;

            var normalizedLatestVersion = NormalizeVersion(tagName);
            _latestReleaseUrl = string.IsNullOrWhiteSpace(releaseUrl) ? AppInfo.ReleasesUrl : releaseUrl;
            LatestVersionText = string.IsNullOrWhiteSpace(tagName) ? "最新版本：未读取到版本号" : $"最新版本 {tagName}";
            LastCheckedText = publishedAt.HasValue
                ? $"最近检查：{DateTime.Now:yyyy-MM-dd HH:mm:ss}，最新发布时间 {publishedAt:yyyy-MM-dd HH:mm}"
                : $"最近检查：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            ReleaseNotesPreview = string.IsNullOrWhiteSpace(body)
                ? "最新版本未提供发布说明，请点击“打开发布页”查看详情。"
                : body.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "最新版本已发布。";

            UpdateStatus = string.Equals(normalizedLatestVersion, AppInfo.CurrentVersion, StringComparison.OrdinalIgnoreCase)
                ? "当前已经是最新版本，可以继续直接使用。"
                : $"发现新版本 {tagName}，可以点击“打开发布页”查看并下载更新包。";
        }
        catch
        {
            LatestVersionText = "最新版本：检查失败";
            UpdateStatus = "暂时无法连接 GitHub Releases，请稍后重试。";
            LastCheckedText = $"最近检查：{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            ReleaseNotesPreview = "本地版本历史仍然可用；如果网络恢复，也可以直接点击“打开发布页”。";
        }
        finally
        {
            _isChecking = false;
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("WorkSolo/1.0.1");
        return client;
    }

    private static string NormalizeVersion(string? version)
    {
        return string.IsNullOrWhiteSpace(version)
            ? string.Empty
            : version.Trim().TrimStart('v', 'V');
    }

    private static void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}
