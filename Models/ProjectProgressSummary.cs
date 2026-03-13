namespace WorkClosure.Models;

public sealed class ProjectProgressSummary
{
    public Guid? ProjectId { get; init; }

    public required string ProjectName { get; init; }

    public int Total { get; init; }

    public int Completed { get; init; }

    public int Open { get; init; }

    public int Overdue { get; init; }
}
