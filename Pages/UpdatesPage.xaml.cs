using Microsoft.UI.Xaml.Controls;
using WorkClosure.ViewModels;

namespace WorkClosure.Pages;

public sealed partial class UpdatesPage : Page
{
    public UpdatesViewModel ViewModel { get; } = new();

    public UpdatesPage()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
