namespace KubeUI.Kubernetes.Resources.Editor;

/// <summary>
/// Identifies the JSON shape a resource editor node renders.
/// </summary>
public enum ResourceEditorValueKind
{
    /// <summary>A JSON object with a fixed set of schema properties.</summary>
    ObjectKind,

    /// <summary>A JSON array with schema-described items.</summary>
    ArrayKind,

    /// <summary>A JSON object with free-form string keys.</summary>
    MapKind,

    /// <summary>A JSON string.</summary>
    StringKind,

    /// <summary>A JSON number.</summary>
    NumberKind,

    /// <summary>A JSON boolean.</summary>
    BooleanKind,

    /// <summary>A JSON string restricted to a set of schema values.</summary>
    EnumKind,

    /// <summary>A value whose shape is not described by the schema.</summary>
    UnknownKind,
}
