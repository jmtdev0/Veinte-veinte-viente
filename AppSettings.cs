namespace TwentyTwentyTray;

internal sealed class AppSettings
{
    public bool RemindersEnabled { get; set; } = true;

    public bool LaunchAtStartup { get; set; } = true;

    public int ReminderIntervalMinutes { get; set; } = 20;
}