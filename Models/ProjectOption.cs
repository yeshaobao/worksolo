namespace WorkClosure.Models;

public sealed class ProjectOption
{
    public const string AllFilterValue = "all";
    public const string UncategorizedFilterValue = "uncategorized";
    private const string ProjectFilterPrefix = "project:";

    public Guid? Id { get; init; }

    public required string Name { get; init; }

    public bool IsAllOption { get; init; }

    public bool IsUncategorizedOption { get; init; }

    public string FilterValue
    {
        get
        {
            if (IsAllOption)
            {
                return AllFilterValue;
            }

            if (IsUncategorizedOption)
            {
                return UncategorizedFilterValue;
            }

            return ToFilterValue(Id);
        }
    }

    public static string ToFilterValue(Guid? projectId)
    {
        return projectId.HasValue
            ? $"{ProjectFilterPrefix}{projectId.Value:N}"
            : UncategorizedFilterValue;
    }

    public static string NormalizeFilterValue(string? filterValue)
    {
        if (string.IsNullOrWhiteSpace(filterValue))
        {
            return AllFilterValue;
        }

        var normalizedFilterValue = filterValue.Trim();

        return normalizedFilterValue == AllFilterValue ||
               normalizedFilterValue == UncategorizedFilterValue ||
               TryGetProjectId(normalizedFilterValue, out _)
            ? normalizedFilterValue
            : AllFilterValue;
    }

    public static bool TryGetProjectId(string? filterValue, out Guid projectId)
    {
        projectId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(filterValue) ||
            !filterValue.StartsWith(ProjectFilterPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        return Guid.TryParseExact(filterValue[ProjectFilterPrefix.Length..], "N", out projectId);
    }
}
