﻿namespace KubeUI;

public enum LocalThemeVariant
{
    Default,
    Light,
    Dark,
}

public sealed partial class Settings : ObservableObject
{
    [ObservableProperty]
    private LocalThemeVariant _theme;

    [ObservableProperty]
    private bool _loggingEnabled = false;

    [ObservableProperty]
    private bool _telemetryEnabled = true;

    [ObservableProperty]
    private bool preReleaseChannel = true;
}
