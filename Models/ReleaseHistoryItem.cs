namespace WorkClosure.Models;

public sealed class ReleaseHistoryItem
{
    public string Version { get; init; } = string.Empty;

    public string ReleaseDate { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public string TagText { get; init; } = string.Empty;
}
