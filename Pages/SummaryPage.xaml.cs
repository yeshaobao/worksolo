using Microsoft.UI.Xaml.Controls;
using WorkClosure.ViewModels;

namespace WorkClosure.Pages;

public sealed partial class SummaryPage : Page
{
    public SummaryViewModel ViewModel { get; } = new();

    public SummaryPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public string ResolveProjectName(Guid? projectId)
    {
        return ViewModel.ResolveProjectName(projectId);
    }
}
