using Microsoft.UI.Xaml.Controls;
using WorkClosure.ViewModels;

namespace WorkClosure.Pages;

public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; } = new();

    public DashboardPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public string ResolveProjectName(Guid? projectId)
    {
        return ViewModel.ResolveProjectName(projectId);
    }
}
