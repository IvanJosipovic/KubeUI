param(
    [switch]$Watch
)

if ($Watch) {
    $previousRestartOnRudeEdit = $env:DOTNET_WATCH_RESTART_ON_RUDE_EDIT
    try {
        $env:DOTNET_WATCH_RESTART_ON_RUDE_EDIT = '1'
        dotnet watch --project src\KubeUI.Desktop\KubeUI.Desktop.csproj
    } finally {
        $env:DOTNET_WATCH_RESTART_ON_RUDE_EDIT = $previousRestartOnRudeEdit
    }
} else {
    dotnet run --project src\KubeUI.Desktop\KubeUI.Desktop.csproj
}
