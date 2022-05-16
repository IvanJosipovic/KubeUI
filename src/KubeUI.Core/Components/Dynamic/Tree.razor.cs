using Microsoft.AspNetCore.Components;
using KubeUI.Core.Client;
using Microsoft.Extensions.Logging;
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

            foreach (var property in obj.GetType().GetProperties().Where(x => x.PropertyType.FullName.StartsWith("k8s.") || x.PropertyType.FullName.StartsWith("System.Collections.")))
            {
                try
                {
                    var item = property.GetValue(obj);

                    if (item == null)
                    {
                        try
                        {
                            item = Activator.CreateInstance(property.PropertyType);
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

            int n = (int)type.GetProperty("Count").GetValue(collection);

            for (int i = 0; i < n; i++)
            {
                object[] index = { i };

                object myObject = type.GetProperty("Item").GetValue(collection, index);

                string? displayPropertyName = null; //Common.GetDisplayInTreeName(attributes);

                string? propertyValue = null;

                if (displayPropertyName != null)
                {
                    var property = genType.GetProperty(displayPropertyName);

                    propertyValue = property?.GetValue(myObject)?.ToString();
                }

                if (string.IsNullOrEmpty(propertyValue))
                {
                    propertyValue = $"Item {index[0]}";
                }

                tree.Add(new TreeItem()
                {
                    Name = propertyValue,
                    TreeItems = BuildTree(myObject),
                    Object = myObject,
                    //Summary = genType.GetSummary(),
                    IsCollectionItem = true,
                    Collection = collection
                });
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

            var newObj = Activator.CreateInstance(genType);

            object[] data = { newObj };

            obj.GetType().GetMethod("Add").Invoke(obj, data);

            StateHasChanged();
        }
    }
}