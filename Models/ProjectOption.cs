namespace WorkClosure.Models;

public sealed class ProjectOption
{
    public Guid? Id { get; init; }

    public required string Name { get; init; }

    public bool IsAllOption { get; init; }

    public bool IsUncategorizedOption { get; init; }
}
