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
    private Guid? _selectedProjectFilterId;
    private IReadOnlyList<ProjectOption> _projectFilters = [];
    private IReadOnlyList<WorkTaskItem> _filteredTasks = [];
    private WorkTaskItem? _selectedTask;

    public TasksViewModel()
    {
        Editor = new TaskEditorViewModel();
        QuickAddCommand = new RelayCommand(async () => await QuickAddAsync(), () => !string.IsNullOrWhiteSpace(QuickAddTitle));
        SaveTaskCommand = new RelayCommand(async () => await SaveTaskAsync(), CanSaveTask);
        NewTaskCommand = new RelayCommand(NewTask);
        StartTaskCommand = new RelayCommand(async () => await UpdateSelectedStatusAsync(WorkTaskStatus.InProgress), () => SelectedTask is not null);
        CompleteTaskCommand = new RelayCommand(async () => await UpdateSelectedStatusAsync(WorkTaskStatus.Completed), () => SelectedTask is not null);
        CancelTaskCommand = new RelayCommand(async () => await UpdateSelectedStatusAsync(WorkTaskStatus.Cancelled), () => SelectedTask is not null);
        DeleteTaskCommand = new RelayCommand(async () => await DeleteSelectedTaskAsync(), () => SelectedTask is not null);
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
    }

    public TaskEditorViewModel Editor { get; }

    public IReadOnlyList<OptionItem> ScopeOptions { get; } =
    [
        new() { Label = "全部", Value = "all" },
        new() { Label = "未闭环", Value = "open" },
        new() { Label = "已闭环", Value = "closed" },
        new() { Label = "今天到期", Value = "today" },
        new() { Label = "已逾期", Value = "overdue" },
        new() { Label = "异常事项", Value = "anomaly" }
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
                    Editor.LoadNew();
                }
                else
                {
                    Editor.LoadFrom(value);
                }

                SaveTaskCommand.NotifyCanExecuteChanged();
                StartTaskCommand.NotifyCanExecuteChanged();
                CompleteTaskCommand.NotifyCanExecuteChanged();
                CancelTaskCommand.NotifyCanExecuteChanged();
                DeleteTaskCommand.NotifyCanExecuteChanged();
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

    public RelayCommand QuickAddCommand { get; }
    public RelayCommand SaveTaskCommand { get; }
    public RelayCommand NewTaskCommand { get; }
    public RelayCommand StartTaskCommand { get; }
    public RelayCommand CompleteTaskCommand { get; }
    public RelayCommand CancelTaskCommand { get; }
    public RelayCommand DeleteTaskCommand { get; }

    public string ResolveProjectName(Guid? projectId)
    {
        return _state.GetProjectName(projectId);
    }

    public void Refresh()
    {
        ProjectFilters = _state.GetProjectOptions();
        Editor.RefreshProjectOptions(ProjectFilters);

        IEnumerable<WorkTaskItem> query = _state.Tasks;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            query = query.Where(task => task.Title.Contains(SearchText.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        query = SelectedScopeFilter switch
        {
            "open" => query.Where(task => !task.IsClosed),
            "closed" => query.Where(task => task.IsClosed),
            "today" => query.Where(task => !task.IsClosed && task.DueDate.HasValue && task.DueDate.Value.Date == DateTimeOffset.Now.Date),
            "overdue" => query.Where(task => task.IsOverdue),
            "anomaly" => query.Where(task => task.Anomaly != TaskAnomaly.None || task.IsOverdue),
            _ => query
        };

        if (SelectedProjectFilterId.HasValue)
        {
            query = query.Where(task => task.ProjectId == SelectedProjectFilterId);
        }

        FilteredTasks = query
            .OrderBy(task => task.IsClosed ? 1 : 0)
            .ThenBy(task => task.DueDate ?? DateTimeOffset.MaxValue)
            .ThenByDescending(task => task.UpdatedAt)
            .ToList();

        if (SelectedTask is not null)
        {
            SelectedTask = FilteredTasks.FirstOrDefault(task => task.Id == SelectedTask.Id);
        }
        else
        {
            DeleteTaskCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanSaveTask()
    {
        return !string.IsNullOrWhiteSpace(Editor.Title);
    }

    private async Task QuickAddAsync()
    {
        var title = QuickAddTitle;
        QuickAddTitle = string.Empty;
        await _state.AddQuickTaskAsync(title);
    }

    private async Task SaveTaskAsync()
    {
        await _state.SaveTaskAsync(Editor.ToTask());
        SelectedTask = _state.Tasks.FirstOrDefault(task => task.Id == Editor.TaskId);
    }

    private async Task UpdateSelectedStatusAsync(WorkTaskStatus status)
    {
        if (SelectedTask is null)
        {
            return;
        }

        await _state.UpdateTaskStatusAsync(SelectedTask, status);
        Editor.LoadFrom(SelectedTask);
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
    }

    private void NewTask()
    {
        SelectedTask = null;
        Editor.LoadNew();
    }
}
