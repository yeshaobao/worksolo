using System.Text.Json.Serialization;
using WorkClosure.Helpers;

namespace WorkClosure.Models;

public sealed class WorkTaskItem : ObservableObject
{
    private string _title = string.Empty;
    private Guid? _projectId;
    private string _projectName = "未分类";
    private WorkTaskStatus _status = WorkTaskStatus.NotStarted;
    private TaskAnomaly _anomaly = TaskAnomaly.None;
    private string _anomalyNote = string.Empty;
    private DateTimeOffset _createdAt = DateTimeOffset.Now;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _dueDate;
    private DateTimeOffset? _completedAt;
    private DateTimeOffset _updatedAt = DateTimeOffset.Now;

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public Guid? ProjectId
    {
        get => _projectId;
        set
        {
            if (SetProperty(ref _projectId, value))
            {
                OnPropertyChanged(nameof(HasProject));
            }
        }
    }

    [JsonIgnore]
    public string ProjectName
    {
        get => _projectName;
        set => SetProperty(ref _projectName, value);
    }

    public WorkTaskStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                ApplyLifecycleRules();
                NotifyComputedChanged();
            }
        }
    }

    public TaskAnomaly Anomaly
    {
        get => _anomaly;
        set
        {
            if (SetProperty(ref _anomaly, value))
            {
                NotifyComputedChanged();
            }
        }
    }

    public string AnomalyNote
    {
        get => _anomalyNote;
        set => SetProperty(ref _anomalyNote, value);
    }

    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        set
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
        set
        {
            if (SetProperty(ref _startedAt, value))
            {
                OnPropertyChanged(nameof(StartedText));
            }
        }
    }

    public DateTimeOffset? DueDate
    {
        get => _dueDate;
        set
        {
            if (SetProperty(ref _dueDate, value))
            {
                NotifyComputedChanged();
            }
        }
    }

    public DateTimeOffset? CompletedAt
    {
        get => _completedAt;
        set
        {
            if (SetProperty(ref _completedAt, value))
            {
                NotifyComputedChanged();
                OnPropertyChanged(nameof(CompletedText));
            }
        }
    }

    public DateTimeOffset UpdatedAt
    {
        get => _updatedAt;
        set => SetProperty(ref _updatedAt, value);
    }

    [JsonIgnore]
    public bool HasProject => ProjectId.HasValue;

    [JsonIgnore]
    public bool IsClosed => Status is WorkTaskStatus.Completed or WorkTaskStatus.Cancelled;

    [JsonIgnore]
    public bool IsOverdue => !IsClosed && DueDate.HasValue && DueDate.Value.Date < DateTimeOffset.Now.Date;

    [JsonIgnore]
    public string StatusText => Status switch
    {
        WorkTaskStatus.NotStarted => "未开始",
        WorkTaskStatus.InProgress => "进行中",
        WorkTaskStatus.Completed => "已完成",
        WorkTaskStatus.Cancelled => "已取消",
        _ => "未知"
    };

    [JsonIgnore]
    public string AnomalyText => Anomaly switch
    {
        TaskAnomaly.None => "正常",
        TaskAnomaly.Delayed => "延期",
        TaskAnomaly.Blocked => "阻塞",
        TaskAnomaly.CancelledReason => "取消原因",
        TaskAnomaly.Other => "其他异常",
        _ => "正常"
    };

    [JsonIgnore]
    public string DueText => DueDate.HasValue ? $"截止：{DueDate.Value:yyyy-MM-dd}" : "截止：未设置";

    [JsonIgnore]
    public string CreatedText => $"创建：{CreatedAt:yyyy-MM-dd HH:mm}";

    [JsonIgnore]
    public string StartedText => StartedAt.HasValue ? $"开始：{StartedAt.Value:yyyy-MM-dd HH:mm}" : "开始：未设置";

    [JsonIgnore]
    public string CompletedText => CompletedAt.HasValue ? $"完成：{CompletedAt.Value:yyyy-MM-dd HH:mm}" : "完成：未设置";

    public WorkTaskItem Clone()
    {
        return new WorkTaskItem
        {
            Id = Id,
            Title = Title,
            ProjectId = ProjectId,
            ProjectName = ProjectName,
            Status = Status,
            Anomaly = Anomaly,
            AnomalyNote = AnomalyNote,
            CreatedAt = CreatedAt,
            StartedAt = StartedAt,
            DueDate = DueDate,
            CompletedAt = CompletedAt,
            UpdatedAt = UpdatedAt
        };
    }

    public void ApplyLifecycleRules()
    {
        if (Status == WorkTaskStatus.InProgress && StartedAt is null)
        {
            StartedAt = DateTimeOffset.Now;
        }

        if (Status == WorkTaskStatus.Completed && CompletedAt is null)
        {
            CompletedAt = DateTimeOffset.Now;
        }

        if (Status != WorkTaskStatus.Completed)
        {
            CompletedAt = null;
        }
    }

    public void Touch()
    {
        UpdatedAt = DateTimeOffset.Now;
    }

    private void NotifyComputedChanged()
    {
        OnPropertyChanged(nameof(IsClosed));
        OnPropertyChanged(nameof(IsOverdue));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(AnomalyText));
        OnPropertyChanged(nameof(DueText));
    }
}
