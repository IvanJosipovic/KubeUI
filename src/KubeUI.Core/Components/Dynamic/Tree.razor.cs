using k8s.Models;
using KubeUI.Core.Client;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;

namespace KubeUI.Core.Components.Dynamic
{
    public partial class Tree<TItem>
    {
        [Inject] private ILogger<Tree<TItem>> Logger { get; set; }

        [Parameter]
        public TItem Item { get; set; }

        [Parameter]
        public EventCallback<object> ObjectSelected { get; set; }

        private HashSet<TreeItem> TreeItems { get; set; } = new HashSet<TreeItem>();

        public HashSet<TreeItem> BuildTree(object obj)
        {
            HashSet<TreeItem> Tree = new HashSet<TreeItem>();

            if (obj is V1JSONSchemaProps)
            {
                return Tree;
            }

            foreach (var property in obj.GetType().GetProperties().Where(x => x.PropertyType.FullName.StartsWith("k8s.") || x.PropertyType.FullName.StartsWith("System.Collections.")))
            {
                try
                {
                    var item = property.GetValue(obj);

                    if (item == null)
                    {
                        try
                        {
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
                            IsCollection = true
                            //Summary = property.GetSummary()
                        });
                    }
                    else
                    {
                        Tree.Add(new TreeItem()
                        {
                            Name = property.Name.AddSpacesBeforeCapitals(),
                            TreeItems = BuildTree(item),
                            Object = item,
                            //Summary = property.GetSummary()
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
                        //Summary = genType.GetSummary(),
                        IsCollectionItem = true,
                        Collection = collection
                    });
                }
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = (IList)collection;
                int n = list.Count;

                for (int i = 0; i < n; i++)
                {
                    object myObject = list[i];

                    string? propertyValue = null;

                    tree.Add(new TreeItem()
                    {
                        Name = $"Item {i}",
                        TreeItems = BuildTree(myObject),
                        Object = myObject,
                        //Summary = genType.GetSummary(),
                        IsCollectionItem = true,
                        Collection = collection
                    });
                }
            }

            return tree;
        }

        private TreeItem SelectedValue { get; set; }

        protected override void OnInitialized()
        {
            TreeItems.Add(new TreeItem()
            {
                Name = Item.GetType().Name,
                Object = Item,
                IsExpanded = true,
                TreeItems = BuildTree(Item)
                //Summary = property.GetSummary()
            });
        }

        private void AddItem(object obj)
        {
            var genType = obj.GetType().GetTypeInfo().GenericTypeArguments[0];

            var newObj = Utilities.CreateInstance(genType);

            ((IList)genType).Add(newObj);

            StateHasChanged();
        }
    }
}