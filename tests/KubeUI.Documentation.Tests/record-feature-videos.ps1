$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$videoDirectory = Join-Path $repoRoot 'website\static\video'
$recordingProject = Join-Path $PSScriptRoot 'KubeUI.Documentation.Tests.csproj'
$ffmpegPath = $env:KUBEUI_FFMPEG_PATH
if ($ffmpegPath) {
    if (-not (Test-Path -LiteralPath $ffmpegPath -PathType Leaf)) { throw "FFmpeg executable not found: $ffmpegPath" }
}
elseif (-not (Get-Command ffmpeg -ErrorAction SilentlyContinue)) {
    throw 'FFmpeg is required. Add it to PATH or set KUBEUI_FFMPEG_PATH.'
}

New-Item -ItemType Directory -Path $videoDirectory -Force | Out-Null

dotnet build $recordingProject --configuration Release --tl:off -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { throw 'Documentation recording build failed.' }

$oldVideos = @(Get-ChildItem -LiteralPath $videoDirectory -Filter '*.mp4' -File)
foreach ($video in $oldVideos) {
    $stream = $null
    try {
        $stream = [System.IO.File]::Open(
            $video.FullName,
            [System.IO.FileMode]::Open,
            [System.IO.FileAccess]::Read,
            [System.IO.FileShare]::None)
    }
    catch [System.IO.IOException] {
        throw "Cannot clear old video '$($video.Name)' because another app is using it. Close the video preview and rerun."
    }
    finally {
        if ($null -ne $stream) { $stream.Dispose() }
    }
}

$oldVideos | Remove-Item -Force

$env:KUBEUI_DOCS_VIDEO_DIR = $videoDirectory
try {
    dotnet test --project $recordingProject --configuration Release --no-build --no-restore --minimum-expected-tests 1 --hangdump-timeout 3m
    if ($LASTEXITCODE -ne 0) { throw 'Documentation video recording tests failed.' }
}
finally {
    Remove-Item Env:KUBEUI_DOCS_VIDEO_DIR -ErrorAction SilentlyContinue
}

Write-Host "Created feature videos in $videoDirectory"
