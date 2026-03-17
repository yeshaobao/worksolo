using Microsoft.UI.Xaml;
using WorkClosure.Helpers;
using WorkClosure.Models;
using WorkClosure.Services;

namespace WorkClosure.ViewModels;

public sealed class TasksViewModel : ObservableObject
{
    private readonly AppState _state = ((App)Application.Current).State;
    private string _quickAddTitle = string.Empty;
    private string _searchText = string.Empty;
    private string _selectedScopeFilter = "all";
    private Guid? _selectedProjectFilterId = Guid.Empty;
    private IReadOnlyList<ProjectOption> _projectFilters = [];
    private IReadOnlyList<WorkTaskItem> _filteredTasks = [];
    private WorkTaskItem? _selectedTask;
    private int _selectedInspectorTabIndex;
    private IReadOnlyList<TaskProgressEntry> _progressEntries = [];
    private DateTimeOffset _newProgressDate = DateTimeOffset.Now;
    private string _newProgressText = string.Empty;
    private string _newIssueText = string.Empty;
    private string _newNextStepText = string.Empty;
    private bool _newNeedFollowUp = true;
    private bool _newIsKeyMilestone;

    public TasksViewModel()
    {
        Editor = new TaskEditorViewModel();
        QuickAddCommand = new RelayCommand(async () => await QuickAddAsync(), () => !string.IsNullOrWhiteSpace(QuickAddTitle));
        SaveTaskCommand = new RelayCommand(async () => await SaveDialogAsync(), CanSaveTask);
        NewTaskCommand = new RelayCommand(PrepareNewTaskForDialog);
        StartTaskCommand = new RelayCommand(async () => await UpdateSelectedStatusAsync(WorkTaskStatus.InProgress), () => SelectedTask is not null);
        CompleteTaskCommand = new RelayCommand(async () => await UpdateSelectedStatusAsync(WorkTaskStatus.Completed), () => SelectedTask is not null);
        CancelTaskCommand = new RelayCommand(async () => await UpdateSelectedStatusAsync(WorkTaskStatus.Cancelled), () => SelectedTask is not null);
        DeleteTaskCommand = new RelayCommand(async () => await DeleteSelectedTaskAsync(), () => SelectedTask is not null);
        ClearDueDateCommand = new RelayCommand(ClearDueDate);
        AddProgressCommand = new RelayCommand(async () => await AddProgressOnlyAsync(), CanAddProgress);

        Editor.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName is nameof(TaskEditorViewModel.Title))
            {
                SaveTaskCommand.NotifyCanExecuteChanged();
            }
        };

        _state.StateChanged += (_, _) => Refresh();

        Refresh();
        Editor.LoadNew();
        ApplyPendingNavigation();
    }

    public TaskEditorViewModel Editor { get; }

    public IReadOnlyList<OptionItem> ScopeOptions { get; } =
    [
        new() { Label = "全部", Value = "all" },
        new() { Label = "待推进", Value = "open" },
        new() { Label = "已闭环", Value = "closed" },
        new() { Label = "超期事项", Value = "overdue" },
        new() { Label = "异常 / 阻塞", Value = "anomaly" },
        new() { Label = "最近更新", Value = "recent" }
    ];

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

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                Refresh();
            }
        }
    }

    public string SelectedScopeFilter
    {
        get => _selectedScopeFilter;
        set
        {
            if (SetProperty(ref _selectedScopeFilter, value))
            {
                Refresh();
            }
        }
    }

    public Guid? SelectedProjectFilterId
    {
        get => _selectedProjectFilterId;
        set
        {
            if (SetProperty(ref _selectedProjectFilterId, value))
            {
                Refresh();
            }
        }
    }

    public WorkTaskItem? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (SetProperty(ref _selectedTask, value))
            {
                if (value is null)
                {
                    ProgressEntries = [];
                }
                else
                {
                    RefreshProgressEntries(value);
                }

                SaveTaskCommand.NotifyCanExecuteChanged();
                StartTaskCommand.NotifyCanExecuteChanged();
                CompleteTaskCommand.NotifyCanExecuteChanged();
                CancelTaskCommand.NotifyCanExecuteChanged();
                DeleteTaskCommand.NotifyCanExecuteChanged();
                AddProgressCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(HasSelectedTask));
                OnPropertyChanged(nameof(SelectedTaskVisibility));
                OnPropertyChanged(nameof(EmptySelectionVisibility));
                OnPropertyChanged(nameof(ProgressPanelHint));
            }
        }
    }

    public IReadOnlyList<ProjectOption> ProjectFilters
    {
        get => _projectFilters;
        private set => SetProperty(ref _projectFilters, value);
    }

    public IReadOnlyList<WorkTaskItem> FilteredTasks
    {
        get => _filteredTasks;
        private set => SetProperty(ref _filteredTasks, value);
    }

    public int SelectedInspectorTabIndex
    {
        get => _selectedInspectorTabIndex;
        set
        {
            if (SetProperty(ref _selectedInspectorTabIndex, value))
            {
                OnPropertyChanged(nameof(IsBasicInfoSelected));
                OnPropertyChanged(nameof(IsProgressSelected));
            }
        }
    }

    public IReadOnlyList<TaskProgressEntry> ProgressEntries
    {
        get => _progressEntries;
        private set
        {
            if (SetProperty(ref _progressEntries, value))
            {
                OnPropertyChanged(nameof(HasProgressEntries));
                OnPropertyChanged(nameof(ProgressEntriesVisibility));
                OnPropertyChanged(nameof(EmptyProgressVisibility));
            }
        }
    }

    public bool HasSelectedTask => SelectedTask is not null;

    public Visibility SelectedTaskVisibility => HasSelectedTask ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmptySelectionVisibility => HasSelectedTask ? Visibility.Collapsed : Visibility.Visible;

    public bool HasProgressEntries => ProgressEntries.Count > 0;

    public Visibility ProgressEntriesVisibility => HasProgressEntries ? Visibility.Visible : Visibility.Collapsed;

    public Visibility EmptyProgressVisibility => HasProgressEntries ? Visibility.Collapsed : Visibility.Visible;

    public bool IsBasicInfoSelected => SelectedInspectorTabIndex == 0;

    public bool IsProgressSelected => SelectedInspectorTabIndex == 1;

    public bool HasProgressDraft =>
        !string.IsNullOrWhiteSpace(NewProgressText) ||
        !string.IsNullOrWhiteSpace(NewIssueText) ||
        !string.IsNullOrWhiteSpace(NewNextStepText);

    public string ProgressPanelHint => SelectedTask is null
        ? "请先选中或保存一条事项，再补充推进记录。"
        : "有推进、有变化或遇到问题时再记录，不需要每天强制写一条。";

    public DateTimeOffset NewProgressDate
    {
        get => _newProgressDate;
        set => SetProperty(ref _newProgressDate, value);
    }

    public string NewProgressText
    {
        get => _newProgressText;
        set
        {
            if (SetProperty(ref _newProgressText, value))
            {
                AddProgressCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NewIssueText
    {
        get => _newIssueText;
        set
        {
            if (SetProperty(ref _newIssueText, value))
            {
                AddProgressCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string NewNextStepText
    {
        get => _newNextStepText;
        set
        {
            if (SetProperty(ref _newNextStepText, value))
            {
                AddProgressCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool NewNeedFollowUp
    {
        get => _newNeedFollowUp;
        set => SetProperty(ref _newNeedFollowUp, value);
    }

    public bool NewIsKeyMilestone
    {
        get => _newIsKeyMilestone;
        set => SetProperty(ref _newIsKeyMilestone, value);
    }

    public RelayCommand QuickAddCommand { get; }
    public RelayCommand SaveTaskCommand { get; }
    public RelayCommand NewTaskCommand { get; }
    public RelayCommand StartTaskCommand { get; }
    public RelayCommand CompleteTaskCommand { get; }
    public RelayCommand CancelTaskCommand { get; }
    public RelayCommand DeleteTaskCommand { get; }
    public RelayCommand ClearDueDateCommand { get; }
    public RelayCommand AddProgressCommand { get; }

    public string ResolveProjectName(Guid? projectId)
    {
        return _state.GetProjectName(projectId);
    }

    public void Refresh()
    {
        ProjectFilters = _state.GetProjectFilterOptions();
        Editor.RefreshProjectOptions(_state.GetProjectOptions());

        IEnumerable<WorkTaskItem> query = _state.Tasks;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var keyword = SearchText.Trim();
            query = query.Where(task =>
                task.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                task.ProjectName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                task.ProgressEntries.Any(entry =>
                    entry.ProgressText.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    entry.IssueText.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    entry.NextStepText.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        query = SelectedScopeFilter switch
        {
            "open" => query.Where(task => !task.IsClosed),
            "closed" => query.Where(task => task.IsClosed),
            "overdue" => query.Where(task => task.IsOverdue),
            "anomaly" => query.Where(task => task.Anomaly != TaskAnomaly.None || task.IsOverdue),
            "recent" => query.OrderByDescending(task => task.UpdatedAt).Take(20),
            _ => query
        };

        if (SelectedProjectFilterId == Guid.Empty)
        {
        }
        else if (SelectedProjectFilterId.HasValue)
        {
            query = query.Where(task => task.ProjectId == SelectedProjectFilterId);
        }
        else
        {
            query = query.Where(task => task.ProjectId is null);
        }

        FilteredTasks = query
            .OrderBy(task => task.IsClosed ? 1 : 0)
            .ThenBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
            .ThenByDescending(task => task.UpdatedAt)
            .ToList();

        if (SelectedTask is not null)
        {
            SelectedTask = FilteredTasks.FirstOrDefault(task => task.Id == SelectedTask.Id)
                ?? _state.FindTask(SelectedTask.Id);
        }
        else
        {
            DeleteTaskCommand.NotifyCanExecuteChanged();
        }
    }

    public void PrepareNewTaskForDialog()
    {
        Editor.LoadNew();
        ProgressEntries = [];
        ResetProgressDraft();
        SelectedInspectorTabIndex = 0;
    }

    public bool PrepareSelectedTaskForDialog(bool openProgressSection = false)
    {
        if (SelectedTask is null)
        {
            return false;
        }

        Editor.LoadFrom(SelectedTask);
        RefreshProgressEntries(SelectedTask);
        SelectedInspectorTabIndex = openProgressSection ? 1 : 0;
        return true;
    }

    public async Task<bool> SaveDialogAsync()
    {
        if (!CanSaveTask())
        {
            return false;
        }

        await _state.SaveTaskAsync(Editor.ToTask());
        SelectedTask = _state.FindTask(Editor.TaskId);

        if (SelectedTask is not null && HasProgressDraft)
        {
            await _state.AddTaskProgressAsync(SelectedTask.Id, new TaskProgressEntry
            {
                EntryDate = NewProgressDate,
                ProgressText = NewProgressText,
                IssueText = NewIssueText,
                NextStepText = NewNextStepText,
                NeedFollowUp = NewNeedFollowUp,
                IsKeyMilestone = NewIsKeyMilestone,
                CreatedAt = DateTimeOffset.Now
            });

            SelectedTask = _state.FindTask(SelectedTask.Id);
        }

        ResetProgressDraft();
        SelectedInspectorTabIndex = 0;
        return true;
    }

    private bool CanSaveTask()
    {
        return !string.IsNullOrWhiteSpace(Editor.Title);
    }

    private bool CanAddProgress()
    {
        return SelectedTask is not null && HasProgressDraft;
    }

    private async Task QuickAddAsync()
    {
        var title = QuickAddTitle;
        QuickAddTitle = string.Empty;
        await _state.AddQuickTaskAsync(title);

        var createdTask = _state.Tasks
            .OrderByDescending(task => task.CreatedAt)
            .FirstOrDefault(task => string.Equals(task.Title, title.Trim(), StringComparison.OrdinalIgnoreCase));

        if (createdTask is not null)
        {
            SelectedTask = createdTask;
        }
    }

    private async Task UpdateSelectedStatusAsync(WorkTaskStatus status)
    {
        if (SelectedTask is null)
        {
            return;
        }

        await _state.UpdateTaskStatusAsync(SelectedTask, status);
        SelectedTask = _state.FindTask(SelectedTask.Id);
    }

    private async Task DeleteSelectedTaskAsync()
    {
        if (SelectedTask is null)
        {
            return;
        }

        var deletingTaskId = SelectedTask.Id;
        SelectedTask = null;
        await _state.DeleteTaskAsync(deletingTaskId);
        Editor.LoadNew();
        ResetProgressDraft();
    }

    private async Task AddProgressOnlyAsync()
    {
        if (SelectedTask is null || !HasProgressDraft)
        {
            return;
        }

        await _state.AddTaskProgressAsync(SelectedTask.Id, new TaskProgressEntry
        {
            EntryDate = NewProgressDate,
            ProgressText = NewProgressText,
            IssueText = NewIssueText,
            NextStepText = NewNextStepText,
            NeedFollowUp = NewNeedFollowUp,
            IsKeyMilestone = NewIsKeyMilestone,
            CreatedAt = DateTimeOffset.Now
        });

        SelectedTask = _state.FindTask(SelectedTask.Id);
        SelectedInspectorTabIndex = 1;
        ResetProgressDraft();
    }

    private void RefreshProgressEntries(WorkTaskItem task)
    {
        ProgressEntries = task.ProgressEntries
            .OrderByDescending(entry => entry.EntryDate)
            .ThenByDescending(entry => entry.CreatedAt)
            .ToList();
        AddProgressCommand.NotifyCanExecuteChanged();
    }

    private void ClearDueDate()
    {
        Editor.DueDate = null;
    }

    private void ResetProgressDraft()
    {
        NewProgressDate = DateTimeOffset.Now;
        NewProgressText = string.Empty;
        NewIssueText = string.Empty;
        NewNextStepText = string.Empty;
        NewNeedFollowUp = true;
        NewIsKeyMilestone = false;
        AddProgressCommand.NotifyCanExecuteChanged();
    }

    private void ApplyPendingNavigation()
    {
        var request = _state.ConsumeTaskNavigation();
        if (request is null)
        {
            return;
        }

        SelectedScopeFilter = request.ScopeFilter;
        SelectedProjectFilterId = request.ApplyProjectFilter ? request.ProjectId : Guid.Empty;

        if (request.TaskId.HasValue)
        {
            SelectedTask = _state.FindTask(request.TaskId.Value);
        }

        SelectedInspectorTabIndex = request.InspectorTabIndex;
    }
}
