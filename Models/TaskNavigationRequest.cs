namespace WorkClosure.Models;

public sealed class TaskNavigationRequest
{
    public string ScopeFilter { get; init; } = "all";

    public bool ApplyProjectFilter { get; init; }

    public Guid? ProjectId { get; init; }

    public Guid? TaskId { get; init; }

    public int InspectorTabIndex { get; init; }
}
