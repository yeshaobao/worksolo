using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WorkClosure.Models;
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

    private void CreatedCard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenCreatedInspector();
    }

    private void CompletedCard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenCompletedInspector();
    }

    private void OpenCard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenOpenInspector();
    }

    private void OverdueCard_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenOverdueInspector();
    }

    private void ProjectSummaries_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not ProjectProgressSummary summary)
        {
            return;
        }

        ViewModel.OpenProjectInspector(summary);
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
