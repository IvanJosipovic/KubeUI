using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Avalonia.Features.Resources.Editor;

public sealed partial class ResourceEditorNodeViewModel : ObservableObject, INotifyDataErrorInfo
{
    public const string SectionValidationPropertyName = "Section";

    private readonly ResourceEditorSchemaNode _schema;
    private readonly Action<JsonNode?> _replace;
    private readonly Action _changed;
    private readonly Action<ResourceEditorNodeViewModel>? _remove;
    private ResourceEditorNodeViewModel? _parent;
    private JsonNode? _value;
    private string _stringValue = string.Empty;
    private string _numberValue = string.Empty;
    private bool _booleanValue;
    private string? _selectedEnum;
    private string _yamlValue = string.Empty;
    private readonly Dictionary<string, IReadOnlyList<string>> _localValidationErrors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, IReadOnlyList<string>> _documentValidationErrors = new(StringComparer.Ordinal);

    private ResourceEditorNodeViewModel(
        ResourceEditorSchemaNode schema,
        JsonNode? value,
        string path,
        string displayName,
        bool isRequired,
        Action<JsonNode?> replace,
        Action changed,
        Action<ResourceEditorNodeViewModel>? remove = null,
        ResourceEditorNodeViewModel? parent = null)
    {
        _schema = schema;
        _value = value;
        _replace = replace;
        _changed = changed;
        _remove = remove;
        _parent = parent;
        Name = schema.Name;
        Path = path;
        DisplayName = isRequired ? $"{displayName} *" : displayName;
        Description = schema.Description;
        Kind = schema.ValueKind;
        IsRequired = isRequired;
        IsExpanded = string.IsNullOrEmpty(path) || !path.Contains('.', StringComparison.Ordinal);
        EnumValues = schema.EnumValues;
        EnumOptions = isRequired
            ? schema.EnumValues.Select(value => new ResourceEditorEnumOption(value, value)).ToArray()
            : [new(null, Assets.Resources.ResourceEditorView_Unset), .. schema.EnumValues.Select(value => new ResourceEditorEnumOption(value, value))];
        LoadScalarValue(value);
        RebuildChildren();
    }

    public string Name { get; }
    public string Path { get; }
    public string Description { get; }
    public ResourceEditorValueKind Kind { get; }
    public bool IsRequired { get; }
    public bool IsReadOnly => string.Equals(Path, "apiVersion", StringComparison.Ordinal)
        || string.Equals(Path, "kind", StringComparison.Ordinal)
        || string.Equals(Path, "status", StringComparison.Ordinal)
        || Path.StartsWith("status.", StringComparison.Ordinal);
    public bool IsSection => !IsYamlText && (Kind is ResourceEditorValueKind.Object or ResourceEditorValueKind.Array or ResourceEditorValueKind.Map);
    public bool CanAddItem => Kind == ResourceEditorValueKind.Array && !IsReadOnly;
    public bool CanAddMapEntry => Kind == ResourceEditorValueKind.Map && !IsReadOnly;
    public bool CanClearSection => IsSection && !string.IsNullOrEmpty(Path) && !IsReadOnly;
    public bool CanRemove => _remove is not null && !IsReadOnly;
    public IReadOnlyList<string> EnumValues { get; }
    public IReadOnlyList<ResourceEditorEnumOption> EnumOptions { get; }
    public ObservableCollection<ResourceEditorNodeViewModel> Children { get; } = [];
    public string DisplayValue => Kind switch
    {
        ResourceEditorValueKind.Boolean => _booleanValue ? "true" : "false",
        ResourceEditorValueKind.Enum => _selectedEnum ?? string.Empty,
        ResourceEditorValueKind.Number => _numberValue,
        _ => _stringValue,
    };
    public bool IsYamlText { get; private set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }
    public string YamlValue
    {
        get => _yamlValue;
        set
        {
            if (!IsYamlText || !SetProperty(ref _yamlValue, value))
                return;
            try
            {
                var parsed = KubernetesYaml.Deserialize<JsonNode>(value);
                if (parsed is null)
                {
                    SetLocalValidation(nameof(YamlValue), "Enter a YAML value.");
                    _changed();
                    return;
                }

                SetLocalValidation(nameof(YamlValue), null);
                ReplaceValue(_value is JsonValue && TryGetValue<string>(_value) is not null
                    ? JsonValue.Create(parsed.ToJsonString())
                    : parsed);
            }
            catch (Exception ex) when (ex is YamlDotNet.Core.YamlException or JsonException)
            {
                SetLocalValidation(nameof(YamlValue), ex.Message);
                _changed();
            }
        }
    }

    [ObservableProperty]
    public partial string DisplayName { get; private set; }

    [ObservableProperty]
    public partial string NewMapKey { get; set; } = string.Empty;

    partial void OnNewMapKeyChanged(string value)
    {
        SetLocalValidation(nameof(NewMapKey), null);
    }

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => ValidationErrorsByProperty.Count > 0;

    public IReadOnlyDictionary<string, IReadOnlyList<string>> ValidationErrorsByProperty { get; private set; } =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

    public string ValidationPropertyName => IsYamlText
        ? nameof(YamlValue)
        : Kind switch
        {
            ResourceEditorValueKind.String or ResourceEditorValueKind.Unknown => nameof(StringValue),
            ResourceEditorValueKind.Number => nameof(NumberValue),
            ResourceEditorValueKind.Boolean => nameof(BooleanValue),
            ResourceEditorValueKind.Enum => nameof(SelectedEnumOption),
            _ => SectionValidationPropertyName,
        };

    public string StringValue
    {
        get => _stringValue;
        set
        {
            if (IsReadOnly)
                return;
            if (!SetProperty(ref _stringValue, value))
                return;
            SetLocalValidation(nameof(StringValue), null);
            ReplaceValue(JsonValue.Create(value));
            OnPropertyChanged(nameof(DisplayValue));
        }
    }

    public string NumberValue
    {
        get => _numberValue;
        set
        {
            if (IsReadOnly)
                return;
            if (!SetProperty(ref _numberValue, value))
                return;
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
            {
                SetLocalValidation(nameof(NumberValue), "Enter a valid number.");
                _changed();
                return;
            }
            SetLocalValidation(nameof(NumberValue), null);
            ReplaceValue(JsonValue.Create(number));
            OnPropertyChanged(nameof(DisplayValue));
        }
    }

    public bool BooleanValue
    {
        get => _booleanValue;
        set
        {
            if (IsReadOnly)
                return;
            if (!SetProperty(ref _booleanValue, value))
                return;
            SetLocalValidation(nameof(BooleanValue), null);
            ReplaceValue(JsonValue.Create(value));
            OnPropertyChanged(nameof(DisplayValue));
        }
    }

    public string? SelectedEnum
    {
        get => _selectedEnum;
        set
        {
            if (IsReadOnly)
                return;
            if (!SetProperty(ref _selectedEnum, value))
                return;
            if (value is not null && !EnumValues.Contains(value, StringComparer.Ordinal))
            {
                SetLocalValidation(nameof(SelectedEnumOption), "Select a valid value.");
                _changed();
                return;
            }
            SetLocalValidation(nameof(SelectedEnumOption), null);
            ReplaceValue(value is null ? null : JsonValue.Create(value));
            OnPropertyChanged(nameof(DisplayValue));
        }
    }

    public ResourceEditorEnumOption? SelectedEnumOption
    {
        get => EnumOptions.FirstOrDefault(option => string.Equals(option.Value, _selectedEnum, StringComparison.Ordinal));
        set => SelectedEnum = value?.Value;
    }

    public static ResourceEditorNodeViewModel Create(
        ResourceEditorSchemaNode schema,
        JsonNode value,
        Action? changed = null)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(value);
        return new(schema, value, string.Empty, schema.Name, false, _ => { }, changed ?? NoOp);
    }

    [RelayCommand]
    public void AddItem()
    {
        if (IsReadOnly)
            return;
        if (Kind != ResourceEditorValueKind.Array)
            throw new InvalidOperationException("Only array nodes can add items.");

        var array = EnsureArray();
        var itemSchema = _schema.Items ?? CreateInferredSchema("item", null);
        array.Add(CreateDefaultValue(itemSchema));
        RebuildChildren();
        _changed();
    }

    [RelayCommand(CanExecute = nameof(CanClearSection))]
    private void ClearSection()
    {
        if (!CanClearSection)
            return;

        IsExpanded = false;
        _value = null;
        _replace(null);
        RebuildChildren();
        _changed();
    }

    public bool TryAddMapEntry(string key)
    {
        if (IsReadOnly)
            return false;
        if (Kind != ResourceEditorValueKind.Map)
            throw new InvalidOperationException("Only map nodes can add entries.");
        if (string.IsNullOrWhiteSpace(key))
        {
            SetLocalValidation(nameof(NewMapKey), "Enter a key.");
            return false;
        }

        var map = EnsureObject();
        key = key.Trim();
        if (map.ContainsKey(key))
        {
            SetLocalValidation(nameof(NewMapKey), "Key already exists.");
            return false;
        }

        var itemSchema = _schema.Items ?? CreateInferredSchema(key, null);
        map[key] = CreateDefaultValue(itemSchema);
        SetLocalValidation(nameof(NewMapKey), null);
        NewMapKey = string.Empty;
        RebuildChildren();
        _changed();
        return true;
    }

    [RelayCommand]
    private void AddMapEntry()
    {
        TryAddMapEntry(NewMapKey);
    }

    [RelayCommand(CanExecute = nameof(CanRemove))]
    private void Remove()
    {
        _remove?.Invoke(this);
    }

    public void RemoveChild(ResourceEditorNodeViewModel child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (IsReadOnly)
            return;
        if (Kind == ResourceEditorValueKind.Array && _value is JsonArray array)
        {
            var index = Children.IndexOf(child);
            if (index < 0)
                return;
            array.RemoveAt(index);
        }
        else if (Kind == ResourceEditorValueKind.Map && _value is JsonObject map)
        {
            if (!map.Remove(child.DisplayName))
                return;
        }
        else
        {
            throw new InvalidOperationException("Only array and map nodes can remove children.");
        }

        RebuildChildren();
        _changed();
    }

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName is null)
            return ValidationErrorsByProperty.Values.SelectMany(static errors => errors);

        return ValidationErrorsByProperty.TryGetValue(propertyName, out var propertyErrors)
            ? propertyErrors
            : Array.Empty<string>();
    }

    public void AppendLocalValidationErrors(ICollection<ResourceEditorValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        foreach (var propertyErrors in _localValidationErrors.Values)
        foreach (var error in propertyErrors)
            errors.Add(new ResourceEditorValidationError(Path, error));

        foreach (var child in Children)
            child.AppendLocalValidationErrors(errors);
    }

    public void SetDocumentValidationErrors(IReadOnlyCollection<ResourceEditorValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        var propertyName = ValidationPropertyName;
        var nextErrors = errors
            .Where(error => string.Equals(error.Path, Path, StringComparison.Ordinal))
            .Select(error => error.Message)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (nextErrors.Length == 0)
            _documentValidationErrors.Clear();
        else
        {
            _documentValidationErrors.Clear();
            _documentValidationErrors[propertyName] = nextErrors;
        }

        PublishValidationErrors();
        foreach (var child in Children)
            child.SetDocumentValidationErrors(errors);
    }

    private void SetLocalValidation(string propertyName, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            _localValidationErrors.Remove(propertyName);
        else
            _localValidationErrors[propertyName] = [message];

        PublishValidationErrors();
    }

    private void PublishValidationErrors()
    {
        var oldErrors = ValidationErrorsByProperty;
        var oldHasErrors = HasErrors;
        var nextErrors = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        foreach (var propertyName in _localValidationErrors.Keys.Concat(_documentValidationErrors.Keys).Distinct(StringComparer.Ordinal))
        {
            var messages = new List<string>();
            if (_localValidationErrors.TryGetValue(propertyName, out var localErrors))
                messages.AddRange(localErrors);
            if (_documentValidationErrors.TryGetValue(propertyName, out var documentErrors))
                messages.AddRange(documentErrors);
            nextErrors[propertyName] = messages.Distinct(StringComparer.Ordinal).ToArray();
        }

        if (IsSection)
        {
            var sectionErrors = new List<string>();
            if (nextErrors.TryGetValue(SectionValidationPropertyName, out var ownSectionErrors))
                sectionErrors.AddRange(ownSectionErrors);

            AppendOwnSectionErrors(_localValidationErrors, sectionErrors);
            AppendOwnSectionErrors(_documentValidationErrors, sectionErrors);
            foreach (var child in Children)
                child.AppendValidationSummaries(sectionErrors);

            if (sectionErrors.Count > 0)
                nextErrors[SectionValidationPropertyName] = sectionErrors.Distinct(StringComparer.Ordinal).ToArray();
        }

        ValidationErrorsByProperty = nextErrors;
        OnPropertyChanged(nameof(ValidationErrorsByProperty));
        if (oldHasErrors != HasErrors)
            OnPropertyChanged(nameof(HasErrors));

        foreach (var propertyName in oldErrors.Keys.Concat(nextErrors.Keys).Distinct(StringComparer.Ordinal))
        {
            oldErrors.TryGetValue(propertyName, out var oldPropertyErrors);
            nextErrors.TryGetValue(propertyName, out var nextPropertyErrors);
            if (!Enumerable.SequenceEqual(oldPropertyErrors ?? [], nextPropertyErrors ?? [], StringComparer.Ordinal))
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        _parent?.PublishValidationErrors();
    }

    private void AppendValidationSummaries(ICollection<string> errors)
    {
        AppendValidationSummaries(_localValidationErrors, errors);
        AppendValidationSummaries(_documentValidationErrors, errors);
        foreach (var child in Children)
            child.AppendValidationSummaries(errors);
    }

    private void AppendOwnSectionErrors(
        IReadOnlyDictionary<string, IReadOnlyList<string>> errorsByProperty,
        ICollection<string> errors)
    {
        foreach (var (propertyName, messages) in errorsByProperty)
        {
            if (string.Equals(propertyName, SectionValidationPropertyName, StringComparison.Ordinal))
                continue;

            AppendValidationSummaries(messages, errors);
        }
    }

    private void AppendValidationSummaries(
        IReadOnlyDictionary<string, IReadOnlyList<string>> errorsByProperty,
        ICollection<string> errors)
    {
        foreach (var messages in errorsByProperty.Values)
            AppendValidationSummaries(messages, errors);
    }

    private void AppendValidationSummaries(IEnumerable<string> messages, ICollection<string> errors)
    {
        foreach (var message in messages)
            errors.Add(string.IsNullOrEmpty(Path) ? message : $"{Path}: {message}");
    }

    private void LoadScalarValue(JsonNode? value)
    {
        switch (Kind)
        {
            case ResourceEditorValueKind.String:
                _stringValue = TryGetValue<string>(value) ?? string.Empty;
                InitializeYamlEditor(value);
                break;
            case ResourceEditorValueKind.Number:
                _numberValue = value?.ToJsonString() ?? string.Empty;
                break;
            case ResourceEditorValueKind.Boolean:
                _booleanValue = TryGetValue<bool>(value);
                break;
            case ResourceEditorValueKind.Enum:
                _selectedEnum = TryGetValue<string>(value);
                break;
            case ResourceEditorValueKind.Unknown:
                _stringValue = value is JsonValue ? TryGetValue<string>(value) ?? value.ToJsonString() : value?.ToJsonString() ?? string.Empty;
                InitializeYamlEditor(value);
                break;
        }

        if (Kind is ResourceEditorValueKind.Object or ResourceEditorValueKind.Map)
            InitializeYamlEditor(value);
    }

    private void RebuildChildren()
    {
        foreach (var child in Children)
            child._parent = null;
        Children.Clear();
        if (Kind == ResourceEditorValueKind.Object)
            BuildObjectChildren();
        else if (Kind == ResourceEditorValueKind.Array)
            BuildArrayChildren();
        else if (Kind == ResourceEditorValueKind.Map)
            BuildMapChildren();
        PublishValidationErrors();
    }

    private void BuildObjectChildren()
    {
        var obj = _value as JsonObject;
        foreach (var property in _schema.Properties)
        {
            JsonNode? childValue = null;
            obj?.TryGetPropertyValue(property.Key, out childValue);
            var childPath = JoinPath(Path, property.Key);
            var localKey = property.Key;
            Children.Add(new(
                property.Value,
                childValue,
                childPath,
                property.Key,
                _schema.Required.Contains(property.Key),
                replacement =>
                {
                    var target = EnsureObject();
                    target[localKey] = replacement;
                },
                _changed,
                parent: this));
        }

        if (obj is null)
            return;
        foreach (var property in obj.Where(pair => !_schema.Properties.ContainsKey(pair.Key)))
        {
            var inferred = CreateInferredSchema(property.Key, property.Value);
            var localKey = property.Key;
            Children.Add(new(inferred, property.Value, JoinPath(Path, property.Key), property.Key, false,
                replacement => obj[localKey] = replacement, _changed, parent: this));
        }
    }

    private void BuildArrayChildren()
    {
        if (_value is not JsonArray array)
            return;
        for (var index = 0; index < array.Count; index++)
        {
            var itemSchema = _schema.Items ?? CreateInferredSchema("item", array[index]);
            var itemIndex = index;
            Children.Add(new(itemSchema, array[index], $"{Path}[{index}]", $"Item {index + 1}", false,
                replacement => array[itemIndex] = replacement, _changed, RemoveChild, this));
        }
    }

    private void BuildMapChildren()
    {
        if (_value is not JsonObject map)
            return;
        foreach (var property in map)
        {
            var itemSchema = _schema.Items ?? CreateInferredSchema(property.Key, property.Value);
            var localKey = property.Key;
            Children.Add(new(itemSchema, property.Value, JoinPath(Path, property.Key), property.Key, false,
                replacement => map[localKey] = replacement, _changed, RemoveChild, this));
        }
    }

    private JsonObject EnsureObject()
    {
        if (_value is JsonObject obj)
            return obj;
        obj = new JsonObject();
        ReplaceValue(obj);
        return obj;
    }

    private JsonArray EnsureArray()
    {
        if (_value is JsonArray array)
            return array;
        array = [];
        ReplaceValue(array);
        return array;
    }

    private void ReplaceValue(JsonNode? value)
    {
        _value = value;
        _replace(value);
        _changed();
    }

    private static JsonNode? CreateDefaultValue(ResourceEditorSchemaNode schema)
        => schema.ValueKind switch
        {
            ResourceEditorValueKind.Object or ResourceEditorValueKind.Map => new JsonObject(),
            ResourceEditorValueKind.Array => new JsonArray(),
            ResourceEditorValueKind.Boolean => JsonValue.Create(false),
            ResourceEditorValueKind.Number => JsonValue.Create(0m),
            ResourceEditorValueKind.Enum => schema.EnumValues.Count > 0 ? JsonValue.Create(schema.EnumValues[0]) : null,
            _ => JsonValue.Create(string.Empty),
        };

    private static ResourceEditorSchemaNode CreateInferredSchema(string name, JsonNode? value)
    {
        var kind = value switch
        {
            JsonObject => ResourceEditorValueKind.Map,
            JsonArray => ResourceEditorValueKind.Array,
            JsonValue jsonValue when jsonValue.TryGetValue<bool>(out _) => ResourceEditorValueKind.Boolean,
            JsonValue jsonValue when jsonValue.TryGetValue<decimal>(out _) => ResourceEditorValueKind.Number,
            JsonValue => ResourceEditorValueKind.String,
            _ => ResourceEditorValueKind.Unknown,
        };
        return new(name, kind, null,
            new Dictionary<string, ResourceEditorSchemaNode>(StringComparer.Ordinal), null, [],
            new HashSet<string>(StringComparer.Ordinal), null);
    }

    private static T? TryGetValue<T>(JsonNode? node)
    {
        return node is JsonValue value && value.TryGetValue<T>(out var result) ? result : default;
    }

    private void InitializeYamlEditor(JsonNode? value)
    {
        if (!_schema.IsYamlEditor)
            return;

        IsYamlText = true;
        if (value is JsonValue && TryGetValue<string>(value) is string text)
        {
            try
            {
                var parsed = KubernetesYaml.Deserialize<JsonNode>(text);
                _yamlValue = parsed is null ? text : KubernetesYaml.Serialize(parsed);
                return;
            }
            catch (YamlDotNet.Core.YamlException)
            {
                try
                {
                    var parsed = JsonNode.Parse(text);
                    _yamlValue = parsed is null ? text : KubernetesYaml.Serialize(parsed);
                    return;
                }
                catch (JsonException)
                {
                    _yamlValue = text;
                    return;
                }
            }
        }

        _yamlValue = value is null ? string.Empty : KubernetesYaml.Serialize(value);
    }

    private static string JoinPath(string path, string name)
        => string.IsNullOrEmpty(path) ? name : $"{path}.{name}";

    private static void NoOp()
    {
    }
}
