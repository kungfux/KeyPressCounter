namespace MWH.KeyPressCounter;

using Gma.System.MouseKeyHook;
using System.Timers;

public class CustomApplicationContext : ApplicationContext
{
    private readonly string _dailyLogFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DailySummaryLog.txt");

    private readonly string _logFilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ActivityLog.txt");

    private readonly Counter _keyPressCounter = new();
    private readonly Counter _mouseClickCounter = new();
    private NotifyIcon? _trayIcon;
    private Timer? _dailyLogTimer;
    private Timer? _logTimer;
    private IKeyboardMouseEvents? _globalHook;

    public CustomApplicationContext()
    {
        InitializeContext();
        StartGlobalHooks();
        SetupLogTimer();
        SetupDailyLogTimer();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon?.Dispose();
            _logTimer?.Dispose();
            _dailyLogTimer?.Dispose();
            _globalHook?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeContext()
    {
        _trayIcon = new NotifyIcon()
        {
            Icon = new Icon("favicon.ico"),
            ContextMenuStrip = CreateContextMenu(),
            Visible = true,
            Text = "Double Click Icon for Stats"
        };

        _trayIcon.DoubleClick += TrayIcon_DoubleClick;
    }

    private void StartGlobalHooks()
    {
        _globalHook = Hook.GlobalEvents();
        _globalHook.KeyPress += (sender, e) => _keyPressCounter.Increment();
        _globalHook.MouseClick += (sender, e) => _mouseClickCounter.Increment();
    }

    private void SetupLogTimer()
    {
        _logTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
        _logTimer.Elapsed += LogActivity;
        _logTimer.AutoReset = true;
        _logTimer.Enabled = true;
    }

    private void SetupDailyLogTimer()
    {
        _dailyLogTimer = new Timer(GetRemainingTimeUntilEndOfDay().TotalMilliseconds);
        _dailyLogTimer.Elapsed += LogDailySummary;
        _dailyLogTimer.AutoReset = false;
        _dailyLogTimer.Enabled = true;
    }

    private ContextMenuStrip CreateContextMenu()
    {
        ContextMenuStrip menu = new();
        ToolStripMenuItem exitItem = new("Exit", null, Exit_Click);
        menu.Items.Add(exitItem);

        ToolStripMenuItem statsItem = new("Stats", null, TrayIcon_DoubleClick);
        menu.Items.Add(statsItem);

        return menu;
    }

    private static void Exit_Click(object? sender, EventArgs? e)
    {
        Application.Exit();
    }

    private static TimeSpan GetRemainingTimeUntilEndOfDay()
    {
        var now = DateTime.Now;
        var endOfDay = new DateTime(
            new DateOnly(now.Year, now.Month, now.Day),
            new TimeOnly(23, 59, 59),
            DateTimeKind.Local);
        return endOfDay - now;
    }

    private void LogActivity(object? sender, ElapsedEventArgs? e)
    {
        _keyPressCounter.UpdateIntervalMetrics();
        _mouseClickCounter.UpdateIntervalMetrics();

        var log =
            $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Keystrokes: {_keyPressCounter.TotalCount}, Mouse Clicks: {_mouseClickCounter.TotalCount}, Max Keystrokes/Min: {_keyPressCounter.MaxPerInterval}, Max Clicks/Min: {_mouseClickCounter.MaxPerInterval}";
        File.AppendAllText(_logFilePath, $"{log}{Environment.NewLine}");
    }

    private void LogDailySummary(object? sender, ElapsedEventArgs? e)
    {
        var minutesInDay = TimeSpan.FromDays(1).TotalMinutes;
        var averageClicksPerMinute = _mouseClickCounter.TotalCount / minutesInDay;
        var log =
            $"{DateTime.Now:yyyy-MM-dd}: Total Keystrokes: {_keyPressCounter.TotalCount}, Total Mouse Clicks: {_mouseClickCounter.TotalCount}, Avg Clicks/Min: {averageClicksPerMinute:F2}, Longest No Click Period: {_mouseClickCounter.LongestIntervalWithoutIncrement} minutes";
        File.AppendAllText(_dailyLogFilePath, $"{log}{Environment.NewLine}");

        _keyPressCounter.ResetTotalMetrics();
        _mouseClickCounter.ResetTotalMetrics();

        SetupDailyLogTimer();
    }

    private void TrayIcon_DoubleClick(object? sender, EventArgs? e)
    {
        UpdateTrayIconText();
    }

    private void UpdateTrayIconText()
    {
        var log = $"\n Keystrokes: {_keyPressCounter} \n Mouse Clicks: {_mouseClickCounter}";
        _trayIcon?.ShowBalloonTip(5000, "KeyPressCounter Stats", $"{log}", ToolTipIcon.Info);
    }
}