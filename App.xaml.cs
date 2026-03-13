using Microsoft.UI.Xaml;
using WorkClosure.Services;

namespace WorkClosure;

public partial class App : Application
{
    private Window? _window;
    private readonly string _logPath;

    public App()
    {
        _logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "WorkSolo",
            "startup.log");
        Log("App:ctor before InitializeComponent");
        InitializeComponent();
        Log("App:ctor after InitializeComponent");
        State = new AppState(new StorageService());
        Log("App:state created");
        UnhandledException += OnUnhandledException;
    }

    public AppState State { get; }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Log("OnLaunched:start");
            await State.InitializeAsync();
            Log("OnLaunched:state initialized");
            _window = new MainWindow();
            Log("OnLaunched:window created");
            _window.Activate();
            Log("OnLaunched:window activated");
        }
        catch (Exception ex)
        {
            Log($"OnLaunched:exception {ex}");
            throw;
        }
    }

    private void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        Log($"UnhandledException:{e.Exception}");
    }

    private void Log(string message)
    {
        try
        {
            var folder = Path.GetDirectoryName(_logPath)!;
            Directory.CreateDirectory(folder);
            File.AppendAllText(_logPath, $"{DateTimeOffset.Now:O} {message}{Environment.NewLine}");
        }
        catch
        {
        }
    }
}
