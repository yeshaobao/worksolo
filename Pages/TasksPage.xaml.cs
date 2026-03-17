using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using WorkClosure.Dialogs;
using WorkClosure.ViewModels;

namespace WorkClosure.Pages;

public sealed partial class TasksPage : Page
{
    private TaskEditorDialog? _taskEditorWindow;

    public TasksViewModel ViewModel { get; } = new();

    public TasksPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        UpdateBackButtonVisibility();
    }

    private void BackToPreviousButton_Click(object sender, RoutedEventArgs e)
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
            return;
        }

        Frame?.Navigate(typeof(DashboardPage));
    }

    private void NewTaskDialogButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.PrepareNewTaskForDialog();
        ShowTaskEditorDialog();
    }

    private void EditTaskDialogButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.PrepareSelectedTaskForDialog())
        {
            return;
        }

        ShowTaskEditorDialog();
    }

    private void RecordProgressDialogButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.PrepareSelectedTaskForDialog(openProgressSection: true))
        {
            return;
        }

        ShowTaskEditorDialog();
    }

    private void UpdateBackButtonVisibility()
    {
        BackToPreviousButton.Visibility = Frame?.CanGoBack == true
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ShowTaskEditorDialog()
    {
        if (_taskEditorWindow is not null)
        {
            _taskEditorWindow.FocusCurrentSection();
            _taskEditorWindow.Activate();
            return;
        }

        _taskEditorWindow = new TaskEditorDialog(ViewModel);
        _taskEditorWindow.Closed += TaskEditorWindow_Closed;
        _taskEditorWindow.Activate();
        UpdateBackButtonVisibility();
    }

    private void TaskEditorWindow_Closed(object sender, WindowEventArgs args)
    {
        _taskEditorWindow = null;
    }
}
