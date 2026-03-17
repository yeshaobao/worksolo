using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WorkClosure.Models;
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

    private void OpenPendingTasks_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenPendingInspector();
    }

    private void OpenOverdueTasks_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenOverdueInspector();
    }

    private void OpenExceptionTasks_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenExceptionInspector();
    }

    private void OpenRecentTasks_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenRecentInspector();
    }

    private void InspectorTaskButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not WorkTaskItem task)
        {
            return;
        }

        ViewModel.QueueTaskNavigation(ViewModel.CreateInspectorNavigationRequest(task));
        Frame.Navigate(typeof(TasksPage));
    }
}
