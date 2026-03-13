using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WinRT.Interop;
using WorkClosure.Pages;
using WorkClosure.ViewModels;

namespace WorkClosure;

public sealed partial class MainWindow : Window
{
    private const uint WmSetIcon = 0x0080;
    private const int IconSmall = 0;
    private const int IconBig = 1;
    private const uint ImageIcon = 1;
    private const uint LoadFromFile = 0x00000010;

    private readonly App _app = (App)Application.Current;

    public MainWindowViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        Title = "WorkSolo";
        RootGrid.DataContext = ViewModel;
        ApplyWindowIcon();
        ViewModel.Initialize();
        _app.State.StateChanged += OnStateChanged;
        ApplyThemePreference();
        SelectInitialPage();
    }

    private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer?.Tag is string tag)
        {
            NavigateTo(tag);
        }
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        ApplyThemePreference();
    }

    private void SelectInitialPage()
    {
        var initialTag = _app.State.Preferences.DefaultPageTag;
        var allItems = ShellNavigation.MenuItems.Concat(ShellNavigation.FooterMenuItems).OfType<NavigationViewItem>();
        ShellNavigation.SelectedItem = allItems.FirstOrDefault(item => (string?)item.Tag == initialTag) ?? ShellNavigation.MenuItems[0];
    }

    private void ApplyThemePreference()
    {
        RootGrid.RequestedTheme = _app.State.Preferences.ThemeMode switch
        {
            "light" => ElementTheme.Light,
            "dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }

    private void ApplyWindowIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "WorkSolo.ico");
        if (!File.Exists(iconPath))
        {
            return;
        }

        var hwnd = WindowNative.GetWindowHandle(this);
        var largeIcon = LoadImage(IntPtr.Zero, iconPath, ImageIcon, 32, 32, LoadFromFile);
        var smallIcon = LoadImage(IntPtr.Zero, iconPath, ImageIcon, 16, 16, LoadFromFile);

        if (largeIcon != IntPtr.Zero)
        {
            SendMessage(hwnd, WmSetIcon, (IntPtr)IconBig, largeIcon);
        }

        if (smallIcon != IntPtr.Zero)
        {
            SendMessage(hwnd, WmSetIcon, (IntPtr)IconSmall, smallIcon);
        }
    }

    private void NavigateTo(string tag)
    {
        var pageType = tag switch
        {
            "tasks" => typeof(TasksPage),
            "projects" => typeof(ProjectsPage),
            "summary" => typeof(SummaryPage),
            "settings" => typeof(SettingsPage),
            _ => typeof(DashboardPage)
        };

        if (ContentFrame.CurrentSourcePageType != pageType)
        {
            ContentFrame.Navigate(pageType);
        }
    }

    [DllImport("user32.dll", EntryPoint = "LoadImageW", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr LoadImage(
        IntPtr hInst,
        string name,
        uint type,
        int cx,
        int cy,
        uint fuLoad);

    [DllImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
