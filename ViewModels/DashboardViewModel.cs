using Microsoft.UI.Xaml;
using WorkClosure.Helpers;
using WorkClosure.Models;
using WorkClosure.Services;

namespace WorkClosure.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    private readonly AppState _state = ((App)Application.Current).State;
    private string _quickAddTitle = string.Empty;
    private IReadOnlyList<WorkTaskItem> _todayDueTasks = [];
    private IReadOnlyList<WorkTaskItem> _overdueTasks = [];
    private IReadOnlyList<WorkTaskItem> _recentTasks = [];
    private int _openCount;
    private int _todayDueCount;
    private int _overdueCount;
    private int _completedThisWeekCount;

    public DashboardViewModel()
    {
        QuickAddCommand = new RelayCommand(async () => await QuickAddAsync(), () => !string.IsNullOrWhiteSpace(QuickAddTitle));
        _state.StateChanged += (_, _) => Refresh();
        Refresh();
    }

    public string QuickAddTitle
    {
        get => _quickAddTitle;
        set
        {
            if (SetProperty(ref _quickAddTitle, value))
            {
                QuickAddCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IReadOnlyList<WorkTaskItem> TodayDueTasks
    {
        get => _todayDueTasks;
        private set => SetProperty(ref _todayDueTasks, value);
    }

    public IReadOnlyList<WorkTaskItem> OverdueTasks
    {
        get => _overdueTasks;
        private set => SetProperty(ref _overdueTasks, value);
    }

    public IReadOnlyList<WorkTaskItem> RecentTasks
    {
        get => _recentTasks;
        private set => SetProperty(ref _recentTasks, value);
    }

    public int OpenCount
    {
        get => _openCount;
        private set => SetProperty(ref _openCount, value);
    }

    public int TodayDueCount
    {
        get => _todayDueCount;
        private set => SetProperty(ref _todayDueCount, value);
    }

    public int OverdueCount
    {
        get => _overdueCount;
        private set => SetProperty(ref _overdueCount, value);
    }

    public int CompletedThisWeekCount
    {
        get => _completedThisWeekCount;
        private set => SetProperty(ref _completedThisWeekCount, value);
    }

    public RelayCommand QuickAddCommand { get; }

    public string ResolveProjectName(Guid? projectId)
    {
        return _state.GetProjectName(projectId);
    }

    public void Refresh()
    {
        var tasks = _state.Tasks.ToList();
        var currentDay = DateTimeOffset.Now.Date;
        var startOfWeek = currentDay.AddDays(-((int)DateTimeOffset.Now.DayOfWeek == 0 ? 6 : (int)DateTimeOffset.Now.DayOfWeek - 1));

        TodayDueTasks = tasks
            .Where(task => !task.IsClosed && task.DueDate.HasValue && task.DueDate.Value.Date == currentDay)
            .OrderBy(task => task.DueDate)
            .ToList();
        OverdueTasks = tasks
            .Where(task => task.IsOverdue)
            .OrderBy(task => task.DueDate)
            .ToList();
        RecentTasks = tasks
            .Where(task => !task.IsClosed)
            .OrderBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
            .ThenByDescending(task => task.UpdatedAt)
            .Take(8)
            .ToList();

        OpenCount = AnalyticsService.CountOpen(tasks);
        TodayDueCount = TodayDueTasks.Count;
        OverdueCount = OverdueTasks.Count;
        CompletedThisWeekCount = tasks.Count(task =>
            task.Status == WorkTaskStatus.Completed &&
            task.CompletedAt.HasValue &&
            task.CompletedAt.Value.Date >= startOfWeek);
    }

    private async Task QuickAddAsync()
    {
        var title = QuickAddTitle;
        QuickAddTitle = string.Empty;
        await _state.AddQuickTaskAsync(title);
    }
}
