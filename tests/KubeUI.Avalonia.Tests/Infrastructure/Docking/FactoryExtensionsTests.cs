using Avalonia.Headless.XUnit;
using Dock.Model.Controls;
using Dock.Model.Core;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Docking;

public sealed class FactoryExtensionsTests
{
    [AvaloniaFact]
    public void Add_or_update_adds_missing_document_and_applies_state()
    {
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var id = $"factory-test-{Guid.NewGuid():N}";
        using var document = new TestDocument { Id = id, Title = "before" };
        var updates = 0;

        try
        {
            var live = factory.AddOrUpdateToDocuments(document, item =>
            {
                updates++;
                item.Title = "after";
            });

            live.ShouldBeSameAs(document);
            updates.ShouldBe(1);
            document.Title.ShouldBe("after");
            documents.VisibleDockables.ShouldContain(document);
            documents.ActiveDockable.ShouldBeSameAs(document);
        }
        finally
        {
            factory.CloseDockable(document);
        }
    }

    [AvaloniaFact]
    public void Add_or_update_updates_existing_document_and_disposes_candidate()
    {
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var documents = factory.GetDockable<IDocumentDock>("Documents").ShouldNotBeNull();
        var id = $"factory-test-{Guid.NewGuid():N}";
        using var existing = new TestDocument { Id = id, Title = "initial" };
        using var candidate = new TestDocument { Id = id, Title = "candidate" };

        try
        {
            factory.AddToDocuments(existing).ShouldBeTrue();

            var live = factory.AddOrUpdateToDocuments(candidate, item => item.Title = "updated");

            live.ShouldBeSameAs(existing);
            existing.Title.ShouldBe("updated");
            candidate.IsDisposed.ShouldBeTrue();
            documents.VisibleDockables!.Count(item => item.Id == id).ShouldBe(1);
            documents.ActiveDockable.ShouldBeSameAs(existing);
        }
        finally
        {
            factory.CloseDockable(existing);
        }
    }

    [AvaloniaFact]
    public void Add_or_update_requires_document_identifier()
    {
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        using var document = new TestDocument();

        Should.Throw<InvalidOperationException>(() => factory.AddOrUpdateToDocuments(document))
            .Message.ShouldContain(nameof(TestDocument));
    }

    [AvaloniaFact]
    public void Add_or_update_rejects_document_type_that_owns_identifier()
    {
        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var id = $"factory-test-{Guid.NewGuid():N}";
        using var existing = new TestDocument { Id = id };

        try
        {
            factory.AddToDocuments(existing);

            Should.Throw<InvalidOperationException>(
                () => factory.AddOrUpdateToDocuments(new OtherTestDocument { Id = id }))
                .Message.ShouldContain(id);
        }
        finally
        {
            factory.CloseDockable(existing);
        }
    }

    private sealed class TestDocument : ViewModelBase, IDisposable
    {
        public bool IsDisposed { get; private set; }

        public void Dispose() => IsDisposed = true;
    }

    private sealed class OtherTestDocument : ViewModelBase
    {
    }
}
