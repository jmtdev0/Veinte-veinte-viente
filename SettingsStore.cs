using System.Text.Json;

namespace TwentyTwentyTray;

internal sealed class SettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    private readonly string settingsPath;

    public SettingsStore()
    {
        var appDataDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TwentyTwentyTray");

        settingsPath = Path.Combine(appDataDirectory, "settings.json");
    }

    public AppSettings Load()
    {
        if (!File.Exists(settingsPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(settingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(settingsPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, SerializerOptions);
        File.WriteAllText(settingsPath, json);
    }
}