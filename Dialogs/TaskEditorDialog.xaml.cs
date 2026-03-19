using System.Runtime.InteropServices;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using WinRT.Interop;
using WorkClosure.Models;
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
    private IntPtr _hwnd;
    private AppWindow? _appWindow;

    public event EventHandler? CloseRequested;

    public bool IsCloseRequested { get; private set; }

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
        await _viewModel.SaveDialogAsync();
    }

    private async void SaveAndCloseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var saved = await _viewModel.SaveDialogAsync();
            if (!saved)
            {
                return;
            }

            await Task.Delay(120);
            RequestClose();
        }
        catch
        {
            throw;
        }
    }

    private void EditProgressButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not TaskProgressEntry entry)
        {
            return;
        }

        _viewModel.LoadProgressEntryForEditing(entry.Id);
        ProgressSectionAnchor.StartBringIntoView();
    }

    private async void DeleteProgressButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is not TaskProgressEntry entry)
        {
            return;
        }

        await _viewModel.DeleteProgressEntryAsync(entry.Id);
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        RequestClose();
    }

    private void OnActivated(object sender, WindowActivatedEventArgs args)
    {
        FocusCurrentSection();
    }

    private void ConfigureWindow()
    {
        _hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(_hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        _appWindow.Resize(new SizeInt32(1180, 900));

        if (_appWindow.Presenter is OverlappedPresenter presenter)
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

        var largeIcon = LoadImage(IntPtr.Zero, iconPath, ImageIcon, 32, 32, LoadFromFile);
        var smallIcon = LoadImage(IntPtr.Zero, iconPath, ImageIcon, 16, 16, LoadFromFile);

        if (largeIcon != IntPtr.Zero)
        {
            SendMessage(_hwnd, WmSetIcon, (IntPtr)IconBig, largeIcon);
        }

        if (smallIcon != IntPtr.Zero)
        {
            SendMessage(_hwnd, WmSetIcon, (IntPtr)IconSmall, smallIcon);
        }
    }

    private void RequestClose()
    {
        if (IsCloseRequested)
        {
            return;
        }

        IsCloseRequested = true;
        CloseRequested?.Invoke(this, EventArgs.Empty);

        DispatcherQueue.TryEnqueue(() =>
        {
            try
            {
                if (_appWindow is not null)
                {
                    _appWindow.Destroy();
                    return;
                }
            }
            catch
            {
            }

            try
            {
                Close();
            }
            catch
            {
            }
        });
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
