using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WorkClosure.Helpers;

namespace WorkClosure.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly App _app = (App)Application.Current;
    private bool _isReminderOpen;
    private string _reminderTitle = string.Empty;
    private string _reminderMessage = string.Empty;
    private InfoBarSeverity _reminderSeverity = InfoBarSeverity.Informational;

    public bool IsReminderOpen
    {
        get => _isReminderOpen;
        set => SetProperty(ref _isReminderOpen, value);
    }

    public string ReminderTitle
    {
        get => _reminderTitle;
        set => SetProperty(ref _reminderTitle, value);
    }

    public string ReminderMessage
    {
        get => _reminderMessage;
        set => SetProperty(ref _reminderMessage, value);
    }

    public InfoBarSeverity ReminderSeverity
    {
        get => _reminderSeverity;
        set => SetProperty(ref _reminderSeverity, value);
    }

    public void Initialize()
    {
        _app.State.StateChanged += OnStateChanged;
        RefreshReminder();
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        RefreshReminder();
    }

    private void RefreshReminder()
    {
        if (!_app.State.Preferences.EnableReminders)
        {
            IsReminderOpen = false;
            ReminderTitle = string.Empty;
            ReminderMessage = string.Empty;
            ReminderSeverity = InfoBarSeverity.Informational;
            return;
        }

        var overdue = _app.State.Tasks.Where(task => task.IsOverdue).ToList();
        var dueToday = _app.State.Tasks
            .Where(task => !task.IsClosed && task.DueDate.HasValue && task.DueDate.Value.Date == DateTimeOffset.Now.Date)
            .ToList();

        if (overdue.Count == 0 && dueToday.Count == 0)
        {
            IsReminderOpen = false;
            ReminderTitle = string.Empty;
            ReminderMessage = string.Empty;
            ReminderSeverity = InfoBarSeverity.Informational;
            return;
        }

        ReminderTitle = overdue.Count > 0 ? "有事项已经逾期" : "今天有到期事项";
        ReminderMessage = $"逾期 {overdue.Count} 项，今日到期 {dueToday.Count} 项。建议优先在“工作台”或“事项清单”中处理。";
        ReminderSeverity = overdue.Count > 0 ? InfoBarSeverity.Warning : InfoBarSeverity.Informational;
        IsReminderOpen = true;
    }
}
