using System.Reflection;

namespace KubeUI.ViewModels;

public sealed partial class AboutViewModel : ViewModelBase
{
    public static string? GetVersion()
    {
        var version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (version.Contains('+'))
        {
            version = version.Substring(0, version.IndexOf('+'));
        }

        return version;
    }

    public string? Version { get; }
}
