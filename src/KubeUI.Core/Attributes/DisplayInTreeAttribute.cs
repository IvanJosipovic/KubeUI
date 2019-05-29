using System;

namespace KubeUI.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class DisplayInTreeAttribute : Attribute
    {
        public string DisplayName { get; set; }
    }
}