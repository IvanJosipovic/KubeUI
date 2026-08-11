namespace KubeUI.AI.Agents;

public sealed record KubernetesResourceReference(string ApiVersion, string Kind, string Name, string? Namespace = null);

public sealed record AgentContext
{
    public IReadOnlyList<KubernetesResourceReference> SelectedResources { get; init; } = [];
    public string? Namespace { get; init; }
    public IReadOnlyList<KubernetesResourceReference> RelatedResources { get; init; } = [];

    public string ToPromptContext()
    {
        var lines = new List<string>(SelectedResources.Count + RelatedResources.Count + 1);
        foreach (var selected in SelectedResources)
            lines.Add($"Selected resource: {selected.ApiVersion}/{selected.Kind} {selected.Namespace ?? Namespace ?? "<cluster>"}/{selected.Name}");

        if (SelectedResources.Count == 0 && !string.IsNullOrWhiteSpace(Namespace))
            lines.Add($"Selected namespace: {Namespace}");

        foreach (var related in RelatedResources)
            lines.Add($"Related resource: {related.ApiVersion}/{related.Kind} {related.Namespace ?? Namespace ?? "<cluster>"}/{related.Name}");

        return string.Join(Environment.NewLine, lines);
    }
}
