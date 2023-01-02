using KubeUI.Core;
using System.Collections;
using System.Reflection;

namespace KubeUI.UI.Components.Dynamic;

public partial class Tree<TItem>
{
    [Inject]
    private ILogger<Tree<TItem>> Logger { get; set; }

    [Parameter]
    public TItem Item { get; set; }

    [Parameter]
    public EventCallback<TItem> ItemChanged { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; }

    [Parameter]
    public EventCallback<RenderFragment> ObjectSelected { get; set; }

    private HashSet<TreeItem> TreeItems { get; set; } = new HashSet<TreeItem>();

    public HashSet<TreeItem> BuildTree(object obj)
    {
        HashSet<TreeItem> Tree = new();

        if (obj is V1JSONSchemaProps)
        {
            return Tree;
        }

        foreach (var property in obj.GetType().GetProperties().Where(ShowInTree))
        {
            try
            {
                var item = property.GetValue(obj);

                if (item == null && ReadOnly)
                {
                    continue;
                }

                if (item == null)
                {
                    try
                    {
                        if (property.PropertyType == typeof(IntstrIntOrString))
                        {
                            continue;
                        }
                        item = Utilities.CreateInstance(property.PropertyType);
                        property.SetValue(obj, item);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to create instance of {0}", property.PropertyType.FullName);
                        continue;
                    }
                }

                if (item.GetType().FullName.StartsWith("System.Collections."))
                {
                    Tree.Add(new TreeItem()
                    {
                        Name = property.Name.AddSpacesBeforeCapitals(),
                        TreeItems = GetCollectionItems(item),
                        Object = item,
                        IsCollection = true,
                        Summary = property.GetSummary()
                    });
                }
                else
                {
                    Tree.Add(new TreeItem()
                    {
                        Name = property.Name.AddSpacesBeforeCapitals(),
                        TreeItems = BuildTree(item),
                        Object = item,
                        Summary = property.GetSummary()
                    });
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e, "BuildTree Failed: {msg}", e.Message);
            }
        }

        return Tree;
    }

    public HashSet<TreeItem> GetCollectionItems(object collection)
    {
        var tree = new HashSet<TreeItem>();
        var type = collection.GetType();
        var genType = type.GetTypeInfo().GenericTypeArguments[0];

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>) && genType == typeof(string))
        {
            foreach (DictionaryEntry obj in (IDictionary)collection)
            {
                tree.Add(new TreeItem()
                {
                    Name = obj.Key.ToString(),
                    TreeItems = BuildTree(obj),
                    Object = obj,
                    Summary = genType.GetSummary(),
                    IsCollectionItem = true,
                    Collection = collection
                });
            }
        }
        else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var list = (IList)collection;
            var n = list.Count;

            for (var i = 0; i < n; i++)
            {
                var myObject = list[i];

                tree.Add(new TreeItem()
                {
                    Name = $"Item {i}",
                    TreeItems = BuildTree(myObject),
                    Object = myObject,
                    Summary = genType.GetSummary(),
                    IsCollectionItem = true,
                    Collection = collection
                });
            }
        }
        else
        {
            Logger.LogWarning("Missing Collection Type: {type}", type.FullName);
        }

        return tree;
    }

    private TreeItem SelectedValue { get; set; }

    protected override void OnInitialized()
    {
        var rootItem = new TreeItem()
        {
            Name = Item.GetType().Name,
            Object = Item,
            IsExpanded = true,
            TreeItems = BuildTree(Item),
            Summary = Item.GetType().GetSummary()
        };

        TreeItems.Clear();
        TreeItems.Add(rootItem);
        SelectedValue = rootItem;
    }

    private void AddItem(object obj)
    {
        var genType = obj.GetType().GetTypeInfo().GenericTypeArguments[0];

        var newObj = Utilities.CreateInstance(genType);

        ((IList)obj).Add(newObj);

        OnInitialized();

        StateHasChanged();
    }

    private void DeleteItem(object collection, object obj)
    {
        ((IList)collection).Remove(obj);

        OnInitialized();

        StateHasChanged();
    }

    private RenderFragment RenderForm(object obj)
    {
        return builder =>
        {
            builder.OpenComponent(0, typeof(Controls<>).MakeGenericType(obj.GetType()));
            builder.AddAttribute(1, "Item", obj);
            //builder.AddAttribute(2, "ItemChanged", EventCallback.Factory.Create<TItem>(this, (e) => obj = e));
            builder.AddAttribute(3, "ReadOnly", ReadOnly);
            builder.CloseComponent();
        };
    }

    public static bool ShowInTree(PropertyInfo prop)
    {
        if (!prop.PropertyType.Namespace.Equals(typeof(V1Deployment).Namespace) &&
            !prop.PropertyType.Namespace.StartsWith("KubeUI.") &&
            !prop.PropertyType.Namespace.StartsWith("System.Collections.") &&
            !prop.PropertyType.Namespace.StartsWith("KubernetesCRDModelGen.Models")) return false;

        if (prop.PropertyType == typeof(IList<string>)) return false;

        if (prop.PropertyType == typeof(IDictionary<string, string>)) return false;

        if (prop.PropertyType == typeof(IDictionary<string, ResourceQuantity>)) return false;

        return true;
    }
}