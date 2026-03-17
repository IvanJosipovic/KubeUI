---
name: ilspy-decompile
description: Understand implementation details of .NET code by decompiling assemblies. Use when the user wants to see how a .NET API works internally, inspect NuGet package source, view framework implementation, or understand compiled .NET binaries.
allowed-tools: Bash(dnx:*)
---

# .NET Assembly Decompilation with ILSpy

Use this skill to understand how .NET code works internally by decompiling compiled assemblies.

## Quick start

```bash
# Decompile an assembly to stdout (no install needed with .NET 10+)
dnx ilspycmd MyLibrary.dll

# Decompile to an output folder
dnx ilspycmd -o output-folder MyLibrary.dll
```

## Common .NET Assembly Locations

### NuGet Packages
```bash
# Windows
~/.nuget/packages/<package-name>/<version>/lib/<tfm>/

# Example: Newtonsoft.Json
~/.nuget/packages/newtonsoft.json/13.0.3/lib/netstandard2.0/Newtonsoft.Json.dll

# Example: Microsoft.Extensions.DependencyInjection
~/.nuget/packages/microsoft.extensions.dependencyinjection/8.0.0/lib/net8.0/Microsoft.Extensions.DependencyInjection.dll
```

### .NET Runtime Libraries
```bash
# Find .NET install location
dotnet --list-runtimes

# Windows typical paths
C:/Program Files/dotnet/shared/Microsoft.NETCore.App/<version>/
C:/Program Files/dotnet/shared/Microsoft.AspNetCore.App/<version>/

# Linux/macOS typical paths
/usr/share/dotnet/shared/Microsoft.NETCore.App/<version>/
/usr/share/dotnet/shared/Microsoft.AspNetCore.App/<version>/

# Example: System.Text.Json from runtime
C:/Program Files/dotnet/shared/Microsoft.NETCore.App/8.0.0/System.Text.Json.dll
```

### .NET SDK Reference Assemblies
```bash
# Find SDK location
dotnet --list-sdks

# Reference assemblies (design-time facades)
C:/Program Files/dotnet/packs/Microsoft.NETCore.App.Ref/<version>/ref/net8.0/
```

### Project Build Output
```bash
# Debug build
./bin/Debug/net8.0/<AssemblyName>.dll

# Published output
./bin/Release/net8.0/publish/<AssemblyName>.dll
```

## Core workflow

1. Identify what you want to understand (API, class, method)
2. Locate the assembly containing that code
3. Use `dnx ilspycmd -l class` to find the exact type name
4. Decompile the specific type with `-t`

## Commands

### Basic Decompilation

```bash
# Decompile to stdout
dnx ilspycmd MyLibrary.dll

# Decompile to output directory
dnx ilspycmd -o ./decompiled MyLibrary.dll

# Decompile as compilable project
dnx ilspycmd -p -o ./project MyLibrary.dll

# Decompile with nested namespace folders
dnx ilspycmd -p -o ./project --nested-directories MyLibrary.dll
```

### Targeted Decompilation

```bash
# Decompile a specific type
dnx ilspycmd -t Namespace.ClassName MyLibrary.dll

# Decompile with specific C# version
dnx ilspycmd -lv CSharp12_0 MyLibrary.dll

# Decompile with reference path for dependencies
dnx ilspycmd -r ./dependencies MyLibrary.dll
```

### View IL Code

```bash
# Show IL code instead of C#
dnx ilspycmd -il MyLibrary.dll

# Show IL for specific type
dnx ilspycmd -il -t Namespace.ClassName MyLibrary.dll
```

### List Types

```bash
# List all classes
dnx ilspycmd -l class MyLibrary.dll

# List interfaces
dnx ilspycmd -l interface MyLibrary.dll

# List structs
dnx ilspycmd -l struct MyLibrary.dll

# List enums
dnx ilspycmd -l enum MyLibrary.dll

# List delegates
dnx ilspycmd -l delegate MyLibrary.dll
```

### Options

```bash
# Show help
dnx ilspycmd -h

# Disable update check (useful for CI/automation)
dnx ilspycmd --disable-updatecheck MyLibrary.dll

# Remove dead code
dnx ilspycmd --no-dead-code MyLibrary.dll
```

## Example: Understand how JsonSerializer works

```bash
# Find System.Text.Json in the runtime
dotnet --list-runtimes
# Microsoft.NETCore.App 8.0.0 [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]

# List classes to find JsonSerializer
dnx ilspycmd -l class "C:/Program Files/dotnet/shared/Microsoft.NETCore.App/8.0.0/System.Text.Json.dll"

# Decompile JsonSerializer
dnx ilspycmd -t System.Text.Json.JsonSerializer "C:/Program Files/dotnet/shared/Microsoft.NETCore.App/8.0.0/System.Text.Json.dll"
```

## Example: Inspect a NuGet package implementation

```bash
# Decompile Polly's retry logic
dnx ilspycmd -t Polly.Retry.RetryPolicy ~/.nuget/packages/polly/8.0.0/lib/netstandard2.0/Polly.dll

# Decompile entire package to project for exploration
dnx ilspycmd -p -o ./polly-src ~/.nuget/packages/polly/8.0.0/lib/netstandard2.0/Polly.dll
```

## Example: See how ASP.NET Core handles requests

```bash
# Find the ASP.NET Core runtime
# C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App\8.0.0\

# Decompile the Kestrel server
dnx ilspycmd -l class "C:/Program Files/dotnet/shared/Microsoft.AspNetCore.App/8.0.0/Microsoft.AspNetCore.Server.Kestrel.Core.dll"

dnx ilspycmd -t Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServer "C:/Program Files/dotnet/shared/Microsoft.AspNetCore.App/8.0.0/Microsoft.AspNetCore.Server.Kestrel.Core.dll"
```

## Example: Compare C# and IL

```bash
# View C# source
dnx ilspycmd -t MyNamespace.MyClass MyLibrary.dll

# View IL code for same type
dnx ilspycmd -il -t MyNamespace.MyClass MyLibrary.dll
```

## C# Language Versions

Available versions for `-lv` option:
- CSharp1 through CSharp12_0
- Latest, Preview
