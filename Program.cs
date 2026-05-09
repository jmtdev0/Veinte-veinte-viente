namespace TwentyTwentyTray;

static class Program
{
    private static Mutex? singleInstanceMutex;

    [STAThread]
    static void Main()
    {
        singleInstanceMutex = new Mutex(true, @"Local\TwentyTwentyTray.Singleton", out var createdNew);
        if (!createdNew)
        {
            singleInstanceMutex.Dispose();
            return;
        }

        try
        {
            Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.OnActivated += OnNotificationInvoked;

            var settingsStore = new SettingsStore();
            var settings = settingsStore.Load();
            settingsStore.Save(settings);
            StartupManager.SetEnabled(settings.LaunchAtStartup);

            ApplicationConfiguration.Initialize();

            using var applicationContext = new TrayApplicationContext();
            Application.Run(applicationContext);
        }
        finally
        {
            Microsoft.Toolkit.Uwp.Notifications.ToastNotificationManagerCompat.OnActivated -= OnNotificationInvoked;

            singleInstanceMutex.ReleaseMutex();
            singleInstanceMutex.Dispose();
        }
    }

    private static void OnNotificationInvoked(
        Microsoft.Toolkit.Uwp.Notifications.ToastNotificationActivatedEventArgsCompat args)
    {
        TrayApplicationContext.Current?.HandleNotificationInvocation(args.Argument);
    }
}