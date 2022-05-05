using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using MudBlazor;
using KubeUI.Core.Shared;
using KubeUI.Core.Components;
using k8s.Models;
using k8s;
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

        private HashSet<TreeItem> TreeItems { get; set; } = new HashSet<TreeItem>();

        public HashSet<TreeItem> BuildTree(object obj)
        {
            if (obj.GetType().FullName.StartsWith("System.Collections."))
            {
                return GetCollectionItems(obj);
            }

            //Logger.LogDebug("BuildTree {0}", obj.GetType());
            HashSet<TreeItem> Tree = new HashSet<TreeItem>();

            foreach (var property in obj.GetType().GetProperties().Where(x => x.PropertyType.FullName.StartsWith("k8s.")))
            {
                try
                {
                    //Logger.LogDebug("BuildTree {0}", property.PropertyType);

                    var item = property.GetValue(obj);
                    if (item == null)
                    {
                        item = Activator.CreateInstance(property.PropertyType);
                        property.SetValue(obj, item);
                    }

                    Tree.Add(new TreeItem()
                    {
                        Name = property.Name.AddSpacesBeforeCapitals(),
                        TreeItems = BuildTree(item),
                        Object = item,
                        //Summary = property.GetSummary()
                    });
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "BuildTree Failed: " + e.Message);
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

                string displayPropertyName = null; //Common.GetDisplayInTreeName(attributes);

                string propertyValue = null;

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
                });
            }

            return tree;
        }

        private TreeItem SelectedValue { get; set; }
        
        protected override void OnInitialized()
        {
            TreeItems = BuildTree(Item);
        }
    }
}