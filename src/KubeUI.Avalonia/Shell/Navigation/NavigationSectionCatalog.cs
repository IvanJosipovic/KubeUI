using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Collects built-in and provider-declared navigation sections and their documents.</summary>
public sealed class NavigationSectionCatalog
{
    private readonly Dictionary<string, NavigationSection> _sections;
    private readonly Dictionary<string, INavigationDocument> _documents;

    /// <summary>Creates a catalog from section providers registered with dependency injection.</summary>
    public NavigationSectionCatalog(IEnumerable<INavigationSectionProvider> providers)
    {
        Providers = providers
            .OrderBy(static provider => provider.Section.Order)
            .ThenBy(static provider => provider.Section.Name, StringComparer.Ordinal)
            .ToArray();

        var sections = new List<NavigationSection>(NavigationSections.BuiltIn);
        foreach (var section in Providers.Select(static provider => provider.Section))
        {
            var existing = sections.FirstOrDefault(candidate => string.Equals(candidate.Id, section.Id, StringComparison.Ordinal));
            if (existing is null)
            {
                sections.Add(section);
            }
            else if (existing != section)
            {
                throw new InvalidOperationException($"Navigation section id '{section.Id}' conflicts with an existing section definition.");
            }
        }

        var duplicate = sections
            .GroupBy(static section => section.Id, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new InvalidOperationException($"Navigation section id '{duplicate.Key}' is registered more than once.");
        }

        Sections = sections;
        _sections = sections.ToDictionary(static section => section.Id, StringComparer.Ordinal);

        var documents = Providers.SelectMany(static provider => provider.Documents).ToArray();
        var duplicateDocument = documents
            .GroupBy(static document => document.Id, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1);
        if (duplicateDocument is not null)
        {
            throw new InvalidOperationException($"Navigation document id '{duplicateDocument.Key}' is registered more than once.");
        }

        _documents = documents.ToDictionary(static document => document.Id, StringComparer.Ordinal);
    }

    /// <summary>Gets section providers ordered by section sort position.</summary>
    public IReadOnlyList<INavigationSectionProvider> Providers { get; }

    /// <summary>Gets every known section, built-in and provider-declared.</summary>
    public IReadOnlyList<NavigationSection> Sections { get; }

    /// <summary>Finds a section by id.</summary>
    public NavigationSection? Find(string? id)
        => !string.IsNullOrWhiteSpace(id) && _sections.TryGetValue(id, out var section) ? section : null;

    /// <summary>Resolves a registered navigation document by id.</summary>
    public bool TryGetDocument(string id, out INavigationDocument document)
        => _documents.TryGetValue(id, out document!);
}
