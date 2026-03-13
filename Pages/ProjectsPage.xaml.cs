using Microsoft.UI.Xaml.Controls;
using WorkClosure.ViewModels;

namespace WorkClosure.Pages;

public sealed partial class ProjectsPage : Page
{
    public ProjectsViewModel ViewModel { get; } = new();

    public ProjectsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
