using System.Reflection;
using KubeUI.App.Infrastructure.Presentation;

namespace KubeUI.App.Shell.Documents.About;

public sealed partial class AboutViewModel : ViewModelBase
{
    public AboutViewModel()
    {
        Title = SharedL10n.Instance.AboutView_Title.CurrentValue()!;
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

