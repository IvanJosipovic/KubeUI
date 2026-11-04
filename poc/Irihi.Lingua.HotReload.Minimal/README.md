# Minimal Lingua hot reload

One partial class, one metadata handler. No runtime file loading.

```sh
dotnet watch run
```

## Value edits

Edit `Greeting_Message` in `Resources/Strings.json`. The console prints the new value without restarting.

## Key additions

Also live. The handler creates a fresh manager, merges new keys into the existing key map, and republishes values. A consumer must resolve the key after the reload; an Avalonia view that references it rebuilds and binds then.

## Key removals

.NET hot reload cannot remove generated members. Restart for removals:

```sh
dotnet watch --no-hot-reload run
```

`Resources\**\*.json` is both an `AdditionalFiles` input for Lingua and a `Watch` item for restart mode.
