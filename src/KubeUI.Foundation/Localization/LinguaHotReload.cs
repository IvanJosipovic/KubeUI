#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia.Threading;
using Irihi.Lingua;

namespace KubeUI.Foundation.Localization;

/// <summary>
/// Provides a statically callable bridge to a manager's generated resources in Debug.
/// </summary>
public interface ILinguaHotReload : ILinguaManager
{
    /// <summary>
    /// Rebuilds and republishes the manager's compiled resources.
    /// </summary>
    static abstract void Reload();
}

/// <summary>
/// Receives metadata updates without initializing the manager for unrelated changes.
/// </summary>
/// <typeparam name="TManager">The statically registered manager.</typeparam>
public static class LinguaHotReload<TManager> where TManager : ILinguaHotReload
{
    /// <summary>
    /// Refreshes the manager if it participated in the metadata update.
    /// </summary>
    /// <param name="updatedTypes">Changed types, or null for a global update.</param>
    [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Public API contract; the static generic bridge is intentional. Revisit in next major version.")]
    public static void UpdateApplication(Type[]? updatedTypes)
        => LinguaHotReload.UpdateApplication<TManager>(updatedTypes, static () => TManager.Reload());
}

/// <summary>
/// Debug-only support for republishing updated compiled Lingua catalogs.
/// </summary>
public static class LinguaHotReload
{
    /// <summary>
    /// Dispatches a changed manager's refresh to the UI thread.
    /// </summary>
    /// <param name="updatedTypes">Types changed by the metadata update, or null for a global update.</param>
    /// <typeparam name="TManager">The statically registered manager type.</typeparam>
    /// <param name="refresh">Refreshes that manager without replacing its singleton.</param>
    public static void UpdateApplication<TManager>(Type[]? updatedTypes, Action refresh)
        where TManager : ILinguaManager
    {
        ArgumentNullException.ThrowIfNull(refresh);

        if (updatedTypes is not null && Array.IndexOf(updatedTypes, typeof(TManager)) < 0)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            refresh();
        }
        else
        {
            Dispatcher.UIThread.Post(refresh);
        }
    }

    /// <summary>
    /// Refreshes an existing manager using its regenerated compiled resources.
    /// </summary>
    /// <param name="manager">The existing manager singleton.</param>
    /// <param name="compiled">Its generated compiled-resource store.</param>
    /// <param name="replacement">The newly generated compiled values.</param>
    public static void Reload(
        this ILinguaManager manager,
        LinguaRuntimeResources compiled,
        LinguaRuntimeResources replacement)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(compiled);
        ArgumentNullException.ThrowIfNull(replacement);

        var original = compiled.Resolve(CultureInfo.InvariantCulture);
        var invariant = replacement.Resolve(CultureInfo.InvariantCulture);
        if (original is null || invariant is null || invariant.Count != original.Count)
        {
            throw new InvalidOperationException("Lingua resource keys changed. Restart the application to rebuild generated properties.");
        }

        var byCulture = new Dictionary<CultureInfo, Dictionary<string, string>>();
        foreach (var key in original.Keys)
        {
            if (!invariant.ContainsKey(key))
            {
                throw new InvalidOperationException("Lingua resource keys changed. Restart the application to rebuild generated properties.");
            }

            foreach (var (culture, value) in replacement.GetAllValuesForKey(key))
            {
                if (!byCulture.TryGetValue(culture, out var values))
                {
                    values = [];
                    byCulture.Add(culture, values);
                }

                values.Add(key, value);
            }
        }

        // Replace compiled values only; the manager's separate runtime overrides stay intact.
        compiled.Clear();
        foreach (var (culture, values) in byCulture)
        {
            compiled.Add(culture, values);
        }

        manager.UpdateCulture(manager.CurrentCulture);
    }
}
#endif
