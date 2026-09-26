---
sidebar_position: 9
---

# Documentation video recording

The dedicated `tests/KubeUI.Documentation.Tests` project records short, focused feature videos from KubeUI's production Avalonia views. Each test owns its UI actions and narration. Headless and Skia render the views against a deterministic fake Kubernetes cluster.

Run from the repository root in PowerShell:

```powershell
./tests/KubeUI.Documentation.Tests/record-feature-videos.ps1
```

The recording tests are skipped during ordinary test runs. The script sets `KUBEUI_DOCS_VIDEO_DIR` and creates one light and one dark high quality H.264 MP4 per feature in `website/static/video/`. Each clip is embedded in the guide for its workflow; the site shows the clip matching its current light or dark theme. Video uses 1440×900 output at 30 fps, libx264 slow preset, CRF 16, and AAC narration at 192 kb/s. Source frames are lossless PNG captures at the native 1440×900 recording resolution. MP4 metadata is moved to the front for progressive playback. KokoroSharp generates audio locally; FFmpeg encodes each clip. Use an FFmpeg build with `libx264`; add FFmpeg to `PATH` or set `KUBEUI_FFMPEG_PATH` to its executable.
