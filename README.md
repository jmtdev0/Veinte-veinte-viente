# TwentyTwentyTray

TwentyTwentyTray is a lightweight Windows tray app that reminds you every 20 minutes to stand up and rest your eyes.

It runs quietly in the system tray, shows modern Windows notifications, and lets you enable or disable reminders with a single click.

## Features

- Runs as a tray app with no main window.
- Sends a reminder every 20 minutes.
- Lets you enable or disable reminders from the tray menu.
- Can start automatically when you sign in.
- Stores settings in `%APPDATA%\TwentyTwentyTray\settings.json`.
- Ships with self-contained release builds, so end users do not need .NET installed.

## Requirements

- Windows 11 or Windows 10 version 2004 and later.

## Install From A Release

1. Download the correct ZIP from the latest GitHub Release.
2. Extract it to a permanent folder, for example `C:\Apps\TwentyTwentyTray`.
3. Run `TwentyTwentyTray.exe`.
4. If you want it to start with Windows, leave `Launch at startup` enabled in the tray menu.

Important:
If you enable startup, the app stores the current executable path in the Windows `Run` registry key. Move the app to its final folder before enabling startup.

## Use The App

- Right-click the tray icon to enable or disable reminders.
- Use `Send test notification` to verify notifications are working.
- Click `Disable reminders` directly from a notification if you want to pause them quickly.

## Local Development

Build the app:

```powershell
dotnet build .\TwentyTwentyTray.csproj
```

Run the app:

```powershell
dotnet run --project .\TwentyTwentyTray.csproj
```

## Create Self-Contained Release Packages Locally

The repository includes a script that produces standalone ZIP packages for Windows.

```powershell
.\scripts\Publish-Release.ps1 -Version v1.0.0
```

This generates ZIP packages under `dist\release\v1.0.0` for:

- `win-x64`
- `win-arm64`

Each package contains a self-contained single-file executable that does not require .NET to be installed on the target machine.

## GitHub Release Workflow

This repository includes a GitHub Actions workflow that builds release packages automatically.

To publish a release:

1. Commit and push your changes.
2. Create a tag such as `v1.0.0`.
3. Push the tag.

```powershell
git tag v1.0.0
git push origin v1.0.0
```

The workflow will:

- build self-contained `win-x64` and `win-arm64` packages
- create ZIP artifacts
- attach them to a GitHub Release

## Notes

- Windows SmartScreen may warn users about unsigned executables. That is expected until the app is code-signed.
- Notifications rely on Windows desktop toast support.

## License

MIT. See [LICENSE](LICENSE).