using Microsoft.Win32;

namespace TwentyTwentyTray;

internal static class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunValueName = "TwentyTwentyTray";

    public static void SetEnabled(bool enabled)
    {
        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath);

        if (runKey is null)
        {
            return;
        }

        if (!enabled)
        {
            runKey.DeleteValue(RunValueName, false);
            runKey.Flush();
            return;
        }

        var executablePath = Environment.ProcessPath ?? Application.ExecutablePath;
        runKey.SetValue(RunValueName, $"\"{executablePath}\"");
        runKey.Flush();
    }
}