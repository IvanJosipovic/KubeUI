using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace KubeUI.Kubernetes.Serialization;

internal static class YamlInputNormalizer
{
    public static string Normalize(string yaml, bool strict)
    {
        if (strict)
        {
            using StringReader validationReader = new(yaml);
            YamlStream validationStream = new();
            validationStream.Load(validationReader);
        }

        var explicitMappingKeyPositions = GetExplicitMappingKeyPositions(yaml);
        using StringReader reader = new(yaml);
        MergingParser parser = new(new Parser(reader));
        List<ParsingEvent> events = [];
        while (parser.MoveNext())
        {
            events.Add(parser.Current!);
        }

        StringBuilder output = new(yaml.Length);
        using StringWriter writer = new(output);
        Emitter emitter = new(writer);
        var index = 0;
        while (index < events.Count)
        {
            if (events[index] is not DocumentStart documentStart)
            {
                EmitNode(ParseNode(events, ref index), emitter, explicitMappingKeyPositions);
                continue;
            }

            emitter.Emit(documentStart);
            index++;
            if (events[index] is DocumentEnd emptyDocumentEnd)
            {
                emitter.Emit(emptyDocumentEnd);
                index++;
                continue;
            }

            var root = ParseNode(events, ref index);
            if (root is EventNode
                {
                    Event: Scalar
                    {
                        Value.Length: 0,
                        Style: ScalarStyle.Plain,
                        IsPlainImplicit: true,
                    },
                })
            {
                // YamlDotNet represents an explicit empty document separator followed by comments as
                // an empty plain scalar. The YAML adapter treats that scalar as an empty string, while
                // the Kubernetes/Helm stream contract treats empty documents as null and skips them.
                emitter.Emit(new Scalar("null"));
            }
            else
            {
                EmitNode(root, emitter, explicitMappingKeyPositions);
            }

            if (events[index] is DocumentEnd documentEnd)
            {
                emitter.Emit(documentEnd);
                index++;
            }
        }

        return output.ToString();
    }

    private static Dictionary<SourcePosition, HashSet<SourcePosition>> GetExplicitMappingKeyPositions(string yaml)
    {
        using StringReader reader = new(yaml);
        Parser parser = new(reader);
        List<ParsingEvent> events = [];
        while (parser.MoveNext())
        {
            events.Add(parser.Current!);
        }

        Dictionary<SourcePosition, HashSet<SourcePosition>> positions = [];
        var index = 0;
        while (index < events.Count)
        {
            if (events[index] is DocumentStart or DocumentEnd or StreamStart)
            {
                index++;
                continue;
            }

            if (events[index] is StreamEnd)
            {
                break;
            }

            CollectExplicitMappingKeyPositions(ParseNode(events, ref index), positions);
        }

        return positions;
    }

    private static void CollectExplicitMappingKeyPositions(
        YamlEventNode node,
        Dictionary<SourcePosition, HashSet<SourcePosition>> positions)
    {
        switch (node)
        {
            case MappingNode mapping:
                var mappingPosition = GetPosition(mapping.Start.Start);
                HashSet<SourcePosition> keys = [];
                foreach (var entry in mapping.Entries)
                {
                    if (entry.Key is EventNode { Event: Scalar scalarKey })
                    {
                        keys.Add(GetPosition(scalarKey.Start));
                    }

                    CollectExplicitMappingKeyPositions(entry.Key, positions);
                    CollectExplicitMappingKeyPositions(entry.Value, positions);
                }

                positions[mappingPosition] = keys;
                break;
            case SequenceNode sequence:
                foreach (var item in sequence.Items)
                {
                    CollectExplicitMappingKeyPositions(item, positions);
                }

                break;
        }
    }

    private static YamlEventNode ParseNode(IReadOnlyList<ParsingEvent> events, ref int index)
    {
        var current = events[index++];
        if (current is MappingStart mappingStart)
        {
            List<(YamlEventNode Key, YamlEventNode Value)> entries = [];
            while (events[index] is not MappingEnd)
            {
                var key = ParseNode(events, ref index);
                var value = ParseNode(events, ref index);
                entries.Add((key, value));
            }

            return new MappingNode(mappingStart, entries, events[index++]);
        }

        if (current is SequenceStart sequenceStart)
        {
            List<YamlEventNode> items = [];
            while (events[index] is not SequenceEnd)
            {
                items.Add(ParseNode(events, ref index));
            }

            return new SequenceNode(sequenceStart, items, events[index++]);
        }

        return new EventNode(current);
    }

    private static void EmitNode(
        YamlEventNode node,
        IEmitter emitter,
        IReadOnlyDictionary<SourcePosition, HashSet<SourcePosition>> explicitMappingKeyPositions)
    {
        switch (node)
        {
            case EventNode single:
                emitter.Emit(single.Event);
                break;
            case SequenceNode sequence:
                emitter.Emit(sequence.Start);
                foreach (var item in sequence.Items)
                {
                    EmitNode(item, emitter, explicitMappingKeyPositions);
                }

                emitter.Emit(sequence.End);
                break;
            case MappingNode mapping:
                emitter.Emit(mapping.Start);
                explicitMappingKeyPositions.TryGetValue(GetPosition(mapping.Start.Start), out var directKeyPositions);
                HashSet<string> explicitKeys = new(StringComparer.Ordinal);
                Dictionary<string, int> lastExplicitValueByKey = new(StringComparer.Ordinal);
                Dictionary<string, int> lastMergedValueByKey = new(StringComparer.Ordinal);
                for (var entryIndex = 0; entryIndex < mapping.Entries.Count; entryIndex++)
                {
                    if (mapping.Entries[entryIndex].Key is EventNode { Event: Scalar scalarKey } keyNode)
                    {
                        if (directKeyPositions?.Contains(GetPosition(keyNode.Event.Start)) == true)
                        {
                            explicitKeys.Add(scalarKey.Value);
                            lastExplicitValueByKey[scalarKey.Value] = entryIndex;
                        }
                        else
                        {
                            lastMergedValueByKey[scalarKey.Value] = entryIndex;
                        }
                    }
                }

                for (var entryIndex = 0; entryIndex < mapping.Entries.Count; entryIndex++)
                {
                    var entry = mapping.Entries[entryIndex];
                    if (entry.Key is EventNode { Event: Scalar scalarKey } keyNode)
                    {
                        var isExplicit = directKeyPositions?.Contains(GetPosition(keyNode.Event.Start)) == true;
                        if (isExplicit
                            ? lastExplicitValueByKey[scalarKey.Value] != entryIndex
                            : explicitKeys.Contains(scalarKey.Value)
                                || lastMergedValueByKey[scalarKey.Value] != entryIndex)
                        {
                            continue;
                        }
                    }

                    EmitNode(entry.Key, emitter, explicitMappingKeyPositions);
                    EmitNode(entry.Value, emitter, explicitMappingKeyPositions);
                }

                emitter.Emit(mapping.End);
                break;
            default:
                throw new InvalidOperationException($"Unsupported YAML node '{node.GetType()}'.");
        }
    }

    private abstract record YamlEventNode;

    private sealed record EventNode(ParsingEvent Event) : YamlEventNode;

    private sealed record SequenceNode(SequenceStart Start, IReadOnlyList<YamlEventNode> Items, ParsingEvent End)
        : YamlEventNode;

    private sealed record MappingNode(
        MappingStart Start,
        IReadOnlyList<(YamlEventNode Key, YamlEventNode Value)> Entries,
        ParsingEvent End) : YamlEventNode;

    private static SourcePosition GetPosition(Mark mark) => new(mark.Line, mark.Column);

    private readonly record struct SourcePosition(long Line, long Column);
}
