#if DEBUG
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Irihi.Lingua;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.App.Tests.Foundation.Localization;

public sealed class LinguaHotReloadTests : IDisposable
{
    public void Dispose() => TestLanguageManager.RestoreCompiledResources();

    [AvaloniaFact]
    public void reload_preserves_subscriptions_bound_controls_culture_and_runtime_overrides()
    {
        var manager = TestLanguageManager.CreateForHotReloadTest();
        var observable = manager.L10n_Test_Key;
        var seen = new List<string?>();
        using var subscription = observable.Subscribe(seen.Add);
        var control = new TextBlock();
        using var binding = control.Bind(TextBlock.TextProperty, observable.ToBinding());
        var french = CultureInfo.GetCultureInfo("fr-FR");
        manager.UpdateCulture(french);

        manager.Reload(Values("first", "premier"));
        Dispatcher.UIThread.RunJobs();

        manager.L10n_Test_Key.ShouldBeSameAs(observable);
        observable.CurrentValue().ShouldBe("premier");
        control.Text.ShouldBe("premier");
        manager.CurrentCulture.ShouldBe(french);
        seen.ShouldContain("premier");

        manager.AddResources(french, new Dictionary<string, string> { ["L10n_Test_Key"] = "override" });
        manager.UpdateCulture(french);
        manager.Reload(Values("second", "deuxieme"));
        Dispatcher.UIThread.RunJobs();
        observable.CurrentValue().ShouldBe("override");
        control.Text.ShouldBe("override");

        manager.ClearRuntimeResources();
        manager.UpdateCulture(french);
        Dispatcher.UIThread.RunJobs();
        observable.CurrentValue().ShouldBe("deuxieme");
        control.Text.ShouldBe("deuxieme");
    }

    [Fact]
    public void reload_discards_removed_translations_and_uses_invariant_fallback()
    {
        var manager = TestLanguageManager.CreateForHotReloadTest();
        manager.UpdateCulture(CultureInfo.GetCultureInfo("fr-FR"));

        manager.Reload(Values("first", "premier"));
        manager.L10n_Test_Key.CurrentValue().ShouldBe("premier");
        manager.Reload(Values("second"));

        manager.L10n_Test_Key.CurrentValue().ShouldBe("second");
        manager.CurrentCulture.Name.ShouldBe("fr-FR");
    }

    [Fact]
    public void structural_key_changes_require_restart_without_changing_published_values()
    {
        var manager = TestLanguageManager.CreateForHotReloadTest();
        var added = Values("new");
        added.Add(CultureInfo.InvariantCulture, new Dictionary<string, string> { ["Extra_Key"] = "extra" });

        Should.Throw<InvalidOperationException>(() => manager.Reload(added)).Message.ShouldContain("restart", Case.Insensitive);
        Should.Throw<InvalidOperationException>(() => manager.Reload(new LinguaRuntimeResources())).Message.ShouldContain("restart", Case.Insensitive);
        var renamed = new LinguaRuntimeResources();
        renamed.Add(CultureInfo.InvariantCulture, new Dictionary<string, string> { ["Renamed_Key"] = "renamed" });
        Should.Throw<InvalidOperationException>(() => manager.Reload(renamed)).Message.ShouldContain("restart", Case.Insensitive);
        manager.L10n_Test_Key.CurrentValue().ShouldBe("Test Value");
        manager.GetObservable("Extra_Key").ShouldBeNull();
    }

    [AvaloniaFact]
    public void metadata_dispatch_ignores_unrelated_updates_without_resolving_a_manager()
    {
        var called = false;
        LinguaHotReload.UpdateApplication<TestLanguageManager>([typeof(string)], () => called = true);
        Dispatcher.UIThread.RunJobs();
        called.ShouldBeFalse();
    }

    [AvaloniaFact]
    public void manager_callback_does_not_initialize_an_unused_catalog_for_unrelated_updates()
    {
        LinguaHotReload<UninitializedLanguageManager>.UpdateApplication([typeof(string)]);
        Dispatcher.UIThread.RunJobs();
        HotReloadInitializationProbe.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public void unknown_updated_types_refresh_the_catalog_but_an_empty_list_does_not()
    {
        var manager = TestLanguageManager.CreateForHotReloadTest();
        LinguaHotReload.UpdateApplication<TestLanguageManager>([], () => manager.Reload(Values("ignored")));
        manager.L10n_Test_Key.CurrentValue().ShouldBe("Test Value");
        LinguaHotReload.UpdateApplication<TestLanguageManager>(null, () => manager.Reload(Values("updated")));
        manager.L10n_Test_Key.CurrentValue().ShouldBe("updated");
    }

    [AvaloniaFact]
    public async Task metadata_dispatch_publishes_background_updates_on_the_ui_thread()
    {
        var manager = TestLanguageManager.CreateForHotReloadTest();
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var publicationOnUiThread = false;
        using var subscription = manager.L10n_Test_Key.Subscribe(value =>
        {
            if (value == "updated")
            {
                publicationOnUiThread = Dispatcher.UIThread.CheckAccess();
                completed.TrySetResult();
            }
        });

        await Task.Run(() => LinguaHotReload.UpdateApplication<TestLanguageManager>(
            [typeof(TestLanguageManager)],
            () => manager.Reload(Values("updated"))), TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();
        await completed.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        publicationOnUiThread.ShouldBeTrue();
    }

    private static LinguaRuntimeResources Values(string invariant, string? french = null)
    {
        var resources = new LinguaRuntimeResources();
        resources.Add(CultureInfo.InvariantCulture, new Dictionary<string, string> { ["L10n_Test_Key"] = invariant });
        if (french is not null)
        {
            resources.Add(CultureInfo.GetCultureInfo("fr"), new Dictionary<string, string> { ["L10n_Test_Key"] = french });
        }

        return resources;
    }
}

[LinguaManager("./TestLocalizationResources.json")]
internal sealed partial class UninitializedLanguageManager : ILinguaHotReload
{
    internal int InitializationOrdinal { get; } = HotReloadInitializationProbe.OnManagerCreated();

    static void ILinguaHotReload.Reload()
        => Instance.Reload(_lingua_resources, CreateStaticResources());
}

internal static class HotReloadInitializationProbe
{
    internal static int Count { get; private set; }

    internal static int OnManagerCreated() => ++Count;
}

internal sealed partial class TestLanguageManager
{
    internal static TestLanguageManager CreateForHotReloadTest() => new();

    internal void Reload(LinguaRuntimeResources replacement)
        => LinguaHotReload.Reload(this, _lingua_resources, replacement);

    internal static void RestoreCompiledResources()
    {
        Instance.Reload(CreateStaticResources());
        Instance.ClearRuntimeResources();
        Instance.UpdateCulture(CultureInfo.InvariantCulture);
    }
}
#endif
