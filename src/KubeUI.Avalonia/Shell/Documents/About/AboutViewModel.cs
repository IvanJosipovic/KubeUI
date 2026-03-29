using System.Reflection;

namespace KubeUI.Avalonia.ViewModels;

public sealed partial class AboutViewModel : ViewModelBase
{
    public AboutViewModel()
    {
        Title = Assets.Resources.AboutView_Title;
        Id = nameof(AboutViewModel);
    }

    public static string? GetVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (version.Contains('+'))
        {
            version = version[..version.IndexOf('+')];
        }

        return version;
    }

    public string? Version { get; } = GetVersion();
}

