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
    private IReadOnlyList<WorkTaskItem> _createdTasks = [];
    private IReadOnlyList<WorkTaskItem> _completedTasks = [];
    private IReadOnlyList<WorkTaskItem> _openTasks = [];
    private IReadOnlyList<WorkTaskItem> _overdueTasks = [];
    private bool _isInspectorOpen;
    private string _inspectorTitle = "复盘详情";
    private string _inspectorDescription = "点击左侧卡片或项目查看明细。";
    private string _inspectorEmptyText = "当前没有相关事项。";
    private IReadOnlyList<WorkTaskItem> _inspectorTasks = [];
    private string _inspectorScopeFilter = "all";
    private bool _inspectorApplyProjectFilter;
    private Guid? _inspectorProjectId;
    private string _currentInspectorKey = string.Empty;
    private int _createdCount;
    private int _completedCount;
    private int _openCount;
    private int _overdueCount;

    public SummaryViewModel()
    {
        PreviousPeriodCommand = new RelayCommand(MovePrevious);
        NextPeriodCommand = new RelayCommand(MoveNext);
        CloseInspectorCommand = new RelayCommand(CloseInspector);
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

    public IReadOnlyList<WorkTaskItem> InspectorTasks
    {
        get => _inspectorTasks;
        private set
        {
            if (SetProperty(ref _inspectorTasks, value))
            {
                OnPropertyChanged(nameof(HasInspectorTasks));
                OnPropertyChanged(nameof(InspectorTasksVisibility));
                OnPropertyChanged(nameof(InspectorEmptyVisibility));
            }
        }
    }

    public bool HasInspectorTasks => InspectorTasks.Count > 0;

    public Visibility InspectorVisibility => IsInspectorOpen ? Visibility.Visible : Visibility.Collapsed;

    public Visibility InspectorTasksVisibility => HasInspectorTasks ? Visibility.Visible : Visibility.Collapsed;

    public Visibility InspectorEmptyVisibility => HasInspectorTasks ? Visibility.Collapsed : Visibility.Visible;

    public bool IsInspectorOpen
    {
        get => _isInspectorOpen;
        private set
        {
            if (SetProperty(ref _isInspectorOpen, value))
            {
                OnPropertyChanged(nameof(InspectorVisibility));
            }
        }
    }

    public string InspectorTitle
    {
        get => _inspectorTitle;
        private set => SetProperty(ref _inspectorTitle, value);
    }

    public string InspectorDescription
    {
        get => _inspectorDescription;
        private set => SetProperty(ref _inspectorDescription, value);
    }

    public string InspectorEmptyText
    {
        get => _inspectorEmptyText;
        private set => SetProperty(ref _inspectorEmptyText, value);
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
    public RelayCommand CloseInspectorCommand { get; }

    public void QueueTaskNavigation(TaskNavigationRequest request)
    {
        _state.QueueTaskNavigation(request);
    }

    public void Refresh()
    {
        var (start, end) = GetRange();
        var created = _state.Tasks
            .Where(task => task.CreatedAt >= start && task.CreatedAt < end)
            .OrderByDescending(task => task.CreatedAt)
            .ToList();
        var completed = _state.Tasks
            .Where(task => task.Status == WorkTaskStatus.Completed && task.CompletedAt.HasValue && task.CompletedAt.Value >= start && task.CompletedAt.Value < end)
            .OrderByDescending(task => task.CompletedAt)
            .ToList();
        var open = _state.Tasks
            .Where(task => !task.IsClosed)
            .OrderBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
            .ToList();
        var overdue = open
            .Where(task => task.IsOverdue)
            .OrderBy(task => task.DueDate)
            .ToList();

        CreatedCount = created.Count;
        CompletedCount = completed.Count;
        OpenCount = open.Count;
        OverdueCount = overdue.Count;
        _createdTasks = created;
        _completedTasks = completed;
        _openTasks = open;
        _overdueTasks = overdue;
        ProjectSummaries = AnalyticsService.BuildProjectSummaries(_state.Projects, _state.Tasks);
        RangeTitle = $"{start:yyyy-MM-dd} 至 {end.AddDays(-1):yyyy-MM-dd}";

        if (IsInspectorOpen && !string.IsNullOrWhiteSpace(_currentInspectorKey))
        {
            switch (_currentInspectorKey)
            {
                case "created":
                    OpenCreatedInspector();
                    break;
                case "completed":
                    OpenCompletedInspector();
                    break;
                case "open":
                    OpenOpenInspector();
                    break;
                case "overdue":
                    OpenOverdueInspector();
                    break;
                default:
                    var project = ProjectSummaries.FirstOrDefault(item =>
                        item.ProjectId == _inspectorProjectId &&
                        item.ProjectName == _currentInspectorKey);
                    if (project is not null)
                    {
                        OpenProjectInspector(project);
                    }
                    break;
            }
        }
    }

    public void OpenCreatedInspector()
    {
        _currentInspectorKey = "created";
        IsInspectorOpen = true;
        InspectorTitle = "所选周期内新增事项";
        InspectorDescription = "适合回看本周期新进入系统的事项。";
        InspectorEmptyText = "所选周期内没有新增事项。";
        InspectorTasks = _createdTasks;
        _inspectorScopeFilter = "all";
        _inspectorApplyProjectFilter = false;
        _inspectorProjectId = null;
    }

    public void OpenCompletedInspector()
    {
        _currentInspectorKey = "completed";
        IsInspectorOpen = true;
        InspectorTitle = "所选周期内完成事项";
        InspectorDescription = "这些事项已经在当前周期闭环，可作为汇报成果素材。";
        InspectorEmptyText = "所选周期内没有完成事项。";
        InspectorTasks = _completedTasks;
        _inspectorScopeFilter = "closed";
        _inspectorApplyProjectFilter = false;
        _inspectorProjectId = null;
    }

    public void OpenOpenInspector()
    {
        _currentInspectorKey = "open";
        IsInspectorOpen = true;
        InspectorTitle = "当前未闭环事项";
        InspectorDescription = "这些事项仍在推进中，是后续需要持续跟进的主体。";
        InspectorEmptyText = "当前没有未闭环事项。";
        InspectorTasks = _openTasks;
        _inspectorScopeFilter = "open";
        _inspectorApplyProjectFilter = false;
        _inspectorProjectId = null;
    }

    public void OpenOverdueInspector()
    {
        _currentInspectorKey = "overdue";
        IsInspectorOpen = true;
        InspectorTitle = "当前已超期事项";
        InspectorDescription = "这些事项的预计完成时间已过，适合优先处理。";
        InspectorEmptyText = "当前没有超期事项。";
        InspectorTasks = _overdueTasks;
        _inspectorScopeFilter = "overdue";
        _inspectorApplyProjectFilter = false;
        _inspectorProjectId = null;
    }

    public void OpenProjectInspector(ProjectProgressSummary summary)
    {
        _currentInspectorKey = summary.ProjectName;
        IsInspectorOpen = true;
        InspectorTitle = $"{summary.ProjectName} - 事项明细";
        InspectorDescription = "这里列出该项目下的所有事项，可继续进入事项清单详细处理。";
        InspectorEmptyText = "当前项目下没有事项。";
        InspectorTasks = _state.Tasks
            .Where(task => task.ProjectId == summary.ProjectId)
            .OrderBy(task => task.IsClosed ? 1 : 0)
            .ThenBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
            .ThenByDescending(task => task.UpdatedAt)
            .ToList();
        _inspectorScopeFilter = "all";
        _inspectorApplyProjectFilter = true;
        _inspectorProjectId = summary.ProjectId;
    }

    public TaskNavigationRequest CreateInspectorNavigationRequest(WorkTaskItem task)
    {
        return new TaskNavigationRequest
        {
            ScopeFilter = _inspectorScopeFilter,
            ApplyProjectFilter = _inspectorApplyProjectFilter,
            ProjectId = _inspectorProjectId,
            TaskId = task.Id,
            InspectorTabIndex = 0
        };
    }

    private void CloseInspector()
    {
        IsInspectorOpen = false;
        InspectorTitle = "复盘详情";
        InspectorDescription = "点击左侧卡片或项目查看明细。";
        InspectorEmptyText = "当前没有相关事项。";
        InspectorTasks = [];
        _inspectorScopeFilter = "all";
        _inspectorApplyProjectFilter = false;
        _inspectorProjectId = null;
        _currentInspectorKey = string.Empty;
    }

    private (DateTimeOffset Start, DateTimeOffset End) GetRange()
    {
        return SelectedPeriod switch
        {
            "day" => (_anchorDate.Date, _anchorDate.Date.AddDays(1)),
            "month" => (
                new DateTimeOffset(_anchorDate.Year, _anchorDate.Month, 1, 0, 0, 0, _anchorDate.Offset),
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
