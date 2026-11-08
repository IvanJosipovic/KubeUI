using System;
using System.Collections.Generic;
using System.Globalization;
using Irihi.Lingua;
using KubeUI.Foundation.Localization;
using Shouldly;
using Xunit;

namespace KubeUI.Avalonia.Tests.Foundation.Localization;

public sealed class LinguaObservableExtensionsTests
{
    [Fact]
    public void current_value_returns_the_published_value()
    {
        var observable = new LinguaObservableString("Test_Key", "Test Value");

        try
        {
            observable.CurrentValue().ShouldBe("Test Value");
        }
        finally
        {
            observable.ClearSubscribers();
        }
    }

    [Fact]
    public void current_value_tracks_republished_values()
    {
        var observable = new LinguaObservableString("Test_Key", "first");
        var seen = new List<string?>();

        try
        {
            using var subscription = observable.Subscribe(seen.Add);

            observable.OnNext("second");

            observable.CurrentValue().ShouldBe("second");
            seen.ShouldContain("first");
            seen.ShouldContain("second");
        }
        finally
        {
            observable.ClearSubscribers();
        }
    }

    [Fact]
    public void current_value_rejects_non_lingua_observables()
    {
        var plain = new PlainObservable();

        Should.Throw<InvalidCastException>(() => plain.CurrentValue());
    }

    [Fact]
    public void current_value_tracks_culture_updates_and_runtime_resources()
    {
        try
        {
            TestLanguageManager.Instance.L10n_Test_Key.CurrentValue().ShouldBe("Test Value");

            TestLanguageManager.Instance.AddResources(
                new CultureInfo("fr-FR"),
                new Dictionary<string, string> { ["L10n_Test_Key"] = "Valeur de test" });
            TestLanguageManager.Instance.UpdateCulture(new CultureInfo("fr-FR"));

            TestLanguageManager.Instance.L10n_Test_Key.CurrentValue().ShouldBe("Valeur de test");

            var seen = new List<string?>();
            using var subscription = TestLanguageManager.Instance.L10n_Test_Key.Subscribe(seen.Add);

            seen.ShouldNotBeEmpty();
            seen[^1].ShouldBe("Valeur de test");
        }
        finally
        {
            TestLanguageManager.Instance.ClearRuntimeResources();
            TestLanguageManager.Instance.UpdateCulture(CultureInfo.InvariantCulture);
        }
    }

    private sealed class PlainObservable : IObservable<string?>
    {
        public IDisposable Subscribe(IObserver<string?> observer)
        {
            _ = observer;
            return new PlainDisposable();
        }
    }

    private sealed class PlainDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
