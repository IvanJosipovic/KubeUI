using System;

namespace KubeUI.Core
{
    /// <summary>
    /// Display this collection in the tree
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class DisplayInTreeAttribute : Attribute
    {
        /// <summary>
        /// The Name of the property to display in the tree
        /// </summary>
        public string DisplayName { get; set; }
    }
}