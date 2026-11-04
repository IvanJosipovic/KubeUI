using System.Globalization;
using System.Reflection.Metadata;
using Irihi.Lingua;

[assembly: MetadataUpdateHandler(typeof(Irihi.Lingua.HotReload.Minimal.LinguaHotReloadHandler))]

namespace Irihi.Lingua.HotReload.Minimal;

/// <summary>
/// The Lingua manager for the application's resource files.
/// </summary>
[LinguaManager("./Resources/Strings.json")]
internal sealed partial class LanguageManager;

/// <summary>
/// Re-runs Lingua's generated resource loader after .NET hot reload patches generated code.
/// </summary>
/// <remarks>
/// Lingua builds its resource and key maps once in the generated constructor. Hot reload can
/// patch the generated loader body, so this partial class re-runs it, merges new keys, and
/// publishes fresh values through Lingua's runtime-override API.
/// </remarks>
internal sealed partial class LanguageManager
{
    /// <summary>
    /// Publishes the freshly generated resource values for the current culture.
    /// </summary>
    public static void ReloadCompiledResources()
    {
        var resources = CreateStaticResources();

        // The generated constructor builds the key map once. Merge keys added by the
        // current hot-reload pass so GetObservable can resolve them without a restart.
        var fresh = new LanguageManager();
        var observables = (Dictionary<string, LinguaObservableString>)Instance._lingua_observables;
        foreach (var pair in fresh._lingua_observables)
        {
            if (!observables.ContainsKey(pair.Key))
            {
                observables[pair.Key] = pair.Value;
            }
        }

        var valuesByCulture = new Dictionary<CultureInfo, Dictionary<string, string>>();

        foreach (var observable in Instance._lingua_observables.Values)
        {
            foreach (var pair in resources.GetAllValuesForKey(observable.Key))
            {
                if (!valuesByCulture.TryGetValue(pair.Key, out var values))
                {
                    values = [];
                    valuesByCulture[pair.Key] = values;
                }

                values[observable.Key] = pair.Value;
            }
        }

        Instance.ClearRuntimeResources();
        foreach (var (culture, values) in valuesByCulture)
        {
            Instance.AddResources(culture, values);
        }

        Instance.UpdateCulture(Instance.CurrentCulture);
    }
}

/// <summary>
/// Receives .NET hot-reload notifications and refreshes Lingua.
/// </summary>
internal static class LinguaHotReloadHandler
{
    /// <summary>
    /// Invoked after a hot-reload pass applies metadata updates.
    /// </summary>
    /// <param name="updatedTypes">Types updated by the pass.</param>
    public static void ClearCache(Type[]? updatedTypes)
    {
        _ = updatedTypes;
        LanguageManager.ReloadCompiledResources();
    }
}
