using System.Diagnostics;
using Microsoft.UI.Xaml.Controls;
using WorkClosure.ViewModels;

namespace WorkClosure.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; } = new();

    public SettingsPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private void OpenDataFolder_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var folder = Path.GetDirectoryName(ViewModel.DataPath);
        if (string.IsNullOrWhiteSpace(folder))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    private void OpenManual_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var root = Directory.GetParent(Path.GetDirectoryName(ViewModel.DataPath)!)?.FullName;
        if (string.IsNullOrWhiteSpace(root))
        {
            return;
        }

        var manualPath = Path.Combine(root, "使用说明.md");
        if (!File.Exists(manualPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = manualPath,
            UseShellExecute = true
        });
    }
}
