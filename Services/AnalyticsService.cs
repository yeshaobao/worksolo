using WorkClosure.Models;

namespace WorkClosure.Services;

public static class AnalyticsService
{
    public static int CountOpen(IEnumerable<WorkTaskItem> tasks)
    {
        return tasks.Count(task => !task.IsClosed);
    }

    public static int CountOverdue(IEnumerable<WorkTaskItem> tasks)
    {
        return tasks.Count(task => task.IsOverdue);
    }

    public static IReadOnlyList<ProjectProgressSummary> BuildProjectSummaries(
        IEnumerable<ProjectRecord> projects,
        IEnumerable<WorkTaskItem> tasks)
    {
        var taskList = tasks.ToList();
        var summaries = projects
            .Select(project =>
            {
                var projectTasks = taskList.Where(task => task.ProjectId == project.Id).ToList();
                return new ProjectProgressSummary
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name,
                    Total = projectTasks.Count,
                    Completed = projectTasks.Count(task => task.Status == WorkTaskStatus.Completed),
                    Open = projectTasks.Count(task => !task.IsClosed),
                    Overdue = projectTasks.Count(task => task.IsOverdue)
                };
            })
            .OrderByDescending(summary => summary.Open)
            .ThenByDescending(summary => summary.Overdue)
            .ThenBy(summary => summary.ProjectName)
            .ToList();

        var uncategorized = taskList.Where(task => task.ProjectId is null).ToList();
        if (uncategorized.Count > 0)
        {
            summaries.Add(new ProjectProgressSummary
            {
                ProjectId = null,
                ProjectName = "未分类",
                Total = uncategorized.Count,
                Completed = uncategorized.Count(task => task.Status == WorkTaskStatus.Completed),
                Open = uncategorized.Count(task => !task.IsClosed),
                Overdue = uncategorized.Count(task => task.IsOverdue)
            });
        }

        return summaries;
    }
}
