using System.Drawing;
using Microsoft.Toolkit.Uwp.Notifications;

namespace TwentyTwentyTray;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly Icon trayIconImage;
    private readonly SettingsStore settingsStore;
    private readonly Control dispatcher;
    private readonly ContextMenuStrip trayMenu;
    private readonly NotifyIcon notifyIcon;
    private readonly System.Windows.Forms.Timer reminderTimer;
    private readonly ToolStripMenuItem enableRemindersMenuItem;
    private readonly ToolStripMenuItem launchAtStartupMenuItem;
    private AppSettings settings;

    public static TrayApplicationContext? Current { get; private set; }

    public TrayApplicationContext()
    {
        trayIconImage = LoadTrayIcon();
        settingsStore = new SettingsStore();
        settings = settingsStore.Load();

        Current = this;

        dispatcher = new Control();
        _ = dispatcher.Handle;

        enableRemindersMenuItem = new ToolStripMenuItem("Enable reminders");
        enableRemindersMenuItem.Click += (_, _) => SetRemindersEnabled(!settings.RemindersEnabled, true);

        launchAtStartupMenuItem = new ToolStripMenuItem("Launch at startup");
        launchAtStartupMenuItem.Click += (_, _) => SetLaunchAtStartup(!settings.LaunchAtStartup);

        var sendTestNotificationMenuItem = new ToolStripMenuItem("Send test notification");
        sendTestNotificationMenuItem.Click += (_, _) => ShowReminderNotification();

        var exitMenuItem = new ToolStripMenuItem("Exit");
        exitMenuItem.Click += (_, _) => ExitThread();

        trayMenu = new ContextMenuStrip();
        trayMenu.Items.AddRange(
        [
            enableRemindersMenuItem,
            launchAtStartupMenuItem,
            new ToolStripSeparator(),
            sendTestNotificationMenuItem,
            new ToolStripSeparator(),
            exitMenuItem
        ]);

        notifyIcon = new NotifyIcon
        {
            ContextMenuStrip = trayMenu,
            Icon = trayIconImage,
            Text = "20-20 break reminder",
            Visible = true
        };

        reminderTimer = new System.Windows.Forms.Timer();
        reminderTimer.Tick += (_, _) => ShowReminderNotification();

        ApplyLaunchAtStartupSetting();
        ApplyReminderSchedule();
        UpdateMenuState();
        SaveSettings();
    }

    public void HandleNotificationInvocation(string arguments)
    {
        if (dispatcher.IsDisposed || string.IsNullOrWhiteSpace(arguments))
        {
            return;
        }

        dispatcher.BeginInvoke(() => ProcessNotificationArguments(arguments));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Current = null;

            reminderTimer.Stop();
            reminderTimer.Dispose();

            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            trayIconImage.Dispose();

            trayMenu.Dispose();
            dispatcher.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ProcessNotificationArguments(string arguments)
    {
        var parsedArguments = ParseArguments(arguments);
        if (!parsedArguments.TryGetValue("action", out var action))
        {
            return;
        }

        if (string.Equals(action, "disable", StringComparison.OrdinalIgnoreCase))
        {
            SetRemindersEnabled(false, true);
        }
    }

    private void SetRemindersEnabled(bool enabled, bool showConfirmation)
    {
        settings.RemindersEnabled = enabled;
        SaveSettings();
        ApplyReminderSchedule();
        UpdateMenuState();

        if (showConfirmation)
        {
            var title = enabled ? "Reminders enabled" : "Reminders disabled";
            var message = enabled
                ? "You will get a break reminder every 20 minutes."
                : "You can re-enable reminders from the tray menu at any time.";

            ShowStatusNotification(title, message);
        }
    }

    private void SetLaunchAtStartup(bool enabled)
    {
        settings.LaunchAtStartup = enabled;
        SaveSettings();
        ApplyLaunchAtStartupSetting();
        UpdateMenuState();

        var title = enabled ? "Launch at startup enabled" : "Launch at startup disabled";
        var message = enabled
            ? "The app will start automatically when you sign in."
            : "The app will stay off until you launch it manually.";

        ShowStatusNotification(title, message);
    }

    private void ApplyReminderSchedule()
    {
        reminderTimer.Stop();
        reminderTimer.Interval = Math.Max(settings.ReminderIntervalMinutes, 1) * 60 * 1000;

        if (settings.RemindersEnabled)
        {
            reminderTimer.Start();
        }
    }

    private void ApplyLaunchAtStartupSetting()
    {
        StartupManager.SetEnabled(settings.LaunchAtStartup);
    }

    private void UpdateMenuState()
    {
        enableRemindersMenuItem.Checked = settings.RemindersEnabled;
        launchAtStartupMenuItem.Checked = settings.LaunchAtStartup;
    }

    private void ShowReminderNotification()
    {
        ShowNotification(
            "Time to stand up",
            "Take a short break and look away from the screen to rest your eyes.",
            includeDisableButton: true,
            fallbackIcon: ToolTipIcon.Info);
    }

    private void ShowStatusNotification(string title, string message)
    {
        ShowNotification(title, message, includeDisableButton: false, fallbackIcon: ToolTipIcon.Info);
    }

    private void ShowNotification(string title, string message, bool includeDisableButton, ToolTipIcon fallbackIcon)
    {
        try
        {
            var builder = new ToastContentBuilder()
                .AddArgument("source", "twentyTwentyTray")
                .AddText(title)
                .AddText(message)
                .AddText("Use the tray icon to change the reminder status.");

            if (includeDisableButton)
            {
                builder.AddButton("Disable reminders", ToastActivationType.Foreground, "action=disable");
            }

            builder.Show();
        }
        catch
        {
            ShowBalloonTip(title, message, fallbackIcon);
        }
    }

    private void ShowBalloonTip(string title, string message, ToolTipIcon icon)
    {
        notifyIcon.BalloonTipTitle = title;
        notifyIcon.BalloonTipText = message;
        notifyIcon.BalloonTipIcon = icon;
        notifyIcon.ShowBalloonTip(5000);
    }

    private void SaveSettings()
    {
        settingsStore.Save(settings);
    }

    private static Dictionary<string, string> ParseArguments(string arguments)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var segment in arguments.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty;

            values[key] = value;
        }

        return values;
    }

    private static Icon LoadTrayIcon()
    {
        using var extractedIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        return extractedIcon?.Clone() as Icon ?? (Icon)SystemIcons.Information.Clone();
    }
}