using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Collects built-in and provider-declared navigation sections and their documents.</summary>
public sealed class NavigationSectionCatalog
{
    private readonly Dictionary<string, NavigationSection> _sections;
    private readonly Dictionary<string, NavigationDocument> _documents;

    /// <summary>Creates a catalog from section providers registered with dependency injection.</summary>
    public NavigationSectionCatalog(IEnumerable<INavigationSectionProvider> providers)
    {
        Providers = providers
            .OrderBy(static provider => provider.Section.Order)
            .ThenBy(static provider => provider.Section.Name, StringComparer.Ordinal)
            .ToArray();

        _sections = new Dictionary<string, NavigationSection>(StringComparer.Ordinal);
        foreach (var section in NavigationSections.BuiltIn)
        {
            _sections[section.Id] = section;
        }

        foreach (var provider in Providers)
        {
            if (_sections.TryGetValue(provider.Section.Id, out var existing))
            {
                if (existing != provider.Section)
                {
                    throw new InvalidOperationException(
                        $"Navigation section id '{provider.Section.Id}' conflicts with an existing section definition.");
                }

                continue;
            }

            _sections[provider.Section.Id] = provider.Section;
        }

        _documents = new Dictionary<string, NavigationDocument>(StringComparer.Ordinal);
        foreach (var document in Providers.SelectMany(static provider => provider.Documents))
        {
            if (!_documents.TryAdd(document.Id, document))
            {
                throw new InvalidOperationException($"Navigation document id '{document.Id}' is registered more than once.");
            }
        }
    }

    /// <summary>Gets section providers ordered by section sort position.</summary>
    public IReadOnlyList<INavigationSectionProvider> Providers { get; }

    /// <summary>Finds a section by id.</summary>
    public NavigationSection? Find(string? id)
        => !string.IsNullOrWhiteSpace(id) && _sections.TryGetValue(id, out var section) ? section : null;

    /// <summary>Resolves a registered navigation document by id.</summary>
    public bool TryGetDocument(string id, out NavigationDocument document)
        => _documents.TryGetValue(id, out document!);
}
