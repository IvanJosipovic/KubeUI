using System.Reflection;
using System.Text.Json.Nodes;
using YamlDotNet.System.Text.Json;

namespace KubeUI.Core.Components.Dynamic
{
    public partial class Controls<TItem>
    {
        [Parameter]
        public TItem Item { get; set; }

        [Parameter]
        public EventCallback<TItem> ItemChanged { get; set; }

        [Parameter]
        public bool ReadOnly { get; set; }

        [Inject]
        protected ILogger<Controls<TItem>> Logger { get; set; }

        protected void Update()
        {
            if (ItemChanged.HasDelegate)
            {
                ItemChanged.InvokeAsync(Item);
            }
        }

        protected override void OnInitialized()
        {
            if (Item == null)
            {
                Item = Utilities.CreateInstance<TItem>();
                Update();
            }
        }

        protected void OnChange(Object e, PropertyInfo prop)
        {
            try
            {
                if (prop.PropertyType == typeof(string) || prop.DeclaringType == typeof(string))
                {
                    prop.SetValue(Item, e == null ? null : e.ToString());
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(Item, bool.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : default(bool));
                }
                else if (prop.PropertyType == typeof(bool? ))
                {
                    prop.SetValue(Item, bool.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : null);
                }
                else if (prop.PropertyType == typeof(int))
                {
                    prop.SetValue(Item, int.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : default(int));
                }
                else if (prop.PropertyType == typeof(int? ))
                {
                    prop.SetValue(Item, int.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : null);
                }
                else if (prop.PropertyType == typeof(long))
                {
                    prop.SetValue(Item, long.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : default(long));
                }
                else if (prop.PropertyType == typeof(long? ))
                {
                    prop.SetValue(Item, long.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : null);
                }
                else if (prop.PropertyType == typeof(Dictionary<string, string>))
                {
                    prop.SetValue(Item, (Dictionary<string, string>)e);
                }
                else if (prop.PropertyType == typeof(System.Collections.ObjectModel.Collection<string>))
                {
                    prop.SetValue(Item, (System.Collections.ObjectModel.Collection<string>)e);
                }
                else if (prop.PropertyType == typeof(JsonNode))
                {
                    prop.SetValue(Item, YamlConverter.Deserialize<JsonNode>(e?.ToString()));
                }
                else if (prop.PropertyType == typeof(DateTime))
                {
                    prop.SetValue(Item, DateTime.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : default(DateTime));
                }
                else if (prop.PropertyType == typeof(DateTime? ))
                {
                    prop.SetValue(Item, DateTime.TryParse(e?.ToString(), out var tmpvalue) ? tmpvalue : null);
                }
                else
                {
                    prop.SetValue(Item, e);
                }

                Update();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Casting Error : {0} - {1}", e.GetType(), e);
            }
        }

        private object? GetValue(PropertyInfo prop, object item)
        {
            if (prop.PropertyType == typeof(string) || prop.DeclaringType == typeof(string))
            {
                return prop.GetValue(Item) as string;
            }
            else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool? ))
            {
                return prop.GetValue(Item) != null ? bool.Parse(prop.GetValue(Item).ToString()) : null;
            }
            else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int? ))
            {
                return prop.GetValue(Item) != null ? int.Parse(prop.GetValue(Item).ToString()) : null;
            }
            else if (prop.PropertyType == typeof(long) || prop.PropertyType == typeof(long? ))
            {
                return prop.GetValue(Item) != null ? long.Parse(prop.GetValue(Item).ToString()) : null;
            }

            return prop.GetValue(item, null);
        }

        public static bool ShowControl(PropertyInfo prop)
        {
            if (prop.PropertyType.Namespace.Equals(typeof(V1Deployment).Namespace) ||
                prop.PropertyType.Namespace.StartsWith("KubeUI.") ||
                prop.PropertyType.Namespace.StartsWith("KubernetesCRDModelGen.Models")) return false;

            return true;
        }
    }
}