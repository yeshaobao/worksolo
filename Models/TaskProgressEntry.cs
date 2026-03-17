using System.Text.Json.Serialization;

namespace WorkClosure.Models;

public sealed class TaskProgressEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTimeOffset EntryDate { get; set; } = DateTimeOffset.Now;

    public string ProgressText { get; set; } = string.Empty;

    public string IssueText { get; set; } = string.Empty;

    public string NextStepText { get; set; } = string.Empty;

    public bool NeedFollowUp { get; set; }

    public bool IsKeyMilestone { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;

    [JsonIgnore]
    public string EntryDateText => EntryDate.ToString("yyyy-MM-dd");

    [JsonIgnore]
    public string FollowUpText => NeedFollowUp ? "需跟进" : string.Empty;

    [JsonIgnore]
    public string MilestoneText => IsKeyMilestone ? "关键节点" : string.Empty;
}
