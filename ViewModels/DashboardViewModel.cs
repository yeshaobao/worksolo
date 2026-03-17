using Microsoft.UI.Xaml;
using WorkClosure.Helpers;
using WorkClosure.Models;
using WorkClosure.Services;

namespace WorkClosure.ViewModels;

public sealed class DashboardViewModel : ObservableObject
{
    private static readonly DailyQuote[] QuotePool =
    [
        new("看上去不可能的事，常常只是还没开始。", "纳尔逊·曼德拉", "南非"),
        new("把脸朝向阳光，阴影自然会落在身后。", "海伦·凯勒", "美国"),
        new("知道还不够，必须去做。", "歌德", "德国"),
        new("再长的路，只要出发，就会更近一点。", "泰戈尔", "印度"),
        new("志不立，天下无可成之事。", "王阳明", "中国"),
        new("越是困难的时候，越要看见其中的机会。", "爱因斯坦", "德国"),
        new("行动，才是把想法变成现实的第一步。", "达·芬奇", "意大利"),
        new("与其等待时机，不如让自己配得上时机。", "居里夫人", "波兰 / 法国"),
        new("成功不是终点，继续前进才更重要。", "丘吉尔", "英国"),
        new("真正的答案，往往在坚持之后才出现。", "稻盛和夫", "日本"),
        new("只要方向清晰，慢一点也仍在前进。", "塞内加", "古罗马"),
        new("先把今天能做的做好，明天自然会更清晰。", "富兰克林", "美国")
    ];

    private readonly AppState _state = ((App)Application.Current).State;
    private string _quickAddTitle = string.Empty;
    private int _pendingCount;
    private int _overdueCount;
    private int _exceptionCount;
    private int _recentUpdatedCount;
    private bool _isInspectorOpen;
    private string _inspectorTitle = "工作台详情";
    private string _inspectorEmptyText = "当前没有相关事项。";
    private string _inspectorScopeFilter = "open";
    private string _currentInspectorKey = string.Empty;
    private IReadOnlyList<WorkTaskItem> _inspectorTasks = [];

    public DashboardViewModel()
    {
        QuickAddCommand = new RelayCommand(async () => await QuickAddAsync(), () => !string.IsNullOrWhiteSpace(QuickAddTitle));
        CloseInspectorCommand = new RelayCommand(CloseInspector);
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

    public string InspectorEmptyText
    {
        get => _inspectorEmptyText;
        private set => SetProperty(ref _inspectorEmptyText, value);
    }

    public int PendingCount
    {
        get => _pendingCount;
        private set => SetProperty(ref _pendingCount, value);
    }

    public int OverdueCount
    {
        get => _overdueCount;
        private set => SetProperty(ref _overdueCount, value);
    }

    public int ExceptionCount
    {
        get => _exceptionCount;
        private set => SetProperty(ref _exceptionCount, value);
    }

    public int RecentUpdatedCount
    {
        get => _recentUpdatedCount;
        private set => SetProperty(ref _recentUpdatedCount, value);
    }

    public RelayCommand QuickAddCommand { get; }

    public RelayCommand CloseInspectorCommand { get; }

    public string DailyQuoteText => CurrentQuote.Text;

    public string DailyQuoteSource => $"{CurrentQuote.Author} · {CurrentQuote.Country}";

    public void QueueTaskNavigation(TaskNavigationRequest request)
    {
        _state.QueueTaskNavigation(request);
    }

    public void Refresh()
    {
        var tasks = _state.Tasks.ToList();
        var recentThreshold = DateTimeOffset.Now.AddDays(-7);

        PendingCount = AnalyticsService.CountOpen(tasks);
        OverdueCount = tasks.Count(task => task.IsOverdue);
        ExceptionCount = tasks.Count(task => !task.IsClosed && (task.Anomaly != TaskAnomaly.None || task.IsOverdue));
        RecentUpdatedCount = tasks.Count(task => !task.IsClosed && task.UpdatedAt >= recentThreshold);

        if (IsInspectorOpen && !string.IsNullOrWhiteSpace(_currentInspectorKey))
        {
            OpenInspector(_currentInspectorKey);
        }
    }

    public void OpenPendingInspector()
    {
        OpenInspector("pending");
    }

    public void OpenOverdueInspector()
    {
        OpenInspector("overdue");
    }

    public void OpenExceptionInspector()
    {
        OpenInspector("exception");
    }

    public void OpenRecentInspector()
    {
        OpenInspector("recent");
    }

    public TaskNavigationRequest CreateInspectorNavigationRequest(WorkTaskItem task)
    {
        return new TaskNavigationRequest
        {
            ScopeFilter = _inspectorScopeFilter,
            TaskId = task.Id,
            InspectorTabIndex = 0
        };
    }

    private void OpenInspector(string key)
    {
        var tasks = _state.Tasks.ToList();
        var recentThreshold = DateTimeOffset.Now.AddDays(-7);
        _currentInspectorKey = key;
        IsInspectorOpen = true;

        switch (key)
        {
            case "pending":
                InspectorTitle = "待推进事项";
                InspectorEmptyText = "当前没有待推进事项。";
                _inspectorScopeFilter = "open";
                InspectorTasks = tasks
                    .Where(task => !task.IsClosed)
                    .OrderBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
                    .ThenByDescending(task => task.UpdatedAt)
                    .ToList();
                break;
            case "overdue":
                InspectorTitle = "超期事项";
                InspectorEmptyText = "当前没有超期事项。";
                _inspectorScopeFilter = "overdue";
                InspectorTasks = tasks
                    .Where(task => task.IsOverdue)
                    .OrderBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
                    .ThenByDescending(task => task.UpdatedAt)
                    .ToList();
                break;
            case "exception":
                InspectorTitle = "异常 / 阻塞事项";
                InspectorEmptyText = "当前没有异常或阻塞事项。";
                _inspectorScopeFilter = "anomaly";
                InspectorTasks = tasks
                    .Where(task => !task.IsClosed && (task.Anomaly != TaskAnomaly.None || task.IsOverdue))
                    .OrderByDescending(task => task.IsOverdue)
                    .ThenByDescending(task => task.UpdatedAt)
                    .ToList();
                break;
            default:
                InspectorTitle = "最近更新";
                InspectorEmptyText = "近 7 天没有新的推进变化。";
                _inspectorScopeFilter = "recent";
                InspectorTasks = tasks
                    .Where(task => !task.IsClosed && task.UpdatedAt >= recentThreshold)
                    .OrderByDescending(task => task.UpdatedAt)
                    .ToList();
                break;
        }
    }

    private void CloseInspector()
    {
        IsInspectorOpen = false;
        _currentInspectorKey = string.Empty;
        InspectorTasks = [];
    }

    private async Task QuickAddAsync()
    {
        var title = QuickAddTitle;
        QuickAddTitle = string.Empty;
        await _state.AddQuickTaskAsync(title);
        OpenPendingInspector();
    }

    private DailyQuote CurrentQuote
    {
        get
        {
            var index = (DateTime.Now.DayOfYear - 1) % QuotePool.Length;
            return QuotePool[index];
        }
    }

    private sealed record DailyQuote(string Text, string Author, string Country);
}
