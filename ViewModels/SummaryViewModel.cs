using Microsoft.UI.Xaml;
using WorkClosure.Helpers;
using WorkClosure.Models;
using WorkClosure.Services;

namespace WorkClosure.ViewModels;

public sealed class SummaryViewModel : ObservableObject
{
    private readonly AppState _state = ((App)Application.Current).State;
    private string _selectedPeriod = "week";
    private DateTimeOffset _anchorDate = DateTimeOffset.Now;
    private string _rangeTitle = string.Empty;
    private IReadOnlyList<ProjectProgressSummary> _projectSummaries = [];
    private IReadOnlyList<WorkTaskItem> _completedTasks = [];
    private IReadOnlyList<WorkTaskItem> _openTasks = [];
    private int _createdCount;
    private int _completedCount;
    private int _openCount;
    private int _overdueCount;

    public SummaryViewModel()
    {
        PreviousPeriodCommand = new RelayCommand(MovePrevious);
        NextPeriodCommand = new RelayCommand(MoveNext);
        _state.StateChanged += (_, _) => Refresh();
        Refresh();
    }

    public IReadOnlyList<OptionItem> PeriodOptions { get; } =
    [
        new() { Label = "按日", Value = "day" },
        new() { Label = "按周", Value = "week" },
        new() { Label = "按月", Value = "month" }
    ];

    public string SelectedPeriod
    {
        get => _selectedPeriod;
        set
        {
            if (SetProperty(ref _selectedPeriod, value))
            {
                Refresh();
            }
        }
    }

    public string RangeTitle
    {
        get => _rangeTitle;
        private set => SetProperty(ref _rangeTitle, value);
    }

    public IReadOnlyList<ProjectProgressSummary> ProjectSummaries
    {
        get => _projectSummaries;
        private set => SetProperty(ref _projectSummaries, value);
    }

    public IReadOnlyList<WorkTaskItem> CompletedTasks
    {
        get => _completedTasks;
        private set => SetProperty(ref _completedTasks, value);
    }

    public IReadOnlyList<WorkTaskItem> OpenTasks
    {
        get => _openTasks;
        private set => SetProperty(ref _openTasks, value);
    }

    public int CreatedCount
    {
        get => _createdCount;
        private set => SetProperty(ref _createdCount, value);
    }

    public int CompletedCount
    {
        get => _completedCount;
        private set => SetProperty(ref _completedCount, value);
    }

    public int OpenCount
    {
        get => _openCount;
        private set => SetProperty(ref _openCount, value);
    }

    public int OverdueCount
    {
        get => _overdueCount;
        private set => SetProperty(ref _overdueCount, value);
    }

    public RelayCommand PreviousPeriodCommand { get; }
    public RelayCommand NextPeriodCommand { get; }

    public string ResolveProjectName(Guid? projectId)
    {
        return _state.GetProjectName(projectId);
    }

    public void Refresh()
    {
        var (start, end) = GetRange();
        var created = _state.Tasks.Where(task => task.CreatedAt >= start && task.CreatedAt < end).ToList();
        var completed = _state.Tasks
            .Where(task => task.Status == WorkTaskStatus.Completed && task.CompletedAt.HasValue && task.CompletedAt.Value >= start && task.CompletedAt.Value < end)
            .OrderByDescending(task => task.CompletedAt)
            .ToList();
        var open = _state.Tasks
            .Where(task => !task.IsClosed)
            .OrderBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
            .ToList();

        CreatedCount = created.Count;
        CompletedCount = completed.Count;
        OpenCount = open.Count;
        OverdueCount = open.Count(task => task.IsOverdue);
        CompletedTasks = completed;
        OpenTasks = open.Take(20).ToList();
        ProjectSummaries = AnalyticsService.BuildProjectSummaries(_state.Projects, _state.Tasks);
        RangeTitle = $"{start:yyyy-MM-dd} 至 {end.AddDays(-1):yyyy-MM-dd}";
    }

    private (DateTimeOffset Start, DateTimeOffset End) GetRange()
    {
        return SelectedPeriod switch
        {
            "day" => (_anchorDate.Date, _anchorDate.Date.AddDays(1)),
            "month" => (new DateTimeOffset(_anchorDate.Year, _anchorDate.Month, 1, 0, 0, 0, _anchorDate.Offset),
                new DateTimeOffset(_anchorDate.Year, _anchorDate.Month, 1, 0, 0, 0, _anchorDate.Offset).AddMonths(1)),
            _ => GetWeekRange(_anchorDate)
        };
    }

    private static (DateTimeOffset Start, DateTimeOffset End) GetWeekRange(DateTimeOffset anchor)
    {
        var diff = anchor.DayOfWeek == DayOfWeek.Sunday ? -6 : DayOfWeek.Monday - anchor.DayOfWeek;
        var start = anchor.Date.AddDays(diff);
        return (start, start.AddDays(7));
    }

    private void MovePrevious()
    {
        _anchorDate = SelectedPeriod switch
        {
            "day" => _anchorDate.AddDays(-1),
            "month" => _anchorDate.AddMonths(-1),
            _ => _anchorDate.AddDays(-7)
        };
        Refresh();
    }

    private void MoveNext()
    {
        _anchorDate = SelectedPeriod switch
        {
            "day" => _anchorDate.AddDays(1),
            "month" => _anchorDate.AddMonths(1),
            _ => _anchorDate.AddDays(7)
        };
        Refresh();
    }
}
