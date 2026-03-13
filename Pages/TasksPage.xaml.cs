using Microsoft.UI.Xaml.Controls;
using WorkClosure.ViewModels;

namespace WorkClosure.Pages;

public sealed partial class TasksPage : Page
{
    public TasksViewModel ViewModel { get; } = new();

    public TasksPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public string ResolveProjectName(Guid? projectId)
    {
        return ViewModel.ResolveProjectName(projectId);
    }
}
