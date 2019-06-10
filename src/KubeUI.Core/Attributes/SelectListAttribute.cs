using System;

namespace KubeUI.Core
{
    /// <summary>
    /// Display this control as a select list
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false)]
    public class SelectListAttribute : Attribute
    {
        /// <summary>
        /// Select list options
        /// </summary>
        public string[] Options { get; set; }
    }
}