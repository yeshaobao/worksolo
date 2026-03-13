using WorkClosure.Helpers;
using WorkClosure.Models;

namespace WorkClosure.ViewModels;

public sealed class TaskEditorViewModel : ObservableObject
{
    private Guid _taskId;
    private string _title = string.Empty;
    private Guid? _selectedProjectId;
    private string _selectedStatusValue = nameof(WorkTaskStatus.NotStarted);
    private string _selectedAnomalyValue = nameof(TaskAnomaly.None);
    private string _anomalyNote = string.Empty;
    private DateTimeOffset? _dueDate;
    private DateTimeOffset _createdAt = DateTimeOffset.Now;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _completedAt;
    private bool _isEditingExisting;
    private IReadOnlyList<ProjectOption> _projectOptions = [];

    public string HeaderText => IsEditingExisting ? "编辑事项" : "新建事项";

    public Guid TaskId
    {
        get => _taskId;
        private set => SetProperty(ref _taskId, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public Guid? SelectedProjectId
    {
        get => _selectedProjectId;
        set => SetProperty(ref _selectedProjectId, value);
    }

    public string SelectedStatusValue
    {
        get => _selectedStatusValue;
        set => SetProperty(ref _selectedStatusValue, value);
    }

    public string SelectedAnomalyValue
    {
        get => _selectedAnomalyValue;
        set => SetProperty(ref _selectedAnomalyValue, value);
    }

    public string AnomalyNote
    {
        get => _anomalyNote;
        set => SetProperty(ref _anomalyNote, value);
    }

    public DateTimeOffset? DueDate
    {
        get => _dueDate;
        set => SetProperty(ref _dueDate, value);
    }

    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        private set
        {
            if (SetProperty(ref _createdAt, value))
            {
                OnPropertyChanged(nameof(CreatedText));
            }
        }
    }

    public DateTimeOffset? StartedAt
    {
        get => _startedAt;
        private set
        {
            if (SetProperty(ref _startedAt, value))
            {
                OnPropertyChanged(nameof(StartedText));
            }
        }
    }

    public DateTimeOffset? CompletedAt
    {
        get => _completedAt;
        private set
        {
            if (SetProperty(ref _completedAt, value))
            {
                OnPropertyChanged(nameof(CompletedText));
            }
        }
    }

    public bool IsEditingExisting
    {
        get => _isEditingExisting;
        private set
        {
            if (SetProperty(ref _isEditingExisting, value))
            {
                OnPropertyChanged(nameof(HeaderText));
            }
        }
    }

    public IReadOnlyList<ProjectOption> ProjectOptions
    {
        get => _projectOptions;
        private set => SetProperty(ref _projectOptions, value);
    }

    public string CreatedText => $"创建时间：{CreatedAt:yyyy-MM-dd HH:mm}";

    public string StartedText => StartedAt.HasValue ? $"开始时间：{StartedAt.Value:yyyy-MM-dd HH:mm}" : "开始时间：未设置";

    public string CompletedText => CompletedAt.HasValue ? $"完成时间：{CompletedAt.Value:yyyy-MM-dd HH:mm}" : "完成时间：未设置";

    public IReadOnlyList<OptionItem> StatusOptions { get; } =
    [
        new() { Label = "未开始", Value = nameof(WorkTaskStatus.NotStarted) },
        new() { Label = "进行中", Value = nameof(WorkTaskStatus.InProgress) },
        new() { Label = "已完成", Value = nameof(WorkTaskStatus.Completed) },
        new() { Label = "已取消", Value = nameof(WorkTaskStatus.Cancelled) }
    ];

    public IReadOnlyList<OptionItem> AnomalyOptions { get; } =
    [
        new() { Label = "正常", Value = nameof(TaskAnomaly.None) },
        new() { Label = "延期", Value = nameof(TaskAnomaly.Delayed) },
        new() { Label = "阻塞", Value = nameof(TaskAnomaly.Blocked) },
        new() { Label = "取消原因", Value = nameof(TaskAnomaly.CancelledReason) },
        new() { Label = "其他异常", Value = nameof(TaskAnomaly.Other) }
    ];

    public void RefreshProjectOptions(IReadOnlyList<ProjectOption> options)
    {
        ProjectOptions = options;
    }

    public void LoadNew()
    {
        TaskId = Guid.NewGuid();
        Title = string.Empty;
        SelectedProjectId = null;
        SelectedStatusValue = nameof(WorkTaskStatus.NotStarted);
        SelectedAnomalyValue = nameof(TaskAnomaly.None);
        AnomalyNote = string.Empty;
        DueDate = DateTimeOffset.Now;
        CreatedAt = DateTimeOffset.Now;
        StartedAt = null;
        CompletedAt = null;
        IsEditingExisting = false;
    }

    public void LoadFrom(WorkTaskItem task)
    {
        TaskId = task.Id;
        Title = task.Title;
        SelectedProjectId = task.ProjectId;
        SelectedStatusValue = task.Status.ToString();
        SelectedAnomalyValue = task.Anomaly.ToString();
        AnomalyNote = task.AnomalyNote;
        DueDate = task.DueDate;
        CreatedAt = task.CreatedAt;
        StartedAt = task.StartedAt;
        CompletedAt = task.CompletedAt;
        IsEditingExisting = true;
    }

    public WorkTaskItem ToTask()
    {
        return new WorkTaskItem
        {
            Id = TaskId,
            Title = Title.Trim(),
            ProjectId = SelectedProjectId,
            Status = Enum.Parse<WorkTaskStatus>(SelectedStatusValue),
            Anomaly = Enum.Parse<TaskAnomaly>(SelectedAnomalyValue),
            AnomalyNote = AnomalyNote.Trim(),
            CreatedAt = CreatedAt,
            StartedAt = StartedAt,
            DueDate = DueDate,
            CompletedAt = CompletedAt,
            UpdatedAt = DateTimeOffset.Now
        };
    }
}
