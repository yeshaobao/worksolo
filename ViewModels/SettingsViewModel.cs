using System.Reflection;
using Microsoft.UI.Xaml;
using WorkClosure.Helpers;
using WorkClosure.Models;
using WorkClosure.Services;

namespace WorkClosure.ViewModels;

public sealed class SettingsViewModel : ObservableObject
{
    private readonly AppState _state = ((App)Application.Current).State;
    private bool _enableReminders;
    private string _selectedThemeMode = "system";
    private string _selectedDefaultPageTag = "dashboard";
    private string _statusMessage = string.Empty;

    public SettingsViewModel()
    {
        SaveSettingsCommand = new RelayCommand(async () => await SaveAsync());
        ResetSettingsCommand = new RelayCommand(LoadFromState);
        LoadFromState();
    }

    public IReadOnlyList<OptionItem> ThemeOptions { get; } =
    [
        new() { Label = "跟随系统", Value = "system" },
        new() { Label = "浅色", Value = "light" },
        new() { Label = "深色", Value = "dark" }
    ];

    public IReadOnlyList<OptionItem> DefaultPageOptions { get; } =
    [
        new() { Label = "工作台", Value = "dashboard" },
        new() { Label = "事项清单", Value = "tasks" },
        new() { Label = "项目分类", Value = "projects" },
        new() { Label = "总结复盘", Value = "summary" }
    ];

    public string ProductName => AppInfo.ProductName;

    public string ProductDescription => "面向个人工作记录、推进和复盘的轻量桌面工具。";

    public string VersionText
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version is null ? "版本未知" : $"版本 {AppInfo.CurrentVersionTag}";
        }
    }

    public string DataPath => _state.DataFilePath;

    public bool EnableReminders
    {
        get => _enableReminders;
        set => SetProperty(ref _enableReminders, value);
    }

    public string SelectedThemeMode
    {
        get => _selectedThemeMode;
        set => SetProperty(ref _selectedThemeMode, value);
    }

    public string SelectedDefaultPageTag
    {
        get => _selectedDefaultPageTag;
        set => SetProperty(ref _selectedDefaultPageTag, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand SaveSettingsCommand { get; }

    public RelayCommand ResetSettingsCommand { get; }

    private void LoadFromState()
    {
        EnableReminders = _state.Preferences.EnableReminders;
        SelectedThemeMode = _state.Preferences.ThemeMode;
        SelectedDefaultPageTag = _state.Preferences.DefaultPageTag;
        StatusMessage = "当前显示的是已保存设置。";
    }

    private async Task SaveAsync()
    {
        await _state.SavePreferencesAsync(new AppPreferences
        {
            EnableReminders = EnableReminders,
            ThemeMode = SelectedThemeMode,
            DefaultPageTag = SelectedDefaultPageTag
        });

        StatusMessage = $"设置已保存：{DateTime.Now:HH:mm:ss}";
    }
}
