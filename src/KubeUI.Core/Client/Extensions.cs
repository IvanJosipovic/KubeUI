using System.Text.RegularExpressions;

namespace KubeUI.Core.Client;

public static class Extensions
{
    public static string AddSpacesBeforeCapitals(this string str)
    {
        return Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
    }
}
