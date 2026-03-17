using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinRT.Interop;
using WorkClosure.ViewModels;

namespace WorkClosure.Dialogs;

public sealed partial class TaskEditorDialog : Window
{
    private const uint WmSetIcon = 0x0080;
    private const int IconSmall = 0;
    private const int IconBig = 1;
    private const uint ImageIcon = 1;
    private const uint LoadFromFile = 0x00000010;

    private readonly TasksViewModel _viewModel;

    public TaskEditorDialog(TasksViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        RootGrid.DataContext = viewModel;
        Title = "WorkSolo - 事项";
        Activated += OnActivated;
        ConfigureWindow();
        ApplyWindowIcon();
    }

    public void FocusCurrentSection()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            if (_viewModel.IsProgressSelected)
            {
                ProgressSectionAnchor.StartBringIntoView();
                return;
            }

            BasicSectionAnchor.StartBringIntoView();
        });
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var saved = await _viewModel.SaveDialogAsync();
        if (saved)
        {
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        FocusCurrentSection();
    }

    private void ConfigureWindow()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new SizeInt32(1180, 900));

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsMaximizable = true;
            presenter.IsMinimizable = true;
        }
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
